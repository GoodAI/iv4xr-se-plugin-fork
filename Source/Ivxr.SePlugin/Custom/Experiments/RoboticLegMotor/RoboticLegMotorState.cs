using Iv4xr.SePlugin.Custom.Experiments.Common.States;

namespace Iv4xr.SePlugin.Custom.Experiments.RoboticLegMotor
{
    public class RoboticLegMotorState : IExperimentState
    {
        public bool IsResetInProgress { get; set; }

        public RotorState Rotor1 { get; set; }

        public RotorState Hinge1 { get; set; }

        public RotorState Hinge2 { get; set; }

        public RotorState Hinge3 { get; set; }
    }
}