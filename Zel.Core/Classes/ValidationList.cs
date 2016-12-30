// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Zel.Classes
{
    /// <summary>
    ///     Holds validation messages
    /// </summary>
    public class ValidationList : List<ValidationMessage>
    {
        public ValidationList() {}

        public ValidationList(string message)
        {
            Add(new ValidationMessage(null, message));
        }

        public ValidationList(string name, string message)
        {
            Add(new ValidationMessage(name, message));
        }

        /// <summary>
        ///     Returns true if validation is sucessfull
        /// </summary>
        public bool IsValid
        {
            get { return Count == 0; }
        }

        /// <summary>
        ///     Adds a validation message to the validation list
        /// </summary>
        /// <param name="name">Validation name</param>
        /// <param name="message">Validation message</param>
        public void Add(string name, string message)
        {
            Add(new ValidationMessage(name, message));
        }
    }
}