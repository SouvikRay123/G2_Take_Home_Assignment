using System;

namespace Models
{
    public class Report
    {
        public string id { get; set; }

        public string type { get; set; }

        public DateTime start_date { get; set; }

        public DateTime end_date { get; set; }

        public string status { get; set; }

        public string result { get; set; }
    }
}
