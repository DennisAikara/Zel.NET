// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Reflection;
using Zel.DataAccess.Entity;

namespace Zel.DataAccess
{
    public static class DataAccessExtensions
    {
        public static bool IsNew(this IEntity entity)
        {
            return GetKeyValue(entity).ToString() == "0";
        }

        public static object GetKeyValue(this IEntity entity)
        {
            return DataSession.GetEntityDetail(entity.GetType()).KeyProperty.GetValue(entity, null);
        }

        public static void SetKeyValue(this IEntity entity, object keyValue)
        {
            DataSession.GetEntityDetail(entity.GetType()).KeyProperty.SetValue(entity, keyValue);
        }

        private static object GetPropertyValue(object obj, string name)
        {
            return obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .First(x => x.Name == name).GetValue(obj, null);
        }

        public static string ToTraceString(this IQueryable query)
        {
            return
                ((ObjectQuery) GetPropertyValue(GetPropertyValue(query, "InternalQuery"), "ObjectQuery")).ToTraceString();
        }

        public static string ToTraceStringWithParameters(this IQueryable query)
        {
            var objectQuery = (ObjectQuery) GetPropertyValue(GetPropertyValue(query, "InternalQuery"), "ObjectQuery");
            var traceString = objectQuery.ToTraceString();
            foreach (var parameter in objectQuery.Parameters)
            {
                if (parameter.Value == null)
                {
                    traceString = traceString.Replace(" = @" + parameter.Name, " IS NULL");
                }
                else
                {
                    if ((parameter.ParameterType == typeof(bool)) || (parameter.ParameterType.IsGenericType
                                                                      &&
                                                                      (Nullable.GetUnderlyingType(
                                                                           parameter.ParameterType) ==
                                                                       typeof(bool))))
                    {
                        traceString = traceString.Replace("@" + parameter.Name,
                            Convert.ToBoolean(parameter.Value) ? "1" : "0");
                    }
                    else
                    {
                        traceString = traceString.Replace("@" + parameter.Name, parameter.Value.ToString());
                    }
                }
            }
            return traceString;
        }
    }
}