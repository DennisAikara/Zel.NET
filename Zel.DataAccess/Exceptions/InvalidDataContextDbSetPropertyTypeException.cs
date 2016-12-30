// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Zel.DataAccess.Exceptions
{
    public class InvalidDataContextDbSetPropertyTypeException : Exception
    {
        public InvalidDataContextDbSetPropertyTypeException(Type dataContextType, string dbSetName)
        {
            if (dataContextType.BaseType != typeof(DataContext))
            {
                throw new ArgumentException(string.Concat(
                    "Cannot create InvalidDataContextDbSetPropertyTypeException. ",
                    dataContextType.FullName, " is not a data context."));
            }

            DataContextType = dataContextType.FullName;
            DbSetName = dbSetName;
        }

        public string DataContextType { get; set; }
        public string DbSetName { get; set; }
    }
}