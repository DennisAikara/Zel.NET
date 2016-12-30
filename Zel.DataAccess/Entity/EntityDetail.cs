// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Zel.DataAccess.Entity
{
    /// <summary>
    ///     Entity Detail
    /// </summary>
    public class EntityDetail
    {
        public EntityDetail()
        {
            Parents = new List<EntityParent>();
            Children = new List<EntityChild>();
            UniqueConstraints = new Dictionary<string, UniqueConstraintAttribute>();
        }

        /// <summary>
        ///     Flag to indicate if the entity is a view
        /// </summary>
        public bool IsEntityView { get; set; }

        /// <summary>
        ///     Entity Type
        /// </summary>
        public Type EnityType { get; set; }

        /// <summary>
        ///     Entity's context type
        /// </summary>
        public Type ContextType { get; set; }

        /// <summary>
        ///     Entity's model type
        /// </summary>
        public Type ModelType { get; set; }

        /// <summary>
        ///     Entity's key property name
        /// </summary>
        public string KeyName { get; set; }

        /// <summary>
        ///     Entity's key property type
        /// </summary>
        public Type KeyType { get; set; }

        /// <summary>
        ///     Entity's key property
        /// </summary>
        public PropertyInfo KeyProperty { get; set; }

        /// <summary>
        ///     Entity's database table name
        /// </summary>
        public string DbTableName { get; set; }

        /// <summary>
        ///     Entity's database schema name
        /// </summary>
        public string DbSchemaName { get; set; }

        /// <summary>
        ///     Entity's display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        ///     Entity's EntitySet name
        /// </summary>
        public string EntitySetName { get; set; }

        //efEntityInfo.DbSetProperty = context.GetType().GetProperty(dbSetName);
        /// <summary>
        ///     Entity's DbSet property
        /// </summary>
        public PropertyInfo DbSetProperty { get; set; }

        /// <summary>
        ///     Entity's delete confirmation message
        /// </summary>
        public string DeleteConfirmationMessage { get; set; }

        /// <summary>
        ///     List of entity's unique constraints
        /// </summary>
        public Dictionary<string, UniqueConstraintAttribute> UniqueConstraints { get; set; }

        /// <summary>
        ///     List of entity's parents
        /// </summary>
        public List<EntityParent> Parents { get; set; }

        /// <summary>
        ///     List of entity's children
        /// </summary>
        public List<EntityChild> Children { get; set; }

        public string Name { get; set; }
    }
}