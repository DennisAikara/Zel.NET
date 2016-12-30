// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Zel.DataAccess.Exceptions
{
    public class MultipleDomainModelsInAssemblyException : Exception
    {
        public MultipleDomainModelsInAssemblyException(Assembly assembly)
        {
            AssemblyName = assembly.FullName;
        }

        public string AssemblyName { get; private set; }
    }
}