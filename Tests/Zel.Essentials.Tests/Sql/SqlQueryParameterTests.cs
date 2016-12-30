// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zel.Classes;

namespace Zel.Tests.Sql
{
    [TestClass]
    public class SqlQueryParameterTests
    {
        #region Constructor Tests

        [TestMethod]
        public void Constructor_Sets_Properties()
        {
            const string name = "name";
            const string value = "value";

            var sqlQueryParameter = new DatabaseCommandParameter(name, value);

            Assert.AreEqual(name, sqlQueryParameter.ParameterName);
            Assert.AreEqual(value, sqlQueryParameter.ParameterValue);
        }

        #endregion
    }
}