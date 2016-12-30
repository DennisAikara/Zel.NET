// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Zel.Classes;

namespace Zel.Http
{
    public class HttpQueryProvider : IHttpQueryProvider
    {
        #region IHttpQueryProvider Members

        public IHttpQuery Create(string url)
        {
            return new HttpQuery(url);
        }

        #endregion
    }
}