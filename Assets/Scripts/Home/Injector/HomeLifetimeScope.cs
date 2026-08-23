using VContainer;
using VContainer.Unity;

namespace Home.Injector
{
    public class HomeLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<CreditModel>(Lifetime.Scoped).AsSelf();
            builder.RegisterComponentInHierarchy<HomePresenter>().AsSelf().AsImplementedInterfaces();
        }
    }
}
