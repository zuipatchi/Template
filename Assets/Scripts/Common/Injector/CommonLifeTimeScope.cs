using Scripts.Common.SoundManagement;
using Scripts.Common.Store;
using VContainer;
using VContainer.Unity;

namespace Scripts.Common.Injector
{
    public class CommonLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<SoundPlayer>().AsSelf();
            builder.RegisterEntryPoint<SoundStore>(Lifetime.Singleton).AsSelf();
        }
    }
}
