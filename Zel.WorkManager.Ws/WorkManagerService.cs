// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.VisualBasic.Devices;
using Zel.Classes;
using Zel.Helpers;

namespace Zel.WorkManager.Ws
{
    public class WorkManagerService
    {
        private readonly ILogger _logger;
        private readonly UnhandledExceptionEventHandler _unhandledExceptionEventHandler;
        internal IWorker Manager;
        internal AppDomain ManagerAppDomain;
        internal string ManagerPath;
        internal FileSystemWatcher ManagerPathWatcher;
        internal string TempPath;


        public WorkManagerService(ILogger logger, UnhandledExceptionEventHandler unhandledExceptionEventHandler)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            if (unhandledExceptionEventHandler == null)
            {
                throw new ArgumentNullException("unhandledExceptionEventHandler");
            }
            _logger = logger;
            _unhandledExceptionEventHandler = unhandledExceptionEventHandler;
        }

        public void Start()
        {
            //set paths
            ManagerPath = Path.Combine(Application.RootDirectory, "Manager");
            TempPath = Path.Combine(Application.RootDirectory, "temp");

            //delete and create temp path
            if (Directory.Exists(TempPath))
            {
                Directory.Delete(TempPath, true);
                while (Directory.Exists(TempPath))
                {
                    Thread.Sleep(1000);
                }
            }
            Directory.CreateDirectory(TempPath);

            //check manager directory and start manager
            if (!Directory.Exists(ManagerPath))
            {
                _logger.LogCritical("Work Manager missing 'Manager' folder", new LogCode("WM450"));
                return;
            }

            StartManager();
        }

        public void Stop()
        {
            StopManager();
        }

        internal void StartManager()
        {
            if (!Directory.Exists(ManagerPath))
            {
                _logger.LogCritical("'Manager' folder doesn't exist", new LogCode("WM420"));
            }

            //delete and create the temp manager path
            var managerTempPath = Path.Combine(TempPath, "Manager");
            if (Directory.Exists(managerTempPath))
            {
                Directory.Delete(managerTempPath, true);
                while (Directory.Exists(managerTempPath))
                {
                    Thread.Sleep(1000);
                }
            }
            Directory.CreateDirectory(managerTempPath);

            //make sure assembly is in the folder
            var managerAssembly = Path.Combine(ManagerPath, "Zel.WorkManager.dll");
            if (!File.Exists(managerAssembly))
            {
                _logger.LogCritical("Work Manager assembly missing from 'Manager' folder", new LogCode("WM451"));
                return;
            }

            //get manager worker typer from assembly
            var assemblyName = AssemblyName.GetAssemblyName(managerAssembly);
            var managerType = AssemblyHelper.FindTypesThatImplementInterfaceInExternalAssembly<IWorker>(assemblyName)
                .FirstOrDefault();
            if (managerType == null)
            {
                _logger.LogCritical("Work Manager assembly does not have a valid worker", new LogCode("WM452"));
                return;
            }

            //copy all the contents to temp manager folder
            new Computer().FileSystem.CopyDirectory(ManagerPath, managerTempPath, true);

            //set appdomain settings, and create appdomain
            var appDomainSetup = new AppDomainSetup
            {
                ApplicationBase = managerTempPath,
                DisallowBindingRedirects = true,
                DisallowCodeDownload = true,
                ConfigurationFile = Path.Combine(managerTempPath, "Zel.WorkManager.dll.config")
            };
            ManagerAppDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString(),
                null, appDomainSetup);
            ManagerAppDomain.UnhandledException += _unhandledExceptionEventHandler;

            //monitor manager directory for changes
            ManagerPathWatcher = new FileSystemWatcher(ManagerPath);
            ManagerPathWatcher.Changed += ManagerPathWatcherChanged;
            ManagerPathWatcher.Deleted += ManagerPathWatcherChanged;
            ManagerPathWatcher.Created += ManagerPathWatcherChanged;
            ManagerPathWatcher.Renamed += ManagerPathWatcherChanged;

            ManagerPathWatcher.EnableRaisingEvents = true;

            //create an instance of our IWorker in the appdomain
            Manager = (IWorker) ManagerAppDomain
                .CreateInstanceFromAndUnwrap(Path.Combine(managerTempPath, "Zel.WorkManager.dll"), managerType);
        }

        internal void StopManager()
        {
            if (ManagerPathWatcher != null)
            {
                //disable file watcher
                ManagerPathWatcher.EnableRaisingEvents = false;
                ManagerPathWatcher.Dispose();
                ManagerPathWatcher = null;
            }

            if (Manager != null)
            {
                Manager.Stop();
                Manager.Dispose();
            }

            //unload appdomain and set manager to null
            if (ManagerAppDomain != null)
            {
                AppDomain.Unload(ManagerAppDomain);
            }
            ManagerAppDomain = null;
            Manager = null;
        }

        internal void ManagerPathWatcherChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                //stop and start the manager
                StopManager();

                Thread.Sleep(1000*60);

                StartManager();
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
    }
}