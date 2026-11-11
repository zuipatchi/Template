using Common.SoundManagement;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace Main.Injector
{
    // Inspector で parentReference に CommonLifetimeScope を設定すること
    public class MainLifetimeScope : LifetimeScope
    {
        [SerializeField] private UIDocument _snackShopDocument;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<SceneBgmPlayer>(Lifetime.Scoped).WithParameter(Bgm.Main);
            builder.RegisterComponent(_snackShopDocument);
            builder.Register<SnackRoundModel>(Lifetime.Scoped);
            builder.RegisterEntryPoint<SnackShopPresenter>(Lifetime.Scoped);
        }
    }
}
