// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Zel.Classes
{
    public interface ICloudStorageService
    {
        Result<CloudFileIdentifier> CreateFile(byte[] fileContents);
        Result<bool> DeleteFile(CloudFileIdentifier cloudFileIdentifier);
        Result<byte[]> GetFile(CloudFileIdentifier cloudFileIdentifier);
    }
}