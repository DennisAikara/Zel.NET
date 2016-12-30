// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace ExternalAssembly
{
    public class ClassThatImplementInterface : IList<string>
    {
        #region IList<string> Members

        public IEnumerator<string> GetEnumerator()
        {
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(string item) {}

        public void Clear() {}

        public bool Contains(string item)
        {
            return false;
        }

        public void CopyTo(string[] array, int arrayIndex) {}

        public bool Remove(string item)
        {
            return false;
        }

        public int Count
        {
            get { return 0; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(string item)
        {
            return 0;
        }

        public void Insert(int index, string item) {}

        public void RemoveAt(int index) {}

        public string this[int index]
        {
            get { return ""; }
            set { }
        }

        #endregion
    }
}