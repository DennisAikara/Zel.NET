// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.IO;
using System.Linq;
using Zel.Classes;
using Zel.DataAccess.DataEntities;
using Zel.DataAccess.ServiceClasses;

namespace Zel.DataAccess.Services
{
    public class DataService : IDataService
    {
        private static readonly Dictionary<string, int> DataTypeIds;
        private readonly IDataSession _dataSession;
        private readonly ILargeDataService _largeDataService;
        private readonly ILogger _logger;

        static DataService()
        {
            DataTypeIds = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        }


        public DataService(ILargeDataService largeDataService, IDataSession dataSession, ILogger logger)
        {
            if (largeDataService == null)
            {
                throw new ArgumentNullException("largeDataService");
            }
            if (dataSession == null)
            {
                throw new ArgumentNullException("dataSession");
            }
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            _largeDataService = largeDataService;
            _dataSession = dataSession;
            _logger = logger;
        }

        #region Private Methods

        private Result<int> GetDataTypeId(string type, bool create = false)
        {
            if (DataTypeIds.ContainsKey(type))
            {
                return new Result<int>(DataTypeIds[type]);
            }

            var dataTypeId = _dataSession.Query<DataType>(1000).Where(x => x.Type == type)
                .Select(x => x.DataTypeId).FirstOrDefault();

            if ((dataTypeId != 0) || !create)
            {
                if (dataTypeId != 0)
                {
                    DataTypeIds[type] = dataTypeId;
                }
                return new Result<int>(dataTypeId);
            }

            //create type
            var dataType = new DataType
            {
                Type = type
            };

            var validationList = _dataSession.Save(dataType, 1000);
            if (!validationList.IsValid)
            {
                _logger.LogCritical("Failed to create data type.",
                    new LogData(dataType));
                return new Result<int>(validationList);
            }

            DataTypeIds[type] = dataType.DataTypeId;

            return new Result<int>(DataTypeIds[type]);
        }

        private static void ValidateTypeAndName(string type, string name)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (type.Length > 450)
            {
                throw new ArgumentException("Data type should not exceed 450 characters");
            }

            if (name.Length > 448)
            {
                throw new ArgumentException("Data name should not exceed 448 characters");
            }
        }

        private ValidationList Delete(Data data)
        {
            using (var transaction = _dataSession.BeginTransaction())
            {
                var delete = _dataSession.Delete(data);
                if (!delete.IsValid)
                {
                    return delete;
                }
                if (data.LargeDataId != null)
                {
                    var largeDataDelete = _largeDataService.Delete(data.LargeDataId.Value);
                    if (!largeDataDelete.IsValid)
                    {
                        return largeDataDelete;
                    }
                }
                transaction.Commit();
            }

            return new ValidationList();
        }

        private DataResult<T> Get<T>(Data data)
        {
            if (data.LargeDataId == null)
            {
                if (data.Value == null)
                {
                    return new DataResult<T>(default(T));
                }
                return typeof(T) == typeof(string)
                    ? new DataResult<T>((T) (object) data.Value)
                    : new DataResult<T>(data.Value.FromJson<T>());
            }


            if (_largeDataService.IsBig(data.LargeDataId.Value))
            {
                return new DataResult<T>(default(T))
                {
                    ChunkList = _largeDataService.GetChunkList(data.LargeDataId.Value)
                };
            }

            var type = typeof(T);
            if (type == typeof(byte[]))
            {
                var getBytesresult = _largeDataService.GetBytes(data.LargeDataId.Value);
                return getBytesresult.IsValid
                    ? new DataResult<T>((T) (object) getBytesresult.Value)
                    : new DataResult<T>(getBytesresult.ValidationList);
            }

            if (type == typeof(string))
            {
                var getStringResult = _largeDataService.GetString(data.LargeDataId.Value);
                return getStringResult.IsValid
                    ? new DataResult<T>((T) (object) getStringResult.Value)
                    : new DataResult<T>(getStringResult.ValidationList);
            }

            //unknown type
            var result = _largeDataService.GetString(data.LargeDataId.Value);
            return result.IsValid
                ? new DataResult<T>(result.Value.FromJson<T>())
                : new DataResult<T>(result.ValidationList);
        }

