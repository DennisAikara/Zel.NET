// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zel.Helpers;

namespace Zel.Tests.Helpers
{
    [TestClass]
    public class ConfigurationHelperTests
    {
        #region GetConnectionString Tests

        [TestMethod]
        public void GetConnectionString_Returns_ConnectionString_When_ConnectionString_Exists()
        {
            Assert.AreEqual("haha", ConfigurationHelper.GetConnectionString("test"));
        }

        [TestMethod]
        public void GetConnectionString_Returns_NullWhen_ConnectionString_DoesNot_Exists()
        {
            Assert.IsNull(ConfigurationHelper.GetConnectionString("dummy"));
        }

        #endregion

        #region GetConnectionStringProvider Tests

        [TestMethod]
        public void GetConnectionStringProvider_Returns_ConnectionString_When_ConnectionString_Exists()
        {
            Assert.AreEqual("3434", ConfigurationHelper.GetConnectionStringProvider("test"));
        }

        [TestMethod]
        public void GetConnectionStringProvider_Returns_NullWhen_ConnectionString_DoesNot_Exists()
        {
            Assert.IsNull(ConfigurationHelper.GetConnectionStringProvider("dummy"));
        }

        #endregion
    }
}