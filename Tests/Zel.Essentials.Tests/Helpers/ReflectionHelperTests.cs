// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Zel.Tests.Helpers
{
    [TestClass]
    public class ReflectionTests
    {
        #region GetPropertyAttributes Tests

        [TestMethod]
        public void GetPropertyAttribute_Returns_Properties()
        {
            var requiredAttributes = Reflection.GetPropertyAttributes<RequiredAttribute>(typeof(TestClass), "Name");
            Assert.AreEqual(typeof(RequiredAttribute), requiredAttributes[0].GetType());
            Assert.AreEqual(1, requiredAttributes.Count);
        }

        #endregion

        #region PropertyHasAttribute Tests

        [TestMethod]
        public void PropertyHasAttribute_Indicates_If_Property_Has_Attribute_Or_Not()
        {
            Assert.IsTrue(Reflection.PropertyHasAttribute<RequiredAttribute>(typeof(TestClass), "Name"));
            Assert.IsFalse(Reflection.PropertyHasAttribute<KeyAttribute>(typeof(TestClass), "Name"));
        }

        #endregion

        #region GetPropertiesWithAttribute Tests

        [TestMethod]
        public void GetPropertiesWithAttribute_Returns_Properties_With_Specified_Attribute()
        {
            var propertiesWithAttribute = Reflection.GetPropertiesWithAttribute<KeyAttribute>(typeof(TestClass));
            Assert.AreEqual(1, propertiesWithAttribute.Count);
            Assert.AreEqual("TestClassId", propertiesWithAttribute[0].Name);
        }

        #endregion

        #region GetPropertyNames Tests

        [TestMethod]
        public void GetPropertyNames_Returns_Property_Names()
        {
            var propertyNames = Reflection.GetPropertyNames(typeof(TestClass));
            Assert.AreEqual(2, propertyNames.Count);
        }

        #endregion

        #region SetPropertyValue Tests

        [TestMethod]
        public void SetPropertyValue_Sets_Value()
        {
            var testClass = new TestClass
            {
                Name = "name",
                TestClassId = 2
            };
            Reflection.SetPropertyValue(testClass, "Name", "name2");
            Assert.AreEqual("name2", testClass.Name);
        }

        #endregion

        #region GetClassAttributes Tests

        [TestMethod]
        public void GetClassAtttributes_Returns_Specified_Attributes()
        {
            var attributes = Reflection.GetClassAttributes<SerializableAttribute>(typeof(TestClass));
            Assert.AreEqual(1, attributes.Count);
        }

        #endregion

        #region Nested type: TestClass

        [XmlRoot]
        [Serializable]
        public class TestClass
        {
            [Key]
            public int TestClassId { get; set; }

            [Required]
            [StringLength(15)]
            public string Name { get; set; }
        }

        #endregion

        #region GetPropertyValue

        [TestMethod]
        public void GetPropertyValue_Returns_Valid_Property_Value()
        {
            var testClass = new TestClass
            {
                Name = "name",
                TestClassId = 2
            };
            Assert.AreEqual("name", Reflection.GetPropertyValue<string>(testClass, "Name"));
        }

        [TestMethod]
        public void GetPropertyValue_Returns_Default_Value_If_Property_DoesNot_Exist()
        {
            var testClass = new TestClass
            {
                Name = "name",
                TestClassId = 2
            };
            Assert.AreEqual(default(string), Reflection.GetPropertyValue<string>(testClass, "dummyPropp"));
        }

        #endregion

        #region ChangeType Tests

        [TestMethod]
        public void ChangeType_Returns_Default_Value_If_Value_Is_Null()
        {
            Assert.AreEqual(null, Reflection.ChangeType<string>(null));
            Assert.AreEqual(default(Guid), Reflection.ChangeType<Guid>(null));
            Assert.AreEqual(null, Reflection.ChangeType<int?>(null));
        }

        [TestMethod]
        public void ChangeType_Changes_Type()
        {
            var guid = Guid.NewGuid();
            var dateTime = DateTime.Now;

            object guidObject = guid.ToString();
            object dateTimeObject = dateTime.ToString();

            Assert.AreEqual(guid.ToString(), Reflection.ChangeType<Guid>(guidObject).ToString());
            Assert.AreEqual(dateTimeObject.ToString(), Reflection.ChangeType<DateTime>(dateTimeObject).ToString());
        }

        #endregion
    }
}