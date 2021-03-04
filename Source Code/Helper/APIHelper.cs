using Constants;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper
{
    public static class APIHelper
    {
        public static Dictionary<string, string> GetHeaders(APIConfiguration apiConfiguration)
        {
            var headers = new Dictionary<string, string> { };

            SetAuthorizationHeader(apiConfiguration, headers);

            return headers;
        }

        private static void SetAuthorizationHeader(APIConfiguration apiConfiguration, Dictionary<string, string> headers)
        {
            if (!string.IsNullOrWhiteSpace(apiConfiguration.credentials_type))
            {
                switch (apiConfiguration.credentials_type)
                {
                    case "JWT":
                        headers.Add(APIHeaderConstants.AUTHORIZATION, $"Bearer {apiConfiguration.credentials}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
