// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Zel.DataAccess.Entity;

namespace Zel.DataAccess.Exceptions
{
    public class InvalidEntityUniqueConstraintFieldException : Exception
    {
        public InvalidEntityUniqueConstraintFieldException(string message, Type entityType) : base(message)
        {
            if (entityType.GetInterface(typeof(IEntity).FullName) == null)
            {
                throw new ArgumentException(string.Concat("Cannot create InvalidEntityUniqueConstraintFieldException. ",
                    entityType.FullName, " is not an entity."));
            }

            EntityType = entityType.FullName;
        }

        public string EntityType { get; private set; }
    }
}