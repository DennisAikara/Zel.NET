// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zel.Classes;

namespace Zel.Tests.Encryption
{
    [TestClass]
    public class AesKeySizeTests
    {
        [TestMethod]
        public void AesKeySizeHasValidValues()
        {
            Assert.AreEqual(2, Enum.GetNames(typeof(AesKeySize)).Length);
            Assert.AreEqual(128, (int) AesKeySize.OneTwentyEight);
            Assert.AreEqual(256, (int) AesKeySize.TwoFiftySix);
        }
    }
}