        private ValidationList Store(Data data, object obj, DataStorageOption dataStorageOption = null)
        {
            data.Value = null;

            if (obj != null)
            {
                if (obj is string)
                {
                    var stringData = obj.ToString();
                    if (stringData.Length <= 450)
                    {
                        data.Value = stringData;
                    }
                }
                else if (!(obj is byte[]) && !(obj is Stream))
                {
                    var json = obj.ToJson();
                    if (json.Length > LargeDataService.CHUNK_SIZE)
                    {
                        throw new Exception("Data objects should be less than " + LargeDataService.CHUNK_SIZE + 1
                                            + " when serialized.");
                    }
                    obj = json;
                    if (json.Length <= 450)
                    {
                        data.Value = json;
                    }
                }
            }

            using (var transaction = _dataSession.BeginTransaction())
            {
                ValidationList validationList;
                if (data.LargeDataId != null)
                {
                    var largeDataId = data.LargeDataId.Value;
                    data.LargeDataId = null;
                    validationList = _dataSession.Save(data);
                    if (!validationList.IsValid)
                    {
                        return validationList;
                    }

                    var delete = _largeDataService.Delete(largeDataId);
                    if (!delete.IsValid)
                    {
                        return delete;
                    }
                    data.LargeDataId = null;
                }

                if ((data.Value == null) && (obj != null))
                {
                    //large data
                    var largeDataStorageOption = new LargeDataStorageOption
                    {
                        SyncPriority = SyncPriority.VeryLow
                    };
                    if (dataStorageOption != null)
                    {
                        largeDataStorageOption.SyncPriority = dataStorageOption.SyncPriority;
                    }
                    Result<int> saveResult;
                    if (obj is string)
                    {
                        saveResult = _largeDataService.Save(obj as string, largeDataStorageOption);
                    }
                    else if (obj is byte[])
                    {
                        saveResult = _largeDataService.Save(obj as byte[], largeDataStorageOption);
                    }
                    else
                    {
                        saveResult = _largeDataService.Save(obj as Stream, largeDataStorageOption);
                    }

                    if (saveResult.IsValid)
                    {
                        data.LargeDataId = saveResult.Value;
                    }
                    else
                    {
                        return saveResult.ValidationList;
                    }
                }

                validationList = _dataSession.Save(data);
                if (validationList.IsValid)
                {
                    transaction.Commit();
                    return new ValidationList();
                }

                _logger.LogCritical("Failed to store data.",
                    new LogData(data));
                return validationList;
            }
        }

        #endregion

        #region IDataService Members

        public Result<bool> Exists(string type, string name)
        {
            ValidateTypeAndName(type, name);

            var dataTypeId = GetDataTypeId(type);
            if (!dataTypeId.IsValid)
            {
                return new Result<bool>(dataTypeId.ValidationList);
            }

            return dataTypeId.Value == 0
                ? new Result<bool>(false)
                : new Result<bool>(
                    _dataSession.Query<Data>(1000).Any(x => (x.DataTypeId == dataTypeId.Value) && (x.Name == name)));
        }

        public Result<List<string>> GetNames(string type, int skip = 0, int count = 1000)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            const string query = @"
                DECLARE @startAtId INT;
                DECLARE @dataTypeId INT;
                DECLARE @skipCount INT = {0};
                DECLARE @totalRowCount INT;
                DECLARE @takeCount INT = {1};

                SELECT
	                @dataTypeId = dt.[DataTypeId]
                FROM 
	                [dbo].[DataType] dt 
                WHERE
	                dt.[Type] = @Type;

                SELECT
	                @totalRowCount = COUNT(*)
                FROM 
	                [dbo].[Data] d
                WHERE
	                d.[DataTypeId] = @dataTypeId;

				IF @totalRowCount < @skipCount
				BEGIN
					SET @skipCount = @totalRowCount
					RETURN
				END
				ELSE
				BEGIN
					IF @skipCount + @takeCount >= @totalRowCount
					BEGIN
						SET @takeCount  = @totalRowCount - @skipCount + 1;
						IF @takeCount <= 0
						BEGIN
							RETURN
						END
					END
				END
           
                SET ROWCOUNT @skipCount

