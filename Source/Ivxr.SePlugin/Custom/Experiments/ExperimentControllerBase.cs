using System.Collections.Generic;
using System.Linq;
using Iv4xr.SePlugin.Custom.Experiments.Commands;
using Iv4xr.SePlugin.Custom.Experiments.States;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRageMath;

namespace Iv4xr.SePlugin.Custom.Experiments
{
    public abstract class ExperimentControllerBase<TCommand, TState> : IExperimentController<TCommand, TState>
        where TState : IExperimentState
    {
        public IExperimentState ProcessCommand(FlatCommand command)
        {
            return ProcessCommand(ConvertCommand(command));
        }

        IExperimentState IExperimentController.Reset()
        {
            return Reset();
        }

        public abstract TState Reset();

        public abstract TState ProcessCommand(TCommand command);

        protected abstract TCommand ConvertCommand(FlatCommand command);

        protected List<MyEntity> GetEntities()
        {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            var player = players[0];
            var playerPosition = player.Character.PositionComp.GetPosition();
            var sphere = new BoundingSphereD(playerPosition, radius: 15000.0);
            var entities = MyEntities.GetEntitiesInSphere(ref sphere);

            return entities;
        }

        protected List<T> GetEntitiesOfType<T>(List<MyEntity> entities)
        {
            return entities
                .Where(x => x is T)
                .Cast<T>()
                .ToList();
        }

        protected T FindByName<T>(string name, List<MyEntity> entities) where T : MyTerminalBlock
        {
            return GetEntitiesOfType<T>(entities)
                .Single(x => x.CustomName.ToString() == name);
        }

        protected void ConfigureRotor(MyMotorStator rotor, RotorCommand command)
        {
            if (command == null)
            {
                return;
            }

            if (command.VelocityNormalized != null)
            {
                var maxVelocity = 30;
                var velocity = command.VelocityNormalized.Value * maxVelocity;
                rotor.TargetVelocityRPM = velocity;
            }
        }

        protected RotorState GetRotorState(MyMotorAdvancedStator rotor)
        {
            var state = new RotorState();

            return state;
        }
    }
}