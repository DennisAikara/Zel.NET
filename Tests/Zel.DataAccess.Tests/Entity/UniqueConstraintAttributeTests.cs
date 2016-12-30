// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zel.DataAccess.Entity;

namespace Zel.DataAccess.Tests.Entity
{
    [TestClass]
    public class UniqueConstraintAttributeTests
    {
        #region Constructor Tests

        [TestMethod]
        public void Constructor_Sets_ErrorMessage_Fields_And_Name_Properties()
        {
            var columns = new[] {"col1", "col2"};
            const string name = "fsfsfsfas";
            var uniqueConstraintAttribute = new UniqueConstraintAttribute(name, columns);

            Assert.AreEqual(columns, uniqueConstraintAttribute.Fields);
            Assert.AreEqual(name, uniqueConstraintAttribute.Name);
        }

        #endregion
    }
}