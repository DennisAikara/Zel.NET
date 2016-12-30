// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Zel.WorkManager
{
    public interface IWorker : IDisposable
    {
        DateTime LastRunTime { get; }
        string Name { get; }
        WorkerState State { get; }
        string Request(string data);
        void Stop();
    }
}