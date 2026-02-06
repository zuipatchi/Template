using VContainer;
using VContainer.Unity;

namespace Scripts.SampleScene
{

    public class SampleLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<Sample>().AsSelf();
        }
    }
}
