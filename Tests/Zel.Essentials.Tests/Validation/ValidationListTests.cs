// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zel.Classes;

namespace Zel.Tests.Validation
{
    [TestClass]
    public class ValidationListTests
    {
        #region Add Tests

        [TestMethod]
        public void AddError_Adds_Error()
        {
            const string key = "key";
            const string errorMessage = "errorMessage";

            var validationList = new ValidationList();
            validationList.Add(key, errorMessage);

            Assert.AreEqual(key, validationList[0].Name);
            Assert.AreEqual(errorMessage, validationList[0].Message);
        }

        #endregion

        #region IsValid Tests

        [TestMethod]
        public void IsValid_Returns_True_If_ValidationList_DoesNot_Have_Errors()
        {
            var validationList = new ValidationList();

            Assert.IsTrue(validationList.IsValid);
        }

        [TestMethod]
        public void IsValid_Returns_False_If_ValidationList_Has_Errors()
        {
            var validationList = new ValidationList();
            validationList.Add("key", "error");

            Assert.IsFalse(validationList.IsValid);
        }

        #endregion
    }
}