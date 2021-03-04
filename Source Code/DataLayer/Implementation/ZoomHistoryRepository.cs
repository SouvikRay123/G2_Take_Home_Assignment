using Constants;
using DataLayer.Common;
using Helper;
using Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DataLayer
{
    public class ZoomHistoryRepository : IZoomHistoryRepository
    {
        const int insert_batch_size = 1000;

        public void Create(List<ZoomHistory> items)
        {
            using (SqlConnection connection = DatabaseHelper.GetG2IntegrationConnectionString())
            {
                try
                {
                    connection.Open();
                    Logger.Debug("Connection opened");

                    List<APIConfiguration> configurations = new List<APIConfiguration>();

                    int batchesToBeMade  = (items.Count / insert_batch_size) + (items.Count % insert_batch_size > 0 ? 1 : 0);
                    int batchesProcessed = 0;

                    while(batchesToBeMade > batchesProcessed)
                    {
                        var itemsToSkip   = batchesProcessed * insert_batch_size;

                        var itemsToInsert = items.Skip(itemsToSkip).Take(insert_batch_size);

                        var commandText   = GetInsertCommandText(itemsToInsert.ToList());

                        var command       = new SqlCommand(commandText, connection);

                        Logger.Debug($"Processed batch between {itemsToSkip} and {itemsToSkip + insert_batch_size}, rows affected : {command.ExecuteNonQuery()}");

                        batchesProcessed++;
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

        private string GetInsertCommandText(List<ZoomHistory> items)
        {
            var valuesToInsert = new List<string> { };

            foreach (var item in items)
            {
                var valueToInsert = $"(NEWID(), '{item.type}', '{item.start_time}', '{item.end_time}', '{item.host.Replace("'", "''")}', '{item.email}', '{item.user_type}', '{item.participants}', '{item.duration}', '{item.has_pstn}', '{item.has_voip}', '{item.has_3rd_party_audio}', '{item.has_video}', '{item.has_screen_share}', '{item.has_recording}', '{item.has_sip}')";
                valueToInsert.Replace("'", "''");

                valuesToInsert.Add(valueToInsert);
            }

            return $@"INSERT INTO zoom_history(id, type ,start_time ,end_time ,host ,email ,user_type ,participants ,duration ,has_pstn ,has_voip ,has_3rd_party_audio ,has_video ,has_screen_share ,has_recording ,has_sip) 
                                VALUES {string.Join(",", valuesToInsert)}";             
        }

        public List<ZoomHistory> Get(DateTime startDate, DateTime endDate)
        {
            using (SqlConnection connection = DatabaseHelper.GetG2IntegrationConnectionString())
            {
                try
                {
                    connection.Open();
                    Logger.Debug("Connection opened");

                    SqlCommand command = new SqlCommand($"SELECT * FROM zoom_history WHERE end_time <= '{endDate.ToString(ZoomHistoricalDataConstants.DATA_STORE_DATE_TIME_FORMAT)}' AND end_time >= '{startDate.ToString(ZoomHistoricalDataConstants.DATA_STORE_DATE_TIME_FORMAT)}'", connection);

                    DataSet zoomHistoryDataSet = new DataSet();

                    using (SqlDataAdapter zoomHistoryDataAdapter = new SqlDataAdapter())
                    {
                        zoomHistoryDataAdapter.SelectCommand = command;
                        zoomHistoryDataAdapter.Fill(zoomHistoryDataSet);
                    };

                    return ZoomHistory.Get(zoomHistoryDataSet);
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
