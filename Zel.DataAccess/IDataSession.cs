// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Zel.Classes;
using Zel.DataAccess.Entity;

namespace Zel.DataAccess
{
    public interface IDataSession : IDataSessionTransaction
    {
        string Identifier { get; }
        int TimeoutInSeconds { get; }
        DataSessionContext DataSessionContext { get; }
        IQueryable Query(Type entityType, int? timeoutInSeconds = null);
        IQueryable<T> Query<T>(int? timeoutInSeconds = null) where T : class, IEntity;
        T Get<T>(int key, int? timeoutInSeconds = null) where T : class, IEntity;
        T Get<T>(long key, int? timeoutInSeconds = null) where T : class, IEntity;
        T Get<T>(string uniqueIdentifier, int? timeoutInSeconds = null) where T : class, IEntity, IUniqueIdentifier;
        ValidationList Save(IEntityView entity, int? timeoutInSeconds = null);
        ValidationList Save(IEntity entity, int? timeoutInSeconds = null);
        ValidationList Delete(IEntity entity, bool deleteChildren = false, int? timeoutInSeconds = null);
        ValidationList Delete<T>(IEnumerable<int> keys);
        ValidationList Delete<T>(IQueryable<int> query, int? timeoutInSeconds = null);
        ValidationList Delete<T>(IEnumerable<long> keys);
    }
}