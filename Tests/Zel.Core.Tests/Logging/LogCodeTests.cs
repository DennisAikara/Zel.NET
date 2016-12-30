// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zel.Classes;

namespace Zel.Tests.Logging
{
    [TestClass]
    public class LogCodeTests
    {
        [TestMethod]
        public void Constructor_Sets_Code()
        {
            var logCode = new LogCode("82948");
            Assert.AreEqual("82948", logCode.Code);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Throws_ArgumentNullException_When_Code_Is_Null()
        {
            try
            {
                new LogCode(null);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("code", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_Throws_ArgumentException_When_Code_Is_Less_Than_Five_Characters_Long()
        {
            try
            {
                new LogCode("test");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("code", ex.ParamName);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_Throws_ArgumentException_When_Code_Is_Longer_Than_Five_Characters_Long()
        {
            try
            {
                new LogCode("test44");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("code", ex.ParamName);
                throw;
            }
        }
    }
}