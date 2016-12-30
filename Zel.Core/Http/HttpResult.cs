// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Zel.Classes;

namespace Zel.Http
{
    /// <summary>
    ///     Http Result
    /// </summary>
    public sealed class HttpResult : IHttpResult
    {
        #region Properties

        /// <summary>
        ///     The headers returned by the query
        /// </summary>
        public NameValueList Headers { get; set; }

        /// <summary>
        ///     Response content type
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        ///     Resonse content length
        /// </summary>
        public long ContentLength { get; set; }

        /// <summary>
        ///     Resonse http status
        /// </summary>
        public HttpStatusCode HttpStatus { get; set; }

        /// <summary>
        ///     Response uri
        /// </summary>
        public Uri ResponseUri { get; set; }

        /// <summary>
        ///     Query exception
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        ///     Response
        /// </summary>
        public byte[] Response { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Instantiate a new HttpResult
        /// </summary>
        public HttpResult(string statusCode, byte[] response, NameValueList headers, Exception exception)
        {
            Exception = exception;

            if (headers != null)
            {
                var contentType = headers.FirstOrDefault(x => x.Name.ToUpper() == "CONTENT-TYPE");
                if (contentType != null)
                {
                    ContentType = contentType.Value.ToString();
                }
                var contentLength = headers.FirstOrDefault(x => x.Name.ToUpper() == "CONTENT-LENGTH");
                if (contentLength != null)
                {
                    ContentLength = Convert.ToInt64(contentLength.Value.ToString());
                }

                var contentLocation = headers.FirstOrDefault(x => x.Name.ToUpper() == "CONTENT-LOCATION");
                if (contentLocation != null)
                {
                    ResponseUri = new Uri(contentLocation.Value.ToString());
                }
            }

            if (!string.IsNullOrWhiteSpace(statusCode))
            {
                HttpStatus = (HttpStatusCode) Enum.Parse(typeof(HttpStatusCode), statusCode);
            }

            Headers = headers;
            Response = response;
        }

        public HttpResult() {}

        #endregion

        #region Methods

        /// <summary>
        ///     Converts the response stream to string
        /// </summary>
        /// <returns>Response string</returns>
        public string GetResponseString()
        {
            return Response == null ? null : Encoding.UTF8.GetString(Response);
        }

        public string GetResponseDetailsForLogging()
        {
            return new Dictionary<string, object>
            {
                {"Response String", GetResponseString()},
                {"Web Exception", Exception},
                {"Headers", Headers},
                {"Status", HttpStatus}
            }.ToJson();
        }

        #endregion
    }
}