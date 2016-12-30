// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using Zel.Classes;
using Zel.Http;

namespace Zel
{
    /// <summary>
    ///     Http Query
    /// </summary>
    public sealed class HttpQuery : IHttpQuery
    {
        /// <summary>
        ///     Default useragent to use if one is not specified in the query
        /// </summary>
        public const string DEFAULT_USERAGENT = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0)";

        /// <summary>
        ///     Default timeout to use if one is not specified in the query
        /// </summary>
        public const int DEFAULT_TIMEOUT = 10000;

        #region Constructors

        /// <summary>
        ///     Instantiate a new HttpQuery
        /// </summary>
        /// <param name="url">Http url</param>
        public HttpQuery(string url)
        {
            Url = url;
            Headers = new NameValueList();
            HttpMethod = HttpMethod.Get;
        }

        #endregion

        #region Internals

        /// <summary>
        ///     Current web request
        /// </summary>
        private HttpWebRequest _httpWebRequest;

        private HttpWebResponse _httpWebResponse;

        #endregion

        #region Properties

        /// <summary>
        ///     Headers to use in the query
        /// </summary>
        public NameValueList Headers { get; }

        /// <summary>
        ///     Uri to query
        /// </summary>
        public string Url { get; }

        /// <summary>
        ///     Query timeout
        /// </summary>
        public int TimeOut { get; set; }

        /// <summary>
        ///     HttpMethod to use in the query
        /// </summary>
        public HttpMethod HttpMethod { get; set; }

        /// <summary>
        ///     UserAgent string
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        ///     Referring url
        /// </summary>
        public string Referrer { get; set; }

        /// <summary>
        ///     Data to post
        /// </summary>
        public string PostString { get; set; }

        /// <summary>
        ///     Bytes to post
        /// </summary>
        public byte[] PostBytes { get; set; }

        /// <summary>
        ///     Content type
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        ///     Accept
        /// </summary>
        public string Accept { get; set; }

        /// <summary>
        ///     Media type
        /// </summary>
        public string MediaType { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Dispose the http query
        /// </summary>
        public void Dispose()
        {
            //dispose the response
            if (_httpWebResponse == null)
            {
                return;
            }

            _httpWebResponse.Close();
            _httpWebResponse.Dispose();
            _httpWebResponse = null;
        }

        /// <summary>
        ///     Executes the query
        /// </summary>
        /// <returns>Query Result</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public IHttpResult Execute()
        {
            _httpWebRequest = (HttpWebRequest) WebRequest.Create(Url);

            //set headers
            SetHeaders();

            //set referrer
            if (Referrer != null)
            {
                _httpWebRequest.Referer = Referrer;
            }

            //set useragent
            _httpWebRequest.UserAgent = UserAgent ?? DEFAULT_USERAGENT;

            //default timeout is 10secs
            _httpWebRequest.Timeout = TimeOut == 0 ? DEFAULT_TIMEOUT : TimeOut;

            //set request method
            _httpWebRequest.Method = HttpMethod.ToString();

            //set content type
            _httpWebRequest.ContentType = ContentType;

            //set accept
            _httpWebRequest.Accept = Accept;

            //set media type
            _httpWebRequest.MediaType = MediaType;

            //set post data
            if (HttpMethod == HttpMethod.Post)
            {
                SetPostData();
            }


            //execute
            try
            {
                _httpWebResponse = _httpWebRequest.GetResponse() as HttpWebResponse;
                var headers =
                    new NameValueList(
                        _httpWebResponse.Headers.AllKeys.Select(
                            x => new NameValue(x, _httpWebResponse.Headers[x])));

                return new HttpResult(_httpWebResponse.StatusCode.ToString(),
                    _httpWebResponse.GetResponseStream().ToByteArray(), headers, null);
            }
            catch (WebException ex)
            {
                //exception, return exception response as response
                _httpWebResponse = ex.Response as HttpWebResponse;
                NameValueList headers = null;
                var httpWebResponse = _httpWebResponse;
                var statusCode = string.Empty;
                byte[] response = null;
                if ((httpWebResponse != null) && (httpWebResponse.Headers != null))
                {
                    headers =
                        new NameValueList(
                            httpWebResponse.Headers.AllKeys.Select(
                                x => new NameValue(x, _httpWebResponse.Headers[x])));
                    statusCode = httpWebResponse.StatusCode.ToString();
                    response = httpWebResponse.GetResponseStream().ToByteArray();
                }

                return new HttpResult(statusCode, response, headers, ex);
            }
        }


        private void SetPostData()
        {
            if ((HttpMethod == HttpMethod.Post) && (PostString != null))
            {
                //Extra steps for HTTP POST
                _httpWebRequest.ContentLength = PostString.Length;
                var streamWriter = new StreamWriter(_httpWebRequest.GetRequestStream());
                streamWriter.Write(PostString);
                streamWriter.Flush();
                streamWriter.Close();
            }
            else if ((HttpMethod == HttpMethod.Post) && (PostBytes != null))
            {
                //Extra steps for HTTP POST
                _httpWebRequest.ContentLength = PostBytes.Length;
                var requestStream = _httpWebRequest.GetRequestStream();
                requestStream.Write(PostBytes, 0, PostBytes.Length);
                requestStream.Close();
                //using (var streamWriter = new StreamWriter(this._httpWebRequest.GetRequestStream()))
                //{
                //    streamWriter.Write(this.PostBytes);
                //    streamWriter.Flush();
                //}
            }
            else if (HttpMethod == HttpMethod.Post)
            {
                _httpWebRequest.ContentLength = 0;
            }
        }

        private void SetHeaders()
        {
            foreach (var header in Headers)
            {
                _httpWebRequest.Headers.Add(header.Name, header.Value.ToString());
            }
        }

        #endregion
    }
}