// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace Zel.WorkManager.Classes
{
    internal class WorkerInfo
    {
        public string WorkerId { get; set; }
        public string Name { get; set; }
        public string SourceDirectory { get; set; }
        public string AssemblyFile { get; set; }
        public string TypeName { get; set; }
        public string TemporaryDirectory { get; set; }
        public bool Start { get; set; }
        public bool Stop { get; set; }
        public bool Remove { get; set; }
        public DateTime Restart { get; set; }
        public DateTime StartTime { get; set; }
        public IWorker Instance { get; set; }
        public AppDomain AppDomain { get; set; }
        public FileSystemWatcher FileSystemWatcher { get; set; }
    }
}