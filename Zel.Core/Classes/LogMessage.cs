// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Zel.Classes
{
    public class LogMessage
    {
        #region Constructors

        /// <summary>
        ///     Instantiate a new log message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="type">Log type</param>
        /// <param name="code">Log code</param>
        /// <param name="data">Log Data</param>
        /// <param name="source"></param>
        /// <param name="batchIdentifier"></param>
        public LogMessage(string message, string type, LogCode code, List<LogData> data, LogSource source,
            Guid batchIdentifier)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Invalid message", "message");
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentException("Invalid log type", "type");
            }

            //set log message properties
            Type = type;
            Data = data;
            Message = message;
            if (code != null)
            {
                Code = code.Code;
            }
            TimeStamp = DateTime.UtcNow;
            BatchIdentifier = batchIdentifier;
            ApplicationPath = Application.RootDirectory;
            MachineName = Application.MachineName;
            ApplicationUserName = Application.ApplicationUserName;
            Url = Asp.GetUrl();
            UrlReferrer = Asp.GetUrlReferrer();

            if (source != null)
            {
                Source = source.Source;
                SourceId = source.SourceId;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Message Id
        /// </summary>
        public Guid BatchIdentifier { get; private set; }

        /// <summary>
        ///     Log creation time
        /// </summary>
        public DateTime TimeStamp { get; private set; }

        /// <summary>
        ///     Applications physical path
        /// </summary>
        public string ApplicationPath { get; private set; }

        /// <summary>
        ///     Machine name
        /// </summary>
        public string MachineName { get; private set; }

        /// <summary>
        ///     Url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        ///     Url Referrer
        /// </summary>
        public string UrlReferrer { get; set; }

        /// <summary>
        ///     Source
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///     Source Id
        /// </summary>
        public string SourceId { get; set; }

        /// <summary>
        ///     Type of log
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        ///     Message to log
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        ///     Log Code
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        ///     Username of the account that the application is running under
        /// </summary>
        public string ApplicationUserName { get; private set; }

        /// <summary>
        ///     Additional data
        /// </summary>
        public List<LogData> Data { get; set; }

        #endregion
    }
}