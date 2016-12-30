// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Zel.Classes;
using Zel.Helpers;
using Zel.WorkManager.Classes;

namespace Zel.WorkManager
{
    public class Manager : MarshalByRefObject, IWorker
    {
        private readonly ILogger _logger;

        #region Constructor

        public Manager(ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            _logger = logger;

            _logger.LogInformation("Work Manager service started", new LogCode("WM100"));

            State = WorkerState.Working;

            Workers = new List<WorkerInfo>();

            //set Worker and temp directory, create if they don't exists
            var parentDirectory = new DirectoryInfo(Application.RootDirectory).Parent;
            if (parentDirectory != null)
            {
                var grandParentDirectory = parentDirectory.Parent;
                if (grandParentDirectory != null)
                {
                    TempDirectory = Path.Combine(grandParentDirectory.FullName, "temp");
                    WorkersDirectory = Path.Combine(grandParentDirectory.FullName, "Workers");
                }
            }

            if (!Directory.Exists(WorkersDirectory))
            {
                Directory.CreateDirectory(WorkersDirectory);
            }

            var tempDirectoryInfo = new DirectoryInfo(TempDirectory);
            foreach (var directoryToDelete in tempDirectoryInfo.GetDirectories().Where(x => x.Name.Length == 36))
            {
                Directory.Delete(directoryToDelete.FullName, true);
                while (Directory.Exists(directoryToDelete.FullName))
                {
                    Thread.Sleep(100);
                }
            }

            //this.WebApp = Microsoft.Owin.Hosting.WebApp.Start<ApiConfiguration>("http://localhost:37324/");

            //loop through all the sub directories of Worker directory
            foreach (var dirInfo in new DirectoryInfo(WorkersDirectory).GetDirectories())
            {
                AddWorkersFromDirectory(dirInfo.FullName);
            }

            Timer = ThreadPool.RegisterWaitForSingleObject(new AutoResetEvent(false), TimerElapsed,
                null, 1000*5, true);


            //monitor Worker directory for changes
            FileSystemWatcher = new FileSystemWatcher(WorkersDirectory);
            FileSystemWatcher.Created += WorkerDirectoryChanged;
            FileSystemWatcher.Deleted += WorkerDirectoryChanged;
            FileSystemWatcher.EnableRaisingEvents = true;

            // ApiController.Manager = this;
        }

        #endregion

        #region Internals

        internal readonly string TempDirectory;
        internal readonly string WorkersDirectory;
        internal FileSystemWatcher FileSystemWatcher;
        public RegisteredWaitHandle Timer;
        internal IDisposable WebApp;
        internal List<WorkerInfo> Workers;

