// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Ionic.Zip;

namespace Zel
{
    /// <summary>
    ///     Zip  class
    /// </summary>
    public static class Zip
    {
        #region UnZip Methods

        /// <summary>
        ///     UnZips the specified stream
        /// </summary>
        /// <param name="stream">Zip stream to unzip</param>
        /// <param name="entryName">Unique name to identify the entry to unzip</param>
        /// <param name="password">Zip password</param>
        /// <returns>Unzipped stream</returns>
        public static MemoryStream UnZip(Stream stream, string entryName, string password = null)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (string.IsNullOrWhiteSpace(entryName))
            {
                throw new ArgumentException("Invalid entry name", "entryName");
            }

            var memoryStream = new MemoryStream();

            //set memory stream position to 0, inorder to read from the beginning 
            stream.Position = 0;

            using (var zipFiles = ZipFile.Read(stream))
            {
                if (!zipFiles.ContainsEntry(entryName))
                {
                    throw new InvalidOperationException("Entry doesn't exist");
                }
                var zipEntry = zipFiles[entryName];
                if (password != null)
                {
                    zipEntry.Password = password;
                }
                zipEntry.Extract(memoryStream);
                return memoryStream;
            }
        }

        #endregion

        #region Zip Methods

        /// <summary>
        ///     Zip the specified stream into a memory stream
        /// </summary>
        /// <param name="byteArray">Byte array to zip</param>
        /// <param name="entryName">Unique name to identify the specified byte array in the zip</param>
        /// <param name="password">Zip password</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Zipped stream</returns>
        public static MemoryStream ZipByteArray(byte[] byteArray, string entryName, string password = null)
        {
            if (byteArray == null)
            {
                //byteArray is required 
                throw new ArgumentNullException("byteArray");
            }

            if (entryName == null)
            {
                //entry name is required 
                throw new ArgumentNullException("entryName");
            }

            var memoryStream = new MemoryStream();

            using (var zipFile = new ZipFile())
            {
                if (password != null)
                {
                    zipFile.Password = password;
                    zipFile.Encryption = EncryptionAlgorithm.WinZipAes256;
                }
                zipFile.AddEntry(entryName, byteArray);
                zipFile.Save(memoryStream);
            }

            return memoryStream;
        }


        /// <summary>
        ///     Zip the specified stream into a memory stream
        /// </summary>
        /// <param name="stream">Stream to zip</param>
        /// <param name="entryName">Unique name to identify the specified stream in the zip</param>
        /// <param name="password">Zip password</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Zipped stream</returns>
        public static MemoryStream ZipStream(Stream stream, string entryName, string password = null)
        {
            if (stream == null)
            {
                //stream is required 
                throw new ArgumentNullException("stream");
            }

            if (entryName == null)
            {
                //entry name is required 
                throw new ArgumentNullException("entryName");
            }

            var memoryStream = new MemoryStream();

            using (var zipFile = new ZipFile())
            {
                if (password != null)
                {
                    zipFile.Password = password;
                    zipFile.Encryption = EncryptionAlgorithm.WinZipAes256;
                }
                zipFile.AddEntry(entryName, stream);
                zipFile.Save(memoryStream);
            }

            return memoryStream;
        }


        /// <summary>
        ///     Zip the specified string into a memory stream
        /// </summary>
        /// <param name="stringToZip">String to zip</param>
        /// <param name="entryName">Unique name to identify the specified string in the zip</param>
        /// <param name="password">Zip password</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Zipped stream</returns>
        public static MemoryStream ZipString(string stringToZip, string entryName, string password = null)
        {
            if (stringToZip == null)
            {
                //stringToZip is required 
                throw new ArgumentNullException("stringToZip");
            }

            if (entryName == null)
            {
                //entry name is required 
                throw new ArgumentNullException("entryName");
            }

            var memoryStream = new MemoryStream();
            using (var zipFile = new ZipFile())
            {
                if (password != null)
                {
                    zipFile.Password = password;
                    zipFile.Encryption = EncryptionAlgorithm.WinZipAes256;
                }
                zipFile.AddEntry(entryName, stringToZip);
                zipFile.Save(memoryStream);
            }

            return memoryStream;
        }

        #endregion
    }
}