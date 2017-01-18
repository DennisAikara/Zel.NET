// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Zel
{
    public interface ILogger
    {
        Guid BatchIdentifier { get; }

        /// <summary>
        ///     Logs trace
        /// </summary>
        /// <param name="message"> Message to log </param>
        void LogTrace(params object[] message);

        /// <summary>
        ///     Logs debug
        /// </summary>
        /// <param name="message"> Message to log </param>
        void LogDebug(params object[] message);

        /// <summary>
        ///     Logs information
        /// </summary>
        /// <param name="message"> Message to log </param>
        void LogInformation(params object[] message);

        /// <summary>
        ///     Logs warning
        /// </summary>
        /// <param name="message"> Message to log </param>
        void LogWarning(params object[] message);

        /// <summary>
        ///     Logs critical
        /// </summary>
        /// <param name="message"> Message to log </param>
        void LogCritical(params object[] message);

        /// <summary>
        ///     Logs exception
        /// </summary>
        /// <param name="exception"> Exception to log </param>
        /// <param name="description"> Any additional description about the exception to log </param>
        void LogException(Exception exception, params object[] description);
    }
}