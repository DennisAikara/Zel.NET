// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Security.Principal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Zel.Tests
{
    [TestClass]
    public class ApplicationTests
    {
        [TestMethod]
        public void RootDirectory_Returns_ApplicationDirectory()
        {
            Assert.AreEqual(AppDomain.CurrentDomain.BaseDirectory, Application.RootDirectory);
        }

        [TestMethod]
        public void BinDirectory_Returns_Bin_Directory()
        {
            var binDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
            if (!Directory.Exists(binDirectory))
            {
                binDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }
            Assert.AreEqual(binDirectory, Application.BinDirectory);
        }

        [TestMethod]
        public void OutDirectory_Returns_Out_Directory()
        {
            Assert.AreEqual(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "out"), Application.OutDirectory);
        }

        [TestMethod]
        public void ApplicationUserName_Returns_Current_Users_Name()
        {
            string name = null;
            var windowsIdentity = WindowsIdentity.GetCurrent();
            if (windowsIdentity != null)
            {
                name = windowsIdentity.Name;
            }
            if (name == null)
            {
                name = Environment.UserName;
            }
            Assert.AreEqual(name, Application.ApplicationUserName);
        }

        [TestMethod]
        public void MachineName_Returns_Current_MachineName()
        {
            Assert.AreEqual(Environment.MachineName, Application.MachineName);
        }
    }
}