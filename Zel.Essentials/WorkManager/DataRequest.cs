// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Zel.WorkManager
{
    public class DataRequest
    {
        public DataRequestType Method { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}