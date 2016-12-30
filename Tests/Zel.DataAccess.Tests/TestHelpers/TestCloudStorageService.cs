// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Zel.Classes;

namespace Zel.DataAccess.Tests.TestHelpers
{
    public class TestCloudStorageService : ICloudStorageService
    {
        private readonly string _folder = Path.Combine(Application.OutDirectory, "Out");

        #region ICloudStorageService Members

        public Result<CloudFileIdentifier> CreateFile(byte[] fileContents)
        {
            var name = Guid.NewGuid().ToString("N");
            File.WriteAllBytes(Path.Combine(_folder, name), fileContents);
            return new Result<CloudFileIdentifier>(new CloudFileIdentifier {FileId = name});
        }

        public Result<bool> DeleteFile(CloudFileIdentifier cloudFileIdentifier)
        {
            throw new NotImplementedException();
        }

        public Result<byte[]> GetFile(CloudFileIdentifier cloudFileIdentifier)
        {
            var path = Path.Combine(_folder, cloudFileIdentifier.FileId);
            if (File.Exists(path))
            {
                return new Result<byte[]>(File.ReadAllBytes(path));
            }
            return new Result<byte[]>(new ValidationList("The specified file doesn't exist in cloud storage."));
        }

        #endregion
    }
}