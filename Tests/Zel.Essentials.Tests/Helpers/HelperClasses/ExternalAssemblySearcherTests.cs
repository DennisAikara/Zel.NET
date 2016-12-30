// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zel.Helpers.HelperClasses;

namespace Zel.Tests.Helpers.HelperClasses
{
    [TestClass]
    public class ExternalAssemblySearcherTests
    {
        #region FindClassesInAssemblyThatImplementInterface Tests

        [TestMethod]
        public void FindClassesInAssemblyThatImplementInterface_Returns_Assemblies()
        {
            var basePath = string.Join(Path.DirectorySeparatorChar.ToString(),
                Application.RootDirectory.Split(Path.DirectorySeparatorChar).TakeWhile(x => x != "Zel").ToList());
            var fileInfo =
                new FileInfo(Path.Combine(basePath,
                    @"Zel\DummyProjectsForTesting\ExternalAssembly\bin\Debug\ExternalAssembly.dll"));
            var assemblyName = AssemblyName.GetAssemblyName(fileInfo.FullName);

            var externalAssemblySearcher = new ExternalAssemblySearcher();
            var types = externalAssemblySearcher.FindClassesInAssemblyThatImplementInterface<IList<string>>(assemblyName);

            Assert.AreEqual("ExternalAssembly.ClassThatImplementInterface", types[0]);
        }

        #endregion
    }
}