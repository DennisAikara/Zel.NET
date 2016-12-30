// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Zel.DataAccess.Entity;

namespace Zel.DataAccess.Exceptions
{
    public class EntityMissingTableAttributeException : Exception
    {
        public EntityMissingTableAttributeException(Type entityType)
        {
            if (entityType.GetInterface(typeof(IEntity).FullName) == null)
            {
                throw new ArgumentException(string.Concat("Cannot create EntityMissingTableAttributeException. ",
                    entityType.FullName, " is not an entity."));
            }


            EntityType = entityType.FullName;
        }

        public string EntityType { get; private set; }
    }
}