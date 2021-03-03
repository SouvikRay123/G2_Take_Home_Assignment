using BusinessLayer;
using Constants;
using Helper;
using IOCContainer;
using Ninject;
using Ninject.Modules;
using System.ServiceProcess;

namespace ReportProcessor
{
    public partial class Service1 : ServiceBase
    {
        StandardKernel ninjectKernel = new StandardKernel();        

        public Service1()
        {
            ninjectKernel.Load(new INinjectModule[] { new NinjectBindings() });
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            IAPIConfigurationManager configurationManager = ninjectKernel.Get<IAPIConfigurationManager>();
            var Config = configurationManager.Get(ProductConstants.Zoom);

        }

        public void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStop()
        {
            Logger.Debug("Stopped service");
        }
    }
}
