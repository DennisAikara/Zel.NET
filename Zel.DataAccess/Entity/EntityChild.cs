// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Zel.DataAccess.Entity
{
    /// <summary>
    ///     Entity child
    /// </summary>
    public class EntityChild
    {
        #region Properties

        /// <summary>
        ///     Parent id field
        /// </summary>
        public string ParentIdField { get; set; }

        /// <summary>
        ///     Child entity type
        /// </summary>
        public Type ChildType { get; set; }

        #endregion
    }
}