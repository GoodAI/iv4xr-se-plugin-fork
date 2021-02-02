using System.Collections.Generic;
using System.IO;
using Sandbox;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems;
using Sandbox.Game.Gui;
using Sandbox.Game.SessionComponents;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage;
using VRage.FileSystem;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace Iv4xr.SePlugin.Custom
{
    public static class Utils
    {
        public static void SpawnBlueprint(string name, Vector3D position, Vector3D direction, string newGridName, long ownerId = 0, float gravityOffset = 0, float gravityRotation = 0)
        {
            var localBPPath = Path.Combine(MyFileSystem.UserDataPath, "Blueprints", "local");
            var localBPFullPath = Path.Combine(localBPPath, name, "bp.sbc");
            MyObjectBuilder_ShipBlueprintDefinition[] blueprints = null;

            if (MyFileSystem.FileExists(localBPFullPath))
            {
                MyObjectBuilder_Definitions definitions;
                if (!MyObjectBuilderSerializer.DeserializeXML(localBPFullPath, out definitions))
                {
                    VRage.MyDebug.Fail("Blueprint of name: " + name + " was not found.");
                    return;
                }

                blueprints = definitions.ShipBlueprints;
            }

            if (blueprints == null)
                return;

            // Calculate transformations
            Vector3 gravity = MyGravityProviderSystem.CalculateNaturalGravityInPoint(position);

            // Get artificial gravity
            if (gravity == Vector3.Zero)
                gravity = MyGravityProviderSystem.CalculateArtificialGravityInPoint(position);

            Vector3D up;

            if (gravity != Vector3.Zero)
            {
                gravity.Normalize();
                up = -gravity;
                position = position + gravity * gravityOffset;
                if (direction == Vector3D.Zero)
                {
                    direction = Vector3D.CalculatePerpendicularVector(gravity);
                    if (gravityRotation != 0)
                    {
                        var rotationAlongAxis = MatrixD.CreateFromAxisAngle(up, gravityRotation);
                        direction = Vector3D.Transform(direction, rotationAlongAxis);
                    }
                }
            }
            else
            {
                if (direction == Vector3D.Zero)
                {
                    direction = Vector3D.Right;
                    up = Vector3D.Up;
                }
                else
                {
                    up = Vector3D.CalculatePerpendicularVector(-direction);
                }
            }

            List<MyObjectBuilder_CubeGrid> cubeGrids = new List<MyObjectBuilder_CubeGrid>();
            foreach (var blueprintDefinition in blueprints)
            {
                foreach (var cubeGrid in blueprintDefinition.CubeGrids)
                {
                    var gridBuilder = (MyObjectBuilder_CubeGrid)cubeGrid.Clone();

                    gridBuilder.CreatePhysics = true;
                    gridBuilder.EnableSmallToLargeConnections = true;

                    if (!string.IsNullOrEmpty(newGridName))
                    {
                        gridBuilder.Name = cubeGrids.Count > 0 ? (newGridName + " - " + cubeGrids.Count.ToString()) : newGridName;
                        gridBuilder.DisplayName = cubeGrids.Count > 0 ? (newGridName + " - " + cubeGrids.Count.ToString()) : newGridName;
                    }

                    //foreach (var block in gridBuilder.CubeBlocks)
                    //{
                    //    if (block is MyObjectBuilder_MotorStator motorStator)
                    //    {
                    //        motorStator.CurrentAngle = 1.5f;
                    //        // motorStator.TargetVelocity = 30000;
                    //    }
                    //}

                    cubeGrids.Add(gridBuilder);
                }
            }
            if (!MySandboxGame.IsDedicated)
            {
                MyHud.PushRotatingWheelVisible();
            }

            MatrixD worldMatrix0 = MatrixD.CreateWorld(position, direction, up);
            RelocateGrids(cubeGrids, worldMatrix0);

            MyCubeGrid.RelativeOffset offset = new MyCubeGrid.RelativeOffset();
            offset.Use = false;

            MyMultiplayer.RaiseStaticEvent(s => MyCubeGrid.TryPasteGrid_Implementation, new MyCubeGrid.MyPasteGridParameters(
                cubeGrids, false, false, Vector3.Zero, true, offset, MySession.Static.GetComponent<MySessionComponentDLC>().GetAvailableClientDLCsIds()));
        }

        private static void RelocateGrids(List<MyObjectBuilder_CubeGrid> cubegrids, MatrixD worldMatrix0)
        {
            var original = cubegrids[0].PositionAndOrientation.Value.GetMatrix();
            var invOriginal = Matrix.Invert(original);
            Matrix orientationDelta = invOriginal * worldMatrix0.GetOrientation(); // matrix from original location to new location

            for (int i = 0; i < cubegrids.Count; i++)
            {
                if (!cubegrids[i].PositionAndOrientation.HasValue)
                    continue;

                MatrixD worldMatrix2 = cubegrids[i].PositionAndOrientation.Value.GetMatrix(); //get original rotation and position
                var offset = worldMatrix2.Translation - original.Translation; //calculate offset to first pasted grid

                var offsetTr = Vector3.TransformNormal(offset, orientationDelta); // Transform the offset to new orientation
                worldMatrix2 = worldMatrix2 * orientationDelta; //correct rotation

                Vector3D translation = worldMatrix0.Translation + offsetTr; //correct position

                worldMatrix2.Translation = Vector3D.Zero;
                worldMatrix2 = MatrixD.Orthogonalize(worldMatrix2);
                worldMatrix2.Translation = translation;

                cubegrids[i].PositionAndOrientation = new MyPositionAndOrientation(ref worldMatrix2);// Set the corrected position
            }
        }

        public static void TeleportTo(Vector3D position)
        {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            var player = players[0];
            var character = player.Character;

            var matrix = MatrixD.Identity;

            character.PositionComp.SetWorldMatrix(ref matrix);
            character.PositionComp.SetPosition(position);
        }
    }
}