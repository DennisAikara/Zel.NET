// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using Zel.Classes;
using Zel.DataAccess.Entity;
using Zel.Helpers;

namespace Zel.DataAccess
{
    /// <summary>
    ///     Context
    /// </summary>
    public abstract class DataContext : DbContext
    {
        private readonly IDataSession _dataSession;

        public ObjectContext ObjectContext { get; private set; }

        /// <summary>
        ///     Context's connection string
        /// </summary>
        public string ConnectionString
        {
            get { return ContextConnectionStrings[_contextType]; }
        }

        /// <summary>
        ///     Gets the Repository for the specified entity
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <returns>Repository for the specified entity</returns>
        internal Repository<T> GetRepository<T>() where T : class, IEntity
        {
            if (_entityRepositories.ContainsKey(typeof(T)))
            {
                return (Repository<T>) _entityRepositories[typeof(T)];
            }

            var repository = new Repository<T>(this, _dataSession);
            _entityRepositories[typeof(T)] = repository;

            return repository;
        }

        internal IQueryable<T> Query<T>() where T : class, IEntity
        {
            return ((EntityModel<T>) GetEntityModel(typeof(T))).Query();
        }

        internal IQueryable Query(Type entityType)
        {
            var getMethod = GetType().GetMethod("Query", BindingFlags.Public | BindingFlags.Instance);
            var getMethodGeneric = getMethod.MakeGenericMethod(entityType);
            return (IQueryable) getMethodGeneric.Invoke(this, null);
        }

        internal T Get<T>(int key) where T : class, IEntity
        {
            return ((EntityModel<T>) GetEntityModel(typeof(T))).Get(key);
        }

        internal T Get<T>(long key) where T : class, IEntity
        {
            return ((EntityModel<T>) GetEntityModel(typeof(T))).Get(key);
        }

        internal T Get<T>(string uniqueIdentifier) where T : class, IEntity, IUniqueIdentifier
        {
            return ((EntityModel<T>) GetEntityModel(typeof(T))).Get(uniqueIdentifier);
        }

        internal ValidationList Save(IEntity entity)
        {
            dynamic entityModelInstance = GetEntityModel(entity.GetType());
            return entityModelInstance.Save(entity as dynamic);
        }

        internal ValidationList Delete(IEntity entity, bool deleteChildren = false)
        {
            dynamic entityModelInstance = GetEntityModel(entity.GetType());
            return entityModelInstance.Delete(entity as dynamic, deleteChildren);
        }


        private object GetEntityModel(Type entityType)
        {
            if (_entityModelInstances.ContainsKey(entityType))
            {
                return _entityModelInstances[entityType];
            }

            var entityDetail = DataSession.GetEntityDetail(entityType);
            object model;
            var modelType = entityDetail.ModelType;
            if (modelType != null)
            {
                //has custom model
                model = Activator.CreateInstance(modelType, this);
                _entityModelInstances[entityType] = model;
                return model;
            }

            //no custom model, use default model
            modelType = typeof(EntityModel<>).MakeGenericType(entityType);
            model = Activator.CreateInstance(modelType, this);
            _entityModelInstances[entityType] = model;
            return model;
        }

        #region Internals

        #region Static

        /// <summary>
        ///     List of connection strings
        /// </summary>
        private static readonly Dictionary<Type, string> ContextConnectionStrings;

        #endregion

        #region Instance

        /// <summary>
        ///     Current context type
        /// </summary>
        private readonly Type _contextType;


        /// <summary>
        ///     List of already instantiated entity models in the current domain model
        /// </summary>
        private readonly Dictionary<Type, object> _entityModelInstances;

        /// <summary>
        ///     Entity repositories
        /// </summary>
        private readonly Dictionary<Type, object> _entityRepositories;

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        ///     Static constructor
        /// </summary>
        static DataContext()
        {
            ContextConnectionStrings = new Dictionary<Type, string>();
        }

        /// <summary>
        ///     Initiates the Entity Framework Context
        /// </summary>
        protected DataContext(IDataSession dataSession)
        {
            if (dataSession == null)
            {
                throw new ArgumentNullException("dataSession");
            }
            _dataSession = dataSession;
            _contextType = GetType();
            _entityRepositories = new Dictionary<Type, object>();
            _entityModelInstances = new Dictionary<Type, object>();

            CacheContextItems();
            ObjectContext = ((IObjectContextAdapter) this).ObjectContext;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //turn off validation and tracking
            Configuration.AutoDetectChangesEnabled = false;
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Configuration.ValidateOnSaveEnabled = false;
            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        ///     Cache context items
        /// </summary>
        private void CacheContextItems()
        {
            if (ContextConnectionStrings.ContainsKey(_contextType))
            {
                return;
            }

            //first time dealing with this context
            //cache entities and connection strings

            //add the contexts connection string
            var connectionString = ConfigurationHelper.GetConnectionString(_contextType.Name);
            if (connectionString != null)
            {
                ContextConnectionStrings[_contextType] = connectionString;
            }
            else
            {
                //connection string not found, throw error
                var error = string.Format("Cannot find connection string for context: '{0}'",
                    GetType().AssemblyQualifiedName);
                throw new ApplicationException(error);
            }
        }

        #endregion
    }
}