// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;

namespace Zel.Classes
{
    /// <summary>
    ///     Database Command
    /// </summary>
    [DebuggerDisplay("{Command}")]
    public class DatabaseCommand
    {
        #region Properties

        /// <summary>
        ///     Database Command
        /// </summary>
        public string Command { get; set; }

        public int? Timeout { get; set; }

        /// <summary>
        ///     Database command parameters
        /// </summary>
        public List<DatabaseCommandParameter> Parameters { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Instatiate a new DatabaseCommand
        /// </summary>
        /// <param name="command">Database Command</param>
        public DatabaseCommand(string command) : this()
        {
            Command = command;
        }

        /// <summary>
        ///     Instatiate a new DatabaseCommand
        /// </summary>
        public DatabaseCommand()
        {
            //Instatiate parameter list
            Parameters = new List<DatabaseCommandParameter>();
        }

        #endregion
    }
}