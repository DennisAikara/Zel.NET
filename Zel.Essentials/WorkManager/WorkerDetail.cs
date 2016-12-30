// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Zel.WorkManager
{
    public class WorkerDetail
    {
        public string Name { get; set; }
        public string Identifier { get; set; }
        public DateTime StartTime { get; set; }
        public string State { get; set; }
    }
}