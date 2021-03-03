using DataLayer.Common;
using Helper;
using Models;
using System;
using System.Collections.Generic;
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

                    List<APIConfiguration> configurations = new List<APIConfiguration>();

                    SqlCommand command = new SqlCommand($"SELECT id, name, credentials_type, credentials, base_url FROM api_configurations WHERE {itemIdentifierName} = '{identifierValue}'", connection);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            APIConfiguration configuration = new APIConfiguration
                            {
                                id               = Convert.ToString(reader.GetValue(0)),
                                name             = Convert.ToString(reader.GetValue(1)),
                                credentials_type = Convert.ToString(reader.GetValue(2)),
                                credentials      = Convert.ToString(reader.GetValue(3)),
                                base_url         = Convert.ToString(reader.GetValue(4))
                            };

                            configurations.Add(configuration);
                        }
                    }

                    return configurations;
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

        public void Create(APIConfiguration item)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(string itemIdentifier)
        {
            throw new System.NotImplementedException();
        }

        public void Update(APIConfiguration item)
        {
            throw new System.NotImplementedException();
        }
    }
}
