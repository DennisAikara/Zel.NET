// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Zel.WorkManager
{
    public class WorkResult
    {
        public WorkResult()
        {
            WorkStatus = new Dictionary<string, object>();
        }

        public bool ReduceWorkTimerInterval { get; set; }
        public Dictionary<string, object> WorkStatus { get; set; }
    }
}