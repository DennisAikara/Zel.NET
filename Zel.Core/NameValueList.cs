// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Zel
{
    public class NameValueList : List<NameValue>
    {
        public NameValueList() {}

        public NameValueList(IEnumerable<NameValue> nameValues)
        {
            AddRange(nameValues);
        }

        public void Add(string name, object value)
        {
            Add(new NameValue(name, value));
        }
    }
}