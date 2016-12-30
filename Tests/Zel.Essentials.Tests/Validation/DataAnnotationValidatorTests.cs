// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zel.Classes;
using Zel.Validation;

namespace Zel.Tests.Validation
{
    [TestClass]
    public class DataAnnotationValidatorTest
    {
        #region Nested type: DummyEntity

        private class DummyEntity
        {
            [Required(AllowEmptyStrings = false)]
            [DisplayName("First Name")]
            [StringLength(2, ErrorMessage = "String length should be less than 2")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Last Name is required", AllowEmptyStrings = false)]
            [StringLength(5)]
            public string LastName { get; set; }

            [DelayValidation]
            [Required(ErrorMessage = "Social Security # is required", AllowEmptyStrings = false)]
            public string SSN { get; set; }
        }

        #endregion

        #region Validate Tests

        [TestMethod]
        public void Validate_Does_Not_Validates_Invalid_Entity()
        {
            var entity = new DummyEntity();

            var validationList = DataAnnotationValidator.Validate(entity);


            Assert.AreEqual(false, validationList.IsValid);
        }

        [TestMethod]
        public void Validate_Delays_Validation_On_Fields_With_DelayValidation_Attribute_When_DelayValidation_Is_True()
        {
            var entity = new DummyEntity
            {
                FirstName = "Ji",
                LastName = "Last"
            };

            var validationList = DataAnnotationValidator.Validate(entity, true);

            Assert.IsTrue(validationList.IsValid);
        }

        [TestMethod]
        public void
            Validate_DoesNot_Delays_Validation_On_Fields_With_DelayValidation_Attribute_When_DelayValidation_Is_False()
        {
            var entity = new DummyEntity
            {
                FirstName = "Ji",
                LastName = "Last"
            };

            var validationList = DataAnnotationValidator.Validate(entity);

            Assert.IsFalse(validationList.IsValid);
        }

        [TestMethod]
        public void Validate_Creates_Custom_Required_Field_Error_Message_When_ErrorMessage_Is_Null()
        {
            const string customErrorMessage = "First Name is required.";
            var entity = new DummyEntity
            {
                LastName = "Last",
                SSN = "ssn"
            };

            var validationList = DataAnnotationValidator.Validate(entity);

            Assert.AreEqual(customErrorMessage, validationList[0].Message);
        }

        [TestMethod]
        public void Validate_Creates_Custom_StringLength_Field_Error_Message_When_ErrorMessage_Is_Null()
        {
            const string customErrorMessage = "LastName must be 5 characters or less.";
            var entity = new DummyEntity
            {
                LastName = "Last444",
                SSN = "ssn",
                FirstName = "ji"
            };

            var validationList = DataAnnotationValidator.Validate(entity);

            Assert.AreEqual(customErrorMessage, validationList[0].Message);
        }


        [TestMethod]
        public void Validate_DoesNot_Create_Custom_Required_Field_Error_Message_When_ErrorMessage_Is_Not_Null()
        {
            var entity = new DummyEntity
            {
                LastName = "Last",
                FirstName = "ji"
            };

            var validationList = DataAnnotationValidator.Validate(entity);

            Assert.AreEqual("Social Security # is required", validationList[0].Message);
        }

        [TestMethod]
        public void Validate_DoesNot_Create_Custom_StringLength_Field_Error_Message_When_ErrorMessage_Is_Not_Null()
        {
            var entity = new DummyEntity
            {
                FirstName = "kjdfskfj",
                LastName = "Last",
                SSN = "ssn"
            };

            var validationList = DataAnnotationValidator.Validate(entity);

            Assert.AreEqual("String length should be less than 2", validationList[0].Message);
        }

        #endregion
    }
}