        /// <summary>
        ///     Add the workers in the specified directory to the workers list
        /// </summary>
        /// <param name="directory">Directory to search</param>
        internal void AddWorkersFromDirectory(string directory)
        {
            //if there is already worker from the directory ignore
            if (Workers.Any(x => x.SourceDirectory == directory))
            {
                return;
            }

            //get all dlls in the directory
            var directoryInfo = new DirectoryInfo(directory);
            var fileInfos = directoryInfo.GetFiles("*.dll");

            foreach (var fileInfo in fileInfos)
            {
                try
                {
                    var assemblyName = AssemblyName.GetAssemblyName(fileInfo.FullName);
                    var externalAssemblyHasAttribute = AssemblyHelper
                        .ExternalAssemblyHasAttribute(assemblyName, typeof(HasWorkerAttribute));
                    if (!externalAssemblyHasAttribute)
                    {
                        continue;
                    }

                    var valWorkers = AssemblyHelper.FindTypesThatInheritsTypeInExternalAssembly<Worker>(assemblyName);
                    if (valWorkers != null)
                    {
                        if (!File.Exists(Path.Combine(directory, fileInfo.Name + ".config")))
                        {
                            _logger.LogCritical("Worker assembly does not have a valid config file",
                                new LogData(fileInfo.FullName, "Assembly"));
                            return;
                        }

                        //found Worker(s)
                        //loop through the Worker(s) and create Worker infos
                        foreach (var valWorker in valWorkers)
                        {
                            var worker = new WorkerInfo
                            {
                                SourceDirectory = directory,
                                AssemblyFile = fileInfo.FullName,
                                TypeName = valWorker,
                                WorkerId = Guid.NewGuid().ToString("N"),
                                Start = true
                            };
                            //add the Worker to Worker list
                            Workers.Add(worker);

                            _logger.LogInformation("Added worker: " + valWorker);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex, new LogData(fileInfo.FullName, "Assembly"));
                }
            }
        }

        /// <summary>
        ///     Copies the Worker to temp directory, and instantiate the Worker in new appdomain
        /// </summary>
        /// <param name="workerInfo">Worker to instantiate</param>
        internal void StartWorker(WorkerInfo workerInfo)
        {
            workerInfo.Start = false;

            var tempWorkingAssembly = "";

            if (!Directory.Exists(workerInfo.SourceDirectory))
            {
                return;
            }

            //copy everyting in the Worker dir to a working temp Directory
            workerInfo.TemporaryDirectory = Path.Combine(TempDirectory, Guid.NewGuid().ToString());
            Directory.CreateDirectory(workerInfo.TemporaryDirectory);

            var workerDirInfo = new DirectoryInfo(workerInfo.SourceDirectory);
            foreach (var file in workerDirInfo.GetFiles())
            {
                File.Copy(file.FullName, Path.Combine(workerInfo.TemporaryDirectory, file.Name));
                if (file.FullName == workerInfo.AssemblyFile)
                {
                    tempWorkingAssembly = Path.Combine(workerInfo.TemporaryDirectory, file.Name);
                }
            }

            //set appdomain settings
            var appDomainSetup = new AppDomainSetup
            {
                ApplicationBase = workerInfo.TemporaryDirectory,
                DisallowBindingRedirects = true,
                DisallowCodeDownload = true,
                ConfigurationFile = Path.Combine(workerInfo.TemporaryDirectory,
                    Path.GetFileName(workerInfo.AssemblyFile) + ".config")
            };

            //create appdomain using the appdomain settings
            var appDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString(),
                null, appDomainSetup);

            //create an instance of our IWorker in the appdomain
            try
            {
                workerInfo.StartTime = DateTime.UtcNow;

                var workerInstance = (IWorker) appDomain.CreateInstanceFromAndUnwrap(
                    tempWorkingAssembly, workerInfo.TypeName);

                if (Workers.Any(x => (x.Name == workerInstance.Name) && (x.WorkerId != workerInfo.WorkerId)))
                {
                    _logger.LogCritical("Prevented duplicate worker from starting", new LogData(workerInfo));

                    Workers.Remove(workerInfo);
                    AppDomain.Unload(appDomain);
                    return;
                }

                workerInfo.AppDomain = appDomain;

                //update Worker info
                workerInfo.Name = workerInstance.Name;
                workerInfo.Instance = workerInstance;

                workerInfo.FileSystemWatcher = new FileSystemWatcher(workerInfo.SourceDirectory)
                {
                    EnableRaisingEvents = true
                };
                workerInfo.FileSystemWatcher.Changed += WorkerContentChanged;
                workerInfo.FileSystemWatcher.Deleted += WorkerContentChanged;
                workerInfo.FileSystemWatcher.Created += WorkerContentChanged;
                workerInfo.FileSystemWatcher.Renamed += WorkerContentChanged;

                _logger.LogInformation(string.Format("Started worker: {0}", workerInfo.Name), new LogCode("WM400"));
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                var logData = new LogData(workerInfo.AssemblyFile, "Assembly");
                _logger.LogCritical("Error while starting worker", new LogCode("WM500"), logData);

                Workers.Remove(workerInfo);
                if (appDomain != null)
                {
                    AppDomain.Unload(appDomain);
                }
                if (Directory.Exists(workerInfo.TemporaryDirectory))
                {
                    Directory.Delete(workerInfo.TemporaryDirectory, true);
                }
            }
        }

