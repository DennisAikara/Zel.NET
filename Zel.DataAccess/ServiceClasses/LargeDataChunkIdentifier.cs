// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq;

namespace Zel.DataAccess.ServiceClasses
{
    public class LargeDataChunkIdentifier
    {
        public LargeDataChunkIdentifier(byte[] md5Hash, short? folder)
        {
            if (md5Hash == null)
            {
                throw new ArgumentNullException("md5Hash");
            }
            if (folder == null)
            {
                throw new ArgumentNullException("folder");
            }

            if (md5Hash.Length != 16)
            {
                throw new ArgumentException("Invalid MD5 Hash");
            }
            Hash = md5Hash;
            Folder = folder;
        }

        public LargeDataChunkIdentifier(string md5Hash, short? folder)
        {
            Folder = folder;
            if (md5Hash == null)
            {
                throw new ArgumentNullException("md5Hash");
            }
            if (folder == null)
            {
                throw new ArgumentNullException("folder");
            }

            var chunkIdSplit = md5Hash.Split('-');
            if (chunkIdSplit.Length != 16)
            {
                throw new Exception("Invalid MD5 Hash.");
            }

            Hash = chunkIdSplit.Select(x => byte.Parse(x, NumberStyles.HexNumber)).ToArray();
        }

        public byte[] Hash { get; }

        public string Identifier
        {
            get { return BitConverter.ToString(Hash); }
        }

        public short? Folder { get; }
    }
}