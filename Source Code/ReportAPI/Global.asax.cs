using Helper;
using System.Web.Http;

namespace ReportAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            Logger.SetLogger(log);
        }
    }
}
