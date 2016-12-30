// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Zel.Classes;

namespace Zel.DataAccess.Entity
{
    public abstract class EntityViewModel<TEntityView> where TEntityView : class, IEntity
    {
        public virtual IQueryable<TEntityView> Query(int? timeout = null)
        {
            throw new NotImplementedException();
        }

        public virtual TEntityView Get(int id, int? timeout = null)
        {
            throw new NotImplementedException();
        }

        public virtual TEntityView Get(long id, int? timeout = null)
        {
            throw new NotImplementedException();
        }

        public virtual TEntityView Get(string uniqueIdentifier, int? timeout = null)
        {
            throw new NotImplementedException();
        }

        public virtual ValidationList Save(TEntityView ebayCondition, int? timeout = null)
        {
            throw new NotImplementedException();
        }

        public virtual ValidationList Delete(TEntityView entity, bool deleteChildren = false, int? timeout = null)
        {
            throw new NotImplementedException();
        }

        #region Protected Methods

        protected virtual ValidationList Inserting(TEntityView entity)
        {
            return new ValidationList();
        }

        protected virtual ValidationList Updating(TEntityView entity)
        {
            return new ValidationList();
        }

        protected virtual ValidationList Deleting(TEntityView entity)
        {
            return new ValidationList();
        }

        protected virtual void Inserted(TEntityView entity) {}

        protected virtual void Updated(TEntityView entity) {}

        protected virtual void Deleted(TEntityView entity) {}

        #endregion
    }
}