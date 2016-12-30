// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using Zel.Classes;

namespace Zel.DataAccess.ServiceClasses
{
    public interface ILargeDataService
    {
        bool Exists(int id);
        bool IsBig(int id);
        byte[] GetHash(int id);
        long GetSize(int id);
        List<string> GetChunkList(int id);
        Result<byte[]> GetChunk(string chunkId);
        Result<string> GetString(int id);
        Result<byte[]> GetBytes(int id);
        Result<int> Save(string data, LargeDataStorageOption options = null);
        Result<int> Save(byte[] bytes, LargeDataStorageOption options = null);
        Result<int> Save(Stream stream, LargeDataStorageOption options = null);
        ValidationList Delete(int id);
    }
}