using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Iv4xr.SePlugin.Custom.Experiments;
using Iv4xr.SePlugin.Custom.Experiments.RoboticLegMotor;
using Iv4xr.SePlugin.Custom.Experiments.ThrowerArm;
using Sandbox;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.GameSystems;
using Sandbox.Game.Gui;
using Sandbox.Game.GUI;
using Sandbox.Game.SessionComponents;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage;
using VRage.FileSystem;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace Iv4xr.SePlugin.Custom
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class CustomSessionComponent : MySessionComponentBase
    {
        private bool isInit;
        private bool enableSensors = false;

        private Sensors sensors = new Sensors();
        private List<Sensors.RayCastResult> rayCastResults;

        private string behaviourDescriptorsFile;
        private List<Vector3D> behaviourDescriptors;
        private IEnumerator behaviourDescriptorsEnumerator;
        private RoboticArmController roboticArmController;
        private List<IEnumerator> coroutines = new List<IEnumerator>();
        private List<ThrowerArmController> controllers;
        private List<RoboticLegMotorController> legControllers;

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();

            if (!isInit)
            {
                isInit = true;
                Init();
            }

            if (enableSensors)
            {
                ComputeSensors();
            }

            if (behaviourDescriptorsEnumerator != null)
            {
                var shouldContinue = behaviourDescriptorsEnumerator.MoveNext();
                if (!shouldContinue)
                {
                    behaviourDescriptorsEnumerator = null;
                }
            }

            foreach (var coroutine in coroutines.ToList())
            {
                var shouldContinue = coroutine.MoveNext();
                if (!shouldContinue)
                {
                    coroutines.Remove(coroutine);
                }
            }

            if (controllers != null)
            {
                var random = new Random();
                for (var i = 0; i < controllers.Count; i++)
                {
                    var controller = controllers[i];
                    var baseSpeed = i / (float) controllers.Count;
                    var offset = (float) (random.NextDouble() * 0.4);
                    var speed = baseSpeed + offset;

                    var command = new FlatCommand()
                    {
                        Values = new List<float>()
                        {
                            speed, speed, speed, speed
                        }
                    };

                    controller.ProcessCommand(command);
                }
            }

            if (legControllers != null)
            {
                var random = new Random();
                for (var i = 0; i < legControllers.Count; i++)
                {
                    var controller = legControllers[i];
                    var baseSpeed = i / (float)legControllers.Count;
                    var offset = (float)(random.NextDouble() * 0.4);
                    var speed = baseSpeed + offset;

                    if (random.NextDouble() < 0.5)
                    {
                        speed *= -1;
                    }

                    var command = new FlatCommand()
                    {
                        Values = new List<float>()
                        {
                            speed, speed, speed, speed
                        }
                    };

                    controller.ProcessCommand(command);
                }
            }
        }

        private void Init()
        {
            MyAPIGateway.Utilities.MessageEntered += MessageEntered;
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Utilities.MessageEntered -= MessageEntered;
        }

        private void MessageEntered(string text, ref bool others)
        {
            if (text.StartsWith("/ToggleSensors", StringComparison.InvariantCultureIgnoreCase))
            {
                enableSensors = !enableSensors;
                MyAPIGateway.Utilities.ShowMessage("Helper", $"Sensors {(enableSensors ? "enabled" : "disabled")}");
            }

            if (text.StartsWith("/ToggleMaxSpeed", StringComparison.InvariantCultureIgnoreCase))
            {
                MySandboxGame.Static.EnableMaxSpeed = !MySandboxGame.Static.EnableMaxSpeed;
                MyAPIGateway.Utilities.ShowMessage("Helper",
                    $"Maximum simulation speed {(MySandboxGame.Static.EnableMaxSpeed ? "enabled" : "disabled")}");
            }

            if (text.StartsWith("/bds load", StringComparison.InvariantCultureIgnoreCase))
            {
                var prefix = "/bds load ";
                var prefixLength = prefix.Length;
                var file = text.Substring(prefixLength);

                if (!File.Exists(file))
                {
                    MyAPIGateway.Utilities.ShowMessage("Helper", $"The path \"{file}\" does not exist");
                }
                else
                {
                    MyAPIGateway.Utilities.ShowMessage("Helper", $"Behaviour descriptors file loaded");
                    behaviourDescriptorsFile = file;
                }
            }

            if (text.StartsWith("/bds show", StringComparison.InvariantCultureIgnoreCase))
            {
                if (behaviourDescriptorsFile == null)
                {
                    MyAPIGateway.Utilities.ShowMessage("Helper", $"Please call the \"/bds load filename\" first");
                }
                else
                {
                    behaviourDescriptorsEnumerator = ComputeBehaviourDescriptors();
                }
            }

            if (text.StartsWith("/bds stop", StringComparison.InvariantCultureIgnoreCase))
            {
                behaviourDescriptorsEnumerator = null;
            }

            if (text.StartsWith("/bds clear", StringComparison.InvariantCultureIgnoreCase))
            {
                behaviourDescriptors = null;
                behaviourDescriptorsEnumerator = null;
            }

            if (text.StartsWith("/arm", StringComparison.InvariantCultureIgnoreCase))
            {
                if (roboticArmController == null)
                {
                    roboticArmController = new RoboticArmController();
                    roboticArmController.Init();
                }

                if (text.StartsWith("/arm set", StringComparison.InvariantCultureIgnoreCase))
                {
                    var args = text.Split(' ');

                    if (args.Length - 2 != 3)
                    {
                        MyAPIGateway.Utilities.ShowMessage("Helper", $"Invalid number of arguments");
                    }
                    else
                    {
                        var valid1 = float.TryParse(args[2], out var rotor1Velocity);
                        var valid2 = float.TryParse(args[3], out var rotor2Velocity);
                        var valid3 = float.TryParse(args[4], out var rotor3Velocity);

                        if (!valid1 || !valid2 || !valid3)
                        {
                            MyAPIGateway.Utilities.ShowMessage("Helper", $"Some parameters are not numbers");
                        }
                        else
                        {
                            roboticArmController.Set(rotor1Velocity, rotor2Velocity, rotor3Velocity);
                        }
                    }
                }

                if (text.StartsWith("/arm toggle", StringComparison.InvariantCultureIgnoreCase))
                {
                    var rotor = roboticArmController.GetRotor("Rotor 1");
                    rotor.Enabled = !rotor.Enabled;
                }

                if (text.Equals("/arm resetBall", StringComparison.InvariantCultureIgnoreCase))
                {
                    roboticArmController.ResetBall();
                }

                if (text.Equals("/arm throw", StringComparison.InvariantCultureIgnoreCase))
                {
                    roboticArmController.Throw();
                }

                if (text.Equals("/arm reset", StringComparison.InvariantCultureIgnoreCase))
                {
                    StartCoroutine(roboticArmController.Reset());
                }
            }

            if (text.Equals("/prefab", StringComparison.InvariantCultureIgnoreCase))
            {
                var path = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, "ThrowerArm - NoHinges", "bp.sbc");
                var definition = MyBlueprintUtils.LoadPrefab(path);
                var blueprint = definition.ShipBlueprints[0];

                var tempList = new List<MyObjectBuilder_EntityBase>();
                var offset = new Vector3D(new Vector3(-300, 300, 300));

                // We SHOULD NOT make any changes directly to the prefab, we need to make a Value copy using Clone(), and modify that instead.
                foreach (var grid in blueprint.CubeGrids)
                {
                    var gridBuilder = (MyObjectBuilder_CubeGrid)grid.Clone();
                    gridBuilder.PositionAndOrientation = new MyPositionAndOrientation(grid.PositionAndOrientation.Value.Position + offset, grid.PositionAndOrientation.Value.Forward, grid.PositionAndOrientation.Value.Up);

                    tempList.Add(gridBuilder);
                }

                var entities = new List<IMyEntity>();

                MyAPIGateway.Entities.RemapObjectBuilderCollection(tempList);
                foreach (var item in tempList)
                    entities.Add(MyAPIGateway.Entities.CreateFromObjectBuilderAndAdd(item));
                // MyAPIGateway.Multiplayer.SendEntitiesCreated(tempList);
            }

            if (text.Equals("/prefab2", StringComparison.InvariantCultureIgnoreCase))
            {
                var path = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, "ThrowerArm - NoHinges", "bp.sbc");
                var definition = MyBlueprintUtils.LoadPrefab(path);

                MyVisualScriptLogicProvider.SpawnLocalBlueprint("ThrowerArm - WithHinges", new Vector3D(new Vector3(-300, 300, 300)), Vector3D.Up, "Test");
            }

            if (text.Equals("/prefab3", StringComparison.InvariantCultureIgnoreCase))
            {
                Utils.SpawnBlueprint("ThrowerArm - WithHinges", new Vector3D(new Vector3(-500, 300, 300)), new Vector3D(), "Test");
            }

            if (text.StartsWith("/prefab4", StringComparison.InvariantCultureIgnoreCase))
            {
                var args = text.Split(' ');
                var gridSize = int.Parse(args[1]);

                var number = 0;
                var initialPosition = new Vector3D(new Vector3(-300, 300, 300));
                var offsetX = new Vector3D(new Vector3(200, 0, 0));
                var offsetZ = new Vector3D(new Vector3(0, 0, 200));
                
                for (int i = 0; i < gridSize; i++)
                {
                    for (int j = 0; j < gridSize; j++)
                    {
                        number++;

                        var position = initialPosition + i * offsetX + j * offsetZ;
                        var name = $"ThrowerArm - WithHinges {number}";

                        Utils.SpawnBlueprint("ThrowerArm - WithHinges", position, new Vector3D(), name);
                    }
                }
            }

            if (text.StartsWith("/leg1", StringComparison.InvariantCultureIgnoreCase))
            {
                var args = text.Split(' ');
                var gridSize = int.Parse(args[1]);

                var number = 0;
                var initialPosition = new Vector3D(new Vector3(-300, 300, 300));
                var offsetX = new Vector3D(new Vector3(30, 0, 0));
                var offsetZ = new Vector3D(new Vector3(0, 0, 30));

                for (int i = 0; i < gridSize; i++)
                {
                    for (int j = 0; j < gridSize; j++)
                    {
                        number++;

                        var position = initialPosition + i * offsetX + j * offsetZ;
                        var name = $"RoboticLeg v1 {number}";

                        Utils.SpawnBlueprint("RoboticLeg v1", position, new Vector3D(), name);
                    }
                }
            }

            if (text.StartsWith("/leg2", StringComparison.InvariantCultureIgnoreCase))
            {
                var args = text.Split(' ');
                var gridSize = int.Parse(args[1]);

                legControllers = new List<RoboticLegMotorController>();

                for (int i = 0; i < gridSize * gridSize; i++)
                {
                    var number = i + 1;

                    var name = $"RoboticLeg v1 {number}";
                    var controller = new RoboticLegMotorController(name);
                    controller.Init();
                    legControllers.Add(controller);
                }
            }

            if (text.StartsWith("/prefab5", StringComparison.InvariantCultureIgnoreCase))
            {
                var args = text.Split(' ');
                var gridSize = int.Parse(args[1]);

                controllers = new List<ThrowerArmController>();

                for (int i = 0; i < gridSize * gridSize; i++)
                {
                    var number = i + 1;

                    var name = $"ThrowerArm - WithHinges {number}";
                    var controller = new ThrowerArmController(name);
                    controller.Init();
                    controllers.Add(controller);
                }
            }

            if (text.Equals("/remove", StringComparison.InvariantCultureIgnoreCase))
            {
                var players = new List<IMyPlayer>();
                MyAPIGateway.Players.GetPlayers(players);
                var player = players[0];
                var playerPosition = player.Character.PositionComp.GetPosition();
                var sphere = new BoundingSphereD(playerPosition, radius: 1500.0);
                var entities = MyEntities.GetEntitiesInSphere(ref sphere);

                foreach (var entity in entities)
                {
                    if (entity is MyCubeGrid grid)
                    {
                        grid.SendGridCloseRequest();
                    }
                }

                entities.Clear();
            }

            if (text.Equals("/pos", StringComparison.InvariantCultureIgnoreCase))
            {
                var players = new List<IMyPlayer>();
                MyAPIGateway.Players.GetPlayers(players);
                var player = players[0];
                var playerPosition = player.Character.PositionComp.GetPosition();
                var orientation = player.Character.PositionComp.GetOrientation();

                MyAPIGateway.Utilities.ShowMessage("Helper", $"Current position: {playerPosition}");
                MyAPIGateway.Utilities.ShowMessage("Helper", $"Current orientation: {orientation}");
            }

            //if (text.Equals("/exp", StringComparison.InvariantCultureIgnoreCase))
            //{
            //    var controller = new ThrowerArmController();
            //    controller.Init();
            //}
        }

        private void StartCoroutine(IEnumerator coroutine)
        {
            coroutines.Add(coroutine);
        }



        private void GetEntities()
        {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            var player = players[0];
            var playerPosition = player.Character.PositionComp.GetPosition();
            var sphere = new BoundingSphereD(playerPosition, radius: 50.0);

            var entities = MyEntities.GetEntitiesInSphere(ref sphere);

            var rotor1 = entities
                .Where(x => x is MyMotorAdvancedStator)
                .Cast<MyMotorAdvancedStator>()
                .SingleOrDefault(x => x.CustomName.ToString() == "Rotor 1");

            entities.Clear();
        }

        public override void Draw()
        {
            base.Draw();

            if (enableSensors)
            {
                DrawSensors();
            }

            if (behaviourDescriptors != null)
            {
                DrawBehaviourDescriptors();
            }
        }

        private void DrawBehaviourDescriptors()
        {
            var bounds = 0.5 * Vector3D.One;
            var boundingBox = new BoundingBoxD(-1 * bounds, bounds);
            var color = Color.Red;
            var GIZMO_LINE_MATERIAL_WHITE = MyStringId.GetOrCompute("WeaponLaserIgnoreDepth");

            if (behaviourDescriptors != null)
            {
                foreach (Vector3D behaviourDescriptor in behaviourDescriptors)
                {
                    var matrix = MatrixD.CreateWorld(behaviourDescriptor);
                    MySimpleObjectDraw.DrawTransparentBox(ref matrix, ref boundingBox, ref color, MySimpleObjectRasterizer.SolidAndWireframe, 1, 0.25f, lineMaterial: GIZMO_LINE_MATERIAL_WHITE);
                }
            }
        }

        /// <summary>
        /// Compute sensory data
        /// </summary>
        private void ComputeSensors()
        {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            var player = players[0];

            rayCastResults = sensors.CastRaysAroundPlayer(player, 30, 16);
        }

        /// <summary>
        /// Draw sensory data there were previously computed by ComputeSensors()
        /// </summary>
        private void DrawSensors()
        {
            if (rayCastResults != null)
            {
                var GIZMO_LINE_MATERIAL_WHITE = MyStringId.GetOrCompute("WeaponLaserIgnoreDepth");

                foreach (Sensors.RayCastResult result in rayCastResults)
                {
                    var from = result.From;
                    var to = result.To;
                    var color = Color.White.ToVector4();

                    if (result.IsHit)
                    {
                        var fractionColor = (int)(255 * result.HitDistance.Value / result.MaxDistance);
                        color = new Color(255, fractionColor, fractionColor);
                        to = result.HitPosition.Value;
                    }

                    MySimpleObjectDraw.DrawLine(from, to, GIZMO_LINE_MATERIAL_WHITE, ref color, 0.5f);
                }
            }
        }

        private IEnumerator ComputeBehaviourDescriptors()
        {
            var filename = behaviourDescriptorsFile;
            var lines = File.ReadAllLines(filename);

            for (var i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                var individuals = line.Split(';').ToList();
                var positions = new List<Vector3D>();

                foreach (string individual in individuals)
                {
                    var parts = individual.Split(',');

                    var x = double.Parse(parts[0]);
                    var z = double.Parse(parts[1]);
                    var position = new Vector3D(new Vector2D(x - 510.71, 377), z + 385.2);

                    positions.Add(position);
                }

                behaviourDescriptors = positions;

                var timer = new Stopwatch();
                timer.Start();

                // MyAPIGateway.Utilities.ShowMessage("Helper", $"Generation {i + 1}");
                MyAPIGateway.Utilities.ShowNotification($"Generation {i + 1}");

                while (timer.ElapsedMilliseconds < 1000)
                {
                    yield return null;
                }
            }
        }
    }
}