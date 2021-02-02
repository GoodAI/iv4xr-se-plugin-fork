using System.Collections;
using System.Collections.Generic;
using Iv4xr.SePlugin.Custom.ChatCommands;
using VRage.Game.Components;
using VRageMath;

namespace Iv4xr.SePlugin.Custom.Experiments.RoboticLegGrid
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class RoboticLegGridInGameController : InGameControllerBase
    {
        private RoboticLegGridController controller;
        private bool doRandomMoves = false;

        protected override void Init()
        {
            ConfigureChatCommands("/grid ", "Grid");
            RegisterCommand(new MessagePattern(MessagePatternType.Prefix, "spawn"), SpawnGrid);
            RegisterCommand(new MessagePattern(MessagePatternType.Prefix, "load"), Load);
            RegisterCommand(new MessagePattern(MessagePatternType.Exact, "clear all"), ClearAll);
            RegisterCommand(new MessagePattern(MessagePatternType.Prefix, "clear"), Clear);
            RegisterCommand(new MessagePattern(MessagePatternType.Prefix, "restart"), Restart);
            RegisterCommand(new MessagePattern(MessagePatternType.Exact, "teleport"), Teleport);
            RegisterCommand(new MessagePattern(MessagePatternType.Exact, "random run"), RandomRun);
            RegisterCommand(new MessagePattern(MessagePatternType.Exact, "random stop"), RandomStop);
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
        }

        private IEnumerator RandomRun(string command, string[] arguments)
        {
            if (controller == null)
            {
                ShowMessage("Please load the grid controller.");
                yield break;
            }

            var stepCounter = 0;

            doRandomMoves = true;
            while (doRandomMoves)
            {
                stepCounter++;
                if (stepCounter == 10)
                {
                    controller.DoRandomMoves();
                    stepCounter = 0;
                }
                
                yield return null;
            }
        }

        private void RandomStop(string command, string[] arguments)
        {
            StopRunningTasks();
        }

        private void StopRunningTasks()
        {
            doRandomMoves = false;
        }

        private void Load(string command, string[] arguments)
        {
            StopRunningTasks();

            var (sizeX, sizeY) = ParseSize(arguments);

            if (sizeX == -1)
            {
                return;
            }

            controller = new RoboticLegGridController();
            controller.Load(sizeX, sizeY);
        }

        private void Teleport(string command, string[] arguments)
        {
            var position = new Vector3D(new Vector3(-280, 390, 270));
            var orientation = new MatrixD(-0.75, -0.6, -0.65, -0.5, 75, 0.5, 0.45, 0.68, -0.6);

            Utils.TeleportTo(position);
        }

        private void Restart(string command, string[] arguments)
        {
            StopRunningTasks();

            var (x, y) = ParseSize(arguments);

            if (x == -1)
            {
                return;
            }

            controller.RestartExperiment(x, y);
        }

        private void ClearAll(string command, string[] arguments)
        {
            StopRunningTasks();

            RoboticLegGridController.ClearAll();
        }

        private void Clear(string command, string[] arguments)
        {
            StopRunningTasks();

            if (controller == null)
            {
                ShowMessage("Please load the grid controller.");
            }
            else
            {
                var (sizeX, sizeY) = ParseSize(arguments);
                controller.ClearExperiment(sizeX, sizeY);
            }
        }

        private IEnumerator SpawnGrid(string command, string[] arguments)
        {
            StopRunningTasks();

            var (sizeX, sizeY) = ParseSize(arguments);

            if (sizeX == -1)
            {
                yield break;
            }

            RoboticLegGridController.ClearAll();
            controller = new RoboticLegGridController();
            yield return controller.SpawnGrid(sizeX, sizeY);
        }

        private (int sizeX, int sizeY) ParseSize(string[] arguments)
        {
            if (arguments.Length == 1)
            {
                var gridSizeX = int.Parse(arguments[0]);
                return (gridSizeX, gridSizeX);
            }
            else if (arguments.Length == 2)
            {
                var gridSizeX = int.Parse(arguments[0]);
                var gridSizeY = int.Parse(arguments[1]);
                return (gridSizeX, gridSizeY);
            }
            else
            {
                ShowMessage("Invalid grid size provided.");
                return (-1, -1);
            }
        }
    }
}