using Newtonsoft.Json;
using System.Net;

namespace ReportAPI
{
    public static class APIHelper
    {
        public static string GetAPIResponseMessage(HttpStatusCode code, string message)
        {
            return JsonConvert.SerializeObject(new
            {
                code    = code,
                message = message
            });
        }
    }
}