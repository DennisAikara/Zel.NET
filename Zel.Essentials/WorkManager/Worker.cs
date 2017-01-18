// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using Zel.Classes;

namespace Zel.WorkManager
{
    public abstract class Worker : MarshalByRefObject, IWorker
    {
        private readonly ILogger _logger;

        public override object InitializeLifetimeService()
        {
            return null;
        }

        #region Internals

        private readonly LogCode _finishedWorkingLogCode;
        private readonly LogCode _workingLogCode;

        private int _originalTimerInterval;
        private RegisteredWaitHandle _registeredWaitHandle;
        private bool _stopWorking;
        private int _timerInterval;
        private IContainerService _containerService;
        private Func<IContainerService, WorkResult> _workFunc;

        #endregion

        #region IWorker Members

        public void Dispose()
        {
            if (_registeredWaitHandle != null)
            {
                _registeredWaitHandle.Unregister(null);
                _registeredWaitHandle = null;
            }

            DisposeAction?.Invoke();
        }

        public DateTime LastRunTime { get; private set; }

        public string Name { get; }

        public WorkerState State { get; protected set; }

        public string Request(string data)
        {
            return RequestHandler != null ? RequestHandler(data) : null;
        }

        public void Stop()
        {
            if (State == WorkerState.Working)
            {
                _stopWorking = true;
                StopWorkAction?.Invoke();
            }
            else
            {
                _stopWorking = true;
                State = WorkerState.Stopped;
                if (_registeredWaitHandle != null)
                {
                    _registeredWaitHandle.Unregister(null);
                    _registeredWaitHandle = null;
                }
            }
        }

        #endregion

        #region Private Methods

        private void ReduceTimerInterval()
        {
            if (_timerInterval >= 1000*60*60)
            {
                return;
            }

            var delay = new[]
            {
                1000,
                1000*5,
                1000*30,
                1000*60,
                1000*60*5,
                1000*60*10,
                1000*60*30,
                1000*60*60
            };

            _timerInterval = delay.First(x => x > _timerInterval);
        }

        private void WorkTimerElapsed(object state, bool timedOut)
        {
            if (DateTime.UtcNow.Subtract(LastRunTime).TotalMilliseconds >= _timerInterval)
            {
                try
                {
                    if (State == WorkerState.Idle)
                    {
                        State = WorkerState.Working;


                        var startTime = DateTime.UtcNow;
                        _logger.LogInformation(string.Format("Running {0}", Name), _workingLogCode);

                        var workResult = _workFunc(_containerService);


                        var timeElapsed = DateTime.UtcNow.Subtract(startTime);

                        if (workResult.ReduceWorkTimerInterval)
                        {
                            ReduceTimerInterval();
                        }
                        else
                        {
                            _timerInterval = _originalTimerInterval;
                        }

                        var duration = string.Format("{0:00}h:{1:00}m:{2:00}s", (int) timeElapsed.TotalHours,
                            timeElapsed.Minutes,
                            timeElapsed.Seconds);

                        workResult.WorkStatus["Duration"] = duration;

                        var message = string.Format("Finished running {0}", Name);

                        _logger.LogInformation(message, _finishedWorkingLogCode, new LogData(workResult.WorkStatus));


                        State = _stopWorking ? WorkerState.Stopped : WorkerState.Idle;
                        LastRunTime = DateTime.UtcNow;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                }
            }

            if (State != WorkerState.Stopped)
            {
                _registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(new AutoResetEvent(false),
                    WorkTimerElapsed,
                    null, 1000, true);
            }
        }

        #endregion

        #region Protected 

        protected Worker(string name, LogCode workingLogCode, LogCode finishedWorkingLogCode, ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            _logger = logger;

            _finishedWorkingLogCode = finishedWorkingLogCode;
            _workingLogCode = workingLogCode;

            Name = name;
            State = WorkerState.Idle;
        }


        protected Action DisposeAction { private get; set; }

        protected Action StopWorkAction { private get; set; }

        protected Func<string, string> RequestHandler { private get; set; }

        protected void StartWorkTimer(int timerInterval, Func<IContainerService, WorkResult> workFunc,
            IContainerService containerService)
        {
            if (workFunc == null)
            {
                throw new ArgumentNullException("workFunc");
            }

            if (containerService == null)
            {
                throw new ArgumentNullException("containerService");
            }

            _containerService = containerService;
            _workFunc = workFunc;

            _timerInterval = timerInterval;
            _originalTimerInterval = timerInterval;

            _registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(new AutoResetEvent(false),
                WorkTimerElapsed,
                null, 1000, true);
        }

        #endregion
    }
}