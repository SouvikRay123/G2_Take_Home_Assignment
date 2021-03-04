using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Helper
{
    public static class APICaller
    {
        public static T Get<T>(string url, Dictionary<string,string> headers, string contentType = "application/json")
        {
            var webRequest = WebRequest.Create(new Uri(url));

            webRequest.Method      = "GET";
            webRequest.ContentType = contentType;
            AttachHeaders(webRequest, headers);

            using (var reader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
            }
        }

        private static void AttachHeaders(WebRequest webRequest, Dictionary<string, string> headers)
        {
            if(headers != null && headers.Count > 0)
            {
                foreach (var header in headers)
                {
                    webRequest.Headers.Add(header.Key, header.Value);
                }
            }
        }
    }
}
