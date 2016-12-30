// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using Zel.Helpers.HelperClasses;

namespace Zel.Helpers
{
    /// <summary>
    ///     Assembly helper class
    /// </summary>
    public static class AssemblyHelper
    {
        #region Methods

        public static void LoadAssembliesWithAssemblyAttribute(string directory, Type attributeType)
        {
            var loadedAssemblies = (from a in AppDomain.CurrentDomain.GetAssemblies().AsParallel()
                select a.FullName).ToList();

            //get the list of dlls in the specified directory
            var directoryInfo = new DirectoryInfo(directory);
            var fileInfos = directoryInfo.GetFiles("*.dll");

            //loop through all the dlls and load them if the assembly has the specified attribute
            foreach (var fileInfo in fileInfos)
            {
                var assemblyName = AssemblyName.GetAssemblyName(fileInfo.FullName);
                var externalAssemblyHasAttribute = ExternalAssemblyHasAttribute(assemblyName, attributeType);
                if (!externalAssemblyHasAttribute)
                {
                    continue;
                }
                if (loadedAssemblies.All(x => x != assemblyName.FullName))
                {
                    Assembly.LoadFrom(fileInfo.FullName);
                }
            }
        }

        public static List<Assembly> GetAssembliesWithAssemblyAttribute(Type attributeType)
        {
            return (from a in AppDomain.CurrentDomain.GetAssemblies().AsParallel()
                where a.GetCustomAttributes(attributeType, false).Any()
                select a).ToList();
        }


        /// <summary>
        ///     Finds all the types in the specified assembly that derivies from the specified type
        /// </summary>
        /// <param name="type">Derivied type</param>
        /// <param name="assembly">Assembly to search</param>
        /// <returns>List of types</returns>
        public static List<Type> FindTypesThatAreSubclassOfClass(Type type, Assembly assembly)
        {
            if (!type.IsGenericType)
            {
                return (from t in assembly.GetTypes().AsParallel()
                    where t.IsClass && t.IsSubclassOf(type)
                    select t).ToList();
            }

            return (from t in assembly.GetTypes().AsParallel()
                where t.IsClass && (t.BaseType != null) && t.BaseType.IsGenericType
                      && (t.BaseType.GetGenericTypeDefinition() == type)
                select t).ToList();
        }

        /// <summary>
        ///     Finds all the types in the specified assembly that implements the specified interface
        /// </summary>
        /// <param name="type">Interface type</param>
        /// <param name="assembly">Assembly to search</param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns>List of types</returns>
        public static List<Type> FindTypesThatImplementInterface(Type type, Assembly assembly)
        {
            if (!type.IsInterface)
            {
                //T should be an interface
                throw new InvalidOperationException("Specified type is not an interface");
            }

            var types = (from t in assembly.GetTypes().AsParallel()
                where t.IsClass && t.GetInterfaces().Contains(type)
                select t).ToList();

            return types;
        }

        /// <summary>
        ///     Find all the types in the specified assembly that implement the specified interface
        ///     without loading the assembly into the current app domain
        /// </summary>
        /// <typeparam name="T">Interface type</typeparam>
        /// <param name="externalAssembly">Assembly to search</param>
        /// <returns>List of type names</returns>
        public static List<string> FindTypesThatImplementInterfaceInExternalAssembly<T>(AssemblyName externalAssembly)
        {
            //create a new appdomain
            var evidence = new Evidence(AppDomain.CurrentDomain.Evidence);
            var appDomainSetup = new AppDomainSetup();
            appDomainSetup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;

            var appDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString(), evidence, appDomainSetup);

            //create and instance of ExternalAssemblySearcher in the newly created app domain
            var externalAssemblySearcherName = typeof(ExternalAssemblySearcher).FullName ?? "";
            var externalAssemblySearcher = appDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName,
                externalAssemblySearcherName) as ExternalAssemblySearcher;

            if (externalAssemblySearcher == null)
            {
                return new List<string>();
            }

            //execute ExternalAssemblySearcher to get any matching types
            var plugins = externalAssemblySearcher.FindClassesInAssemblyThatImplementInterface<T>(externalAssembly);

            //unload the appdomain
            AppDomain.Unload(appDomain);

            return plugins;
        }

        /// <summary>
        ///     Find all the types in the specified assembly that implement the specified interface
        ///     without loading the assembly into the current app domain
        /// </summary>
        /// <typeparam name="T">Interface type</typeparam>
        /// <param name="externalAssembly">Assembly to search</param>
        /// <returns>List of type names</returns>
        public static List<string> FindTypesThatInheritsTypeInExternalAssembly<T>(AssemblyName externalAssembly)
        {
            //create a new appdomain
            var evidence = new Evidence(AppDomain.CurrentDomain.Evidence);
            var appDomainSetup = new AppDomainSetup();
            appDomainSetup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;

            var appDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString(), evidence, appDomainSetup);

            //create and instance of ExternalAssemblySearcher in the newly created app domain
            var externalAssemblySearcherName = typeof(ExternalAssemblySearcher).FullName ?? "";
            var externalAssemblySearcher = appDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName,
                externalAssemblySearcherName) as ExternalAssemblySearcher;

            if (externalAssemblySearcher == null)
            {
                return new List<string>();
            }

            //execute ExternalAssemblySearcher to get any matching types
            var plugins = externalAssemblySearcher.FindClassesInAssemblyThatInheritType<T>(externalAssembly);

            //unload the appdomain
            AppDomain.Unload(appDomain);

            return plugins;
        }

        public static bool ExternalAssemblyHasAttribute(AssemblyName externalAssemblyName, Type attributeType)
        {
            //create a new appdomain
            var evidence = new Evidence(AppDomain.CurrentDomain.Evidence);
            var appDomainSetup = new AppDomainSetup();
            appDomainSetup.ApplicationBase = Application.BinDirectory;

            var appDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString(), evidence, appDomainSetup);

            //create and instance of ExternalAssemblySearcher in the newly created app domain
            var externalAssemblySearcherName = typeof(ExternalAssemblySearcher).FullName ?? "";
            var externalAssemblySearcher = appDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName,
                externalAssemblySearcherName) as ExternalAssemblySearcher;

            if (externalAssemblySearcher == null)
            {
                throw new ApplicationException("Unable to instantiate ExternalAssemblySearcher");
            }

            //execute ExternalAssemblySearcher to get any matching types
            var hasAssembly = externalAssemblySearcher.AssemblyHasAttribute(externalAssemblyName, attributeType);

            //unload the appdomain
            AppDomain.Unload(appDomain);

            return hasAssembly;
        }

        #endregion
    }
}