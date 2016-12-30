// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Zel.Classes;

namespace Zel
{
    /// <summary>
    ///     Helper class to interact with databases
    /// </summary>
    public static class Sql
    {
        #region Constants

        /// <summary>
        ///     SQL Server provider
        /// </summary>
        private const string SQL_SERVER_PROVIDER = "System.Data.SqlClient";

        #endregion

        private static SqlTransaction GetTransaction(IDbConnection conn)
        {
            if (conn.GetType().Name != "SqlConnection")
            {
                return null;
            }

            var innerConnectionPropertyInfo = typeof(SqlConnection).GetProperty("InnerConnection",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var innerConnection = innerConnectionPropertyInfo.GetValue(conn, null);


            var currentTransactionPropertyInfo = innerConnection.GetType().GetProperty("CurrentTransaction",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var currentTransaction = currentTransactionPropertyInfo.GetValue(innerConnection, null);
            if (currentTransaction == null)
            {
                return null;
            }

            var realTransactionProperty = currentTransaction.GetType().GetProperty("Parent",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var realTransaction = realTransactionProperty.GetValue(currentTransaction, null);
            return (SqlTransaction) realTransaction;
        }

        #region Methods

        /// <summary>
        ///     Creates a database connection from the specified connection string
        /// </summary>
        /// <returns>DbConnection</returns>
        public static DbConnection CreateConnection()
        {
            var providerExists = DbProviderFactories.GetFactoryClasses().Rows.Cast<DataRow>()
                .Any(r => r[2].Equals(SQL_SERVER_PROVIDER));
            if (!providerExists)
            {
                //provider doesn't exist, return null
                return null;
            }

            var factory = DbProviderFactories.GetFactory(SQL_SERVER_PROVIDER);
            return factory.CreateConnection();
        }

        /// <summary>
        ///     Executes a command against a database
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <param name="connection">Connection to use</param>
        /// <param name="isStoredProcedure">Flag to indicate if the command is a stored procedure</param>
        /// <returns>Number of rows affected by the command</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static int ExecuteCommand(DatabaseCommand command, DbConnection connection,
            bool isStoredProcedure = false)
        {
            //open connection if connection is not open
            var closeConnection = false;
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
                closeConnection = true;
            }

            try
            {
                //create a db command object using the command and db connection
                using (var dbCommand = connection.CreateCommand())
                {
                    var sqlTransaction = GetTransaction(connection);
                    if (sqlTransaction != null)
                    {
                        dbCommand.Transaction = sqlTransaction;
                    }

                    dbCommand.CommandText = command.Command;
                    if (command.Timeout != null)
                    {
                        dbCommand.CommandTimeout = command.Timeout.Value;
                    }
                    if (isStoredProcedure)
                    {
                        //command is stored procedure, set command type to stored procedure
                        dbCommand.CommandType = CommandType.StoredProcedure;
                    }

                    foreach (var queryParam in command.Parameters)
                    {
                        //add all the command paramenter to db command
                        var parameter = dbCommand.CreateParameter();
                        parameter.ParameterName = queryParam.ParameterName;
                        parameter.Value = queryParam.ParameterValue;
                        dbCommand.Parameters.Add(parameter);
                    }

                    //execute the command against the database and get the number of rows affected
                    var rowsAffected = dbCommand.ExecuteNonQuery();
                    if (closeConnection)
                    {
                        connection.Close();
                    }
                    return rowsAffected;
                }
            }
            finally
            {
                if (closeConnection)
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        ///     Executes a command against a database
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="isStoredProcedure">Flag to indicate if the command is a stored procedure</param>
        /// <returns>Number of rows affected by the command</returns>
        public static int ExecuteCommand(DatabaseCommand command, string connectionString,
            bool isStoredProcedure = false)
        {
            //create a db connection object using the connection string
            using (var dbConnection = CreateConnection())
            {
                dbConnection.ConnectionString = connectionString;

                //execute the command using the connection
                return ExecuteCommand(command, dbConnection, isStoredProcedure);
            }
        }

        /// <summary>
        ///     Executes a query against a database
        /// </summary>
        /// <param name="query">Command to execute</param>
        /// <param name="connection">Database connection to use</param>
        /// <param name="isStoredProcedure">Flag to indicate if the query is a stored procedure</param>
        /// <returns>Command results in a data table</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static DataTable ExecuteQuery(DatabaseCommand query, DbConnection connection,
            bool isStoredProcedure = false)
        {
            //open connection if connection is not open
            var closeConnection = false;
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
                closeConnection = true;
            }

            try
            {
                //create a db command object using the query and db connection
                using (var dbCommand = connection.CreateCommand())
                {
                    var sqlTransaction = GetTransaction(connection);
                    if (sqlTransaction != null)
                    {
                        dbCommand.Transaction = sqlTransaction;
                    }

                    dbCommand.CommandText = query.Command;
                    if (query.Timeout != null)
                    {
                        dbCommand.CommandTimeout = query.Timeout.Value;
                    }

                    if (isStoredProcedure)
                    {
                        //command is stored procedure, set command type to stored procedure
                        dbCommand.CommandType = CommandType.StoredProcedure;
                    }

                    foreach (var queryParam in query.Parameters)
                    {
                        //add all the command paramenter to db command
                        var parameter = dbCommand.CreateParameter();
                        parameter.ParameterName = queryParam.ParameterName;
                        parameter.Value = queryParam.ParameterValue;
                        dbCommand.Parameters.Add(parameter);
                    }

                    //execute the sql query against the database 
                    using (var dr = dbCommand.ExecuteReader())
                    {
                        //load the data into a data table
                        var dTable = new DataTable
                        {
                            Locale = new CultureInfo("en-us")
                        };
                        dTable.Load(dr);

                        if (closeConnection)
                        {
                            connection.Close();
                        }

                        return dTable;
                    }
                }
            }
            finally
            {
                if (closeConnection)
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        ///     Executes a query against a database
        /// </summary>
        /// <param name="query">Command to execute</param>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="isStoredProcedure">Flag to indicate if the query is a stored procedure</param>
        /// <returns>Command results in a data table</returns>
        public static DataTable ExecuteQuery(DatabaseCommand query, string connectionString,
            bool isStoredProcedure = false)
        {
            //create a db connection object using the connection string
            using (var dbConnection = CreateConnection())
            {
                dbConnection.ConnectionString = connectionString;

                //execute the query using the connection
                return ExecuteQuery(query, dbConnection, isStoredProcedure);
            }
        }


        /// <summary>
        ///     Executes a scalar query against a database
        /// </summary>
        /// <param name="query">Command to execute</param>
        /// <param name="connection"></param>
        /// <param name="isStoredProcedure">Flag to indicate if the query is a stored procedure</param>
        /// <returns>Command result</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static object ExecuteScalar(DatabaseCommand query, DbConnection connection,
            bool isStoredProcedure = false)
        {
            //open connection if connection is not open
            var closeConnection = false;
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
                closeConnection = true;
            }

            try
            {
                //create a db command object using the command and db connection
                using (var dbCommand = connection.CreateCommand())
                {
                    var sqlTransaction = GetTransaction(connection);
                    if (sqlTransaction != null)
                    {
                        dbCommand.Transaction = sqlTransaction;
                    }

                    dbCommand.CommandText = query.Command;
                    if (query.Timeout != null)
                    {
                        dbCommand.CommandTimeout = query.Timeout.Value;
                    }

                    if (isStoredProcedure)
                    {
                        //command is stored procedure, set command type to stored procedure
                        dbCommand.CommandType = CommandType.StoredProcedure;
                    }

                    foreach (var queryParam in query.Parameters)
                    {
                        //add all the command paramenter to db command
                        var parameter = dbCommand.CreateParameter();
                        parameter.ParameterName = queryParam.ParameterName;
                        parameter.Value = queryParam.ParameterValue;
                        dbCommand.Parameters.Add(parameter);
                    }

                    //execute the query against the database 
                    var result = dbCommand.ExecuteScalar();

                    if (closeConnection)
                    {
                        connection.Close();
                    }

                    return result;
                }
            }
            finally
            {
                if (closeConnection)
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        ///     Executes a scalar query against a database
        /// </summary>
        /// <param name="query">Command to execute</param>
        /// <param name="connectionString">Database conection string</param>
        /// <param name="isStoredProcedure">Flag to indicate if the query is a stored procedure</param>
        /// <returns>Command result</returns>
        public static object ExecuteScalar(DatabaseCommand query, string connectionString,
            bool isStoredProcedure = false)
        {
            //create a db connection object using the connection string
            using (var dbConnection = CreateConnection())
            {
                dbConnection.ConnectionString = connectionString;

                //execute the query using the connection
                return ExecuteScalar(query, dbConnection, isStoredProcedure);
            }
        }

        #endregion
    }
}