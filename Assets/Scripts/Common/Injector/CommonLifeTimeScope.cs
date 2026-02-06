using Common.SoundManagement;
using Common.Store;
using VContainer;
using VContainer.Unity;

namespace Common.Injector
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
