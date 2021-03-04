using System;
using System.Data.SqlClient;

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

        public static Report Get(SqlDataReader reader)
        {
            return new Report
            {
                id              = Convert.ToString(reader.GetValue(0)),
                type            = Convert.ToString(reader.GetValue(1)),
                start_date      = Convert.ToDateTime(reader.GetValue(2).ToString()),
                end_date        = Convert.ToDateTime(reader.GetValue(3).ToString()),
                status          = Convert.ToString(reader.GetValue(4)),
                result          = Convert.ToString(reader.GetValue(4)),
            };
        }
    }
}
