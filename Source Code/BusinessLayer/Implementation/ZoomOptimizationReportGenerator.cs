﻿using Constants;
using DataLayer;
using Helper;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BusinessLayer
{
    public class ZoomOptimizationReportGenerator : IZoomOptimizationReportGenerator
    {
        IReportRepository report_repository;
        IZoomHistoryRepository zoom_history_repository;
        IAPIConfigurationManager api_configuration_manager;
        int interval_For_Optimization_Report = 30;
        const int retries                    = 10;
        const int thread_sleep_in_seconds    = 5;

        public ZoomOptimizationReportGenerator(IReportRepository reportRepository, IZoomHistoryRepository zoomHistoryRepository, IAPIConfigurationManager apiConfigurationManager)
        {
            report_repository         = reportRepository;
            zoom_history_repository   = zoomHistoryRepository;
            api_configuration_manager = apiConfigurationManager;
        }

        public void GenerateOptimizationReport()
        {
            try
            {
                int reportDuration = 60;

                Logger.Debug($"Starting generation of {reportDuration} day report");

                var now = DateTime.Now;

                GenerateOptimizationReport(new DateTime(now.Year, now.Month, now.Day, 0, 0, 0).AddDays(-reportDuration), new DateTime(now.Year, now.Month, now.Day, 23, 59, 59).AddDays(-1));

                Logger.Debug($"Completed generation of {reportDuration} day report");
            }
            catch (Exception ex)
            {
                Logger.Fatal($"Error while generating report, error : {JsonConvert.SerializeObject(ex)}");
                throw;
            }
        }

        private void GenerateOptimizationReport(DateTime startDate, DateTime endDate)
        {
            Logger.Debug($"Checking for report to be generated between {startDate.ToShortDateString()} and {endDate.ToShortDateString()}");

            string reportId = GetZoomOptimizationReportId(startDate, endDate);

            if (string.IsNullOrWhiteSpace(reportId))
            {
                Logger.Debug("No report id to be processed");
                return;
            }

            ProcessOptimizationReport(reportId, startDate, endDate);
        }

        private string GetZoomOptimizationReportId(DateTime startDate, DateTime endDate, int retriesLeft = retries)
        {
            try
            {
                return report_repository.GetReportId(ReportTypeConstants.ZOOM_OPTIMIZATION, startDate, endDate);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while fetching zoom optimization report id, retriesLeft: {retriesLeft}, error : {JsonConvert.SerializeObject(ex)}");

                if (retriesLeft > 0)
                {
                    Thread.Sleep(thread_sleep_in_seconds * 1000);
                    return GetZoomOptimizationReportId(startDate, endDate, --retriesLeft);
                }
                else
                    throw;
            }
        }

        private void ProcessOptimizationReport(string reportId, DateTime startDate, DateTime endDate)
        {
            Logger.Debug("Fetching plans usage");

            var zoomPlansUsage = GetZoomPlansUsage();

            Logger.Debug("Preparing zoom optimization report");

            var reportDetails  = ProcessOptimizationReport(zoomPlansUsage, startDate, endDate);

            Logger.Debug("Updating status in reports data store");

            UpdateReportStatus(reportId, ReportStatusConstants.SUCCESS, JsonConvert.SerializeObject(reportDetails));

            Logger.Debug("Setting up next cycle");

            SetupNextReportCycle(startDate, endDate);
        }

        private ZoomPlansUsage GetZoomPlansUsage()
        {
            var zoomAPIConfiguration = GetAPIConfiguration();

            var url                  = $"{zoomAPIConfiguration.base_url}/accounts/me/plans/usage";
                                     
            var headers              = APIHelper.GetHeaders(zoomAPIConfiguration);

            Logger.Debug($"Calling API: {url}");

            return APICaller.Get<ZoomPlansUsage>(url, headers, retriesLeft: retries);
        }

        private APIConfiguration GetAPIConfiguration(int retriesLeft = retries)
        {
            try
            {
                return api_configuration_manager.Get(APIConfigurationConstants.ZOOM_CONFIGURATION_VALUE);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while fetching zoom api configuration, retriesLeft: {retriesLeft}, error : {JsonConvert.SerializeObject(ex)}");

                if (retriesLeft > 0)
                {
                    Thread.Sleep(thread_sleep_in_seconds * 1000);
                    return GetAPIConfiguration(--retriesLeft);
                }
                else
                    throw;
            }
        }

        private Dictionary<string, ZoomOptimizationDetails> ProcessOptimizationReport(ZoomPlansUsage zoomPlansUsage, DateTime startDate, DateTime endDate)
        {
            var historicalUsageData = GetZoomHistoricalData(startDate, endDate);

            return ProcessOptimizationReport(zoomPlansUsage, historicalUsageData);
        }

        private List<ZoomHistory> GetZoomHistoricalData(DateTime startDate, DateTime endDate, int retriesLeft = retries)
        {
            try
            {
                return zoom_history_repository.Get(startDate, endDate);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while fetching zoom historical data, retriesLeft: {retriesLeft}, error : {JsonConvert.SerializeObject(ex)}");

                if (retriesLeft > 0)
                {
                    Thread.Sleep(thread_sleep_in_seconds * 1000);
                    return GetZoomHistoricalData(startDate, endDate, --retriesLeft);
                }
                else
                    throw;
            }
        }

        private Dictionary<string, ZoomOptimizationDetails> ProcessOptimizationReport(ZoomPlansUsage zoomPlansUsage, List<ZoomHistory> historicalUsageData)
        {
            var optimizationReport = new Dictionary<string, ZoomOptimizationDetails> { };

            ProcessPlan(zoomPlansUsage.plan_base, zoom_metrics_type.meetings.ToString(), historicalUsageData, optimizationReport);
            ProcessWebinarPlans(zoomPlansUsage.plan_webinar, historicalUsageData, optimizationReport);
            ProcessPlan(zoomPlansUsage.plan_zoom_rooms, zoom_metrics_type.zoom_rooms.ToString(), historicalUsageData, optimizationReport);

            return optimizationReport;
        }

        private void ProcessWebinarPlans(List<ZoomPlanUsageDetail> zoomWebinarPlansUsages, List<ZoomHistory> historicalUsageData, Dictionary<string, ZoomOptimizationDetails> optimizationReport)
        {
            if (zoomWebinarPlansUsages == null || zoomWebinarPlansUsages.Count == 0)
                return;

            foreach (var zoomPlanUsageDetail in zoomWebinarPlansUsages)
            {
                var metricsType             = zoomPlanUsageDetail.type.Split('_')[0];
                var historicalDataForPlan   = historicalUsageData.Where(x => x.type == zoom_metrics_type.webinars.ToString() && string.Compare(metricsType, x.user_type, true) > -1).ToList();

                var optimizationDetails     = new ZoomOptimizationDetails
                {
                    billing_cycle           = GetBillingCycleOfPlan(zoomPlanUsageDetail),
                    total_licenses          = zoomPlanUsageDetail.hosts,
                    total_licenses_assigned = zoomPlanUsageDetail.usage,
                    total_licenses_unused   = zoomPlanUsageDetail.hosts > 0 ? GetUnusedLicenses(zoomPlanUsageDetail, historicalDataForPlan) : 0
                };

                optimizationReport.Add(metricsType, optimizationDetails);
            }
        }

        private void ProcessPlan(ZoomPlanUsageDetail zoomPlanUsageDetail, string metricsType, List<ZoomHistory> historicalUsageData, Dictionary<string, ZoomOptimizationDetails> optimizationReport)
        {
            if (zoomPlanUsageDetail == null)
                return;

            var historicalDataForPlan   = GetFilteredLicensedUsageForPlan(metricsType, historicalUsageData);
                                        
            var optimizationDetails     = new ZoomOptimizationDetails
            {                           
                billing_cycle           = GetBillingCycleOfPlan(zoomPlanUsageDetail),
                total_licenses          = zoomPlanUsageDetail.hosts,
                total_licenses_assigned = zoomPlanUsageDetail.usage,
                total_licenses_unused   = zoomPlanUsageDetail.hosts > 0 ? GetUnusedLicenses(zoomPlanUsageDetail, historicalDataForPlan) : 0
            };

            optimizationReport.Add(metricsType, optimizationDetails);
        }

        private static List<ZoomHistory> GetFilteredLicensedUsageForPlan(string metricsType, List<ZoomHistory> historicalUsageData)
        {
            return historicalUsageData.Where(x => x.type == metricsType && x.user_type.Contains(ZoomHistoricalDataConstants.LICENSED_USER_TYPE)).ToList();
        }

        private string GetBillingCycleOfPlan(ZoomPlanUsageDetail planUsageDetail)
        {
            var splittedParts = planUsageDetail.type.Split('_');

            return splittedParts[splittedParts.Length - 1];
        }

        private int GetUnusedLicenses(ZoomPlanUsageDetail zoomPlanUsageDetail, List<ZoomHistory> historicalDataForPlan)
        {
            var distinctLicensedUsages = historicalDataForPlan.Select(x => x.email).Distinct();

            return zoomPlanUsageDetail.usage - distinctLicensedUsages.Count();
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
                Logger.Error($"Error while updating zoom optimization report status, retriesLeft: {retriesLeft}, error : {JsonConvert.SerializeObject(ex)}");

                if (retriesLeft > 0)
                {
                    Thread.Sleep(thread_sleep_in_seconds * 1000);
                    UpdateReportStatus(reportId, ReportStatusConstants.SUCCESS, reportResult, --retriesLeft);
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
                    start_date = startDate.AddDays(interval_For_Optimization_Report),
                    end_date   = endDate.AddDays(interval_For_Optimization_Report),
                    type       = ReportTypeConstants.ZOOM_OPTIMIZATION,
                    status     = ReportStatusConstants.IN_PROGRESS
                });;

                Logger.Debug("Scheduled next report generation");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while setting up next zoom optimization report cycle, retriesLeft: {retriesLeft}, error : {JsonConvert.SerializeObject(ex)}");

                if (retriesLeft > 0)
                {
                    Thread.Sleep(thread_sleep_in_seconds * 1000);
                    SetupNextReportCycle(startDate, endDate, --retriesLeft);
                }
                else
                    throw;
            }
        }
    }
}