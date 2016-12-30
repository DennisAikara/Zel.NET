// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Reflection;
using Zel.Classes;
using Zel.DataAccess.Entity;
using Zel.DataAccess.Exceptions;
using Zel.Helpers;

namespace Zel.DataAccess
{
    public class DataSession : IDataSession, IDisposable
    {
        #region IDisposable Members

        public void Dispose()
        {
            if ((DataSessionContext != null) && (DataSessionContext.Identifier != null))
            {
                DataSessionContext.Dispose();
                DataSessionContext = null;
            }
        }

        #endregion

        #region Fields

        /// <summary>
        ///     Indicates if the data session has been successfully initialized
        /// </summary>
        private static bool _isInitialized;

        /// <summary>
        ///     Entity details
        /// </summary>
        private static readonly Dictionary<Type, EntityDetail> EntityDetails = new Dictionary<Type, EntityDetail>();

        private static readonly Dictionary<string, Type> DataContextTypes = new Dictionary<string, Type>();

        #endregion

        #region IDataSession Members

        /// <summary>
        ///     Current data session context
        /// </summary>
        public DataSessionContext DataSessionContext { get; private set; }

        public string Identifier
        {
            get { return DataSessionContext.Identifier; }
        }

        public int TimeoutInSeconds { get; private set; }

        public ITransaction BeginTransaction()
        {
            if (DataSessionContext.Transaction != null)
            {
                return new ChildTransaction(this);
            }

            DataSessionContext.Transaction = new Transaction(this);
            return DataSessionContext.Transaction;
        }

        public IQueryable Query(Type entityType, int? timeout = null)
        {
            if (!EntityDetails.ContainsKey(entityType))
            {
                throw new EntityNotFoundException(entityType);
            }

            var entityDetail = EntityDetails[entityType];
            var context = DataSessionContext.GetDataContext(entityDetail);
            context.ObjectContext.CommandTimeout = timeout;

            return entityDetail.IsEntityView
                ? ((dynamic) DataSessionContext.GetViewModel(entityDetail)).Query()
                : context.Query(entityType);
        }

        public IQueryable<T> Query<T>(int? timeout = null) where T : class, IEntity
        {
            var entityType = typeof(T);
            if (!EntityDetails.ContainsKey(entityType))
            {
                throw new EntityNotFoundException(entityType);
            }

            var entityDetail = EntityDetails[entityType];
            if (entityDetail.IsEntityView)
            {
                return ((EntityViewModel<T>) DataSessionContext.GetViewModel(entityDetail)).Query(timeout);
            }
            var context = DataSessionContext.GetDataContext(entityDetail);
            context.ObjectContext.CommandTimeout = timeout;
            return context.Query<T>();
        }

        public T Get<T>(int key, int? timeout = null) where T : class, IEntity
        {
            var entityType = typeof(T);
            if (!EntityDetails.ContainsKey(entityType))
            {
                throw new EntityNotFoundException(entityType);
            }

            var context = DataSessionContext.GetDataContext(EntityDetails[entityType]);
            context.ObjectContext.CommandTimeout = timeout;

            return context.Get<T>(key);
        }

        public T Get<T>(long key, int? timeout = null) where T : class, IEntity
        {
            var entityType = typeof(T);
            if (!EntityDetails.ContainsKey(entityType))
            {
                throw new EntityNotFoundException(entityType);
            }

            var context = DataSessionContext.GetDataContext(EntityDetails[entityType]);
            context.ObjectContext.CommandTimeout = timeout;

            return context.Get<T>(key);
        }

        public T Get<T>(string uniqueIdentifier, int? timeout = null) where T : class, IEntity, IUniqueIdentifier
        {
            var entityType = typeof(T);
            if (!EntityDetails.ContainsKey(entityType))
            {
                throw new EntityNotFoundException(entityType);
            }

            var context = DataSessionContext.GetDataContext(EntityDetails[entityType]);
            context.ObjectContext.CommandTimeout = timeout;

            return context.Get<T>(uniqueIdentifier);
        }

        public ValidationList Save(IEntityView entity, int? timeout = null)
        {
            var entityType = entity.GetType();
            if (!EntityDetails.ContainsKey(entityType))
            {
                throw new EntityNotFoundException(entityType);
            }

            return ((dynamic) DataSessionContext.GetViewModel(EntityDetails[entityType])).Save(entity as dynamic);
        }

