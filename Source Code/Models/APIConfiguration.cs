using System;
using System.Collections.Generic;
using System.Data;

namespace Models
{
    public class APIConfiguration
    {
        public string id { get; set; }

        public string name { get; set; }

        public string credentials_type { get; set; }

        public string credentials { get; set; }

        public string base_url { get; set; }

        public static List<APIConfiguration> Get(DataSet apiConfigurationDataSet)
        {
            List<APIConfiguration> apiConfigurations = new List<APIConfiguration> { };

            if(apiConfigurationDataSet.Tables.Count > 0)
            {
                if (apiConfigurationDataSet.Tables[0].Rows != null)
                {
                    foreach (DataRow row in apiConfigurationDataSet.Tables[0].Rows)
                    {
                        apiConfigurations.Add(new APIConfiguration
                        {
                            id               = Convert.ToString(row["id"]),
                            name             = Convert.ToString(row["name"]),
                            credentials_type = Convert.ToString(row["credentials_type"]),
                            credentials      = Convert.ToString(row["credentials"]),
                            base_url         = Convert.ToString(row["base_url"])
                        });
                    }
                }
            }

            return apiConfigurations;
        }
    }
}
