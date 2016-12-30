// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Web;
using Newtonsoft.Json;

namespace Zel
{
    public static class Extensions
    {
        public static bool IsJsonRequest(this HttpRequest httpRequest)
        {
            return (httpRequest.RequestType == "POST") && httpRequest.ContentType.Contains("application/json");
        }

        public static T GetJsonAsObject<T>(this HttpRequest httpRequest)
        {
            var rawJson = httpRequest.GetJson();
            return rawJson != null ? JsonConvert.DeserializeObject<T>(rawJson) : default(T);
        }

        public static string GetJson(this HttpRequest httpRequest)
        {
            if (httpRequest.IsJsonRequest())
            {
                if (HttpContext.Current.Items.Contains("JSON"))
                {
                    return HttpContext.Current.Items["JSON"] as string;
                }

                using (var streamReader = new StreamReader(HttpContext.Current.Request.InputStream))
                {
                    var rawJson = streamReader.ReadToEnd();
                    HttpContext.Current.Items["JSON"] = rawJson;
                    return rawJson;
                }
            }
            return null;
        }
    }
}