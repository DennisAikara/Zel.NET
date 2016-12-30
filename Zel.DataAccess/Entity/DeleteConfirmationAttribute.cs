// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Zel.DataAccess.Entity
{
    /// <summary>
    ///     Delete confirmation message
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DeleteConfirmationAttribute : Attribute
    {
        /// <summary>
        ///     Initialize a new instance of DeleteConfirmationAttribute
        /// </summary>
        /// <param name="confirmationMessage">Delete confirmation message</param>
        public DeleteConfirmationAttribute(string confirmationMessage)
        {
            ConfirmationMessage = confirmationMessage;
        }

        /// <summary>
        ///     Delete confirmation message
        /// </summary>
        public string ConfirmationMessage { get; private set; }
    }
}