        /// <summary>
        ///     Stops then unloads the specified worker
        /// </summary>
        /// <param name="worker">Worker to unload</param>
        internal void StopAndUnloadWorker(WorkerInfo worker)
        {
            worker.Start = false;

            if (worker.FileSystemWatcher != null)
            {
                worker.FileSystemWatcher.EnableRaisingEvents = false;
                worker.FileSystemWatcher.Changed -= WorkerContentChanged;
                worker.FileSystemWatcher = null;
            }

            if ((worker.Instance != null) && (worker.Instance.State != WorkerState.Stopped))
            {
                worker.Instance.Stop();
                return;
            }

            if (worker.Instance != null)
            {
                worker.Instance.Dispose();
            }

            if (worker.AppDomain != null)
            {
                AppDomain.Unload(worker.AppDomain);
            }

            worker.AppDomain = null;

            worker.Instance = null;
            worker.Stop = false;
            worker.StartTime = default(DateTime);
            if ((worker.TemporaryDirectory != null) && Directory.Exists(worker.TemporaryDirectory))
            {
                Directory.Delete(worker.TemporaryDirectory, true);
            }
            worker.TemporaryDirectory = null;

            _logger.LogInformation(
                string.IsNullOrWhiteSpace(worker.Name)
                    ? string.Format("Stopped worker: {0}", worker.AssemblyFile)
                    : string.Format("Stopped worker: {0}", worker.Name), new LogCode("WM401"));

            if (worker.Restart != default(DateTime))
            {
                worker.Start = true;
                worker.Restart = default(DateTime);
            }

            if (worker.Remove)
            {
                Workers.Remove(worker);
                _logger.LogInformation(string.IsNullOrWhiteSpace(worker.Name)
                    ? string.Format("Removed worker: {0}", worker.AssemblyFile)
                    : string.Format("Removed worker: {0}", worker.Name), new LogCode("WM406"));
            }
        }

        internal void TimerElapsed(object state, bool timedOut)
        {
            try
            {
                var workersToRestart =
                    Workers.Where(x => (x.Restart != default(DateTime)) && !x.Start && !x.Stop).ToList();
                foreach (var worker in workersToRestart)
                {
                    if (DateTime.UtcNow.Subtract(worker.Restart).TotalSeconds > 60)
                    {
                        worker.Stop = true;
                    }
                }

                var workersToStart = Workers.Where(x => x.Start).ToList();
                foreach (var workerToStart in workersToStart)
                {
                    StartWorker(workerToStart);
                }

                var workersToStop = Workers.Where(x => x.Stop).ToList();
                foreach (var workerToStop in workersToStop)
                {
                    StopAndUnloadWorker(workerToStop);
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }

            Timer = ThreadPool.RegisterWaitForSingleObject(new AutoResetEvent(false), TimerElapsed,
                null, 1000*5, true);
        }

        /// <summary>
        ///     Fires when worker directory content changes
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="fileSystemEventArgs">Event Args</param>
        internal void WorkerContentChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            var workers = Workers.Where(x => fileSystemEventArgs.FullPath.StartsWith(x.SourceDirectory)).ToList();
            foreach (var worker in workers)
            {
                lock (worker)
                {
                    if (worker.Restart == default(DateTime))
                    {
                        worker.Restart = DateTime.UtcNow;
                        _logger.LogInformation("Worker directory changed: " + worker.Name);
                    }
                }
            }
        }

