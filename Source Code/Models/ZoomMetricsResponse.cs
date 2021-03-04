using System.Collections.Generic;

namespace Models
{
    public class ZoomMetricsResponse
    {
        public string next_page_token { get; set; }

        public int total_records { get; set; }

        public List<ZoomHistory> meetings { get; set; }

        public List<ZoomHistory> webinars { get; set; }

        public List<ZoomHistory> zoom_rooms { get; set; }
    }
}
