// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Zel.Http
{
    /// <summary>
    ///     Http Url
    /// </summary>
    public class HttpUrl
    {
        #region Constructors

        /// <summary>
        ///     Instantiate a new HttpUrl
        /// </summary>
        /// <param name="text">Url text</param>
        /// <param name="url">Url</param>
        public HttpUrl(string text, string url)
        {
            Text = text;
            Url = url;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Url text
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        ///     Url
        /// </summary>
        public string Url { get; private set; }

        #endregion
    }
}