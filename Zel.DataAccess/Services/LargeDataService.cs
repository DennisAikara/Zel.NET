// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Zel.Classes;
using Zel.DataAccess.DataEntities;
using Zel.DataAccess.ServiceClasses;

namespace Zel.DataAccess.Services
{
    public class LargeDataService : ILargeDataService
    {
        public const int CHUNK_SIZE = 16*1024*1024;

        private static short _currentFolder;
        private static int _getCount;
        private static readonly object CurrentFolderLock = new object();
        private readonly ICloudStorageService _cloudStorageService;
        private readonly IDataSession _dataSession;
        private readonly ILargeDataChunkRepository _largeDataChunkRepository;
        private readonly ILogger _logger;

        #region Constructors

        public LargeDataService(ICloudStorageService cloudStorageService,
            ILargeDataChunkRepository largeDataChunkRepository, IDataSession dataSession, ILogger logger)
        {
            if (cloudStorageService == null)
            {
                throw new ArgumentNullException("cloudStorageService");
            }
            if (largeDataChunkRepository == null)
            {
                throw new ArgumentNullException("largeDataChunkRepository");
            }
            if (dataSession == null)
            {
                throw new ArgumentNullException("dataSession");
            }
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            _cloudStorageService = cloudStorageService;
            _largeDataChunkRepository = largeDataChunkRepository;
            _dataSession = dataSession;
            _logger = logger;
        }

        #endregion

        private static short GetNextFolder()
        {
            lock (CurrentFolderLock)
            {
                if ((_currentFolder == 0) || (_getCount == 1000))
                {
                    _currentFolder = (short) new Random().Next(10000, 20000);
                    _getCount = 0;
                }

                _getCount++;
                return _currentFolder;
            }
        }

        #region ILargeDataService Members

        public bool Exists(int id)
        {
            return _dataSession.Query<LargeData>().Any(x => x.LargeDataId == id);
        }

        public bool IsBig(int id)
        {
            return _dataSession.Query<LargeDataChunk>().Count(x => x.LargeDataId == id) > 4;
        }

        public byte[] GetHash(int id)
        {
            var hash = (from ld in _dataSession.Query<LargeData>()
                join h in _dataSession.Query<Hash>()
                on ld.HashId equals h.HashId
                where ld.LargeDataId == id
                select h.Code).FirstOrDefault();

            if (hash == null)
            {
                throw new Exception("The specified data doesn't exist.");
            }

            return hash;
        }

        public long GetSize(int id)
        {
            var largeData = _dataSession.Get<LargeData>(id);
            if (largeData == null)
            {
                throw new Exception("The specified data doesn't exist.");
            }

            return largeData.Size;
        }

        public List<string> GetChunkList(int id)
        {
            var hashes = (from ldc in _dataSession.Query<LargeDataChunk>()
                join ldcf in _dataSession.Query<LargeDataChunkFile>()
                on ldc.LargeDataChunkFileId equals ldcf.LargeDataChunkFileId
                join h in _dataSession.Query<Hash>()
                on ldcf.HashId equals h.HashId
                where ldc.LargeDataId == id
                orderby ldc.Position
                select h.Code).ToList();

            return new List<string>(hashes.Select(x => BitConverter.ToString(x)));
        }


        public Result<byte[]> GetChunk(string chunkId)
        {
            if (chunkId == null)
            {
                throw new ArgumentNullException("chunkId");
            }

            var chunkIdSplit = chunkId.Split('-');
            if (chunkIdSplit.Length != 16)
            {
                throw new Exception("Invalid chunk identifier format.");
            }

            var hash = chunkIdSplit.Select(x => byte.Parse(x, NumberStyles.HexNumber)).ToArray();

            var chunkDetails = (from h in _dataSession.Query<Hash>()
                join ldcf in _dataSession.Query<LargeDataChunkFile>()
                on h.HashId equals ldcf.HashId
                join ldc in _dataSession.Query<LargeDataChunk>()
                on ldcf.LargeDataChunkFileId equals ldc.LargeDataChunkFileId
                where h.Code == hash
                orderby ldcf.SyncIdentifier descending
                select new
                {
                    ldcf.Folder,
                    ldcf.SyncIdentifier,
                    ldcf.SyncAccount
                }).FirstOrDefault();

            if (chunkDetails == null)
            {
                throw new Exception("Data chunk doesnot exist.");
            }

            if (chunkDetails.Folder != null)
            {
                var chunkData =
                    _largeDataChunkRepository.Get(new LargeDataChunkIdentifier(hash, chunkDetails.Folder.Value));
                if ((chunkData == null) && (chunkDetails.SyncIdentifier == null))
                {
                    return new Result<byte[]>(new ValidationList("Data chunk doesnot exist."));
                }

                if (chunkData != null)
                {
                    return new Result<byte[]>(chunkData);
                }
            }


            var getFileResult = _cloudStorageService.GetFile(new CloudFileIdentifier
            {
                FileId = chunkDetails.SyncIdentifier,
                AccountId = chunkDetails.SyncAccount
            });
            return !getFileResult.IsValid
                ? new Result<byte[]>(getFileResult.ValidationList)
                : new Result<byte[]>(getFileResult.Value);
        }

