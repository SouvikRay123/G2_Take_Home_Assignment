using Constants;
using DataLayer;
using Helper;
using IOCContainer;
using Newtonsoft.Json;
using Ninject;
using Ninject.Modules;
using ReportAPI.Filters;
using System;
using System.Web.Http;

namespace ReportAPI.Controllers
{
    [G2Authorize]
    public class ReportController : ApiController
    {
        public static StandardKernel ninject_kernel;
        IReportRepository report_repository;

        public ReportController()
        {
            ninject_kernel = new StandardKernel();
            ninject_kernel.Load(new INinjectModule[] { new NinjectBindings() });
            report_repository = ninject_kernel.Get<IReportRepository>();
        }

        [HttpGet]
        public IHttpActionResult usage(string from, string to)
        {
            try
            {
                DateTime fromDateTime = DateTime.MinValue, toDateTime = DateTime.MinValue;

                SetDateTimeValues(from, to, out fromDateTime, out toDateTime);

                return Ok(report_repository.GetReportResult(ReportTypeConstants.ZOOM_USAGE, fromDateTime, toDateTime));
            }
            catch(FormatException formatException)
            {
                return BadRequestResponse(formatException);
            }
            catch (Exception ex)
            {
                return InternalServerErrorResponse(from, to, ex);
            }
        }

        [HttpGet]
        public IHttpActionResult optimization(string from, string to)
        {
            try
            {
                DateTime fromDateTime = DateTime.MinValue, toDateTime = DateTime.MinValue;

                SetDateTimeValues(from, to, out fromDateTime, out toDateTime);

                return Ok(report_repository.GetReportResult(ReportTypeConstants.ZOOM_OPTIMIZATION, fromDateTime, toDateTime));
            }
            catch (FormatException formatException)
            {
                return BadRequestResponse(formatException);
            }
            catch (Exception ex)
            {
                return InternalServerErrorResponse(from, to, ex);
            }
        }

        private IHttpActionResult BadRequestResponse(FormatException formatException)
        {
            Logger.Fatal($"Incorrect values provided, error : {JsonConvert.SerializeObject(formatException)}");

            return BadRequest(APIHelper.GetAPIResponseMessage(System.Net.HttpStatusCode.BadRequest, formatException.Message));
        }

        private IHttpActionResult InternalServerErrorResponse(string fromDate, string toDate, Exception ex)
        {
            Logger.Fatal($"Error in fetching usage reports between {fromDate} and {toDate}, error : {JsonConvert.SerializeObject(ex)}");

            return InternalServerError();
        }

        private void SetDateTimeValues(string fromDate, string toDate, out DateTime fromDateTime, out DateTime toDateTime)
        {
            SetDateTimeValue(fromDate, out fromDateTime);
            SetDateTimeValue(toDate, out toDateTime);

            ExchangeDateTimeValuesIfNeeded(ref fromDateTime, ref toDateTime);
        }

        private void ExchangeDateTimeValuesIfNeeded(ref DateTime fromDateTime, ref DateTime toDateTime)
        {
            if(fromDateTime > toDateTime)
            {
                DateTime tempDateTime = fromDateTime;
                fromDateTime          = toDateTime;
                toDateTime            = tempDateTime;
            }
        }

        private void SetDateTimeValue(string stringDate, out DateTime dateTimeObject)
        {
            if (!DateTime.TryParse(stringDate, out dateTimeObject))
                throw new FormatException($"Incorrect value provided: {stringDate}. Value should be in {ReportDataTypeConstants.DATE_FORMAT} format");
        }
    }
}