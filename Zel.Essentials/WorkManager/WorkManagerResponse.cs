// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Zel.Classes;

namespace Zel.WorkManager
{
    public class WorkManagerResponse
    {
        public string RequestId { get; set; }
        public string Status { get; set; }
        public string Data { get; set; }
        public ValidationList ValidationList { get; set; }
    }
}