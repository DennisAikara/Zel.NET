// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Zel.DataAccess.Entity
{
    /// <summary>
    ///     Unique constraint attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class UniqueConstraintAttribute : Attribute
    {
        #region Constructors

        /// <summary>
        ///     Initialize a new instance of UniqueConstraintAttribute
        /// </summary>
        /// <param name="fields">Unique constraint fields</param>
        public UniqueConstraintAttribute(string name, params string[] fields)
        {
            Name = name;
            Fields = fields;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Unique constraint name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Unique constraint fields
        /// </summary>
        public string[] Fields { get; private set; }

        #endregion
    }
}