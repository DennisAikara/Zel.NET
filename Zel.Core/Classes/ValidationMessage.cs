// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Zel.Classes
{
    /// <summary>
    ///     Validation message
    /// </summary>
    public class ValidationMessage
    {
        public ValidationMessage(string name, string message)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = string.Empty;
            }
            Name = name;
            Message = message;
        }

        /// <summary>
        ///     Name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     Validation message
        /// </summary>
        public string Message { get; private set; }
    }
}