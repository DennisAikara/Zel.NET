// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

namespace Zel.Classes
{
    /// <summary>
    ///     Database command parameter
    /// </summary>
    [DebuggerDisplay("{ParameterName}")]
    public class DatabaseCommandParameter
    {
        #region Constructors

        /// <summary>
        ///     Instatiate a new DatabaseCommand parameter
        /// </summary>
        /// <param name="parameterName">Parameter Name</param>
        /// <param name="parameterValue">Parameter Value</param>
        public DatabaseCommandParameter(string parameterName, object parameterValue)
        {
            ParameterName = parameterName;
            ParameterValue = parameterValue;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Parameter Name
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        ///     Parameter value
        /// </summary>
        public object ParameterValue { get; private set; }

        #endregion
    }
}