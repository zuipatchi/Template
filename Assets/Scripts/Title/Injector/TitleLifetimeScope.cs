using Common.SoundManagement;
using Title.GameStartButton;
using VContainer;
using VContainer.Unity;

namespace Title.Injector
{
    public class TitleLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<GameStartButtonPresenter>().AsSelf();
            builder.RegisterEntryPoint<SceneBgmPlayer>(Lifetime.Scoped).WithParameter(Bgm.Title);
        }
    }
}
