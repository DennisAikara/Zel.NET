// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zel.Helpers;

namespace Zel.Tests.Helpers
{
    [TestClass]
    public class FileSystemHelperTests
    {
        #region CopyDirectory Tests

        [TestMethod]
        public void CopyDirectory_Copies_Directory_And_Files_From_Source_To_TargetDirectory()
        {
            var sourceDir = Path.Combine(Application.RootDirectory, "FileHelper", "Source");
            var targetDir = Path.Combine(Application.RootDirectory, "FileHelper", "Target");

            if (Directory.Exists(sourceDir))
            {
                FileSystemHelper.EmptyDirectory(sourceDir);
                Directory.Delete(sourceDir);
            }
            Directory.CreateDirectory(sourceDir);
            Directory.CreateDirectory(Path.Combine(sourceDir, "sub1"));
            Directory.CreateDirectory(Path.Combine(sourceDir, "sub2"));
            Directory.CreateDirectory(Path.Combine(sourceDir, "sub2", "sub2sub1"));

            if (Directory.Exists(targetDir))
            {
                FileSystemHelper.EmptyDirectory(targetDir);
                Directory.Delete(targetDir);
            }
            Directory.CreateDirectory(targetDir);

            File.WriteAllText(Path.Combine(sourceDir, "source.txt"), "");
            File.WriteAllText(Path.Combine(sourceDir, "sub1", "sub1.txt"), "");
            File.WriteAllText(Path.Combine(sourceDir, "sub2", "sub2sub1", "sub2sub1.txt"), "");

            FileSystemHelper.CopyDirectory(sourceDir, targetDir);

            Assert.IsTrue(Directory.Exists(Path.Combine(targetDir, "sub1")));
            Assert.IsTrue(Directory.Exists(Path.Combine(targetDir, "sub2")));
            Assert.IsTrue(Directory.Exists(Path.Combine(targetDir, "sub2", "sub2sub1")));
            Assert.IsTrue(File.Exists(Path.Combine(targetDir, "source.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(targetDir, "sub1", "sub1.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(targetDir, "sub2", "sub2sub1", "sub2sub1.txt")));
        }

        #endregion

        #region EmptyDirectory Tests

        [TestMethod]
        public void EmptyDirectory_Deletes_All_SubDirectories_And_Files()
        {
            var sourceDir = Path.Combine(Application.RootDirectory, "FileHelper", "Source");
            if (Directory.Exists(sourceDir))
            {
                FileSystemHelper.EmptyDirectory(sourceDir);
                Directory.Delete(sourceDir);
            }
            Directory.CreateDirectory(sourceDir);
            Directory.CreateDirectory(Path.Combine(sourceDir, "sub1"));
            Directory.CreateDirectory(Path.Combine(sourceDir, "sub2"));
            Directory.CreateDirectory(Path.Combine(sourceDir, "sub2", "sub2sub1"));
            File.WriteAllText(Path.Combine(sourceDir, "source.txt"), "");
            File.WriteAllText(Path.Combine(sourceDir, "sub1", "sub1.txt"), "");
            File.WriteAllText(Path.Combine(sourceDir, "sub2", "sub2sub1", "sub2sub1.txt"), "");

            FileSystemHelper.EmptyDirectory(sourceDir);

            Assert.IsTrue(Directory.Exists(sourceDir));

            var directoryInfo = new DirectoryInfo(sourceDir);
            Assert.AreEqual(0, directoryInfo.GetFiles().Length);
            Assert.AreEqual(0, directoryInfo.GetDirectories().Length);
        }

        #endregion
    }
}