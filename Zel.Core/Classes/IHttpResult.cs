// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;

namespace Zel.Classes
{
    public interface IHttpResult
    {
        /// <summary>
        ///     The headers returned by the query
        /// </summary>
        NameValueList Headers { get; set; }

        /// <summary>
        ///     Response content type
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        ///     Resonse content length
        /// </summary>
        long ContentLength { get; set; }

        /// <summary>
        ///     Resonse http status
        /// </summary>
        HttpStatusCode HttpStatus { get; set; }

        /// <summary>
        ///     Response uri
        /// </summary>
        Uri ResponseUri { get; set; }

        /// <summary>
        ///     Query exception
        /// </summary>
        Exception Exception { get; set; }

        /// <summary>
        ///     Response
        /// </summary>
        byte[] Response { get; set; }

        /// <summary>
        ///     Converts the response stream to string
        /// </summary>
        /// <returns>Response string</returns>
        string GetResponseString();

        string GetResponseDetailsForLogging();
    }
}