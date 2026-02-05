using VContainer;
using VContainer.Unity;

namespace Scripts.Common
{
    public class CommonLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<SoundPlayer>().AsSelf();
            builder.RegisterEntryPoint<Store>(Lifetime.Singleton).AsSelf();
        }
    }
}
