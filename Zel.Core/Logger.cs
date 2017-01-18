// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Zel.Classes;
using Zel.Logging;

namespace Zel
{
    public class Logger : ILogger
    {
        #region Nested type: LogType

        /// <summary>
        ///     Log type
        /// </summary>
        public enum LogType
        {
            Trace,
            Debug,
            Information,
            Warning,
            Exception,
            Critical
        }

        #endregion

        private readonly ILogStore _logStore;
        private readonly LogType _minimumLogLevel;

        #region Constructor

        public Logger(ILogStore logStore, Guid batchIdentifier, string minimumLogLevel = "Information")
        {
            if (logStore == null)
            {
                throw new ArgumentNullException(nameof(logStore));
            }

            switch (minimumLogLevel)
            {
                case "Trace":
                    _minimumLogLevel = LogType.Trace;
                    break;
                case "Debug":
                    _minimumLogLevel = LogType.Debug;
                    break;
                default:
                    _minimumLogLevel = LogType.Information;
                    break;
            }

            _logStore = logStore;
            if (batchIdentifier==null)
            {
                batchIdentifier = Guid.NewGuid();
            }
            BatchIdentifier = batchIdentifier;
            LogTrace("Logger created with log level: ", _minimumLogLevel);
        }

        #endregion

        #region Nested type: ExceptionDetail

        private class ExceptionDetail
        {
            public ExceptionDetail(Exception exception, string description = null)
            {
                if (exception == null)
                {
                    throw new ArgumentNullException("exception");
                }

                Description = description;
                ExceptionType = exception.GetType().FullName;
                ExceptionMessage = exception.Message;
                Source = exception.Source ?? string.Empty;
                StackTrace = exception.StackTrace ?? string.Empty;
                if (exception.Data.Count > 0)
                {
                    Data = new List<DictionaryEntry>();
                    foreach (DictionaryEntry data in exception.Data)
                    {
                        Data.Add(data);
                    }
                }
                if (exception.InnerException != null)
                {
                    InnerException = new ExceptionDetail(exception.InnerException);
                }
            }


            public string Description { get; private set; }
            public string ExceptionType { get; set; }
            public string ExceptionMessage { get; private set; }
            public string Source { get; private set; }
            public string StackTrace { get; private set; }
            public ExceptionDetail InnerException { get; private set; }
            public List<DictionaryEntry> Data { get; }
        }

        #endregion

        #region ILogger Members

        public Guid BatchIdentifier { get; }

        /// <summary>
        ///     Logs trace
        /// </summary>
        /// <param name="message"> Message to log </param>
        public void LogTrace(params object[] message)
        {
            if (LogType.Trace >= _minimumLogLevel)
            {
                Log(message, LogType.Trace);
            }
        }

        /// <summary>
        ///     Logs debug
        /// </summary>
        /// <param name="message"> Message to log </param>
        public void LogDebug(params object[] message)
        {
            if (LogType.Debug >= _minimumLogLevel)
            {
                Log(message, LogType.Debug);
            }
        }

        /// <summary>
        ///     Logs information
        /// </summary>
        /// <param name="message"> Message to log </param>
        public void LogInformation(params object[] message)
        {
            Log(message, LogType.Information);
        }

        /// <summary>
        ///     Logs warning
        /// </summary>
        /// <param name="message"> Message to log </param>
        public void LogWarning(params object[] message)
        {
            Log(message, LogType.Warning);
        }

        /// <summary>
        ///     Logs critical
        /// </summary>
        /// <param name="message"> Message to log </param>
        public void LogCritical(params object[] message)
        {
            Log(message, LogType.Critical);
        }

        /// <summary>
        ///     Logs exception
        /// </summary>
        /// <param name="exception"> Exception to log </param>
        /// <param name="description"> Any additional description about the exception to log </param>
        public void LogException(Exception exception, params object[] description)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            var descriptionMessage = description.Where(x => (x.GetType() != typeof(LogCode))
                                                            && (x.GetType() != typeof(LogData))).ToArray();
            var exceptionDetail = new ExceptionDetail(exception, JoinMessage(descriptionMessage)).ToJson();

            var message = description.Where(x => (x.GetType() == typeof(LogCode))
                                                 || (x.GetType() == typeof(LogData))).Select(x => x).ToList();
            message.Add(exceptionDetail);

            Log(message, LogType.Exception);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Log
        /// </summary>
        /// <param name="message"> Message to log </param>
        /// <param name="type"> Log type </param>
        private void Log(IEnumerable<object> message, LogType type)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            var messageList = message as IList<object> ?? message.ToList();
            var messageString = JoinMessage(messageList.Where(x => (x.GetType() != typeof(LogCode))
                                                                   && (x.GetType() != typeof(LogData))
                                                                   && (x.GetType() != typeof(LogSource))).ToArray());
            if (string.IsNullOrWhiteSpace(messageString))
            {
                return;
            }

            var logCode = messageList.OfType<LogCode>().FirstOrDefault();
            var logData = messageList.OfType<LogData>().ToList();
            var logSource = messageList.OfType<LogSource>().FirstOrDefault();

            //create LogMessage
            var logMessage = new LogMessage(messageString, type.ToString(), logCode, logData, logSource, BatchIdentifier);


            if (_logStore != null)
            {
                if (_logStore.WriteToLog(logMessage))
                {
                    //handled
                    return;
                }
            }

            var fileLogStore = new FileLogStore();
            fileLogStore.WriteToLog(logMessage);
        }

        private static string JoinMessage(object[] message)
        {
            return string.Join(" ", message).Replace("  ", " ");
        }

        #endregion
    }
}