using BusinessLayer;
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
            var zoomReportGenerator = ninjectKernel.Get<IZoomUsageReportGenerator>();
            zoomReportGenerator.Generate90DayUsageReport();

            var zoomOptimizationReportGenerator = ninjectKernel.Get<IZoomOptimizationReportGenerator>();
            zoomOptimizationReportGenerator.GenerateOptimizationReport();
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
