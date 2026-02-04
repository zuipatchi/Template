using VContainer;
using VContainer.Unity;

public class RootLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // builder.Register<LoadAsset>(Lifetime.Singleton);
        builder.RegisterEntryPoint<LoadAsset>(Lifetime.Singleton).AsSelf();
        builder.RegisterComponentInHierarchy<Sample>();
    }
}
