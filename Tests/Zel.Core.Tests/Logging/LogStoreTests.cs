// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zel.Classes;

namespace Zel.Tests.Logging
{
    [TestClass]
    public class LogStoreTests
    {
        [TestMethod]
        public void GetHostCode_Returns_000000_If_OnGetHostCode_Is_Not_Subscribed_To()
        {
            var logStoreTest = new LogStoreTest();
            Assert.AreEqual("000000", logStoreTest.GetHostCode());
        }

        [TestMethod]
        public void GetHostCode_Returns_HostCode_From_The_Subscribed_OnGetHostCode()
        {
            const string url = "http://tempuri.org";
            HttpContext.Current = new HttpContext(new HttpRequest("", url, ""),
                new HttpResponse(new StringWriter()));

            var logStoreTest = new LogStoreTest();
            Assert.AreEqual(Asp.GetHostCode().ToString(), logStoreTest.GetHostCode());
        }

        [TestMethod]
        public void GetHostCode_Returns_Default_HostCode_From_The_Subscribed_OnGetHostCode_If_Returned_HostCode_Is_Null()
        {
            HttpContext.Current = null;
            var logStoreTest = new LogStoreTest();
            Assert.AreEqual("000000", logStoreTest.GetHostCode());
        }

        #region Nested type: LogStoreTest

        private class LogStoreTest : LogStore
        {
            public override bool WriteToLog(LogMessage message)
            {
                throw new NotImplementedException();
            }

            public new string GetHostCode()
            {
                return base.GetHostCode();
            }
        }

        #endregion
    }
}