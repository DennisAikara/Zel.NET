// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Linq;

namespace Zel.Helpers
{
    public static class FileSystemHelper
    {
        #region Methods

        /// <summary>
        ///     Copies all the files and sub-directories from the specified source directory to the specified target directory
        /// </summary>
        /// <param name="sourceDirectory">Source directory</param>
        /// <param name="targetDirectory">Target directory</param>
        public static void CopyDirectory(string sourceDirectory, string targetDirectory)
        {
            //Create directories
            foreach (var dirPath in
                Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourceDirectory, targetDirectory));
            }

            //Copy all the files
            foreach (var newPath in
                Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourceDirectory, targetDirectory));
            }
        }

        /// <summary>
        ///     Removes all the files and directories from the specified directory
        /// </summary>
        /// <param name="directory">Directory</param>
        public static void EmptyDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                //delete all the files
                foreach (var file in
                    Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
                {
                    File.Delete(file);
                }

                //delete all the sub directories
                foreach (var dir in
                    Directory.GetDirectories(directory, "*", SearchOption.AllDirectories)
                        .OrderByDescending(x => x.Length))
                {
                    Directory.Delete(dir);
                }
            }
        }

        #endregion
    }
}