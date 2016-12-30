// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Zel.Classes;

namespace Zel
{
    public class Result<T>
    {
        public Result(T value)
        {
            Value = value;
            ValidationList = new ValidationList();
        }

        public Result(ValidationList validationList)
        {
            if (validationList == null)
            {
                throw new ArgumentNullException("validationList");
            }

            ValidationList = validationList;
        }

        public T Value { get; private set; }

        public ValidationList ValidationList { get; }

        public bool IsValid
        {
            get { return (ValidationList == null) || ValidationList.IsValid; }
        }
    }
}