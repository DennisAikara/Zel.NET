// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Zel.DataAccess.Exceptions
{
    public class InvalidParentEntityException : Exception
    {
        public InvalidParentEntityException(Type relatedEntity)
        {
            RelatedEntity = relatedEntity.FullName;
        }

        public string RelatedEntity { get; set; }
    }
}