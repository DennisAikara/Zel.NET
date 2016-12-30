// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Zel.Classes
{
    public class ConcurrentProcessor<T> : IConcurrentProcessor<T>
    {
        private readonly object _lockObject = new object();
        private readonly Action<IConcurrentData<T>> _processMethod;
        private readonly ActionBlock<IConcurrentData<T>> _processor;
        private readonly Dictionary<string, T> _queue = new Dictionary<string, T>();
        private bool _stop;

        public ConcurrentProcessor(Action<IConcurrentData<T>> processMethod, int maxDegreeOfParallelism = 0)
        {
            _processMethod = processMethod;
            _processor = new ActionBlock<IConcurrentData<T>>(Process,
                new ExecutionDataflowBlockOptions {MaxDegreeOfParallelism = maxDegreeOfParallelism});
        }

        public bool Stopped => _queue.Count == 0;

        public int QueueCount
        {
            get
            {
                lock (_lockObject)
                {
                    return _queue.Count;
                }
            }
        }

        public void Start()
        {
            _stop = false;
        }

        public void Stop()
        {
            _stop = true;
        }

        public bool Queue(IConcurrentData<T> concurrentData)
        {
            if (_stop)
            {
                return false;
            }
            lock (_lockObject)
            {
                if (_queue.ContainsKey(concurrentData.UniqueIdentifier))
                {
                    return false;
                }
                _queue[concurrentData.UniqueIdentifier] = concurrentData.Data;
                _processor.Post(concurrentData);
                return true;
            }
        }

        private async Task Process(IConcurrentData<T> data)
        {
            if (!_stop)
            {
                await Task.Run(() => _processMethod(data));
            }

            lock (_lockObject)
            {
                _queue.Remove(data.UniqueIdentifier);
            }
        }
    }
}