// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;

namespace Zel.DataAccess
{
    public interface ITransaction : IDisposable
    {
        /// <summary>
        ///     Commits the current transaction
        /// </summary>
        void Commit();
    }

    public class ChildTransaction : ITransaction
    {
        private readonly IDataSession _dataSession;
        private readonly string _identifier;
        private bool _commited;
        private bool _disposed;

        public ChildTransaction(IDataSession dataSession)
        {
            if (dataSession == null)
            {
                throw new ArgumentNullException("dataSession");
            }
            _dataSession = dataSession;
            _identifier = Guid.NewGuid().ToString("N");
            _dataSession.DataSessionContext.Transaction.RegisterChildTransaction(_identifier);
        }

        #region ITransaction Members

        public void Dispose()
        {
            if (!_commited && !_disposed)
            {
                _dataSession.DataSessionContext.Transaction.RollBackChildTransaction(_identifier);
                _disposed = true;
            }
        }

        public void Commit()
        {
            if (!_commited && !_disposed)
            {
                _dataSession.DataSessionContext.Transaction.CommitChildTransaction(_identifier);
                _commited = true;
            }
        }

        #endregion
    }

    public class Transaction : ITransaction
    {
        private readonly IDataSession _dataSession;

        internal Transaction(IDataSession dataSession)
        {
            if (dataSession == null)
            {
                throw new ArgumentNullException("dataSession");
            }
            _dataSession = dataSession;
            _savePoints = new List<string>();
            _dataContextTransactions = new Dictionary<DataContext, SqlTransaction>();
        }

        internal void RegisterChildTransaction(string identifier)
        {
            _savePoints.Add(identifier);

            foreach (var dataContextTransaction in _dataContextTransactions)
            {
                dataContextTransaction.Value.Save(identifier);
            }
        }

        internal void CommitChildTransaction(string identifier)
        {
            if (_savePoints[_savePoints.Count - 1] != identifier)
            {
                throw new Exception("Nested transactions should be commited in the order they where created.");
            }
            _savePoints.RemoveAt(_savePoints.Count - 1);
        }

        internal void RollBackChildTransaction(string identifier)
        {
            if (_savePoints[_savePoints.Count - 1] != identifier)
            {
                throw new Exception("Nested transactions should be commited in the order they where created.");
            }
            _savePoints.RemoveAt(_savePoints.Count - 1);

            foreach (var dataContextTransaction in _dataContextTransactions)
            {
                dataContextTransaction.Value.Rollback(identifier);
            }
        }


        /// <summary>
        ///     Enlists a data context in the transaction if it is not currenlty enlisted
        /// </summary>
        /// <param name="dataContext">Data context to enlist</param>
        internal void EnlistContext(DataContext dataContext)
        {
            if (dataContext == null)
            {
                throw new ArgumentNullException("dataContext");
            }

            if (_dataContextTransactions.ContainsKey(dataContext))
            {
                return;
            }

            //datacontext is not currently enlisted
            var dbConnection = dataContext.ObjectContext.Connection;
            if (dbConnection.State != ConnectionState.Open)
            {
                //open the connection so, entity framework won't close the connection automatically
                dbConnection.Close();
                dbConnection.Open();
            }

            var sqlTransaction = GetSqlTransaction(dbConnection.BeginTransaction(IsolationLevel.Snapshot));

            if (_savePoints.Count > 0)
            {
                var currentSavePont = _savePoints[_savePoints.Count - 1];
                sqlTransaction.Save(currentSavePont);
            }

            _dataContextTransactions[dataContext] = sqlTransaction;
        }

        private static SqlTransaction GetSqlTransaction(DbTransaction dbTransaction)
        {
            var entityTransaction = (EntityTransaction) dbTransaction;

            return (SqlTransaction) entityTransaction.StoreTransaction;
        }

        #region ITransaction Members

        public void Dispose()
        {
            foreach (var keyValuePair in _dataContextTransactions)
            {
                var dbTransaction = keyValuePair.Value;
                if (dbTransaction != null)
                {
                    if ((dbTransaction.Connection != null) && (dbTransaction.Connection.State != ConnectionState.Closed))
                    {
                        dbTransaction.Connection.Close();
                        dbTransaction.Dispose();
                    }
                }

                if (keyValuePair.Key.ObjectContext.Connection.State != ConnectionState.Closed)
                {
                    keyValuePair.Key.ObjectContext.Connection.Close();
                }
            }

            _dataContextTransactions = null;

            var currentDomainModelContext = _dataSession.DataSessionContext;
            if (currentDomainModelContext.Transaction == this)
            {
                currentDomainModelContext.Transaction = null;
            }
        }

        /// <summary>
        ///     Commits the current transaction
        /// </summary>
        public void Commit()
        {
            if (_dataSession.DataSessionContext.Transaction != this)
            {
                return;
            }

            foreach (var keyValuePair in _dataContextTransactions)
            {
                var dbTransaction = keyValuePair.Value;
                if (dbTransaction != null)
                {
                    dbTransaction.Commit();
                }
                if (keyValuePair.Key.ObjectContext.Connection.State != ConnectionState.Closed)
                {
                    keyValuePair.Key.ObjectContext.Connection.Close();
                }
            }
        }

        #endregion

        #region Internals

        private readonly List<string> _savePoints;

        /// <summary>
        ///     List of contexts that are currently enlisted in the current transaction
        /// </summary>
        private Dictionary<DataContext, SqlTransaction> _dataContextTransactions;

        #endregion
    }
}