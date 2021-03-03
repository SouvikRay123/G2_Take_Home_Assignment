namespace Models
{
    public class ZoomHistory
    {
        public string id { get; set; }

        public string type { get; set; }

        public string start_date { get; set; }

        public string end_date { get; set; }

        public string host_name { get; set; }

        public string email { get; set; }

        public string participants { get; set; }

        public string duration { get; set; }

        public string has_pstn { get; set; }

        public string has_voip { get; set; }

        public string has_3rd_party_audio { get; set; }

        public string has_video { get; set; }

        public string has_screen_share { get; set; }

        public string has_recording { get; set; }

        public string has_sip { get; set; }
    }
}
