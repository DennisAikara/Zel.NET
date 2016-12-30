// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Zel.Classes;
using Zel.Validation;

namespace Zel.DataAccess.Entity
{
    public class EntityModel<TEntity> where TEntity : class, IEntity
    {
        #region Constuctor

        public EntityModel(DataContext context)
        {
            if (context == null)
            {
                throw new NullReferenceException("Context is null");
            }

            var repository = context.GetRepository<TEntity>();
            if (repository != null)
            {
                Repository = repository;
            }
            else
            {
                throw new NullReferenceException(string.Format("Cannot find repository for entity: '{0}'",
                    typeof(TEntity).AssemblyQualifiedName));
            }
        }

        #endregion

        #region Internals

        private Repository<TEntity> Repository { get; }

        #endregion

        public virtual IQueryable<TEntity> Query()
        {
            return Repository.Query();
        }

        public virtual TEntity Get(int id)
        {
            var entityDetail = DataSession.GetEntityDetail(typeof(TEntity));

            return Query().Where(string.Format("{0}=={1}", entityDetail.KeyName, id)).FirstOrDefault();
        }

        public virtual TEntity Get(long id)
        {
            var entityDetail = DataSession.GetEntityDetail(typeof(TEntity));

            return Query().Where(string.Format("{0}=={1}", entityDetail.KeyName, id)).FirstOrDefault();
        }

        public virtual TEntity Get(string uniqueIdentifier)
        {
            return Query().Where(string.Format("UniqueIdentifier ==\"{0}\"", uniqueIdentifier)).FirstOrDefault();
        }

        public virtual ValidationList Save(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            var newEntity = entity.IsNew();

            var validationList = newEntity ? Inserting(entity) : Updating(entity);

            if (!validationList.IsValid)
            {
                return validationList;
            }

            validationList = DataAnnotationValidator.Validate(entity, true);
            if (validationList.IsValid)
            {
                validationList = Repository.Save(entity);
            }

            if (!validationList.IsValid)
            {
                return validationList;
            }

            if (newEntity)
            {
                Inserted(entity);
            }
            else
            {
                Updated(entity);
            }

            return validationList;
        }

        public virtual ValidationList Delete(TEntity entity, bool deleteChildren = false)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            var validationList = Deleting(entity);
            if (!validationList.IsValid)
            {
                return validationList;
            }

            validationList = Repository.Delete(entity, deleteChildren);
            if (validationList.IsValid)
            {
                Deleted(entity);
            }

            return validationList;
        }

        #region Protected Methods

        protected virtual ValidationList Inserting(TEntity entity)
        {
            return new ValidationList();
        }

        protected virtual ValidationList Updating(TEntity entity)
        {
            return new ValidationList();
        }

        protected virtual ValidationList Deleting(TEntity entity)
        {
            return new ValidationList();
        }

        protected virtual void Inserted(TEntity entity) {}

        protected virtual void Updated(TEntity entity) {}

        protected virtual void Deleted(TEntity entity) {}

        #endregion
    }
}