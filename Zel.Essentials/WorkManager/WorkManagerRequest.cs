// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Zel.WorkManager
{
    public class WorkManagerRequest
    {
        public DateTime TimeStamp { get; set; }
        public string Signature { get; set; }
        public string WorkerId { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
    }
}