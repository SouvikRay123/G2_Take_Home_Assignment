using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace Helper
{
    public static class APICaller
    {
        const int thread_sleep_time = 5;

        public static T Get<T>(string url, Dictionary<string,string> headers, string contentType = "application/json", int retriesLeft = 0)
        {
            try
            {
                var webRequest = WebRequest.Create(new Uri(url));

                webRequest.Method = "GET";
                webRequest.ContentType = contentType;
                AttachHeaders(webRequest, headers);

                using (var response = (HttpWebResponse)webRequest.GetResponse())
                using (var receiveStream = response.GetResponseStream())
                using (var reader = new StreamReader(receiveStream))
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while calling api: {url}, error : {JsonConvert.SerializeObject(ex)}");

                if (retriesLeft > 0)
                {
                    Thread.Sleep(thread_sleep_time * 1000);
                    return Get<T>(url, headers, contentType, --retriesLeft);
                }
                else
                    throw;
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
