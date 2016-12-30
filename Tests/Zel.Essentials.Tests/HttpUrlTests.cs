// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zel.Http;

namespace Zel.Tests
{
    [TestClass]
    public class HttpUrlTests
    {
        #region Constructor Tests

        [TestMethod]
        public void Constructor_Sets_Properties()
        {
            const string text = "text";
            const string url = "http://www.url.com";

            var httpUrl = new HttpUrl(text, url);

            Assert.AreEqual(text, httpUrl.Text);
            Assert.AreEqual(url, httpUrl.Url);
        }

        #endregion
    }
}