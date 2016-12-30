// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

namespace Zel
{
    [DebuggerDisplay("{Name}")]
    public class NameValue
    {
        public NameValue(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public NameValue() {}

        public string Name { get; set; }
        public object Value { get; set; }
    }
}