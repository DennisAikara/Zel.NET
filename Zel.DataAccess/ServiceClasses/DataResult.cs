// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Zel.Classes;

namespace Zel.DataAccess.ServiceClasses
{
    public class DataResult<T> : Result<T>
    {
        public DataResult(T value)
            : base(value)
        {
            ChunkList = new List<string>();
        }

        public DataResult(ValidationList validationList)
            : base(validationList)
        {
            ChunkList = new List<string>();
        }

        public bool IsBig
        {
            get { return ChunkList.Count > 0; }
        }

        public List<string> ChunkList { get; set; }
    }
}