        public Result<string> GetString(int id)
        {
            var getBytesResult = GetBytes(id);
            return !getBytesResult.IsValid
                ? new Result<string>(getBytesResult.ValidationList)
                : new Result<string>(Encoding.UTF8.GetString(getBytesResult.Value));
        }

        public Result<byte[]> GetBytes(int id)
        {
            var hash = (from ld in _dataSession.Query<LargeData>()
                join h in _dataSession.Query<Hash>()
                on ld.HashId equals h.HashId
                where ld.LargeDataId == id
                select h.Code).FirstOrDefault();
            if (hash == null)
            {
                throw new Exception("The specified data id doesn't exist.");
            }

            var chunks = GetChunkList(id);
            if (chunks.Count == 0)
            {
                return new Result<byte[]>(new ValidationList("The specified data doesn't have any chunks."));
            }

            if (chunks.Count > 4)
            {
                return new Result<byte[]>(new ValidationList("The specified data is too big."));
            }

            var data = new MemoryStream();

            foreach (var chunkIdentifier in chunks)
            {
                var result = GetChunk(chunkIdentifier);
                if (!result.IsValid)
                {
                    return new Result<byte[]>(result.ValidationList);
                }
                data.Write(result.Value, 0, result.Value.Length);
            }

            data.Position = 0;

            return hash.SequenceEqual(data.GetMd5Hash())
                ? new Result<byte[]>(data.ToArray())
                : new Result<byte[]>(new ValidationList("Data doesn't match hash."));
        }

        public Result<int> Save(string data, LargeDataStorageOption options = null)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return Save(Encoding.UTF8.GetBytes(data), options);
        }

