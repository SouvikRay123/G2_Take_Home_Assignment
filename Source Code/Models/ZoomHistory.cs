using System;
using System.Collections.Generic;
using System.Data;

namespace Models
{
    public class ZoomHistory
    {
        public string id { get; set; }

        public string type { get; set; }

        public string user_type { get; set;  }

        public DateTime start_time { get; set; }

        public DateTime end_time { get; set; }

        public string host { get; set; }

        public string email { get; set; }

        public int participants { get; set; }

        public string duration { get; set; }

        public string has_pstn { get; set; }

        public string has_voip { get; set; }

        public string has_3rd_party_audio { get; set; }

        public string has_video { get; set; }

        public string has_screen_share { get; set; }

        public string has_recording { get; set; }

        public string has_sip { get; set; }

        public static List<ZoomHistory> Get(DataSet zoomHistoryDataSet)
        {
            List<ZoomHistory> zoomHistory = new List<ZoomHistory> { };

            if (zoomHistoryDataSet.Tables.Count > 0)
            {
                if (zoomHistoryDataSet.Tables[0].Rows != null)
                {
                    foreach (DataRow row in zoomHistoryDataSet.Tables[0].Rows)
                    {
                        zoomHistory.Add(new ZoomHistory
                        {
                            id                  = Convert.ToString(row["id"]),
                            type                = Convert.ToString(row["type"]),
                            start_time          = Convert.ToDateTime(row["start_time"]),
                            end_time            = Convert.ToDateTime(row["end_time"]),
                            host                = Convert.ToString(row["host"]),
                            email               = Convert.ToString(row["email"]),
                            user_type           = Convert.ToString(row["user_type"]),
                            participants        = Convert.ToUInt16(row["participants"]),
                            duration            = Convert.ToString(row["duration"]),
                            has_pstn            = Convert.ToString(row["has_pstn"]),
                            has_voip            = Convert.ToString(row["has_voip"]),
                            has_3rd_party_audio = Convert.ToString(row["has_3rd_party_audio"]),
                            has_video           = Convert.ToString(row["has_video"]),
                            has_screen_share    = Convert.ToString(row["has_screen_share"]),
                            has_recording       = Convert.ToString(row["has_recording"]),
                            has_sip             = Convert.ToString(row["has_sip"])
                        });
                    }
                }
            }

            return zoomHistory;
        }
    }
}
