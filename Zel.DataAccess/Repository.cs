// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Zel.Classes;
using Zel.DataAccess.Entity;
using Zel.DataAccess.Entity.Auditing;
using Zel.Validation;

namespace Zel.DataAccess
{
    /// <summary>
    ///     Repository
    /// </summary>
    /// <typeparam name="TEntity">Entity</typeparam>
    internal class Repository<TEntity> where TEntity : class, IEntity
    {
        #region Constructors

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="context">Entity context</param>
        /// <param name="dataSession"></param>
        internal Repository(DataContext context, IDataSession dataSession)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (dataSession == null)
            {
                throw new ArgumentNullException("dataSession");
            }
            _context = context;
            _dataSession = dataSession;
            _entityDetail = DataSession.GetEntityDetail(typeof(TEntity));
            _dbSet =
                (DbSet<TEntity>)
                _entityDetail.DbSetProperty.GetValue(context,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
        }

        #endregion

        /// <summary>
        ///     Gets the IQueryable for the specified type
        /// </summary>
        /// <returns>IQueryable for the specified type</returns>
        public IQueryable<TEntity> Query()
        {
            EnlistInTransaction();
            return _dbSet.AsNoTracking();
        }

        /// <summary>
        ///     Saves the specified entity to the data store
        /// </summary>
        /// <param name="entityToSave">Entity to save</param>
        /// <returns>Validation list</returns>
        public ValidationList Save(TEntity entityToSave)
        {
            return entityToSave.IsNew() ? InsertEntity(entityToSave) : UpdateEntity(entityToSave);
        }

        /// <summary>
        ///     Delete the specified entity from the data store
        /// </summary>
        /// <param name="entityToDelete">Entity to delete</param>
        /// <param name="deleteChildren">
        ///     Flag to indicate if all the child entities that reference the entityToDelete should be
        ///     deleted
        /// </param>
        /// <returns>True if delete is sucessfull, else false</returns>
        public ValidationList Delete(TEntity entityToDelete, bool deleteChildren = false)
        {
            EnlistInTransaction();

            if (deleteChildren)
            {
                using (var transaction = _dataSession.BeginTransaction())
                {
                    //delete children
                    foreach (var entityChild in _entityDetail.Children)
                    {
                        var queryable = _dataSession.Query(entityChild.ChildType);
                        var query = string.Format("{0} == @0", entityChild.ParentIdField);
                        var children = queryable.Where(query, entityToDelete.GetKeyValue());
                        foreach (var child in children)
                        {
                            //delete child
                            var validationList = _dataSession.Delete((IEntity) child, true);
                            if (!validationList.IsValid)
                            {
                                return validationList;
                            }
                        }
                    }
                    transaction.Commit();
                }
            }


            var logicalDelete = entityToDelete as ILogicalDelete;
            if (logicalDelete != null)
            {
                if (logicalDelete.IsDeleted)
                {
                    return new ValidationList();
                }

                logicalDelete.IsDeleted = true;
                return UpdateEntity(entityToDelete);
            }

            //clear any item in the context
            ClearContextEntries();

            //attach the entity to the context, remove it and save
            _context.ObjectContext.AttachTo(_entityDetail.EntitySetName, entityToDelete);
            _dbSet.Remove(entityToDelete);


            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (ex.Message.IndexOf("Entities may have been modified or deleted since entities were loaded.",
                        StringComparison.Ordinal) > -1)
                {
                    var message = string.Concat("The specified ", _entityDetail.DisplayName, " doesn't exist.");
                    return new ValidationList
                    {
                        {string.Empty, message}
                    };
                }
                throw;
            }

            //clear entity key
            entityToDelete.SetKeyValue(Activator.CreateInstance(_entityDetail.KeyType));

            return new ValidationList();
        }

        #region Internals

        /// <summary>
        ///     Entity context
        /// </summary>
        private readonly DataContext _context;

        private readonly IDataSession _dataSession;

