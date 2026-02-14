using Main.Player.Action;
using Main.Player.Action.Dash;
using Main.Player.Action.Move;
using VContainer;
using VContainer.Unity;

public class MainLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<PlayerInputCallbacks>(Lifetime.Scoped).AsSelf();
        builder.Register<PlayerControls>(Lifetime.Scoped).AsSelf();
        builder.RegisterComponentInHierarchy<PlayerMove>().As<IMove>();
        builder.RegisterComponentInHierarchy<PlayerDash>().As<IDash>();
        builder.RegisterEntryPoint<ActionBinder>().AsSelf();
    }
}
