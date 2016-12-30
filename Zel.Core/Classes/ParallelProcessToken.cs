// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Zel.Classes
{
    public class ParallelProcessToken
    {
        private readonly int _itemTimeoutInMilliseconds;
        private readonly int _startTime;
        private bool _cancel;

        public ParallelProcessToken(int itemTimeoutInMilliseconds)
        {
            _itemTimeoutInMilliseconds = itemTimeoutInMilliseconds;
            _startTime = Environment.TickCount;
        }

        public object Data { get; set; }

        public void Cancel()
        {
            _cancel = true;
        }

        public bool IsCancellationRequested()
        {
            if (!_cancel && (_itemTimeoutInMilliseconds > 0))
            {
                _cancel = Environment.TickCount - _startTime >= _itemTimeoutInMilliseconds;
            }

            return _cancel;
        }
    }
}