using System.Timers;
using BusinessLayer;
using Helper;

namespace ReportProcessor
{
    class ZoomOptimizationReportGenerationTimer
    {
        IZoomOptimizationReportGenerator zoom_optimization_report_generator;
        const int timer_interval = 5 * 60 * 1000;
        bool can_stop_service    = true;

        readonly Timer optimization_report_generation_timer = new Timer();

        public ZoomOptimizationReportGenerationTimer(IZoomOptimizationReportGenerator zoomOptimizationReportGenerator)
        {
            zoom_optimization_report_generator = zoomOptimizationReportGenerator;
        }

        public void Setup()
        {
            optimization_report_generation_timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            optimization_report_generation_timer.Interval = timer_interval;
            optimization_report_generation_timer.Enabled = true;
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            can_stop_service = false;
            optimization_report_generation_timer.Stop();

            zoom_optimization_report_generator.GenerateOptimizationReport();


            optimization_report_generation_timer.Start();
            can_stop_service = true;
        }

        public void Stop()
        {
            // if timer is not enabled then its stopped
            while (!can_stop_service)
            {
                int sleepDuration = 60 * 1000;
                Logger.Debug($"ZoomOptimizationReportGenerationTimer: Timer still running, sleeping for {sleepDuration} milliseconds");
                System.Threading.Thread.Sleep(sleepDuration); // sleep for 1 min and then check again
            }

            optimization_report_generation_timer.Stop();
        }
    }
}
