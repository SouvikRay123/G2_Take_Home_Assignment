using System.Collections.Generic;

namespace Models
{
    public class ZoomPlansUsage
    {
        public ZoomPlanUsageDetail plan_base { get; set; }

        public ZoomPlanUsageDetail plan_zoom_rooms { get; set; }

        public List<ZoomPlanUsageDetail> plan_webinar { get; set; }
    }

    public class ZoomPlanUsageDetail
    {
        public string type { get; set; }

        public int hosts { get; set; }

        public int usage { get; set; }
    }
}
