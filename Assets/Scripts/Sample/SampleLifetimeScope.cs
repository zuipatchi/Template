using VContainer;
using VContainer.Unity;

namespace Scripts.Sample
{

    public class SampleLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<Sample>().AsSelf();
        }
    }
}
