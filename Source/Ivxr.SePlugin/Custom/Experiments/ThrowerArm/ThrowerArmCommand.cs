using Iv4xr.SePlugin.Custom.Experiments.Common.Commands;

namespace Iv4xr.SePlugin.Custom.Experiments.ThrowerArm
{
    public class ThrowerArmCommand
    {
        public RotorCommand Rotor1 { get; set; }

        public RotorCommand Hinge1{ get; set; }

        public RotorCommand Hinge2 { get; set; }

        public RotorCommand Hinge3 { get; set; }
    }
}