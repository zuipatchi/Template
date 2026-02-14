using Common.Option;
using Common.SceneManagement;
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
            builder.RegisterEntryPoint<ModalStore>(Lifetime.Singleton).AsSelf();
            builder.RegisterComponentInHierarchy<OptionPresenter>().AsSelf();
            builder.RegisterEntryPoint<OptionModel>(Lifetime.Singleton).AsSelf();
            builder.RegisterComponentInHierarchy<SoundPlayer>().AsSelf();
            builder.RegisterEntryPoint<SoundStore>(Lifetime.Singleton).AsSelf();
            builder.RegisterComponentInHierarchy<SceneTransitioner>().AsSelf();
        }
    }
}