        /// <summary>
        ///     Entity DbSet
        /// </summary>
        private readonly IDbSet<TEntity> _dbSet;

        /// <summary>
        ///     Entity detail
        /// </summary>
        private readonly EntityDetail _entityDetail;

        #endregion

        #region Methods

        /// <summary>
        ///     Insert the specified entity into the data store
        /// </summary>
        /// <param name="entityToInsert">Entity to insert</param>
        /// <returns>True if insert is successfull, else false</returns>
        private ValidationList InsertEntity(TEntity entityToInsert)
        {
            //populate any insert fields
            AuditInsert(entityToInsert);

            if (entityToInsert is IUniqueIdentifier)
            {
                Reflection.SetPropertyValue(entityToInsert, "UniqueIdentifier", Guid.NewGuid().ToString("N"));
            }

            var validationList = DataAnnotationValidator.Validate(entityToInsert);
            if (!validationList.IsValid)
            {
                return validationList;
            }

            //clear any item in the context
            ClearContextEntries();

            //add the entity to the context and save
            _dbSet.Add(entityToInsert);
            EnlistInTransaction();
            try
            {
                if (_context.ObjectContext.Connection.State == ConnectionState.Broken)
                {
                    _context.ObjectContext.Connection.Close();
                }
                _context.SaveChanges();
            }
            catch (DbUpdateException dbUpdateException)
            {
                if ((dbUpdateException.InnerException != null) &&
                    (dbUpdateException.InnerException.InnerException != null)
                    && dbUpdateException.InnerException.InnerException is SqlException)
                {
                    var sqlException = dbUpdateException.InnerException.InnerException;
                    if (sqlException.Message.StartsWith("Cannot insert duplicate key row in object '"))
                    {
                        var index = sqlException.Message.IndexOf("with unique index '", StringComparison.Ordinal);
                        if (index > -1)
                        {
                            var uniqueIndexNameStart = index + 19;
                            var uniqueIndexNameEnd = sqlException.Message.IndexOf("'", uniqueIndexNameStart,
                                StringComparison.Ordinal);
                            if (uniqueIndexNameEnd > uniqueIndexNameStart)
                            {
                                var uniqueIndexName = sqlException.Message.Substring(index + 19,
                                    uniqueIndexNameEnd - uniqueIndexNameStart);
                                if (_entityDetail.UniqueConstraints.ContainsKey(uniqueIndexName))
                                {
                                    var uniqueContraint = _entityDetail.UniqueConstraints[uniqueIndexName];
                                    foreach (var field in uniqueContraint.Fields)
                                    {
                                        validationList.Add(field, uniqueContraint.Name);
                                    }

                                    return validationList;
                                }
                            }
                        }
                    }
                    if (
                        sqlException.Message.StartsWith(
                            "The INSERT statement conflicted with the FOREIGN KEY constraint "))
                    {
                        var foreignKeyNameStart = string.Concat("FK_", _entityDetail.EnityType.Name, "_");
                        var parentEntityPropertyNameStart = sqlException.Message.IndexOf(foreignKeyNameStart,
                            StringComparison.Ordinal);
                        if (parentEntityPropertyNameStart > -1)
                        {
                            parentEntityPropertyNameStart += foreignKeyNameStart.Length;
                            var parentEntityPropertyNameEnd = sqlException.Message.IndexOf("\".",
                                parentEntityPropertyNameStart,
                                StringComparison.Ordinal);
                            if (parentEntityPropertyNameEnd > parentEntityPropertyNameStart)
                            {
                                var parentEntityPropertyName =
                                    sqlException.Message.Substring(parentEntityPropertyNameStart,
                                        parentEntityPropertyNameEnd - parentEntityPropertyNameStart);
                                if (parentEntityPropertyName.Length > 0)
                                {
                                    var entityParentAttribute =
                                        _entityDetail.Parents.FirstOrDefault(
                                            x => x.ParentIdField == parentEntityPropertyName);
                                    if (entityParentAttribute != null)
                                    {
                                        validationList.Add(parentEntityPropertyName, entityParentAttribute.ErrorMessage);
                                        return validationList;
                                    }
                                }
                            }
                        }
                    }
                }

                throw;
            }

            return validationList;
        }

