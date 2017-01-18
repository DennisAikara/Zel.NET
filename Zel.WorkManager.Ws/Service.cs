// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Configuration;
using System.IO;
using System.ServiceProcess;
using Zel.Classes;
using Zel.DataAccess;

namespace Zel.WorkManager.Ws
{
    public partial class Service : ServiceBase
    {
        private readonly Logger _logger;
        private WorkManagerService _workManagerService;

        public Service()
        {
            _logger =
                new Logger(new SqlLogStore(ConfigurationManager.ConnectionStrings["Log"].ConnectionString, null),
                    Guid.NewGuid());
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _workManagerService = new WorkManagerService(_logger, CurrentDomainUnhandledException);
                _workManagerService.Start();
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogException(ex, new LogData(ex.FileName, "FileName"),
                    new LogData(ex.FusionLog, "FusionLog"));
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }

        protected override void OnStop()
        {
            try
            {
                if (_workManagerService != null)
                {
                    _workManagerService.Stop();
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }

        private void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.LogException((Exception) e.ExceptionObject);
        }
    }
}