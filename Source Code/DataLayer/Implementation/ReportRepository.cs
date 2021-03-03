using DataLayer.Common;
using Helper;
using Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DataLayer
{
    public class ReportRepository : IReportRepository
    {
        public void Create(Report item)
        {
            using (SqlConnection connection = DatabaseHelper.GetG2IntegrationConnectionString())
            {
                try
                {
                    connection.Open();
                    Logger.Debug("Connection opened");

                    List<APIConfiguration> configurations = new List<APIConfiguration>();

                    SqlCommand command = new SqlCommand($@"INSERT INTO Report(id, type, start_date, end_date, status, result) 
                                                           VALUES NEWID(), '{item.type}', '{item.start_date}', '{item.end_date}', '{item.status}', '{item.result}'", connection);

                    Logger.Debug($"Rows affected : {command.ExecuteNonQuery()}");
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

        public void Delete(string itemIdentifier)
        {
            throw new System.InvalidOperationException("Not allowed to delete report");
        }

        public List<Report> Get(string itemIdentifierName, string identifierValue)
        {
            using (SqlConnection connection = DatabaseHelper.GetG2IntegrationConnectionString())
            {
                try
                {
                    connection.Open();
                    Logger.Debug("Connection opened");

                    List<Report> reports = new List<Report>();

                    SqlCommand command = new SqlCommand($"SELECT id, type, start_date, end_date, status, result FROM report WHERE {itemIdentifierName} = '{identifierValue}'", connection);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Report configuration = new Report
                            {
                                id              = Convert.ToString(reader.GetValue(0)),
                                type            = Convert.ToString(reader.GetValue(1)),
                                start_date      = Convert.ToDateTime(reader.GetValue(2).ToString()),
                                end_date        = Convert.ToDateTime(reader.GetValue(3).ToString()),
                                status          = Convert.ToString(reader.GetValue(4)),
                                result          = Convert.ToString(reader.GetValue(4)),
                            };

                            reports.Add(configuration);
                        }
                    }

                    return reports;
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

        public void Update(Report item)
        {
            using (SqlConnection connection = DatabaseHelper.GetG2IntegrationConnectionString())
            {
                try
                {
                    connection.Open();
                    Logger.Debug("Connection opened");

                    List<APIConfiguration> configurations = new List<APIConfiguration>();

                    SqlCommand command = new SqlCommand($@"UPDATE Report
                                                           SET type = '{item.type}', start_date = '{item.start_date}',
                                                           end_date = {item.end_date}, status = '{item.status}', result = '{item.result}'
                                                           WHERE id = {item.id}", connection);

                    Logger.Debug($"Rows affected : {command.ExecuteNonQuery()}");
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
