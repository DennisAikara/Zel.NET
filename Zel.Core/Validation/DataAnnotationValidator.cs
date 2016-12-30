// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Zel.Classes;

namespace Zel.Validation
{
    /// <summary>
    ///     Validates Data Annotations
    /// </summary>
    public static class DataAnnotationValidator
    {
        #region Methods

        /// <summary>
        ///     Validates the specified object
        /// </summary>
        /// <param name="objectToValidate">Object to validate</param>
        /// <param name="delayValidation">
        ///     Flag to indicate if validation should be delayed for
        ///     properties with DelayValidationAttribute
        /// </param>
        /// <returns>Validation List</returns>
        public static ValidationList Validate(object objectToValidate, bool delayValidation = false)
        {
            var validationResults = new List<ValidationResult>();
            var validationList = new ValidationList();

            Validator.TryValidateObject(objectToValidate, new ValidationContext(objectToValidate, null, null),
                validationResults,
                true);

            var delayValidationProperties = new List<string>();
            if (delayValidation)
            {
                delayValidationProperties =
                    Reflection.GetPropertiesWithAttribute<DelayValidationAttribute>(objectToValidate.GetType())
                        .Select(x => x.Name)
                        .ToList();
            }

            foreach (var result in validationResults)
            {
                var key = result.MemberNames.First();
                if (delayValidation || delayValidationProperties.Contains(key))
                {
                    //error is on a delay validation property, so ignore
                    continue;
                }


                //default messages created by .Net Framework
                var requiredFieldErrorMessage = string.Format("The {0} field is required.", key);
                var stringLengthErrorMessage = string.Format(
                    "The field {0} must be a string with a maximum length of ", key);

                string errorMessage;
                if (result.ErrorMessage == requiredFieldErrorMessage)
                {
                    //null required field error message so generate error message
                    errorMessage = GenerateRequiredFieldErrorMessage(objectToValidate, key);
                }
                else if (result.ErrorMessage.StartsWith(stringLengthErrorMessage))
                {
                    //null string length error message so generate error message
                    errorMessage = GenerateStringLengthErrorMessage(objectToValidate, key);
                }
                else
                {
                    //custom error message, so use the one specified in object
                    errorMessage = result.ErrorMessage;
                }


                validationList.Add(key, errorMessage);
            }

            return validationList;
        }

        #endregion

        #region Static Methods

        /// <summary>
        ///     Create custom error message for blank/empty required field
        /// </summary>
        /// <param name="objectBeingValidated">Object the field belongs to</param>
        /// <param name="fieldName">Name of the field</param>
        /// <returns>Custom error message</returns>
        private static string GenerateRequiredFieldErrorMessage(object objectBeingValidated, string fieldName)
        {
            const string errorMessage = "{0} is required.";

            //get the display name for the property
            var displayName = Reflection.GetPropertyAttributes<DisplayNameAttribute>(objectBeingValidated.GetType(),
                fieldName);

            if (displayName.Count > 0)
            {
                //display attribute exists
                fieldName = displayName[0].DisplayName;
            }

            return string.Format(errorMessage, fieldName);
        }

        /// <summary>
        ///     Create custom error message for longer than max length fields
        /// </summary>
        /// <param name="objectBeingValidated">Object the field belongs to</param>
        /// <param name="fieldName">Name of the field</param>
        /// <returns>Custom error message</returns>
        private static string GenerateStringLengthErrorMessage(object objectBeingValidated, string fieldName)
        {
            const string errorMessage = "{0} must be {1} characters or less.";

            //get the display name for the property
            var displayName = Reflection.GetPropertyAttributes<DisplayNameAttribute>(objectBeingValidated.GetType(),
                fieldName);
            if (displayName.Count > 0)
            {
                //display attribute exists
                fieldName = displayName[0].DisplayName;
            }

            //get the max length of the field
            var stringLengthAttributes =
                Reflection.GetPropertyAttributes<StringLengthAttribute>(objectBeingValidated.GetType(), fieldName);
            var length = stringLengthAttributes[0].MaximumLength;

            //no display attribute user property name instead
            return string.Format(errorMessage, fieldName, length);
        }

        #endregion
    }
}