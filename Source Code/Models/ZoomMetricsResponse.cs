using System.Collections.Generic;

namespace Models
{
    public class ZoomResponseBase
    {
        public string next_page_token { get; set; }

        public int total_records { get; set; }
    }

    public class ZoomMetricsResponse : ZoomResponseBase
    {
        public List<ZoomHistory> meetings { get; set; }

        public List<ZoomHistory> webinars { get; set; }

        public List<ZoomHistory> zoom_rooms { get; set; }
    }
    
    public class ZoomRoomIdListResponse : ZoomResponseBase
    {
        public List<ZoomRoomMetaData> zoom_rooms { get; set; }
    }

    public class ZoomRoomMetaData
    {
        public string id { get; set; }
    }

    public class ZoomRoomMeetingDetails
    {
        public ZoomMetricsResponse past_meetings { get; set; }
    }
}
