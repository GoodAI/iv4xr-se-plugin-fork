﻿using Iv4xr.SePlugin.Custom.Experiments.States;

namespace Iv4xr.SePlugin.Custom.Experiments.ThrowerArm
{
    public class ThrowerArmState : IExperimentState
    {
        public bool IsResetInProgress { get; set; }

        public RotorState Rotor1 { get; set; }

        public RotorState Hinge1 { get; set; }

        public RotorState Hinge2 { get; set; }

        public RotorState Hinge3 { get; set; }
    }
}