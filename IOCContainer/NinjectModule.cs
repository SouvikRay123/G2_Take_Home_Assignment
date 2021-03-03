using BusinessLayer;
using DataLayer;

namespace IOCContainer
{
    public class NinjectBindings : Ninject.Modules.NinjectModule
    {
        public override void Load()
        {
            Bind<IAPIConfigurationManager>().To<APIConfigurationManager>();
            Bind<IAPIConfigurationRepository>().To<APIConfigurationRepository>();
        }
    }
}
