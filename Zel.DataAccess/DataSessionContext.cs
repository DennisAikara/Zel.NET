// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using Zel.DataAccess.Entity;
using Zel.DataAccess.Exceptions;

namespace Zel.DataAccess
{
    /// <summary>
    ///     Domain session context
    /// </summary>
    public sealed class DataSessionContext : IDisposable
    {
        private readonly IDataSession _dataSession;

        /// <summary>
        ///     Current instances of contexts
        /// </summary>
        private Dictionary<Type, DataContext> _dataContexts;

        /// <summary>
        ///     Current instances of view models
        /// </summary>
        private Dictionary<Type, object> _entityViewModels;

        internal DataSessionContext(IDataSession dataSession)
        {
            if (dataSession == null)
            {
                throw new ArgumentNullException("dataSession");
            }
            _dataSession = dataSession;
            _dataContexts = new Dictionary<Type, DataContext>();
            _entityViewModels = new Dictionary<Type, object>();
            Items = new Dictionary<string, object>();
            var requestIdentifier = Asp.GetRequestIdentifier();
            if (requestIdentifier == null)
            {
                Identifier = Guid.NewGuid().ToString("N");
            }
            else
            {
                Identifier = requestIdentifier;
            }

            UserName = "system";
        }

        /// <summary>
        ///     Current transaction
        /// </summary>
        internal Transaction Transaction { get; set; }

        /// <summary>
        ///     Domain session context items (settings etc for the current context)
        /// </summary>
        public Dictionary<string, object> Items { get; }

        /// <summary>
        ///     Current User's Id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        ///     Current User's UserName
        /// </summary>
        public string UserName { get; set; }

        public string Identifier { get; private set; }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (var dataContext in _dataContexts)
            {
                dataContext.Value.Dispose();
            }
            _dataContexts = new Dictionary<Type, DataContext>();
            _entityViewModels = new Dictionary<Type, object>();

            if (Transaction != null)
            {
                Transaction.Dispose();
                Transaction = null;
            }

            if (Items != null)
            {
                //dispose all disposables
                foreach (var disposable in Items.Select(item => item.Value).OfType<IDisposable>())
                {
                    disposable.Dispose();
                }
            }

            Identifier = null;
        }

        #endregion

        public DbConnection GetConnection(string contextName)
        {
            var dataContext = GetDataContext(contextName);
            if (dataContext == null)
            {
                return null;
            }
            var entityConnection = (EntityConnection) dataContext.ObjectContext.Connection;
            return entityConnection.StoreConnection;
        }

        public DataContext GetDataContext(Type contextType)
        {
            if (contextType == null)
            {
                throw new ArgumentNullException("contextType");
            }

            if (_dataContexts.ContainsKey(contextType))
            {
                return _dataContexts[contextType];
            }

            var context = (DataContext) Activator.CreateInstance(contextType, _dataSession);
            _dataContexts[contextType] = context;
            return context;
        }

        private DataContext GetDataContext(string contextName)
        {
            if (contextName == null)
            {
                throw new ArgumentNullException("contextName");
            }

            var contextType = DataSession.GetDataContext(contextName);

            if (contextType == null)
            {
                throw new ApplicationException("Invalid context " + contextName);
            }

            return GetDataContext(contextType);
        }

        public DataContext GetDataContext(EntityDetail entityDetail)
        {
            if (entityDetail == null)
            {
                throw new ArgumentNullException("entityDetail");
            }

            var contextType = entityDetail.ContextType;
            if (contextType == null)
            {
                throw new EntityMissingDataContextException(entityDetail.EnityType);
            }

            if (_dataContexts.ContainsKey(contextType))
            {
                return _dataContexts[contextType];
            }

            var context = (DataContext) Activator.CreateInstance(contextType, _dataSession);
            _dataContexts[contextType] = context;
            return context;
        }

        public object GetViewModel(EntityDetail entityDetail)
        {
            if (entityDetail == null)
            {
                throw new ArgumentNullException("entityDetail");
            }

            var modelType = entityDetail.ModelType;
            if (modelType == null)
            {
                //TODO create custom exception
                throw new EntityMissingDataContextException(entityDetail.EnityType);
            }

            if (_entityViewModels.ContainsKey(modelType))
            {
                return _entityViewModels[modelType];
            }

            var viewModel = Activator.CreateInstance(modelType);
            _entityViewModels[modelType] = viewModel;
            return viewModel;
        }
    }
}