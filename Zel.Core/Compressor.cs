// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.IO.Compression;
using Zel.Classes;

namespace Zel
{
    public class Compressor : ICompressor
    {
        #region ICompressor Members

        public byte[] GZip(byte[] data)
        {
            using (var inputStream = new MemoryStream(data))
            {
                using (var outputStream = new MemoryStream())
                {
                    using (var gZipStream = new GZipStream(outputStream, CompressionMode.Compress))
                    {
                        inputStream.CopyTo(gZipStream);
                    }
                    return outputStream.ToArray();
                }
            }
        }

        public byte[] UnGZip(byte[] data)
        {
            using (var inputStream = new MemoryStream(data))
            {
                using (var outputStream = new MemoryStream())
                {
                    using (var gZipStream = new GZipStream(inputStream, CompressionMode.Decompress))
                    {
                        gZipStream.CopyTo(outputStream);
                    }

                    return outputStream.ToArray();
                }
            }
        }

        #endregion
    }
}