        /// <summary>
        ///     Updates the specified entity in the data store
        /// </summary>
        /// <param name="entityToUpdate">Entity to update</param>
        /// <returns>True if update is successfull, else false</returns>
        private ValidationList UpdateEntity(TEntity entityToUpdate)
        {
            //populate any update fields
            AuditUpdate(entityToUpdate);

            var validationList = DataAnnotationValidator.Validate(entityToUpdate);
            if (!validationList.IsValid)
            {
                return validationList;
            }

            //clear any item in the context
            ClearContextEntries();

            //attach the entity to the context, mark it as modified and save
            _context.ObjectContext.AttachTo(_entityDetail.EntitySetName, entityToUpdate);
            _context.Entry(entityToUpdate).State = EntityState.Modified;
            EnlistInTransaction();
            try
            {
                if (_context.ObjectContext.Connection.State == ConnectionState.Broken)
                {
                    _context.ObjectContext.Connection.Close();
                }
                _context.SaveChanges();
            }
            catch (DbUpdateException dbUpdateException)
            {
                if ((dbUpdateException.InnerException != null) &&
                    (dbUpdateException.InnerException.InnerException != null)
                    && dbUpdateException.InnerException.InnerException is SqlException)
                {
                    var sqlException = dbUpdateException.InnerException.InnerException;
                    if (sqlException.Message.StartsWith("Cannot insert duplicate key row in object '"))
                    {
                        var index = sqlException.Message.IndexOf("with unique index '", StringComparison.Ordinal);
                        if (index > -1)
                        {
                            var uniqueIndexNameStart = index + 19;
                            var uniqueIndexNameEnd = sqlException.Message.IndexOf("'", uniqueIndexNameStart,
                                StringComparison.Ordinal);
                            if (uniqueIndexNameEnd > uniqueIndexNameStart)
                            {
                                var uniqueIndexName = sqlException.Message.Substring(index + 19,
                                    uniqueIndexNameEnd - uniqueIndexNameStart);
                                if (_entityDetail.UniqueConstraints.ContainsKey(uniqueIndexName))
                                {
                                    var uniqueContraint = _entityDetail.UniqueConstraints[uniqueIndexName];
                                    foreach (var field in uniqueContraint.Fields)
                                    {
                                        validationList.Add(field, uniqueContraint.Name);
                                    }

                                    return validationList;
                                }
                            }
                        }
                    }
                    if (
                        sqlException.Message.StartsWith(
                            "The UPDATE statement conflicted with the FOREIGN KEY constraint "))
                    {
                        var foreignKeyNameStart = string.Concat("FK_", _entityDetail.EnityType.Name, "_");
                        var parentEntityPropertyNameStart = sqlException.Message.IndexOf(foreignKeyNameStart,
                            StringComparison.Ordinal);
                        if (parentEntityPropertyNameStart > -1)
                        {
                            parentEntityPropertyNameStart += foreignKeyNameStart.Length;
                            var parentEntityPropertyNameEnd = sqlException.Message.IndexOf("\".",
                                parentEntityPropertyNameStart,
                                StringComparison.Ordinal);
                            if (parentEntityPropertyNameEnd > parentEntityPropertyNameStart)
                            {
                                var parentEntityPropertyName =
                                    sqlException.Message.Substring(parentEntityPropertyNameStart,
                                        parentEntityPropertyNameEnd - parentEntityPropertyNameStart);
                                if (parentEntityPropertyName.Length > 0)
                                {
                                    var entityParentAttribute =
                                        _entityDetail.Parents.FirstOrDefault(
                                            x => x.ParentIdField == parentEntityPropertyName);
                                    if (entityParentAttribute != null)
                                    {
                                        validationList.Add(parentEntityPropertyName, entityParentAttribute.ErrorMessage);
                                        return validationList;
                                    }
                                }
                            }
                        }
                    }
                }
                if (dbUpdateException.InnerException is OptimisticConcurrencyException)
                {
                    var entityDoesntExistMessage = string.Concat(
                        "Store update, insert, or delete statement affected an",
                        " unexpected number of rows (0). Entities may have been modified or deleted since entities were loaded.");
                    if (dbUpdateException.InnerException.Message.StartsWith(entityDoesntExistMessage))
                    {
                        var message = string.Concat("The specified ", _entityDetail.DisplayName, " doesn't exist.");
                        return new ValidationList
                        {
                            {string.Empty, message}
                        };
                    }
                }

                throw;
            }

            return validationList;
        }

