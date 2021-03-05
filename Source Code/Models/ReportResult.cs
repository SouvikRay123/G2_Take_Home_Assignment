using System;
using System.Data;

namespace Models
{
    public class ReportResult
    {
        public string status { get; set; }

        public string result { get; set; }

        public static ReportResult Get(DataSet reportResultDataSet)
        {
            if (reportResultDataSet.Tables.Count > 0)
            {
                if (reportResultDataSet.Tables[0].Rows != null)
                {
                    DataRow row = reportResultDataSet.Tables[0].Rows[0];
                    
                    return new ReportResult
                    {
                        status = Convert.ToString(row["status"]),
                        result = Convert.ToString(row["result"])                        
                    };
                }
            }

            return null;
        }
    }
}
