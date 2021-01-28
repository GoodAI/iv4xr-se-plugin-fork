namespace Iv4xr.SePlugin.Custom.Experiments
{
    public interface IExperimentController
    {
        IExperimentState ProcessCommand(FlatCommand command);

        IExperimentState Reset();
    }

    public interface IExperimentController<in TCommand, out TState> : IExperimentController where TState : IExperimentState
    {
        TState ProcessCommand(TCommand command);

        new TState Reset();
    }
}