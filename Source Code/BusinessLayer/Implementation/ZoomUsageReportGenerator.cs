﻿using Constants;
using DataLayer;
using Helper;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace BusinessLayer
{
    public class ZoomUsageReportGenerator : IZoomUsageReportGenerator
    {
        IReportRepository report_repository;
        IZoomHistoryRepository zoom_history_repository;
        IAPIConfigurationManager api_configuration_manager;
        const int page_size                 = 300;
        const int rate_limit                = 10;
        const int max_days_of_data_to_fetch = 30;
        const int retries                   = 10;
        const int thread_sleep_in_seconds   = 5;

        public ZoomUsageReportGenerator(IReportRepository reportRepository, IZoomHistoryRepository zoomHistoryRepository, IAPIConfigurationManager apiConfigurationManager)
        {
            report_repository         = reportRepository;
            zoom_history_repository   = zoomHistoryRepository;
            api_configuration_manager = apiConfigurationManager;
        }

        public void Generate90DayUsageReport()
        {
            try
            {
                Logger.Debug("Starting generation of 90 day report");

                var now = DateTime.Now;

                // generate from 12AM midnight of T-90th day to 11:59:59PM of T-1th day
                GenerateUsageReport(new DateTime(now.Year, now.Month, now.Day, 0, 0, 0).AddDays(-90), new DateTime(now.Year, now.Month, now.Day, 23, 59, 59).AddDays(-1));

                Logger.Debug("Completed generation of 90 day report");
            }
            catch (Exception ex)
            {
                Logger.Fatal($"Unable to generate 90 day report: {JsonConvert.SerializeObject(ex)}");
            }
        }

        private void GenerateUsageReport(DateTime startDate, DateTime endDate)
        {
            string reportId = "";

            try
            {
                Logger.Debug($"Checking for report to be generated between {startDate.ToShortDateString()} and {endDate.ToShortDateString()}");

                reportId = GetZoomUsageReportId(startDate, endDate);

                if (string.IsNullOrWhiteSpace(reportId))
                {
                    Logger.Debug("No report id to be processed");
                    return;
                }

                ProcessZoomData(reportId, startDate, endDate);
            }
            catch (Exception ex)
            {
                UpdateReportStatus(reportId, ReportStatusConstants.ERROR, JsonConvert.SerializeObject(ex.Message));
                throw;
            }
        }

        private string GetZoomUsageReportId(DateTime startDate, DateTime endDate, int retriesLeft = retries)
        {
            try
            {
                return report_repository.GetReportId(ReportTypeConstants.ZOOM_USAGE, startDate, endDate);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while fetching zoom usage report id, retriesLeft: {retriesLeft}, error : {JsonConvert.SerializeObject(ex)}");

                if (retriesLeft > 0)
                {
                    Thread.Sleep(thread_sleep_in_seconds * 1000);
                    return GetZoomUsageReportId(startDate, endDate, --retriesLeft);
                }
                else
                    throw;
            }
        }

        private void ProcessZoomData(string reportId, DateTime startDate, DateTime endDate)
        {
            Logger.Debug("Fetching zoom data");

            var zoomData = GetZoomData(startDate, endDate);

            Logger.Debug("Preparing zoom usage report");

            var reportDetails = PrepareReport(zoomData);

            UpdateReportStatus(reportId, ReportStatusConstants.SUCCESS, JsonConvert.SerializeObject(reportDetails));

            Logger.Debug("Setting up next cycle");

            SetupNextReportCycle(startDate, endDate);
        }

        private void UpdateReportStatus(string reportId, string reportStatus, string reportResult, int retriesLeft = retries)
        {
            try
            {
                report_repository.Update(new Report
                {
                    id     = reportId,
                    status = reportStatus,
                    result = reportResult
                });

                Logger.Debug("Updated report status");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while updating zoom usage report status, reportId: {reportId}, retriesLeft: {retriesLeft}, error : {JsonConvert.SerializeObject(ex)}");

                if (retriesLeft > 0)
                {
                    Thread.Sleep(thread_sleep_in_seconds * 1000);
                    UpdateReportStatus(reportId, ReportStatusConstants.SUCCESS, reportStatus, --retriesLeft);
                }
                else
                    throw;
            }
        }

        private void SetupNextReportCycle(DateTime startDate, DateTime endDate, int retriesLeft = retries)
        {
            try
            {
                report_repository.Create(new Report
                {
                    start_date = startDate.AddDays(1),
                    end_date = endDate.AddDays(1),
                    type = ReportTypeConstants.ZOOM_USAGE,
                    status = ReportStatusConstants.IN_PROGRESS
                }); ;

                Logger.Debug("Scheduled next report generation");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while setting next report cycle, startDate: {startDate}, endDate: {endDate}, error : {JsonConvert.SerializeObject(ex)}");

                if (retriesLeft > 0)
                {
                    Thread.Sleep(thread_sleep_in_seconds * 1000);
                    SetupNextReportCycle(startDate, endDate, --retriesLeft);
                }
                else
                    throw;
            }
        }

        private Dictionary<zoom_metrics_type, ZoomUsageReport> PrepareReport(List<ZoomHistory> zoomData)
        {
            var report = new Dictionary<zoom_metrics_type, ZoomUsageReport> { };

            foreach (zoom_metrics_type metricType in Enum.GetValues(typeof(zoom_metrics_type)))
                report.Add(metricType, new ZoomUsageReport 
                { 
                    total_duration = new TimeSpan()
                });

            foreach (var singleZoomData in zoomData)
            {
                var metricType = (zoom_metrics_type)Enum.Parse(typeof(zoom_metrics_type), singleZoomData.type, true);

                UpdateUsageOfMetricType(report, metricType, singleZoomData);
            }

            return report;
        }

        private void UpdateUsageOfMetricType(Dictionary<zoom_metrics_type, ZoomUsageReport> report, zoom_metrics_type metricType, ZoomHistory singleZoomData)
        {
            try
            {
                report[metricType].total_records++;

                report[metricType].total_duration                       = report[metricType].total_duration.Add(TimeSpan.Parse(singleZoomData.duration));
            
                report[metricType].total_participants                   += singleZoomData.participants;
            
                report[metricType].total_meetings_with_active_participants += string.Compare(singleZoomData.user_type ,ZoomHistoricalDataConstants.LICENSED_USER_TYPE, StringComparison.OrdinalIgnoreCase) > -1 ? 1 : 0;

                report[metricType].total_meetings_with_pstn             += string.Equals(singleZoomData.has_pstn , GlobalConstants.TRUE, StringComparison.OrdinalIgnoreCase) ? 1 : 0;

                report[metricType].total_meetings_with_voip             += string.Equals(singleZoomData.has_voip , GlobalConstants.TRUE, StringComparison.OrdinalIgnoreCase) ? 1 : 0;

                report[metricType].total_meetings_with_3rd_party_audio  += string.Equals(singleZoomData.has_3rd_party_audio , GlobalConstants.TRUE, StringComparison.OrdinalIgnoreCase) ? 1 : 0;

                report[metricType].total_meetings_with_video            += string.Equals(singleZoomData.has_video , GlobalConstants.TRUE, StringComparison.OrdinalIgnoreCase) ? 1 : 0;

                report[metricType].total_meetings_with_screen_share     += string.Equals(singleZoomData.has_screen_share , GlobalConstants.TRUE, StringComparison.OrdinalIgnoreCase) ? 1 : 0;

                report[metricType].total_meetings_with_sip              += string.Equals(singleZoomData.has_sip , GlobalConstants.TRUE, StringComparison.OrdinalIgnoreCase) ? 1 : 0;

                report[metricType].total_meetings_with_recordings       += string.Equals(singleZoomData.has_recording , GlobalConstants.TRUE, StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in processing data: {JsonConvert.SerializeObject(singleZoomData)}, error: {ex.Message}");
                throw;
            }
        }

        private List<ZoomHistory> GetZoomData(DateTime startDate, DateTime endDate)
        {
            var zoomData = GetZoomDataFromDataStore(startDate, endDate);

            Logger.Debug($"Fetched data from data store, count : {zoomData.Count}");

            var lastHistoricalDates = GetLastCapturedDataDates(zoomData);

            Logger.Debug($"Last captured dates: {JsonConvert.SerializeObject(lastHistoricalDates)}");

            foreach (var keyValuePair in lastHistoricalDates)
                UpdateDataFromZoom(keyValuePair, zoomData, startDate, endDate);

            return zoomData;
        }

        private List<ZoomHistory> GetZoomDataFromDataStore(DateTime startDate, DateTime endDate, int retriesLeft = retries)
        {
            try
            {
                return zoom_history_repository.Get(startDate, endDate);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while fetching zoom data from data store, startDate: {startDate}, endDate: {endDate}, error : {JsonConvert.SerializeObject(ex)}");

                if (retriesLeft > 0)
                {
                    Thread.Sleep(thread_sleep_in_seconds * 1000);
                    return GetZoomDataFromDataStore(startDate, endDate, --retriesLeft);
                }
                else
                    throw;
            }
        }

        private Dictionary<zoom_metrics_type, DateTime> GetLastCapturedDataDates(List<ZoomHistory> zoomData)
        {
            var lastCapturedData = new Dictionary<zoom_metrics_type, DateTime> { };

            foreach (zoom_metrics_type metricType in Enum.GetValues(typeof(zoom_metrics_type)))
                lastCapturedData.Add(metricType, DateTime.MinValue);

            foreach (var singleZoomData in zoomData)
            {
                var metricType = (zoom_metrics_type)Enum.Parse(typeof(zoom_metrics_type), singleZoomData.type, true);

                Logger.Debug($"Zoom data for metric type : {metricType} is {JsonConvert.SerializeObject(singleZoomData)}");

                if (singleZoomData.end_time > lastCapturedData[metricType])
                    lastCapturedData[metricType] = singleZoomData.end_time;
            }

            return lastCapturedData;
        }

        private void UpdateDataFromZoom(KeyValuePair<zoom_metrics_type, DateTime> lastHistoricalDate, List<ZoomHistory> zoomData, DateTime startDate, DateTime endDate)
        {
            if (lastHistoricalDate.Value.Date >= endDate.Date)
                return;

            if (lastHistoricalDate.Value > startDate)
                startDate = lastHistoricalDate.Value;

            var dataFromZoomAPI = FetchDataFromZoomAPI(lastHistoricalDate.Key, startDate, endDate);

            if (dataFromZoomAPI != null && dataFromZoomAPI.Count > 0)
            {
                SaveFetchedDataInDataStore(dataFromZoomAPI);

                zoomData.AddRange(dataFromZoomAPI);
            }
        }

        private void SaveFetchedDataInDataStore(List<ZoomHistory> dataFromZoomAPI, int retriesLeft = retries)
        {
            try
            {
                zoom_history_repository.Create(dataFromZoomAPI);

                Logger.Debug("Saved new data in data store");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while saving fetched zoom data in data store, error : {JsonConvert.SerializeObject(ex)}");

                if (retriesLeft > 0)
                {
                    Thread.Sleep(thread_sleep_in_seconds * 1000);
                    SaveFetchedDataInDataStore(dataFromZoomAPI, --retriesLeft);
                }
                else
                    throw;
            }
        }

        private List<ZoomHistory> FetchDataFromZoomAPI(zoom_metrics_type metricType, DateTime startDate, DateTime endDate)
        {
            var zoomAPIConfiguration = GetAPIConfiguration();

            int batches         = ((endDate.Date - startDate.Date).Days + 1) / max_days_of_data_to_fetch;

            var dataFromAPI     = new List<ZoomHistory> { };

            var fromDate        = endDate.AddDays(-max_days_of_data_to_fetch + 1) < startDate ? startDate : endDate.AddDays(-max_days_of_data_to_fetch + 1);
            var todate          = endDate;

            var batchIndex      = 0;

            var headers         = APIHelper.GetHeaders(zoomAPIConfiguration);

            var dataFromZoomAPI = new List<ZoomMetricsResponse> { };

            if (metricType == zoom_metrics_type.zoom_rooms)
                dataFromZoomAPI = FetchDataFromZoomRoomsAPI(zoomAPIConfiguration, headers, startDate, batches, dataFromAPI, fromDate, todate, batchIndex);
            else
            {
                string baseUrl  = $"{zoomAPIConfiguration.base_url}/metrics/{metricType}?type=past&page_size={page_size}";

                dataFromZoomAPI = FetchDataFromZoomAPI<ZoomMetricsResponse>(baseUrl, metricType, startDate, headers, batches, fromDate, todate, batchIndex);
            }

            dataFromAPI = ConsolidateDataFromZoomAPI(metricType, dataFromZoomAPI);

            return dataFromAPI;
        }

        private List<ZoomHistory> ConsolidateDataFromZoomAPI(zoom_metrics_type metricType, List<ZoomMetricsResponse> dataFromZoomAPI)
        {
            var consolidatedDataFromZoomAPI = new List<ZoomHistory> { };

            foreach (var data in dataFromZoomAPI)
            {
                switch (metricType)
                {
                    case zoom_metrics_type.webinars:
                        if (data.webinars != null && data.webinars.Count > 0)
                            consolidatedDataFromZoomAPI.AddRange(data.webinars);
                        break;

                    case zoom_metrics_type.zoom_rooms:
                        if (data.zoom_rooms != null && data.zoom_rooms.Count > 0)
                            consolidatedDataFromZoomAPI.AddRange(data.zoom_rooms);
                        break;

                    case zoom_metrics_type.meetings:
                    default:
                        if (data.meetings != null && data.meetings.Count > 0)
                            consolidatedDataFromZoomAPI.AddRange(data.meetings);
                        break;
                }
            }

            PrepareFinalData(metricType, consolidatedDataFromZoomAPI);

            return consolidatedDataFromZoomAPI;
        }

        private List<ZoomMetricsResponse> FetchDataFromZoomRoomsAPI(APIConfiguration zoomAPIConfiguration, Dictionary<string, string> headers, DateTime startDate, int batches, List<ZoomHistory> dataFromAPI, DateTime fromDate, DateTime todate, int batchIndex)
        {
            var zoomRoomsMetricsList = new List<ZoomMetricsResponse> { };

            var zoomRoomIdsUrl       = $"{zoomAPIConfiguration.base_url}/metrics/zoomrooms?page_size={page_size}";
                                     
            var zoomRoomIdsPagedData = FetchDataFromZoomAPI<ZoomRoomIdListResponse>(zoomRoomIdsUrl, headers);

            if (zoomRoomIdsPagedData != null && zoomRoomIdsPagedData.Count > 0)
            {
                var zoomRoomsIds = GetAllZoomRoomIds(zoomRoomIdsPagedData);

                foreach (var zoomRoomId in zoomRoomsIds)
                {
                    var zoomRoomDetailsUrl = $"{zoomAPIConfiguration.base_url}/metrics/zoomrooms/{zoomRoomId}?page_size={page_size}";

                    var zoomRoomsMetrics   = FetchDataFromZoomAPI<ZoomRoomMeetingDetails>(zoomRoomIdsUrl, headers);

                    zoomRoomsMetricsList.AddRange(zoomRoomsMetrics.Select(x => x.past_meetings).ToList());
                }
            }

            return zoomRoomsMetricsList;
        }

        private static List<string> GetAllZoomRoomIds(List<ZoomRoomIdListResponse> zoomRoomsPagedData)
        {
            var zoomRoomsIds = new List<string> { };

            zoomRoomsPagedData.ForEach(x =>
            {
                x.zoom_rooms.ForEach(y => zoomRoomsIds.Add(y.id));
            });

            return zoomRoomsIds;
        }

        private List<T> FetchDataFromZoomAPI<T>(string baseUrl, zoom_metrics_type metricType, DateTime startDate, Dictionary<string, string> headers, int batches,  DateTime fromDate, DateTime todate, int batchIndex) where T: ZoomResponseBase
        {
            var dataFromAPI = new List<T> { };

            do
            {
                var url = $"{baseUrl}&from={fromDate.ToString(ZoomHistoricalDataConstants.DATE_FORMAT)}&to={todate.ToString(ZoomHistoricalDataConstants.DATE_FORMAT)}";

                Logger.Debug($"Calling API: {url}");

                var pagedDataFromAPI = FetchDataFromZoomAPI<T>(url, headers);

                if (pagedDataFromAPI != null)
                    dataFromAPI.AddRange(pagedDataFromAPI);

                batchIndex++;

                todate = fromDate.AddDays(-1);

                if (batchIndex == batches - 1)
                    fromDate = startDate;
                else
                    fromDate = todate.AddDays(-max_days_of_data_to_fetch + 1);
            } while (batchIndex < batches);

            return dataFromAPI;
        }

        private APIConfiguration GetAPIConfiguration(int retriesLeft = retries)
        {
            try
            {
                return api_configuration_manager.Get(APIConfigurationConstants.ZOOM_CONFIGURATION_VALUE);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while fetching zoom optimization report id, retriesLeft: {retriesLeft}, error : {JsonConvert.SerializeObject(ex)}");

                if (retriesLeft > 0)
                {
                    Thread.Sleep(thread_sleep_in_seconds * 1000);
                    return GetAPIConfiguration(--retriesLeft);
                }
                else
                    throw;
            }
        }

        private List<T> FetchDataFromZoomAPI<T>(string url, Dictionary<string, string> headers)
        {
            int requestsMade = 0;
            Stopwatch timer  = Stopwatch.StartNew();

            var data         = APICaller.Get<T>(url, headers, retriesLeft: retries);

            requestsMade++;

            Logger.Debug($"Fetched data from zoom API: {url}");

            if (data != null)
            {
                var dataFromZoomAPI = new List<T>
                {
                    data
                };

                string nextPageToken = GetNextPageToken(data);

                while (!string.IsNullOrWhiteSpace(nextPageToken))
                {
                    requestsMade     = HandleRateLimit(requestsMade, timer);

                    var nextPageData = APICaller.Get<T>($"{url}&next_page_token={nextPageToken}", headers, retriesLeft: retries);

                    requestsMade++;

                    dataFromZoomAPI.Add(nextPageData);

                    nextPageToken = GetNextPageToken(nextPageData);
                }

                return dataFromZoomAPI;
            }

            return new List<T> { };
        }

        private static string GetNextPageToken<T>(T data)
        {
            string nextPageToken;
            if (typeof(T) == typeof(ZoomRoomMeetingDetails))
                nextPageToken = (data as ZoomRoomMeetingDetails).past_meetings.next_page_token;
            else
                nextPageToken = (data as ZoomResponseBase).next_page_token;
            return nextPageToken;
        }

        private static int HandleRateLimit(int requestsMade, Stopwatch timer)
        {
            if (requestsMade == rate_limit && timer.ElapsedMilliseconds < 60 * 1000)
            {
                Logger.Debug($"Rate limit {rate_limit}, elapsed {timer.ElapsedMilliseconds} milliseconds");
                Thread.Sleep((int)(60000 - timer.ElapsedMilliseconds)); // sleep for the time remaining for a minute of time to complete
                timer.Restart();
                requestsMade = 0;
            }

            return requestsMade;
        }

        private void PrepareFinalData(zoom_metrics_type metricType, List<ZoomHistory> data)
        {
            switch (metricType)
            {
                case zoom_metrics_type.zoom_rooms:
                    data.ForEach(x =>
                    {
                        x.type     = zoom_metrics_type.zoom_rooms.ToString();
                        x.duration = GetFixedTimespanValue(x.duration);
                    });
                    break;

                case zoom_metrics_type.webinars:
                    data.ForEach(x =>
                    {
                        x.type     = zoom_metrics_type.webinars.ToString();
                        x.duration = GetFixedTimespanValue(x.duration);
                    });
                    break;

                case zoom_metrics_type.meetings:
                default:
                    data.ForEach(x =>
                    {
                        x.type     = zoom_metrics_type.meetings.ToString();
                        x.duration = GetFixedTimespanValue(x.duration);
                    });
                    break;
            }
        }

        public string GetFixedTimespanValue(string duration)
        {
            if (duration.Split(':').Length == 2)
                duration = "00:" + duration;

            return duration;
        }
    }
}