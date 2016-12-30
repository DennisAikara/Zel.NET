// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zel.Classes;

namespace Zel.Tests.Sql
{
    [TestClass]
    public class SqlQueryTests
    {
        #region Constructor Tests

        [TestMethod]
        public void Default_Constructor_Instantiate_Parameters_List()
        {
            var sqlQuery = new DatabaseCommand();

            Assert.IsNotNull(sqlQuery.Parameters);
        }

        [TestMethod]
        public void Constructor_Instantiate_Parameters_And_Set_Query()
        {
            const string query = "command";
            var sqlQuery = new DatabaseCommand(query);

            Assert.IsNotNull(sqlQuery.Parameters);
            Assert.AreEqual(query, sqlQuery.Command);
        }

        #endregion
    }
}