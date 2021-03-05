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
        static readonly StandardKernel ninject_kernel = new StandardKernel();

        ZoomUsageReportGenerationTimer zoom_usage_report_generationTimer;
        ZoomOptimizationReportGenerationTimer zoom_optimization_report_generationTimer;

        public Service1()
        {
            ninject_kernel.Load(new INinjectModule[] { new NinjectBindings() });
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Logger.Debug("Starting service");

            zoom_usage_report_generationTimer = new ZoomUsageReportGenerationTimer(ninject_kernel.Get<IZoomUsageReportGenerator>());
            zoom_usage_report_generationTimer.Setup();

            zoom_optimization_report_generationTimer = new ZoomOptimizationReportGenerationTimer(ninject_kernel.Get<IZoomOptimizationReportGenerator>());
            //zoom_optimization_report_generationTimer.Setup();

            Logger.Debug("Service started");
        }

        public void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStop()
        {
            Logger.Debug("Stopping service");

            zoom_usage_report_generationTimer.Stop();
            zoom_optimization_report_generationTimer.Stop();

            Logger.Debug("Stopped service");
        }
    }
}
