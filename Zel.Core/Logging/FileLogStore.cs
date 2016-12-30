// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Zel.Classes;

namespace Zel.Logging
{
    public class FileLogStore : LogStore
    {
        /// <summary>
        ///     Synchronization objects
        /// </summary>
        private static readonly Dictionary<string, object> Synchronization = new Dictionary<string, object>();

        /// <summary>
        ///     Streamwriters to use for writing log files
        /// </summary>
        private static readonly Dictionary<string, StreamWriter> LogWriter = new Dictionary<string, StreamWriter>();

        /// <summary>
        ///     The current log files used by the _logWriter
        /// </summary>
        private static readonly Dictionary<string, string> LogWriterFile = new Dictionary<string, string>();


        /// <summary>
        ///     Current log file
        /// </summary>
        private static string GetCurrentLogFile(string hostCode)
        {
            if (hostCode == null)
            {
                throw new ArgumentNullException("hostCode");
            }

            //Log file name format = 20091214-384984.msg
            return Path.Combine(Application.OutDirectory,
                string.Format("{0}-{1}.log", DateTime.UtcNow.ToString("yyyyMMddHH"), hostCode));
        }

        /// <summary>
        ///     Writes messageToLog to log
        /// </summary>
        /// <param name="message">Message to log</param>
        public override bool WriteToLog(LogMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            var hostCode = GetHostCode();
            var currentLogFile = GetCurrentLogFile(hostCode);

            //lock so code block at the host code level, so we don't write the log file
            //simultaneously
            if (!Synchronization.ContainsKey(hostCode))
            {
                Synchronization[hostCode] = new object();
            }

            var lockObject = Synchronization[hostCode];
            lock (lockObject)
            {
                //create logwrite and logwritefile entries if they don't exist
                if (!LogWriter.ContainsKey(hostCode))
                {
                    LogWriter[hostCode] = null;
                }

                if (!LogWriterFile.ContainsKey(hostCode))
                {
                    LogWriterFile[hostCode] = null;
                }

                //create a new logwriter if required
                if ((LogWriterFile[hostCode] != currentLogFile) || (LogWriter[hostCode] == null))
                {
                    //log file changed create new stream writer
                    if (LogWriter[hostCode] != null)
                    {
                        LogWriter[hostCode].Close();
                        LogWriter[hostCode].Dispose();
                    }

                    //create the file if it doesn't exist
                    if (!File.Exists(currentLogFile))
                    {
                        File.WriteAllText(currentLogFile, "");
                    }

                    LogWriter[hostCode] = File.AppendText(currentLogFile);
                    LogWriterFile[hostCode] = currentLogFile;
                }

                //write to file
                var jsonMessage = message.ToJson();
                LogWriter[hostCode].WriteLine(jsonMessage);
                LogWriter[hostCode].Flush();
                return true;
            }
        }
    }
}