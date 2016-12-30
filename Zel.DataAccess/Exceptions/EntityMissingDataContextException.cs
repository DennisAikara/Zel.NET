// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Zel.DataAccess.Entity;

namespace Zel.DataAccess.Exceptions
{
    public class EntityMissingDataContextException : Exception
    {
        public EntityMissingDataContextException(Type entityType) : base(entityType.FullName)
        {
            if (entityType.GetInterface(typeof(IEntity).FullName) == null)
            {
                throw new ArgumentException(string.Concat("Cannot create EntityMissingDataContextException. ",
                    entityType.FullName,
                    " is not an entity."));
            }
        }
    }
}