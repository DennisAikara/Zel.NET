// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Zel.Classes;

namespace Zel.DataAccess.ServiceClasses
{
    public interface IDataService
    {
        Result<bool> Exists(string type, string name);
        Result<List<string>> GetNames(string type, int skip = 0, int count = 1000);
        Result<byte[]> GetChunk(string chunkIdentifier);
        DataResult<T> Get<T>(long dataId);
        DataResult<T> Get<T>(string type, string name);
        ValidationList Store(long dataId, object obj, DataStorageOption options = null);
        Result<long> Store(string type, string name, object obj, DataStorageOption options = null);
        ValidationList Delete(long dataId);
        ValidationList Delete(string type, string name);
    }
}