        /// <summary>
        ///     Fires when the Worker directory changes
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="fileSystemEventArgs">Event Args</param>
        internal void WorkerDirectoryChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            try
            {
                switch (fileSystemEventArgs.ChangeType)
                {
                    case WatcherChangeTypes.Created:
                        if (Directory.Exists(fileSystemEventArgs.FullPath))
                        {
                            _logger.LogInformation("New Worker directory detected");

                            Thread.Sleep(60*1000);
                            AddWorkersFromDirectory(fileSystemEventArgs.FullPath);
                        }
                        break;
                    case WatcherChangeTypes.Deleted:
                    {
                        var workers =
                            Workers.Where(x => x.SourceDirectory == fileSystemEventArgs.FullPath).ToList();
                        foreach (var worker in workers)
                        {
                            worker.Stop = true;
                            worker.Remove = true;
                            _logger.LogInformation("Worker directory deleted: " + worker.Name);
                        }
                    }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        #endregion

        #region Public Methods

        public ValidationList StartWorker(string workerId)
        {
            var worker = Workers.FirstOrDefault(x => x.WorkerId == workerId);
            if (worker == null)
            {
                return new ValidationList("Invalid Worker");
            }

            if (worker.Instance != null)
            {
                return new ValidationList();
            }

            worker.Start = true;

            return new ValidationList();
        }

        public ValidationList StopWorker(string workerId)
        {
            var worker = Workers.FirstOrDefault(x => x.WorkerId == workerId);
            if (worker == null)
            {
                return new ValidationList("Invalid Worker");
            }

            worker.Start = false;
            worker.Stop = true;

            return new ValidationList();
        }

        public Result<WorkManagerStatus> GetStatus()
        {
            var workManagerStatus = new WorkManagerStatus();

            using (var performanceCounter = new PerformanceCounter())
            {
                performanceCounter.CategoryName = "Process";
                performanceCounter.CounterName = "Working Set - Private";
                performanceCounter.InstanceName = Process.GetCurrentProcess().ProcessName;
                workManagerStatus.MemoryUsed = Convert.ToInt32(performanceCounter.NextValue())/(1024*1024);
                performanceCounter.Close();
            }

            foreach (var worker in Workers.Where(x => !string.IsNullOrWhiteSpace(x.Name)))
            {
                string state;
                if (worker.Restart != default(DateTime))
                {
                    state = "Restarting";
                }
                else if (worker.Instance != null)
                {
                    state = worker.Stop ? "Stopping" : worker.Instance.State.ToString();
                }
                else
                {
                    state = worker.Start ? "Starting" : "Stopped";
                }

                workManagerStatus.Workers.Add(new WorkerDetail
                {
                    Identifier = worker.WorkerId,
                    Name = worker.Name,
                    StartTime = worker.StartTime,
                    State = state
                });
            }

            return new Result<WorkManagerStatus>(workManagerStatus);
        }

        public Result<string> WorkerRequest(string workerId, string data)
        {
            var worker = Workers.FirstOrDefault(x => x.WorkerId == workerId);
            if (worker == null)
            {
                return new Result<string>(new ValidationList("Invalid Worker"));
            }

            if ((worker.Instance == null) || worker.Stop)
            {
                return new Result<string>(new ValidationList("Invalid state"));
            }

            try
            {
                return new Result<string>(worker.Instance.Request(data));
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                _logger.LogCritical(string.Format("Error while making worker request: {0}.", worker.Name),
                    new LogCode("WM501"),
                    new LogData(data, "Data"));
                return new Result<string>(new ValidationList("An unexpected error occured while processing the request"));
            }
        }

        #endregion

        #region IWorker Members

        public DateTime LastRunTime { get; private set; }

        public string Name
        {
            get { return "Work Manager"; }
        }

        public WorkerState State { get; set; }

        public string Request(string data)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            if (State == WorkerState.Stopped)
            {
                return;
            }

            //remove the Worker directory watch
            FileSystemWatcher.EnableRaisingEvents = false;
            FileSystemWatcher.Created -= WorkerDirectoryChanged;
            FileSystemWatcher.Deleted -= WorkerDirectoryChanged;

            if (Timer != null)
            {
                Timer.Unregister(null);
                Timer = null;
            }

            while (true)
            {
                var workersToStop = Workers.Where(x => x.TemporaryDirectory != null).ToList();
                if (workersToStop.Count == 0)
                {
                    break;
                }

                foreach (var workerToStop in workersToStop)
                {
                    StopAndUnloadWorker(workerToStop);
                }
                Thread.Sleep(1000);
            }

            State = WorkerState.Stopped;

            _logger.LogInformation("Work Manager service stopped", new LogCode("WM101"));
        }

        public void Dispose()
        {
            State = WorkerState.Stopped;

            FileSystemWatcher = null;

            if (Timer != null)
            {
                Timer.Unregister(null);
                Timer = null;
            }

            if (WebApp != null)
            {
                WebApp.Dispose();
                WebApp = null;
            }

            foreach (var worker in Workers)
            {
                if (worker.AppDomain != null)
                {
                    AppDomain.Unload(worker.AppDomain);
                }
                worker.AppDomain = null;
                worker.Instance = null;
            }

            Workers.Clear();
        }

        #endregion
    }
}