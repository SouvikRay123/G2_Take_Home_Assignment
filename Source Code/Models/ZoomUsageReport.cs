using System;

namespace Models
{
    public class ZoomUsageReport
    {
        public int total_records { get; set; }

        public TimeSpan total_duration { get; set; }

        public int total_participants { get; set; }

        public int total_meetings_with_active_participants { get; set; }

        public int total_meetings_with_pstn { get; set; }

        public int total_meetings_with_voip { get; set; }

        public int total_meetings_with_3rd_party_audio { get; set; }

        public int total_meetings_with_video { get; set; }

        public int total_meetings_with_screen_share { get; set; }

        public int total_meetings_with_sip { get; set; }

        public int total_meetings_with_recordings { get; set; }
    }
}
