// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zel.DataAccess.Entity;

namespace Zel.DataAccess.Tests.Entity
{
    [TestClass]
    public class DeleteConfirmationMessageAttributeTests
    {
        #region Constructor Tests

        [TestMethod]
        public void Constructor_Sets_ConfirmationMessage_Property()
        {
            const string confirmationMessage = "msg";
            var deleteConfirmationMessageAttribute = new DeleteConfirmationAttribute(confirmationMessage);

            Assert.AreEqual(confirmationMessage, deleteConfirmationMessageAttribute.ConfirmationMessage);
        }

        #endregion
    }
}