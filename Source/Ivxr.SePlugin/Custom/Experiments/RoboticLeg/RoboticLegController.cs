using System.Collections.Generic;
using System.Linq;
using Iv4xr.SePlugin.Custom.Experiments.Commands;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Entities.Cube;
using VRage.Game.Entity;

namespace Iv4xr.SePlugin.Custom.Experiments.RoboticLeg
{
    public class RoboticLegController : ExperimentControllerBase<RoboticLegCommand, RoboticLegState>
    {
        private MyMotorStator rotor1;
        private MyMotorStator hinge1;
        private MyMotorStator hinge2;
        private MyMotorStator hinge3;
        private readonly string gridName;

        public RoboticLegController(string gridName)
        {
            this.gridName = gridName;
        }

        public void Init()
        {
            var entities = GetEntities();

            var grid = GetEntitiesOfType<MyCubeGrid>(entities)
                .Single(x => x.DisplayName == gridName);

            var gridBlocks = GetAllBlocks(grid);

            rotor1 = FindByName<MyMotorStator>("Leg Rotor 1", gridBlocks);
            hinge1 = FindByName<MyMotorStator>("Leg Hinge 1", gridBlocks);
            hinge2 = FindByName<MyMotorStator>("Leg Hinge 2", gridBlocks);
            hinge3 = FindByName<MyMotorStator>("Leg Hinge 3", gridBlocks);

            entities.Clear();
        }

        private List<MyEntity> GetAllBlocks(MyCubeGrid grid)
        {
            var blocks = new List<MyEntity>();

            foreach (var block in grid.CubeBlocks)
            {
                var fatBlock = block.FatBlock;

                blocks.Add(fatBlock);

                if (fatBlock is MyMechanicalConnectionBlockBase connectionBase)
                {
                    blocks.AddRange(GetAllBlocks(connectionBase.TopGrid));
                }
            }

            return blocks;
        }

        //public IMyTerminalBlock ParentConnector(IMyTerminalBlock childBlock)
        //{
        //    List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
        //    MyEntities.
        //    GridTerminalSystem.GetBlocksOfType<IMyMechanicalConnectionBlock>(blocks, b => ((IMyMechanicalConnectionBlock)b).TopGrid == childBlock.CubeGrid && b is IMyMotorStator);
        //    if (blocks.Count > 0) return blocks[0];
        //    return null;
        //}

        public override RoboticLegState Reset()
        {
            throw new System.NotImplementedException();
        }

        public override RoboticLegState ProcessCommand(RoboticLegCommand command)
        {
            ConfigureRotor(rotor1, command.Rotor1);
            ConfigureRotor(hinge1, command.Hinge1);
            ConfigureRotor(hinge2, command.Hinge2);
            ConfigureRotor(hinge3, command.Hinge3);

            return null;
        }

        protected RoboticLegState GetState()
        {
            return new RoboticLegState()
            {
                // Rotor1 = 
            };
        }

        protected override RoboticLegCommand ConvertCommand(FlatCommand command)
        {
            return new RoboticLegCommand()
            {
                Rotor1 = new RotorCommand()
                {
                    VelocityNormalized = command.Values[0],
                },
                Hinge1 = new RotorCommand()
                {
                    VelocityNormalized = command.Values[1],
                },
                Hinge2 = new RotorCommand()
                {
                    VelocityNormalized = command.Values[2],
                },
                Hinge3 = new RotorCommand()
                {
                    VelocityNormalized = command.Values[3],
                },
            };
        }
    }
}