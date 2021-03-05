using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Helper;
using Newtonsoft.Json;

namespace ReportAPI.Filters
{
    public class G2AuthorizeAttribute : AuthorizeAttribute
    {
        private static readonly string config_auth_token = ConfigurationManager.AppSettings["ApplicationAuthToken"];

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (!AuthorizeRequest(actionContext))
            {
                actionContext.Response         = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
                actionContext.Response.Content = new StringContent(APIHelper.GetAPIResponseMessage(System.Net.HttpStatusCode.Unauthorized, "Invalid access token"));
            }
        }

        private bool AuthorizeRequest(HttpActionContext actionContext)
        {
            bool isAuthenticated = false;

            try
            {
                string authToken = null;
                var headers      = actionContext.Request.Headers;

                if (headers != null && headers.Count() > 0 && headers.Contains("Authorization"))
                {
                    authToken = headers.Where(x => string.Equals(x.Key, "Authorization")).FirstOrDefault().Value.FirstOrDefault();
                    authToken = authToken.Replace("Bearer ", "");

                    isAuthenticated = string.Equals(authToken, config_auth_token, System.StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (System.Exception ex)
            {
                Logger.Error($"Error while authorizing access, ex : {JsonConvert.SerializeObject(ex)}");
            }

            return isAuthenticated;
        }
    }
}