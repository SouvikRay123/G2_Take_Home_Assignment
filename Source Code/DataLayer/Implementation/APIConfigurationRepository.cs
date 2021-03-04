using DataLayer.Common;
using Helper;
using Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DataLayer
{
    public class APIConfigurationRepository : IAPIConfigurationRepository
    {
        public List<APIConfiguration> Get(string itemIdentifierName, string identifierValue)
        {
            using(SqlConnection connection = DatabaseHelper.GetG2IntegrationConnectionString())
            {
                try
                {
                    connection.Open();
                    Logger.Debug("Connection opened");

                    SqlCommand command = new SqlCommand($"SELECT id, name, credentials_type, credentials, base_url FROM api_configurations WHERE {itemIdentifierName} = '{identifierValue}'", connection);

                    DataSet apiConfigurationDataSet = new DataSet();

                    using (SqlDataAdapter apiConfigurationDataAdapter = new SqlDataAdapter())
                    {
                        apiConfigurationDataAdapter.SelectCommand = command;
                        apiConfigurationDataAdapter.Fill(apiConfigurationDataSet);
                    };

                    return APIConfiguration.Get(apiConfigurationDataSet);
                }
                catch (System.Exception ex)
                {
                    Logger.Error(ex);
                    throw;
                }
                finally
                {
                    connection.Close();
                    Logger.Debug("Connection closed");
                }
            }
        }
    }
}
