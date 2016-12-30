// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Zel.DataAccess.Exceptions;

namespace Zel.DataAccess.Entity
{
    /// <summary>
    ///     Parent entity attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ParentEntityAttribute : Attribute
    {
        /// <summary>
        ///     Initialize a new instance of the ParentEntityAttribute
        /// </summary>
        /// <param name="relatedEntity">Primary key entity type</param>
        /// <param name="invalidParentMessage">Error message to display when parent is invalid</param>
        public ParentEntityAttribute(Type relatedEntity, string invalidParentMessage = null)
        {
            if (relatedEntity.GetInterface(typeof(IEntity).FullName) == null)
            {
                throw new InvalidParentEntityException(relatedEntity);
            }

            ParentEntity = relatedEntity;
            ErrorMessage = invalidParentMessage;
        }

        /// <summary>
        ///     Error message to display if validation fails
        /// </summary>
        public string ErrorMessage { get; private set; }


        /// <summary>
        ///     Parent entity type
        /// </summary>
        public Type ParentEntity { get; private set; }
    }
}