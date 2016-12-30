// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Zel.DataAccess.Entity;

namespace Zel.DataAccess.Exceptions
{
    public class InvalidEntityModelNameException : Exception
    {
        public InvalidEntityModelNameException(Type entityModelType)
        {
            if (entityModelType.BaseType != typeof(EntityModel<>))
            {
                throw new ArgumentException(string.Concat("Cannot create InvalidEntityModelNameException. ",
                    entityModelType.FullName, " is not an entity model."));
            }

            EntityModelType = entityModelType.FullName;
        }

        public string EntityModelType { get; set; }
    }
}