        public ValidationList Save(IEntity entity, int? timeout = null)
        {
            var entityType = entity.GetType();
            if (!EntityDetails.ContainsKey(entityType))
            {
                throw new EntityNotFoundException(entityType);
            }

            var context = DataSessionContext.GetDataContext(EntityDetails[entityType]);
            context.ObjectContext.CommandTimeout = timeout;
            return context.Save(entity);
        }

        public ValidationList Delete(IEntity entity, bool deleteChildren = false, int? timeout = null)
        {
            var entityType = entity.GetType();
            if (!EntityDetails.ContainsKey(entityType))
            {
                throw new EntityNotFoundException(entityType);
            }

            var context = DataSessionContext.GetDataContext(EntityDetails[entityType]);
            context.ObjectContext.CommandTimeout = timeout;

            return context.Delete(entity, deleteChildren);
        }

        public ValidationList Delete<T>(IEnumerable<int> keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }

            var deleteQuery = @"DELETE FROM [{0}].[{1}] WHERE [{2}] IN ({3});";

            var entityType = typeof(T);
            if (!EntityDetails.ContainsKey(entityType))
            {
                throw new EntityNotFoundException(entityType);
            }

            var entityDetail = EntityDetails[entityType];
            deleteQuery = string.Format(deleteQuery, entityDetail.DbSchemaName, entityDetail.DbTableName,
                entityDetail.KeyName,
                string.Join(",", keys));

            var databaseCommand = new DatabaseCommand(deleteQuery);
            var entityConnection =
                (EntityConnection) DataSessionContext.GetDataContext(entityDetail).ObjectContext.Connection;
            var dbConnection = entityConnection.StoreConnection;
            Sql.ExecuteCommand(databaseCommand, dbConnection);
            return new ValidationList();
        }

        public ValidationList Delete<T>(IQueryable<int> query, int? timeout = null)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            var deleteQuery = @"DELETE FROM [{0}].[{1}] WHERE [{2}] IN ({3})";

            var entityType = typeof(T);
            if (!EntityDetails.ContainsKey(entityType))
            {
                throw new EntityNotFoundException(entityType);
            }

            var entityDetail = EntityDetails[entityType];
            deleteQuery = string.Format(deleteQuery, entityDetail.DbSchemaName, entityDetail.DbTableName,
                entityDetail.KeyName,
                query.ToTraceString());

