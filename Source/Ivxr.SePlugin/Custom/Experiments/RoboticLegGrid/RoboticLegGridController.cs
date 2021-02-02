using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Iv4xr.SePlugin.Custom.Coroutines;
using Iv4xr.SePlugin.Custom.Experiments.RoboticLegMotor;
using Sandbox.Game.Entities;
using VRage.Game.Entity;
using VRageMath;

namespace Iv4xr.SePlugin.Custom.Experiments.RoboticLegGrid
{
    public class RoboticLegGridController : ExperimentControllerBase<RoboticLegGridCommand, RoboticLegGridState>
    {
        public int GridSizeX { get; private set; }

        public int GridSizeY { get; private set; }

        public static string GridNamePrefix => "ExpGrid";

        private Dictionary<GridPosition, RoboticLegMotorController> motorControllers;

        public static Vector3D GridPositionBase => new Vector3D(new Vector3(-300, 300, 300));

        public IEnumerator SpawnGrid(int sizeX, int sizeY)
        {
            GridSizeX = sizeX;
            GridSizeY = sizeY;

            for (int i = 0; i < GridSizeX; i++)
            {
                for (int j = 0; j < GridSizeY; j++)
                {
                    SpawnExperiment(i, j);
                    yield return CoroutineUtils.WaitForSeconds(0.05);
                }
            }

            yield return CoroutineUtils.WaitForSeconds(1);

            LoadControllers();
        }

        private void LoadControllers()
        {
            motorControllers = new Dictionary<GridPosition, RoboticLegMotorController>();

            for (int i = 0; i < GridSizeX; i++)
            {
                for (int j = 0; j < GridSizeY; j++)
                {
                    var controller = new RoboticLegMotorController(GetGridName(i, j));
                    controller.Init();
                    motorControllers.Add(new GridPosition(i, j), controller);
                }
            }
        }

        private void SpawnExperiment(int x, int y)
        {
            var offsetX = new Vector3D(new Vector3(-30, 0, 0));
            var offsetZ = new Vector3D(new Vector3(0, 0, 30));

            var position = GridPositionBase + x * offsetX + y * offsetZ;
            var name = GetGridName(x, y);

            Utils.SpawnBlueprint("RoboticLeg v1", position, new Vector3D(), name);
        }

        public void ClearExperiment(int x, int y, List<MyCubeGrid> grids = null)
        {
            var gridName = GetGridName(x, y);
            Clear(gridName, grids);
        }

        public void RestartExperiment(int x, int y)
        {
            ClearExperiment(x, y);
            SpawnExperiment(x, y);
        }

        public void DoRandomMoves()
        {
            var random = new Random();
            var controllers = motorControllers.Values.ToList();

            for (var i = 0; i < controllers.Count; i++)
            {
                var controller = controllers[i];
                var baseSpeed = i / (float)controllers.Count;
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

        private static void Clear(string namePrefix, List<MyCubeGrid> grids = null)
        {
            if (grids == null)
            {
                var entities = GetEntities();
                grids = GetEntitiesOfType<MyCubeGrid>(entities);
            }

            foreach (var grid in grids)
            {
                if (string.IsNullOrEmpty(namePrefix) || grid.DisplayName.StartsWith(namePrefix))
                {
                    grid.SendGridCloseRequest();
                }
            }
        }

        public static void ClearAll()
        {
            Clear(GridNamePrefix);
        }

        public void Load(int sizeX, int sizeY)
        {
            GridSizeX = sizeX;
            GridSizeY = sizeY;

            LoadControllers();
        }

        private static (int sizeX, int sizeY) InferSize()
        {
            var entities = GetEntities();
            var grids = GetEntitiesOfType<MyCubeGrid>(entities);

            var sizeX = -1;
            while (true)
            {
                var hasGrid = grids.Any(x => x.DisplayName.StartsWith(GetGridName(sizeX + 1, 0)));

                if (hasGrid)
                {
                    sizeX++;
                }
                else
                {
                    break;
                }
            }

            var sizeY = -1;
            while (true)
            {
                var hasGrid = grids.Any(x => x.DisplayName.StartsWith(GetGridName(0, sizeY + 1)));

                if (hasGrid)
                {
                    sizeY++;
                }
                else
                {
                    break;
                }
            }

            if (sizeX == -1 || sizeY == -1)
            {
                throw new InvalidOperationException("Invalid grid state");
            }

            return (sizeX, sizeY);
        }

        private static string GetGridName(int x, int y)
        {
            return $"{GridNamePrefix} {x} {y}";
        }

        public override RoboticLegGridState Reset()
        {
            throw new System.NotImplementedException();
        }

        public override RoboticLegGridState ProcessCommand(RoboticLegGridCommand command)
        {
            throw new System.NotImplementedException();
        }

        protected override RoboticLegGridCommand ConvertCommand(FlatCommand command)
        {
            throw new System.NotImplementedException();
        }

        private class GridPosition
        {
            public int X { get; }

            public int Y { get; }

            public GridPosition(int x, int y)
            {
                X = x;
                Y = y;
            }

            protected bool Equals(GridPosition other)
            {
                return X == other.X && Y == other.Y;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((GridPosition)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (X * 397) ^ Y;
                }
            }

            public static bool operator ==(GridPosition left, GridPosition right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(GridPosition left, GridPosition right)
            {
                return !Equals(left, right);
            }
        }
    }
}