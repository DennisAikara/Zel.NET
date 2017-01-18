// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Zel.Classes;

namespace Zel.DataAccess
{
    public class SqlLogStore : LogStore
    {
        private static string _sqlDatabaseConnectionString;
        private static Func<string> _getCurrentContextUser;

        public SqlLogStore(string sqlDatabaseConnectionString, Func<string> getCurrentContextUser)
        {
            if (sqlDatabaseConnectionString == null)
            {
                throw new ArgumentNullException("sqlDatabaseConnectionString");
            }

            _sqlDatabaseConnectionString = sqlDatabaseConnectionString;
            _getCurrentContextUser = getCurrentContextUser;
        }

        public override bool WriteToLog(LogMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            const string command = @"
            INSERT INTO
                [dbo].[Log]
            SELECT
                @BatchIdentifier,
                @Type,
                @Url,
                @UrlReferrer,
                @Message,
                @Data,
                @Source,
                @SourceId,
                @Code,
                @CreatedOn,
                @CreatedBy";

            var databaseCommand = new DatabaseCommand(command);

            databaseCommand.Parameters.Add(new DatabaseCommandParameter("@BatchIdentifier", message.BatchIdentifier));

            databaseCommand.Parameters.Add(new DatabaseCommandParameter("@Type", message.Type));

            if ((message.Url != null) && (message.Url.Length > 450))
            {
                databaseCommand.Parameters.Add(new DatabaseCommandParameter("@Url", message.Url.Substring(0, 450)));
            }
            else
            {
                databaseCommand.Parameters.Add(message.Url == null
                    ? new DatabaseCommandParameter("@Url", DBNull.Value)
                    : new DatabaseCommandParameter("@Url", message.Url));
            }

            if ((message.UrlReferrer != null) && (message.UrlReferrer.Length > 450))
            {
                databaseCommand.Parameters.Add(new DatabaseCommandParameter("@UrlReferrer",
                    message.UrlReferrer.Substring(0, 450)));
            }
            else
            {
                databaseCommand.Parameters.Add(message.UrlReferrer == null
                    ? new DatabaseCommandParameter("@UrlReferrer", DBNull.Value)
                    : new DatabaseCommandParameter("@UrlReferrer", message.UrlReferrer));
            }

            databaseCommand.Parameters.Add(new DatabaseCommandParameter("@Message", message.Message));
            databaseCommand.Parameters.Add(new DatabaseCommandParameter("@MachineName", message.MachineName));
            databaseCommand.Parameters.Add(new DatabaseCommandParameter("@ApplicationPath", message.ApplicationPath));
            databaseCommand.Parameters.Add((message.Data == null) || (message.Data.Count == 0)
                ? new DatabaseCommandParameter("@Data", DBNull.Value)
                : new DatabaseCommandParameter("@Data", message.Data.ToJson()));
            databaseCommand.Parameters.Add(message.Source == null
                ? new DatabaseCommandParameter("@Source", DBNull.Value)
                : new DatabaseCommandParameter("@Source", message.Source));
            databaseCommand.Parameters.Add(message.SourceId == null
                ? new DatabaseCommandParameter("@SourceId", DBNull.Value)
                : new DatabaseCommandParameter("@SourceId", message.SourceId));
            databaseCommand.Parameters.Add(message.Code == null
                ? new DatabaseCommandParameter("@Code", DBNull.Value)
                : new DatabaseCommandParameter("@Code", message.Code));
            databaseCommand.Parameters.Add(new DatabaseCommandParameter("@Handled", false));
            databaseCommand.Parameters.Add(new DatabaseCommandParameter("@CreatedOn", message.TimeStamp));

            var currentUser = string.Empty;
            if (_getCurrentContextUser != null)
            {
                currentUser = _getCurrentContextUser.Invoke();
            }
            databaseCommand.Parameters.Add(string.IsNullOrWhiteSpace(currentUser)
                ? new DatabaseCommandParameter("@CreatedBy", message.ApplicationUserName)
                : new DatabaseCommandParameter("@CreatedBy", currentUser));

            Sql.ExecuteCommand(databaseCommand, _sqlDatabaseConnectionString);


            return true;
        }
    }
}