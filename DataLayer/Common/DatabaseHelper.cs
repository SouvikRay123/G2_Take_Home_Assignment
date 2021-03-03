using System;
using System.Configuration;
using System.Data.SqlClient;

namespace DataLayer.Common
{
    public static class DatabaseHelper
    {
        public static SqlConnection GetG2IntegrationConnectionString()
        {
            if (ConfigurationManager.ConnectionStrings["g2_integration"] == null || string.IsNullOrWhiteSpace(ConfigurationManager.ConnectionStrings["g2_integration"].ConnectionString))
                throw new Exception("Invalid connection string");
                        
            return new SqlConnection(ConfigurationManager.ConnectionStrings["g2_integration"].ConnectionString);
        }
    }
}
