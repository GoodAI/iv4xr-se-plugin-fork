﻿using Iv4xr.SePlugin.Custom.Experiments.Commands;

namespace Iv4xr.SePlugin.Custom.Experiments.RoboticLeg
{
    public class RoboticLegCommand
    {
        public RotorCommand Rotor1 { get; set; }

        public RotorCommand Hinge1{ get; set; }

        public RotorCommand Hinge2 { get; set; }

        public RotorCommand Hinge3 { get; set; }
    }
}