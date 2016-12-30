// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Zel.Helpers.HelperClasses
{
    [Serializable]
    public class ExternalAssemblySearcher : MarshalByRefObject
    {
        public List<string> FindClassesInAssemblyThatImplementInterface<T>(AssemblyName assemblyName)
        {
            var assembly = Assembly.LoadFrom(assemblyName.CodeBase);
            IEnumerable<string> assemblies = from t in assembly.GetTypes().AsParallel()
                where t.IsClass && t.GetInterfaces().Contains(typeof(T))
                select t.FullName;

            return assemblies.ToList();
        }

        public List<string> FindClassesInAssemblyThatInheritType<T>(AssemblyName assemblyName)
        {
            var assembly = Assembly.LoadFrom(assemblyName.CodeBase);
            IEnumerable<string> assemblies = from t in assembly.GetTypes().AsParallel()
                where t.IsSubclassOf(typeof(T))
                select t.FullName;

            return assemblies.ToList();
        }

        public bool AssemblyHasAttribute(AssemblyName assemblyName, Type type)
        {
            var assembly = Assembly.LoadFrom(assemblyName.CodeBase);
            return assembly.GetCustomAttributes(type, false).Any();
        }
    }
}