        /// <summary>
        ///     Populate the entity's created audit fields if they exists
        /// </summary>
        /// <param name="entity">Entity to audit</param>
        private void AuditInsert(TEntity entity)
        {
            var createdOn = default(DateTime);

            var auditCreatedOn = entity as IAuditCreatedOn;
            if (auditCreatedOn != null)
            {
                //has createdon property
                createdOn = DateTime.UtcNow;
                auditCreatedOn.CreatedOn = createdOn;
            }

            var auditCreatedBy = entity as IAuditCreatedBy;
            if (auditCreatedBy != null)
            {
                auditCreatedBy.CreatedBy = _dataSession.DataSessionContext.UserId;
            }
            else
            {
                var auditCreatedByName = entity as IAuditCreatedByName;
                if (auditCreatedByName != null)
                {
                    auditCreatedByName.CreatedBy = _dataSession.DataSessionContext.UserName;
                }
            }

            //populate the modified audit fields
            AuditUpdate(entity, createdOn);
        }

        /// <summary>
        ///     Populate the entity's modified audit fields if they exists
        /// </summary>
        /// <param name="entity">Entity to audit</param>
        /// <param name="modifiedOn">Modified on date time to use</param>
        private void AuditUpdate(TEntity entity, DateTime modifiedOn = default(DateTime))
        {
            var auditModifiedOn = entity as IAuditModifiedOn;
            if (auditModifiedOn != null)
            {
                //if modifiedOn is set the method is called from auditinsert, if so use modifiedOn as the modified date
                auditModifiedOn.ModifiedOn = modifiedOn == default(DateTime) ? DateTime.UtcNow : modifiedOn;
            }

            var auditModifiedBy = entity as IAuditModifiedBy;
            if (auditModifiedBy != null)
            {
                auditModifiedBy.ModifiedBy = _dataSession.DataSessionContext.UserId;
            }
            else
            {
                var auditModifiedByName = entity as IAuditModifiedByName;
                if (auditModifiedByName != null)
                {
                    auditModifiedByName.ModifiedBy = _dataSession.DataSessionContext.UserName;
                }
            }
        }

        /// <summary>
        ///     Removes any existing items from the context
        /// </summary>
        private void ClearContextEntries()
        {
            const EntityState allStates =
                EntityState.Added | EntityState.Deleted | EntityState.Modified | EntityState.Unchanged;
            var entities = _context.ObjectContext.ObjectStateManager.GetObjectStateEntries(allStates);
            foreach (var objectStateEntry in entities)
            {
                //remove
                _context.ObjectContext.Detach(objectStateEntry.Entity);
            }
        }

        /// <summary>
        ///     Enlist the current context with the current transaction if one exists
        /// </summary>
        private void EnlistInTransaction()
        {
            var currentDomainModelContext = _dataSession.DataSessionContext;
            if (currentDomainModelContext.Transaction != null)
            {
                currentDomainModelContext.Transaction.EnlistContext(_context);
            }
        }

        #endregion
    }
}