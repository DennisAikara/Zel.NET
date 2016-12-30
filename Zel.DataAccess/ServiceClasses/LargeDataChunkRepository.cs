// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace Zel.DataAccess.ServiceClasses
{
    public class LargeDataChunkRepository : ILargeDataChunkRepository
    {
        private readonly string _largeDataDirectory;

        public LargeDataChunkRepository(string largeDataDirectory)
        {
            if (largeDataDirectory == null)
            {
                throw new ArgumentNullException("largeDataDirectory");
            }

            if (!Directory.Exists(largeDataDirectory))
            {
                throw new DirectoryNotFoundException("Large data directory does't exist. " + largeDataDirectory);
            }
            _largeDataDirectory = largeDataDirectory;
        }

        #region ILargeDataChunkRepository Members

        public bool Exist(LargeDataChunkIdentifier identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }
            return
                File.Exists(Path.Combine(_largeDataDirectory, identifier.Folder.ToString(), identifier.Identifier));
        }

        public byte[] Get(LargeDataChunkIdentifier identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }
            return Exist(identifier)
                ? File.ReadAllBytes(Path.Combine(_largeDataDirectory, identifier.Folder.ToString(),
                    identifier.Identifier))
                : null;
        }

        public void Delete(LargeDataChunkIdentifier identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }

            if (Exist(identifier))
            {
                File.Delete(Path.Combine(_largeDataDirectory, identifier.Folder.ToString(), identifier.Identifier));
            }
        }

        public void Create(LargeDataChunkIdentifier identifier, byte[] content)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }
            if (Exist(identifier))
            {
                return;
            }
            File.WriteAllBytes(
                Path.Combine(_largeDataDirectory, identifier.Folder.ToString(), identifier.Identifier), content);
        }

        #endregion
    }
}