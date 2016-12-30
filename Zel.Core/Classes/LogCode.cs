// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Zel.Classes
{
    /// <summary>
    ///     Log code
    /// </summary>
    public class LogCode
    {
        /// <summary>
        ///     Creates a new instance of LogCode
        /// </summary>
        /// <param name="code">Log code</param>
        public LogCode(string code)
        {
            if (code == null)
            {
                throw new ArgumentNullException("code");
            }

            if (code.Trim().Length != 5)
            {
                throw new ArgumentException("Log code must be five characters in length", "code");
            }

            Code = code;
        }

        public string Code { get; private set; }
    }
}