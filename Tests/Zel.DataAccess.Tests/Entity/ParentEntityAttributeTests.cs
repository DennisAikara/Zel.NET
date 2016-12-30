// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zel.DataAccess.Entity;
using Zel.DataAccess.Exceptions;
using Zel.DataAccess.Tests.TestHelpers.Zel;

namespace Zel.DataAccess.Tests.Entity
{
    [TestClass]
    public class ParentEntityAttributeTests
    {
        #region Constuctor Tests

        [TestMethod]
        [ExpectedException(typeof(InvalidParentEntityException))]
        public void Constructor_Throws_InvalidParentEntityException_If_RelatedEntity_Is_Not_IEntity()
        {
            const string errorMessage = "errm";
            var parentEntity = typeof(string);

            new ParentEntityAttribute(parentEntity, errorMessage);
        }

        [TestMethod]
        public void Constructor_Sets_ErrorMessage_And_ParentEntity_Properties()
        {
            const string errorMessage = "errm";
            var parentEntity = typeof(Employee);

            var parentEntityAttribute = new ParentEntityAttribute(parentEntity, errorMessage);

            Assert.AreEqual(errorMessage, parentEntityAttribute.ErrorMessage);
            Assert.AreEqual(parentEntity, parentEntityAttribute.ParentEntity);
        }

        #endregion
    }
}