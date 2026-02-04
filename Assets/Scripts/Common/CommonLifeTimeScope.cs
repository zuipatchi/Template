using VContainer;
using VContainer.Unity;

public class CommonLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<Store>(Lifetime.Singleton).AsSelf();
    }
}
