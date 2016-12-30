// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Zel.DataAccess.Entity
{
    /// <summary>
    ///     Entity parent
    /// </summary>
    public class EntityParent
    {
        #region Properties

        /// <summary>
        ///     Parent id field
        /// </summary>
        public string ParentIdField { get; set; }

        /// <summary>
        ///     Parent entity type
        /// </summary>
        public Type ParentType { get; set; }

        /// <summary>
        ///     Indicates if the parent is required
        /// </summary>
        public bool ParentRequired { get; set; }

        /// <summary>
        ///     Error message to display when the parent is invalid
        /// </summary>
        public string ErrorMessage { get; set; }

        #endregion
    }
}