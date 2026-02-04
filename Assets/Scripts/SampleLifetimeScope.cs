using VContainer;
using VContainer.Unity;

public class SampleLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<Sample>().AsSelf();
    }
}
