// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Zel
{
    /// <summary>
    ///     Reflection helper class
    /// </summary>
    public static class Reflection
    {
        #region Class Methods

        /// <summary>
        ///     Gets all the attributes that belongs to the specified type of the specified type
        /// </summary>
        /// <typeparam name="T">Attribute type</typeparam>
        /// <param name="type">Type</param>
        /// <returns>List of attributes</returns>
        public static List<T> GetClassAttributes<T>(Type type) where T : class
        {
            return type.GetCustomAttributes(typeof(T), false).Select(x => x as T).ToList();
        }

        #endregion

        #region Misc Methods

        /// <summary>
        ///     Change the object type to the specified type
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="value">Object</param>
        /// <returns>Converted object</returns>
        public static T ChangeType<T>(object value)
        {
            if (value == null)
            {
                return default(T);
            }

            var typeConverter = TypeDescriptor.GetConverter(typeof(T));

            if (value is Guid)
            {
                return (T) typeConverter.ConvertFrom(value.ToString());
            }

            return (T) typeConverter.ConvertFrom(value);
        }

        #endregion

        #region Property Methods

        /// <summary>
        ///     Get all the attributes of the specified type for the specified type property
        /// </summary>
        /// <typeparam name="T">Attribute type</typeparam>
        /// <param name="type">Type</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="bindingFlags">Property BindingFlags</param>
        /// <returns>List of attributes</returns>
        public static List<T> GetPropertyAttributes<T>(Type type, string propertyName,
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance) where T : class
        {
            var property = type.GetProperty(propertyName, bindingFlags);
            var propertyAttributes = property.GetCustomAttributes(typeof(T), false);
            return propertyAttributes.Select(x => x as T).ToList();
        }

        /// <summary>
        ///     Indicates if the specified type property has the specified attribute
        /// </summary>
        /// <typeparam name="T">Attribute type</typeparam>
        /// <param name="type">Type</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="bindingFlags">Property BindingFlags</param>
        /// <returns>True if attribute exists, else false</returns>
        public static bool PropertyHasAttribute<T>(Type type, string propertyName,
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            var property = type.GetProperty(propertyName, bindingFlags);
            var propertyAttributes = property.GetCustomAttributes(typeof(T), false);

            return propertyAttributes.Length > 0;
        }

        /// <summary>
        ///     Get all the properties that has the specified attribute from the specified type
        /// </summary>
        /// <typeparam name="T">Attribute type</typeparam>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Property BindingFlags</param>
        /// <returns>List of property information</returns>
        public static List<PropertyInfo> GetPropertiesWithAttribute<T>(Type type,
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            var propeties = from p in type.GetProperties(bindingFlags)
                where p.GetCustomAttributes(typeof(T), false).Any()
                select p;
            return propeties.ToList();
        }

        /// <summary>
        ///     Get all the properties from the specified type
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Property BindingFlags</param>
        /// <returns>List of properties</returns>
        public static List<string> GetPropertyNames(Type type,
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            return type.GetProperties(bindingFlags).Select(x => x.Name).ToList();
        }

        /// <summary>
        ///     Get the specified property's value
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="obj">Object to extract the property value from</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>Property value</returns>
        public static T GetPropertyValue<T>(object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);
            return property != null ? ChangeType<T>(property.GetValue(obj, null)) : default(T);
        }

        /// <summary>
        ///     Get the specified property's value
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="obj">Object to extract the property value from</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>Property value</returns>
        public static object GetPropertyValue(object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);
            return property != null ? property.GetValue(obj, null) : null;
        }

        /// <summary>
        ///     Set the specified property's value
        /// </summary>
        /// <param name="obj">Object the property value be set to</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="value">Property value</param>
        public static void SetPropertyValue(object obj, string propertyName, object value)
        {
            var property = obj.GetType().GetProperty(propertyName);
            if (property != null)
            {
                property.SetValue(obj, value, null);
            }
        }

        #endregion
    }
}