            var databaseCommand = new DatabaseCommand(deleteQuery)
            {
                Timeout = timeout
            };
            var dbConnection =
                ((EntityConnection) DataSessionContext.GetDataContext(entityDetail).ObjectContext.Connection)
                    .StoreConnection;
            var deleteCount = Sql.ExecuteCommand(databaseCommand, dbConnection);
            return new ValidationList();
        }

        public ValidationList Delete<T>(IEnumerable<long> keys)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Methods

        private static void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            //load all friendly assemblies
            AssemblyHelper.LoadAssembliesWithAssemblyAttribute(Application.BinDirectory,
                typeof(DataAccessAssemblyAttribute));
            var dataAccessAssemblies =
                AssemblyHelper.GetAssembliesWithAssemblyAttribute(typeof(DataAccessAssemblyAttribute));

            var entityDetails = new List<EntityDetail>();

            var assemblyDataContextTypes = new List<Type>();
            var assemblyEntityTypes = new List<Type>();
            var assemblyEntityModelTypes = new List<Type>();
            var entityContextTypes = new Dictionary<Type, Type>();

            foreach (var dataAccessAssembly in dataAccessAssemblies)
            {
                assemblyDataContextTypes.AddRange(AssemblyHelper.FindTypesThatAreSubclassOfClass(typeof(DataContext),
                    dataAccessAssembly));
                assemblyEntityTypes.AddRange(AssemblyHelper.FindTypesThatImplementInterface(typeof(IEntity),
                    dataAccessAssembly));
                assemblyEntityModelTypes.AddRange(AssemblyHelper.FindTypesThatAreSubclassOfClass(typeof(EntityModel<>),
                    dataAccessAssembly));
                assemblyEntityModelTypes.AddRange(
                    AssemblyHelper.FindTypesThatAreSubclassOfClass(typeof(EntityViewModel<>),
                        dataAccessAssembly));
            }

            foreach (var assemblyDataContextType in assemblyDataContextTypes)
            {
                //get the list of datacontext dbsets
                var dbSets =
                (from p in
                    assemblyDataContextType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic
                                                          | BindingFlags.Instance)
                    let pt = p.PropertyType
                    where
                    pt.IsGenericType && (pt.GetGenericTypeDefinition() == typeof(IDbSet<>))
                    && pt.GetGenericArguments()[0].GetInterfaces().Contains(typeof(IEntity))
                    select new
                    {
                        EntityType = pt.GetGenericArguments()[0],
                        DbSetName = p.Name
                    }).ToList();

                foreach (var dbSet in dbSets)
                {
                    //make sure the DbSets are named properly
                    if (dbSet.DbSetName != string.Concat(dbSet.EntityType.Name, "DbSet"))
                    {
                        throw new InvalidDataContextDbSetPropertyNameException(assemblyDataContextType, dbSet.DbSetName);
                    }

                    //make sure DbSet type is an iEntity
                    if (dbSet.EntityType.GetInterface(typeof(IEntity).FullName) == null)
                    {
                        throw new InvalidDataContextDbSetPropertyTypeException(assemblyDataContextType, dbSet.DbSetName);
                    }

                    entityContextTypes[dbSet.EntityType] = assemblyDataContextType;

                    if (!DataContextTypes.ContainsKey(assemblyDataContextType.Name))
                    {
                        DataContextTypes[assemblyDataContextType.Name] = assemblyDataContextType;
                    }
                    else
                    {
                        if (DataContextTypes[assemblyDataContextType.Name] == assemblyDataContextType)
                        {
                            continue;
                        }
                        var message = string.Concat("Different context types with same name ",
                            assemblyDataContextType.FullName,
                            " and ", DataContextTypes[assemblyDataContextType.Name].FullName);
                        throw new ApplicationException(message);
                    }
                }
            }

            //make sure  entities have a datacontext in the  assembly
            for (var i = assemblyEntityTypes.Count - 1; i >= 0; i--)
            {
                var assemblyEntityType = assemblyEntityTypes[i];
                if ((assemblyEntityType.GetInterface(typeof(IEntityView).FullName) == null)
                    && !entityContextTypes.ContainsKey(assemblyEntityType))
                {
                    assemblyEntityTypes.RemoveAt(i);
                }
            }


            var entityModelTypes = new Dictionary<Type, Type>();
            //make sure all entitymodels are named properly
            foreach (var assemblyEntityModelType in assemblyEntityModelTypes)
            {
                if (assemblyEntityModelType.BaseType != null)
                {
                    var entityType = assemblyEntityModelType.BaseType.GenericTypeArguments[0];
                    if (assemblyEntityModelType.Name != string.Concat(entityType.Name, "Model"))
                    {
                        throw new InvalidEntityModelNameException(assemblyEntityModelType);
                    }
                    entityModelTypes[entityType] = assemblyEntityModelType;
                }
            }

            foreach (var assemblyEntityType in assemblyEntityTypes)
            {
                var isEntityView = assemblyEntityType.GetInterface(typeof(IEntityView).FullName) != null;

                //set entity type and context
                var entityDetail = new EntityDetail
                {
                    EnityType = assemblyEntityType,
                    ContextType = isEntityView ? null : entityContextTypes[assemblyEntityType],
                    IsEntityView = isEntityView,
                    Name = assemblyEntityType.Name
                };


                //set entity model
                if (entityModelTypes.ContainsKey(assemblyEntityType))
                {
                    entityDetail.ModelType = entityModelTypes[assemblyEntityType];
                }

                //set entity key
                var keys = Reflection.GetPropertiesWithAttribute<KeyAttribute>(assemblyEntityType);
                if (keys.Count == 0)
                {
                    throw new InvalidEntityKeyException("Entity doesn't have a key specified", assemblyEntityType);
                }
                if (keys.Count > 1)
                {
                    throw new InvalidEntityKeyException("Entity has more than one key specified", assemblyEntityType);
                }

                if ((keys[0].PropertyType != typeof(int)) && (keys[0].PropertyType != typeof(long)))
                {
                    throw new InvalidEntityKeyException("Entity key type must be an int or long", assemblyEntityType);
                }

                entityDetail.KeyName = keys[0].Name;
                entityDetail.KeyType = keys[0].PropertyType;
                entityDetail.KeyProperty = keys[0];

                //set entity table and schema
                if (!isEntityView)
                {
                    var table = Reflection.GetClassAttributes<TableAttribute>(assemblyEntityType).FirstOrDefault();
                    if (table != null)
                    {
                        entityDetail.DbTableName = table.Name;
                        entityDetail.DbSchemaName = table.Schema;
                    }
                    else
                    {
                        throw new EntityMissingTableAttributeException(assemblyEntityType);
                    }

                    //set entitysetname
                    var dbSetName = string.Format("{0}DbSet", entityDetail.Name);
                    entityDetail.EntitySetName = string.Format("{0}.{1}", entityDetail.ContextType.Name, dbSetName);

                    //set entity dbset property
                    entityDetail.DbSetProperty = entityDetail.ContextType.GetProperty(dbSetName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                }

                //set entity display name
                var displayName =
                    Reflection.GetClassAttributes<DisplayNameAttribute>(assemblyEntityType).FirstOrDefault();
                entityDetail.DisplayName = displayName != null ? displayName.DisplayName : assemblyEntityType.Name;


                //set entity deleteconfirmationmessage
                var deleteConfirmationAttribute =
                    Reflection.GetClassAttributes<DeleteConfirmationAttribute>(assemblyEntityType).FirstOrDefault();
                if ((deleteConfirmationAttribute != null) && (deleteConfirmationAttribute.ConfirmationMessage != null))
                {
                    entityDetail.DeleteConfirmationMessage = deleteConfirmationAttribute.ConfirmationMessage;
                }
                else
                {
                    entityDetail.DeleteConfirmationMessage = string.Concat("Are you sure you want to delete this ",
                        entityDetail.DisplayName, "?");
                }

                //set unique constraints
                var uniqueConstraintAttributes =
                    Reflection.GetClassAttributes<UniqueConstraintAttribute>(assemblyEntityType);
                foreach (var uniqueConstraintAttribute in uniqueConstraintAttributes)
                {
                    foreach (var field in uniqueConstraintAttribute.Fields)
                    {
                        if (field == entityDetail.KeyName)
                        {
                            throw new InvalidEntityUniqueConstraintFieldException("Entity key in unique constraint.",
                                assemblyEntityType);
                        }

                        if (!Reflection.GetPropertyNames(assemblyEntityType).Contains(field))
                        {
                            throw new InvalidEntityUniqueConstraintFieldException(
                                string.Concat("Entity doesn't have a ", field, " property."), assemblyEntityType);
                        }
                    }
                    entityDetail.UniqueConstraints[uniqueConstraintAttribute.Name] = uniqueConstraintAttribute;
                }

                //set parents
                var parentProperties = Reflection.GetPropertiesWithAttribute<ParentEntityAttribute>(assemblyEntityType);
                foreach (var parentProperty in parentProperties)
                {
                    var parentEntityAttribute =
                        parentProperty.GetCustomAttributes(typeof(ParentEntityAttribute), false).First() as
                            ParentEntityAttribute;
                    var entityParent = new EntityParent();
                    if (parentEntityAttribute == null)
                    {
                        continue;
                    }
                    entityParent.ErrorMessage = parentEntityAttribute.ErrorMessage;
                    entityParent.ParentIdField = parentProperty.Name;
                    entityParent.ParentRequired = Reflection.PropertyHasAttribute<RequiredAttribute>(
                        assemblyEntityType, parentProperty.Name);
                    entityParent.ParentType = parentEntityAttribute.ParentEntity;
                    entityDetail.Parents.Add(entityParent);
                }

                entityDetails.Add(entityDetail);
            }

            //set children & add to global list
            foreach (var entityDetail in entityDetails)
            {
                entityDetail.Children = (from aed in entityDetails
                    from p in aed.Parents
                    where p.ParentType.AssemblyQualifiedName == entityDetail.EnityType.AssemblyQualifiedName
                    select new EntityChild
                    {
                        ChildType = aed.EnityType,
                        ParentIdField = p.ParentIdField
                    }).ToList();

                EntityDetails[entityDetail.EnityType] = entityDetail;
            }

            _isInitialized = true;
        }

        /// <summary>
        ///     Get the entity details for the specified entity type
        /// </summary>
        /// <param name="entityType">Entity type</param>
        /// <returns>Entity detail</returns>
        internal static EntityDetail GetEntityDetail(Type entityType)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException("entityType");
            }

            if (EntityDetails.ContainsKey(entityType))
            {
                return EntityDetails[entityType];
            }

            throw new EntityNotFoundException(entityType);
        }

        internal static Type GetDataContext(string contextName)
        {
            if (DataContextTypes.ContainsKey(contextName))
            {
                return DataContextTypes[contextName];
            }
            return null;
        }

        #endregion

        #region Constructors

        /// <summary>
        ///     Static constructor
        /// </summary>
        static DataSession()
        {
            Initialize();
        }

        public DataSession()
        {
            DataSessionContext = new DataSessionContext(this);
        }

        #endregion
    }
}