// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Zel.Classes
{
    public class ParallelProcessOptions
    {
        public int ProcessTimeoutInMilliseconds { get; set; }
        public int ItemTimeoutInMilliseconds { get; set; }
        public int MaxConcurrency { get; set; }
    }
}