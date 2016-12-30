// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zel.Classes;

namespace Zel.Tests
{
    [TestClass]
    public class AspTests
    {
        private const string URL = "http://tempuri.org";

        [TestMethod]
        public void HasHttpContext_Returns_False_If_HttpContext_Doesnot_Exist()
        {
            Assert.IsFalse(Asp.HasHttpContext());
        }

        [TestMethod]
        public void HasHttpContext_Returns_True_If_HttpContext_Exist()
        {
            HttpContext.Current = new HttpContext(new HttpRequest("", URL, ""), new HttpResponse(new StringWriter()));
            Assert.IsTrue(Asp.HasHttpContext());
        }

        [TestMethod]
        public void GetRequestHostCode_Returns_Zero_When_HttpContext_Doesnot_Exist()
        {
            Assert.IsFalse(Asp.HasHttpContext());
            Assert.AreEqual(0, Asp.GetHostCode());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetHttpContextItem_Throws_ArgumentNullException_If_itemName_Is_Null()
        {
            try
            {
                Asp.SetHttpContextItem(null, null);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("itemName", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpContextDoesNotExistException))]
        public void SetHttpContextItem_Throws_HttpContextDoesNotExistException_If_HttpContext_Doesnot_Exist()
        {
            Assert.IsFalse(Asp.HasHttpContext());
            Asp.SetHttpContextItem("test", null);
        }

        [TestMethod]
        public void SetHttpContextItem_Adds_Item_To_HttpContext_Items()
        {
            HttpContext.Current = new HttpContext(new HttpRequest("", URL, ""), new HttpResponse(new StringWriter()));
            Asp.SetHttpContextItem("test", "hello");
            Assert.AreEqual("hello", HttpContext.Current.Items["test"]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetHttpContextItem_Throws_ArgumentNullException_If_itemName_Is_Null()
        {
            try
            {
                Asp.GetHttpContextItem(null);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("itemName", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpContextDoesNotExistException))]
        public void GetHttpContextItem_Throws_HttpContextDoesNotExistException_If_HttpContext_Doesnot_Exist()
        {
            Assert.IsFalse(Asp.HasHttpContext());
            Asp.GetHttpContextItem("test");
        }

        [TestMethod]
        public void GetHttpContextItem_Returns_Item_From_HttpContext_Items()
        {
            HttpContext.Current = new HttpContext(new HttpRequest("", URL, ""), new HttpResponse(new StringWriter()));
            HttpContext.Current.Items["test"] = "hello";

            Assert.AreEqual("hello", Asp.GetHttpContextItem("test"));
        }

        [TestMethod]
        public void GetHttpContextItem_Returns_Null_When_Item_Doesnot_Exist_In_HttpContext_Items()
        {
            HttpContext.Current = new HttpContext(new HttpRequest("", URL, ""), new HttpResponse(new StringWriter()));

            Assert.IsNull(Asp.GetHttpContextItem("test"));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpContextDoesNotExistException))]
        public void DisposeContextItems_Throws_HttpContextDoesNotExistException_If_HttpContext_Doesnot_Exist()
        {
            Assert.IsFalse(Asp.HasHttpContext());
            Asp.DisposeContextItems();
        }

        [TestMethod]
        public void DisposeContextItems_Disposes_HttpContext_Items()
        {
            HttpContext.Current = new HttpContext(new HttpRequest("", URL, ""), new HttpResponse(new StringWriter()));
            var disposableClass = new DisposableClass();
            HttpContext.Current.Items["disposableclass"] = disposableClass;

            Assert.IsFalse(disposableClass.DisposeCalled);
            Asp.DisposeContextItems();
            Assert.IsTrue(disposableClass.DisposeCalled);
        }

        [TestMethod]
        public void GetRequestHostCode_Returns_HostCode_When_HttpContext_Exist()
        {
            HttpContext.Current = new HttpContext(new HttpRequest("", URL, ""), new HttpResponse(new StringWriter()));
            Assert.IsTrue(Asp.HasHttpContext());

            var host = new Uri(URL).Host;

            if (host.Length > 30)
            {
                host = host.Substring(0, 15) + host.Substring(host.Length - 15);
            }

            var code = 0;
            foreach (var c in host)
            {
                if (c%2 == 1)
                {
                    code = code + c;
                }
                else
                {
                    code = code - c;
                }
            }

            code *= host[0] + host[host.Length/2] + host[host.Length - 1]*8080;
            code /= host[host.Length - 1];

            Assert.AreEqual(code, Asp.GetHostCode());
        }

        [TestMethod]
        public void GetRequestHostCode_Returns_HostCode_When_Host_Is_Specified()
        {
            var host = new Uri("http://www.monkeytest.com").Host;

            if (host.Length > 30)
            {
                host = host.Substring(0, 15) + host.Substring(host.Length - 15);
            }

            var code = 0;
            foreach (var c in host)
            {
                if (c%2 == 1)
                {
                    code = code + c;
                }
                else
                {
                    code = code - c;
                }
            }

            code *= host[0] + host[host.Length/2] + host[host.Length - 1]*8080;
            code /= host[host.Length - 1];

            Assert.AreEqual(code, Asp.GetHostCode(host));
        }

        #region Nested type: DisposableClass

        private class DisposableClass : IDisposable
        {
            public bool DisposeCalled { get; private set; }

            #region IDisposable Members

            public void Dispose()
            {
                DisposeCalled = true;
            }

            #endregion
        }

        #endregion
    }
}