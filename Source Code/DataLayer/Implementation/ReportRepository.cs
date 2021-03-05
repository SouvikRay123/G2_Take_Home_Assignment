using Constants;
using DataLayer.Common;
using Helper;
using Models;
using System;
using System.Collections.Generic;
using System.Data;
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

                    SqlCommand command = new SqlCommand($@"INSERT INTO reports(id, type, start_date, end_date, status, result) 
                                                           VALUES (NEWID(), '{item.type}', '{item.start_date}', '{item.end_date}', '{item.status}', '{item.result}')", connection);

                    Logger.Debug($"Rows affected : {command.ExecuteNonQuery()}");
                }
                catch (System.Exception ex)
                {
                    Logger.Error(ex);
                }
                finally
                {
                    connection.Close();
                    Logger.Debug("Connection closed");
                }
            }
        }

        public string GetReportId(string type, DateTime startDate, DateTime endDate)
        {
            using (SqlConnection connection = DatabaseHelper.GetG2IntegrationConnectionString())
            {
                try
                {
                    connection.Open();
                    Logger.Debug("Connection opened");

                    SqlCommand command = new SqlCommand($"SELECT id FROM reports WHERE type = '{type}' AND start_date = '{startDate.ToString(ReportDataTypeConstants.DATE_FORMAT)}' AND end_date = '{endDate.ToString(ReportDataTypeConstants.DATE_FORMAT)}' AND status = '{ReportStatusConstants.IN_PROGRESS}'", connection);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                            return Convert.ToString(reader.GetValue(0));
                        else
                            return "";
                    }
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

        public ReportResult GetReportResult(string type, DateTime startDate, DateTime endDate)
        {
            using (SqlConnection connection = DatabaseHelper.GetG2IntegrationConnectionString())
            {
                try
                {
                    connection.Open();
                    Logger.Debug("Connection opened");

                    SqlCommand command          = new SqlCommand($"SELECT status, result FROM reports WHERE type = '{type}' AND start_date = '{startDate.ToString(ReportDataTypeConstants.DATE_FORMAT)}' AND end_date = '{endDate.ToString(ReportDataTypeConstants.DATE_FORMAT)}'", connection);

                    DataSet ReportResultDataSet = new DataSet();

                    using (SqlDataAdapter apiConfigurationDataAdapter = new SqlDataAdapter())
                    {
                        apiConfigurationDataAdapter.SelectCommand = command;
                        apiConfigurationDataAdapter.Fill(ReportResultDataSet);
                    }

                    return ReportResult.Get(ReportResultDataSet);
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

                    SqlCommand command = new SqlCommand($@"UPDATE reports
                                                           SET status = '{item.status}', result = '{item.result}'
                                                           WHERE id = '{item.id}'", connection);

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