                SELECT 
	                @startAtId = d.[DataId] 
                FROM 
	                [dbo].[Data] AS d
                WHERE 
	                d.[DataTypeId] = @dataTypeId
                ORDER BY 
	                d.[DataId]

                SET ROWCOUNT 0

                SELECT TOP (@takeCount)
	                d.[Name]
                FROM 
	                [dbo].[Data] AS d
                WHERE 
	                d.[DataId] >= @startAtId
	                AND d.[DataTypeId] = @dataTypeId
                ORDER BY 
	                d.[DataId]";

            var databaseCommand = new DatabaseCommand(string.Format(query, skip + 1, count))
            {
                Timeout = 3000
            };
            databaseCommand.Parameters.Add(new DatabaseCommandParameter("@Type", type));

            var entityConnection = (EntityConnection) _dataSession.DataSessionContext
                .GetDataContext(DataSession.GetEntityDetail(typeof(Data)).ContextType).ObjectContext.Connection;

            var data = Sql.ExecuteQuery(databaseCommand, entityConnection.StoreConnection);

            return new Result<List<string>>(data == null
                ? new List<string>()
                : data.AsEnumerable()
                    .Select(x => x.Field<string>("Name")).ToList());
        }

        public Result<byte[]> GetChunk(string chunkIdentifier)
        {
            return _largeDataService.GetChunk(chunkIdentifier);
        }

        public DataResult<T> Get<T>(long dataId)
        {
            var data = _dataSession.Get<Data>(dataId);
            if (data == null)
            {
                throw new Exception("Data not found.");
            }

            return Get<T>(data);
        }

        public DataResult<T> Get<T>(string type, string name)
        {
            ValidateTypeAndName(type, name);

            var dataTypeId = GetDataTypeId(type);
            if (!dataTypeId.IsValid)
            {
                return new DataResult<T>(dataTypeId.ValidationList);
            }

            if (dataTypeId.Value == 0)
            {
                throw new Exception("Data type doesnot exist.");
            }

            var data =
                _dataSession.Query<Data>(1000)
                    .FirstOrDefault(x => (x.DataTypeId == dataTypeId.Value) && (x.Name == name));
            if (data == null)
            {
                throw new Exception("Data not found.");
            }

            return Get<T>(data);
        }

        public ValidationList Store(long dataId, object obj, DataStorageOption options = null)
        {
            var data = _dataSession.Get<Data>(dataId);
            if (data == null)
            {
                throw new Exception("Data not found.");
            }
            var validationList = Store(data, obj, options);
            return validationList.IsValid ? new ValidationList() : validationList;
        }

        public Result<long> Store(string type, string name, object obj, DataStorageOption options = null)
        {
            ValidateTypeAndName(type, name);

            //get data type id
            var dataTypeId = GetDataTypeId(type, true);
            if (!dataTypeId.IsValid)
            {
                return new Result<long>(dataTypeId.ValidationList);
            }

            var data = _dataSession.Query<Data>()
                           .FirstOrDefault(x => (x.Name == name) && (x.DataTypeId == dataTypeId.Value))
                       ?? new Data();
            data.DataTypeId = dataTypeId.Value;
            data.Name = name;

            var validationList = Store(data, obj, options);
            return validationList.IsValid ? new Result<long>(data.DataId) : new Result<long>(validationList);
        }

        public ValidationList Delete(long dataId)
        {
            var data = _dataSession.Get<Data>(dataId);
            if (data == null)
            {
                throw new Exception("Data not found.");
            }

            return Delete(data);
        }

        public ValidationList Delete(string type, string name)
        {
            ValidateTypeAndName(type, name);

            var dataTypeId = GetDataTypeId(type);
            if (!dataTypeId.IsValid)
            {
                return dataTypeId.ValidationList;
            }

            if (dataTypeId.Value == 0)
            {
                throw new Exception("Data type doesnot exist.");
            }

            var data =
                _dataSession.Query<Data>(1000)
                    .FirstOrDefault(x => (x.DataTypeId == dataTypeId.Value) && (x.Name == name));
            if (data == null)
            {
                throw new Exception("Data not found.");
            }

            return Delete(data);
        }

        #endregion
    }
}