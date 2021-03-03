using DataLayer.Common;
using Helper;
using Models;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DataLayer
{
    public class APIConfigurationRepository : IAPIConfigurationRepository
    {
        public void Create(APIConfiguration item)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(string itemIdentifier)
        {
            
        }

        public List<APIConfiguration> Get(string itemIdentifierName, string identifierValue)
        {
            using(SqlConnection connection = DatabaseHelper.GetG2IntegrationConnectionString())
            {
                try
                {
                    connection.Open();
                    Logger.Debug("Connection opened");

                    List<APIConfiguration> configurations = new List<APIConfiguration>();

                    SqlCommand command = new SqlCommand("SELECT * FROM api_configurations where name = '@var_name'", connection);
                    command.Parameters.AddWithValue("var_name", "Zoom");

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            APIConfiguration configuration = new APIConfiguration
                            {

                            };

                            configurations.Add(configuration);
                        }
                    };

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
                }
            }
        }

        public void Update(APIConfiguration item)
        {
            throw new System.NotImplementedException();
        }
    }
}
