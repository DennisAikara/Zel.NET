// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Zel.DataAccess.ServiceClasses
{
    public interface ILargeDataChunkRepository
    {
        bool Exist(LargeDataChunkIdentifier identifier);
        byte[] Get(LargeDataChunkIdentifier identifier);
        void Delete(LargeDataChunkIdentifier identifier);
        void Create(LargeDataChunkIdentifier identifier, byte[] content);
    }
}