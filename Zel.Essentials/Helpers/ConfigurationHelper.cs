// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Configuration;

namespace Zel.Helpers
{
    /// <summary>
    ///     Configuration helper
    /// </summary>
    public static class ConfigurationHelper
    {
        /// <summary>
        ///     Get the specified connection string from the config file
        /// </summary>
        /// <param name="connectionStringName">Connection string name</param>
        /// <returns>Connection string</returns>
        public static string GetConnectionString(string connectionStringName)
        {
            //get the connection string from config file
            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName];

            return connectionString != null ? connectionString.ToString() : null;
        }

        /// <summary>
        ///     Get the specified connection string's provider name from the config file
        /// </summary>
        /// <param name="connectionStringName">Connection string name</param>
        /// <returns>Connection string's provider name</returns>
        public static string GetConnectionStringProvider(string connectionStringName)
        {
            //get the connection string from config file
            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName];

            return connectionString != null ? connectionString.ProviderName : null;
        }
    }
}