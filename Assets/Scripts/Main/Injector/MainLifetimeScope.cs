using Common.SoundManagement;
using VContainer;
using VContainer.Unity;

namespace Main.Injector
{
    // Inspector で parentReference に CommonLifetimeScope を設定すること
    public class MainLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<SceneBgmPlayer>(Lifetime.Scoped).WithParameter(Bgm.Main);
            // Main シーン固有の登録をここに追加する
        }
    }
}
