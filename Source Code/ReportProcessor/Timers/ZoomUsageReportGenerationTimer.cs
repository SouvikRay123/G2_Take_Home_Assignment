using System.Timers;
using BusinessLayer;
using Helper;

namespace ReportProcessor
{
    class ZoomUsageReportGenerationTimer
    {
        IZoomUsageReportGenerator zoom_usage_report_generator;
        const int timer_interval = 5 * 60 * 1000;
        bool can_stop_service = true;

        readonly Timer usage_report_generation_timer = new Timer();

        public ZoomUsageReportGenerationTimer(IZoomUsageReportGenerator zoomUsageReportGenerator)
        {
            zoom_usage_report_generator = zoomUsageReportGenerator;
        }

        public void Setup()
        {
            usage_report_generation_timer.Elapsed   += new ElapsedEventHandler(OnTimedEvent);
            usage_report_generation_timer.Interval  = timer_interval;
            usage_report_generation_timer.Enabled   = true;
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            can_stop_service = false;
            usage_report_generation_timer.Stop();

            zoom_usage_report_generator.Generate90DayUsageReport();

            usage_report_generation_timer.Start();
            can_stop_service = true;
        }

        public void Stop()
        {
            // if timer is not enabled then its stopped
            while (!can_stop_service)
            {
                int sleepDuration = 60 * 1000;
                Logger.Debug($"ZoomUsageReportGenerationTimer: Timer still running, sleeping for {sleepDuration} milliseconds");
                System.Threading.Thread.Sleep(sleepDuration); // sleep for 1 min and then check again
            }

            usage_report_generation_timer.Stop();
        }
    }
}
