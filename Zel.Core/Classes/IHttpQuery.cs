// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Zel.Classes
{
    public interface IHttpQuery : IDisposable
    {
        /// <summary>
        ///     Headers to use in the query
        /// </summary>
        NameValueList Headers { get; }

        /// <summary>
        ///     Uri to query
        /// </summary>
        string Url { get; }

        /// <summary>
        ///     Query timeout
        /// </summary>
        int TimeOut { get; set; }

        /// <summary>
        ///     HttpMethod to use in the query
        /// </summary>
        HttpMethod HttpMethod { get; set; }

        /// <summary>
        ///     UserAgent string
        /// </summary>
        string UserAgent { get; set; }

        /// <summary>
        ///     Referring url
        /// </summary>
        string Referrer { get; set; }

        /// <summary>
        ///     String to post
        /// </summary>
        string PostString { get; set; }

        /// <summary>
        ///     Bytes to post
        /// </summary>
        byte[] PostBytes { get; set; }

        /// <summary>
        ///     Content type
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        ///     Accept
        /// </summary>
        string Accept { get; set; }

        /// <summary>
        ///     Media type
        /// </summary>
        string MediaType { get; set; }

        /// <summary>
        ///     Executes the query
        /// </summary>
        /// <returns>Query Result</returns>
        IHttpResult Execute();
    }
}