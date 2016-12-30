// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zel.Helpers;

namespace Zel.Tests.Helpers
{
    [TestClass]
    public class XmlHelperTests
    {
        #region Nested type: TestObject

        public class TestObject
        {
            public int IntProp { get; set; }
            public string StringProp { get; set; }
        }

        #endregion

        #region Nested type: TestObject2

        [XmlRoot(Namespace = "http://www.dummydummy.com")]
        public class TestObject2
        {
            public int IntProp { get; set; }
            public string StringProp { get; set; }
        }

        #endregion

        #region SerializeObjectToXml Tests

        [TestMethod]
        public void SerializeObjectToXml_Returns_Null_If_ObjectToSerialize_Is_Null()
        {
            var xml = XmlHelper.SerializeObjectToXml(null);
            Assert.IsNull(xml);
        }

        [TestMethod]
        public void SerializeObjectToXml_Returns_String_If_ObjectToSerialize_Is_String()
        {
            var xml = XmlHelper.SerializeObjectToXml("string");
            Assert.AreEqual("string", xml);
        }

        [TestMethod]
        public void SerializeObjectToXml_Serialize_Object_To_Xml_With_Xml_Declartion_And_NameSpace()
        {
            var testObject2 = new TestObject2
            {
                IntProp = 5,
                StringProp = "dogma"
            };

            var xml = XmlHelper.SerializeObjectToXml(testObject2, "http://www.dummydummy.com", false);


            const string expectedXmlString =
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><TestObject2 "
                + "xmlns=\"http://www.dummydummy.com\"><IntProp>5</IntProp><StringProp>dogma" +
                "</StringProp></TestObject2>";

            Assert.AreEqual(expectedXmlString, xml);
        }

        [TestMethod]
        public void SerializeObjectToXml_Serialize_Object_To_Xml_Without_Xml_Declartion_And_NameSpace()
        {
            var testObject = new TestObject
            {
                IntProp = 5,
                StringProp = "dogma"
            };

            var xml = XmlHelper.SerializeObjectToXml(testObject);

            const string expectedXmlString =
                "<TestObject><IntProp>5</IntProp><StringProp>dogma</StringProp></TestObject>";

            Assert.AreEqual(expectedXmlString, xml);
        }

        #endregion

        #region DeserializeObjectFromXml Tests

        [TestMethod]
        public void DeserializeObjectFromXml_Deserialize_Xml_To_Object()
        {
            const string xml = "<TestObject><IntProp>5</IntProp><StringProp>dogma</StringProp></TestObject>";

            var testObject = (TestObject) XmlHelper.DeserializeObjectFromXml(xml, typeof(TestObject));

            Assert.AreEqual(5, testObject.IntProp);
            Assert.AreEqual("dogma", testObject.StringProp);

            var genericObj = XmlHelper.DeserializeObjectFromXml<TestObject>(xml);
            Assert.AreEqual(5, genericObj.IntProp);
            Assert.AreEqual("dogma", genericObj.StringProp);
        }


        [TestMethod]
        public void DeserializeObjectFromXml_Returns_Null_Or_Default_Type_If_XmlString_Is_Null()
        {
            Assert.IsNull(XmlHelper.DeserializeObjectFromXml(null, typeof(string)));
            Assert.IsInstanceOfType(XmlHelper.DeserializeObjectFromXml<Guid>(null), typeof(Guid));
        }

        #endregion
    }
}