        public Result<int> Save(byte[] bytes, LargeDataStorageOption options = null)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            using (var stream = new MemoryStream(bytes))
            {
                return Save(stream, options);
            }
        }

        private class LargeDataChunkDetail
        {
            public LargeDataChunk LargeDataChunk { get; set; }
            public short? Folder { get; set; }
            public byte[] ChunkHash { get; set; }
        }

        public Result<int> Save(Stream stream, LargeDataStorageOption options = null)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            var syncPriority = SyncPriority.VeryLow;

            if (options != null)
            {
                syncPriority = options.SyncPriority;
            }

            var dataHash = stream.GetMd5Hash();

            long largeDataSize = 0;

            var largeDataChunkDetails = new List<LargeDataChunkDetail>();

            var existing = false;

            var existingLargeData = (from ld in _dataSession.Query<LargeData>()
                join h in _dataSession.Query<Hash>()
                on ld.HashId equals h.HashId
                where h.Code == dataHash
                select ld).FirstOrDefault();
            if (existingLargeData != null)
            {
                var existingLargeDataChunks = _dataSession.Query<LargeDataChunk>()
                    .Where(x => x.LargeDataId == existingLargeData.LargeDataId).ToList();
                if (existingLargeDataChunks.Count > 0)
                {
                    largeDataSize = existingLargeData.Size;
                    existing = true;
                    largeDataChunkDetails.AddRange(existingLargeDataChunks.Select(x => new LargeDataChunkDetail
                    {
                        LargeDataChunk = new LargeDataChunk
                        {
                            Position = x.Position,
                            LargeDataChunkFileId = x.LargeDataChunkFileId
                        }
                    }));
                }
            }

            if (!existing)
            {
                largeDataSize = stream.Length;

                var totalChunks = Math.Ceiling(largeDataSize/(double) CHUNK_SIZE);
                long totalBytesRead = 0;
                //split the stream into 16MB files
                while (true)
                {
                    byte[] chunkBytes;
                    if ((totalChunks > 1) && (largeDataChunkDetails.Count < totalChunks - 1))
                    {
                        chunkBytes = new byte[CHUNK_SIZE];
                    }
                    else
                    {
                        chunkBytes = new byte[largeDataSize - totalBytesRead];
                    }

                    var bytesRead = stream.Read(chunkBytes, 0, chunkBytes.Length);
                    if (bytesRead > 0)
                    {
                        var chunkHash = chunkBytes.GetMd5Hash();

                        var largeDataChunkIdentifier = new LargeDataChunkIdentifier(chunkHash, GetNextFolder());
                        _largeDataChunkRepository.Create(largeDataChunkIdentifier, chunkBytes);

                        var largeDataChunkDetail = new LargeDataChunkDetail
                        {
                            LargeDataChunk = new LargeDataChunk
                            {
                                Position = (short) largeDataChunkDetails.Count
                            },
                            Folder = largeDataChunkIdentifier.Folder
                            ,
                            ChunkHash = chunkHash
                        };

                        largeDataChunkDetails.Add(largeDataChunkDetail);
                    }

                    totalBytesRead += bytesRead;

                    if (bytesRead < CHUNK_SIZE)
                    {
                        break;
                    }
                }
            }


            using (var transaction = _dataSession.BeginTransaction())
            {
                var hashId = GetHashId(dataHash);
                if (!hashId.IsValid)
                {
                    return new Result<int>(hashId.ValidationList);
                }

                var largeData = new LargeData
                {
                    HashId = hashId.Value,
                    Size = largeDataSize,
                    SyncPriority = (byte) syncPriority
                };

                var validationList = _dataSession.Save(largeData);
                if (!validationList.IsValid)
                {
                    _logger.LogCritical("Failed to create Large Data.",
                        new LogData(largeData));
                    return new Result<int>(validationList);
                }

                foreach (var largeDataChunkDetail in largeDataChunkDetails)
                {
                    var largeDataChunk = largeDataChunkDetail.LargeDataChunk;
                    if (largeDataChunkDetail.Folder > 0)
                    {
                        var chunkHashId = GetHashId(largeDataChunkDetail.ChunkHash);
                        if (!chunkHashId.IsValid)
                        {
                            return new Result<int>(chunkHashId.ValidationList);
                        }

                        var largeDataChunkFileId = GetLargeDataChunkFileId(chunkHashId.Value,
                            largeDataChunkDetail.Folder.Value);
                        if (!largeDataChunkFileId.IsValid)
                        {
                            return new Result<int>(largeDataChunkFileId.ValidationList);
                        }

                        largeDataChunk.LargeDataChunkFileId = largeDataChunkFileId.Value;
                    }

                    largeDataChunk.LargeDataId = largeData.LargeDataId;
                    validationList = _dataSession.Save(largeDataChunk);
                    if (validationList.IsValid)
                    {
                        continue;
                    }
                    _logger.LogCritical("Failed to create Large Data Chunk.",
                        new LogData(largeDataChunk));
                    return new Result<int>(validationList);
                }

                transaction.Commit();
                return new Result<int>(largeData.LargeDataId);
            }
        }

        private Result<int> GetLargeDataChunkFileId(int hashId, short folder)
        {
            var largeDataChunkFile = _dataSession.Query<LargeDataChunkFile>().FirstOrDefault(x => x.HashId == hashId);
            if (largeDataChunkFile == null)
            {
                largeDataChunkFile = new LargeDataChunkFile
                {
                    HashId = hashId,
                    Folder = folder
                };
                var validationList = _dataSession.Save(largeDataChunkFile);
                if (!validationList.IsValid)
                {
                    _logger.LogCritical("Failed to create LargeDataChunkFile.", new LogData(largeDataChunkFile));
                    return new Result<int>(validationList);
                }
            }
            return new Result<int>(largeDataChunkFile.LargeDataChunkFileId);
        }

        private Result<int> GetHashId(byte[] md5CheckSum)
        {
            var hash = _dataSession.Query<Hash>().FirstOrDefault(x => x.Code == md5CheckSum);
            if (hash == null)
            {
                hash = new Hash
                {
                    Code = md5CheckSum
                };
                var validationList = _dataSession.Save(hash);
                if (!validationList.IsValid)
                {
                    _logger.LogCritical("Failed to create Hash.", new LogData(hash));
                    return new Result<int>(validationList);
                }
            }
            return new Result<int>(hash.HashId);
        }

        public ValidationList Delete(int id)
        {
            var largeData = _dataSession.Get<LargeData>(id);
            if (largeData == null)
            {
                throw new Exception("The specified data doesn't exist.");
            }

            var largeDataChunks = _dataSession.Query<LargeDataChunk>().Where(x => x.LargeDataId == id).ToList();

            using (var transaction = _dataSession.BeginTransaction())
            {
                ValidationList validationList;
                foreach (var largeDataChunk in largeDataChunks)
                {
                    validationList = _dataSession.Delete(largeDataChunk);
                    if (!validationList.IsValid)
                    {
                        return validationList;
                    }
                }

                validationList = _dataSession.Delete(largeData);

                if (!validationList.IsValid)
                {
                    return validationList;
                }

                transaction.Commit();

                return new ValidationList();
            }
        }

        #endregion
    }
}