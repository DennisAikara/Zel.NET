// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Zel.Tests
{
    [TestClass]
    public class CoreExtensionsTests
    {
        [TestMethod]
        public void ToByteArray_Converts_Stream_To_ByteArray()
        {
            const string str = "kfjslfjaf";
            var bytes = Encoding.ASCII.GetBytes(str);
            using (var memoryStream = new MemoryStream(bytes))
            {
                var streamToByteArray = memoryStream.ToByteArray();
                Assert.IsTrue(streamToByteArray.SequenceEqual(bytes));
            }
        }

        [TestMethod]
        public void ToJson_Serializes_Object_To_Json()
        {
            var dictionary = new Dictionary<string, string>();
            dictionary["key1"] = "value1";
            dictionary["key2"] = "value2";

            Assert.AreEqual(@"{""key1"":""value1"",""key2"":""value2""}", dictionary.ToJson());
        }

        [TestMethod]
        public void FromJson_Deserializes_Json_To_Object()
        {
            var dictionary = @"{""key1"":""value1"",""key2"":""value2""}".FromJson<Dictionary<string, string>>();
            Assert.AreEqual("value1", dictionary["key1"]);
            Assert.AreEqual("value2", dictionary["key2"]);
        }

        [TestMethod]
        public void ToBson_Serializes_Object_To_Bson()
        {
            var dictionary = new Dictionary<string, string>();
            dictionary["key1"] = "value1";
            dictionary["key2"] = "value2";
            var bson = BitConverter.ToString(dictionary.ToBson().ToArray());
            Assert.AreEqual(
                "27-00-00-00-02-6B-65-79-31-00-07-00-00-00-76-61-6C-75-65-31-00-02-6B-65-79-32-00-07-00-00-00-76-61-6C-75-65-32-00-00",
                bson);
        }


        [TestMethod]
        public void FromBson_Deserializes_Bson_To_Object()
        {
            var dictionary = new Dictionary<string, string>();
            dictionary["key1"] = "value1";
            dictionary["key2"] = "value2";
            var bson = dictionary.ToBson();
            var deserializedDictionary = bson.FromBson<Dictionary<string, string>>();
            Assert.AreEqual("value1", deserializedDictionary["key1"]);
            Assert.AreEqual("value2", deserializedDictionary["key2"]);
        }
    }
}