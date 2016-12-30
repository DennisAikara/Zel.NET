// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Zel.WorkManager
{
    public class WorkManagerStatus
    {
        public WorkManagerStatus()
        {
            Workers = new List<WorkerDetail>();
        }

        public List<WorkerDetail> Workers { get; set; }
        public int MemoryUsed { get; set; }
    }
}