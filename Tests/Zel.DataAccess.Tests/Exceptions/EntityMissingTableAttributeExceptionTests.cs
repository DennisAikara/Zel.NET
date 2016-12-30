// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zel.DataAccess.Exceptions;
using Zel.DataAccess.Tests.TestHelpers.Zel;

namespace Zel.DataAccess.Tests.Exceptions
{
    [TestClass]
    public class EntityMissingTableAttributeExceptionTests
    {
        [TestMethod]
        public void Constructor_Sets_EntityType()
        {
            var entityMissingTableAttributeException = new EntityMissingTableAttributeException(typeof(Employee));
            Assert.AreEqual(typeof(Employee).FullName, entityMissingTableAttributeException.EntityType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_Throws_ArgumentException_When_EntityType_Is_Not_Entity()
        {
            try
            {
                new EntityMissingTableAttributeException(typeof(string));
            }
            catch (Exception ex)
            {
                var message = string.Concat("Cannot create EntityMissingTableAttributeException. ",
                    typeof(string).FullName,
                    " is not an entity.");
                Assert.AreEqual(message, ex.Message);
                throw;
            }
        }
    }
}