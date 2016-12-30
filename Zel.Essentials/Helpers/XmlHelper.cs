// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Zel.Helpers
{
    public static class XmlHelper
    {
        /// <summary>
        ///     Serialize the specified object to XML
        /// </summary>
        /// <param name="objectToSerialize">Object to serialize</param>
        /// <param name="nameSpace">Namespace to use</param>
        /// <param name="omitXmlDeclaration">Indicate if the xml declartion should be ommitted</param>
        /// <returns>XML representation of the object</returns>
        public static string SerializeObjectToXml(object objectToSerialize, string nameSpace = null,
            bool omitXmlDeclaration = true)
        {
            if (objectToSerialize == null)
            {
                //can't serialize null, so return null back
                return null;
            }

            if (objectToSerialize is string)
            {
                //object is string simply return the string back
                return objectToSerialize.ToString();
            }

            var writerSettings = new XmlWriterSettings
            {
                OmitXmlDeclaration = omitXmlDeclaration
            };

            var ns = new XmlSerializerNamespaces();
            ns.Add("", nameSpace ?? "");

            using (var stringWriter = new StringWriter())
            {
                var xmlWriter = XmlWriter.Create(stringWriter, writerSettings);

                var xs = new XmlSerializer(objectToSerialize.GetType());
                xs.Serialize(xmlWriter, objectToSerialize, ns);

                return stringWriter.ToString();
            }
        }

        /// <summary>
        ///     Deserialize the specified XML string into the specified type
        /// </summary>
        /// <param name="xmlString">XML string representation of an object</param>
        /// <param name="objectType">Object type</param>
        /// <returns>Object representation of the xml string</returns>
        public static object DeserializeObjectFromXml(string xmlString, Type objectType)
        {
            if (xmlString == null)
            {
                //can't deserialize null, return null back
                return null;
            }

            using (var stringReader = new StringReader(xmlString))
            {
                var xmlReader = new XmlTextReader(stringReader);
                var ser = new XmlSerializer(objectType);

                return ser.Deserialize(xmlReader);
            }
        }

        /// <summary>
        ///     Deserialize the specified XML string into the specified type
        /// </summary>
        /// <param name="xmlString">XML string representation of an object</param>
        /// <returns>Object representation of the xml string</returns>
        public static T DeserializeObjectFromXml<T>(string xmlString)
        {
            var obj = DeserializeObjectFromXml(xmlString, typeof(T));

            if (obj != null)
            {
                return (T) obj;
            }

            //null
            return default(T);
        }
    }
}