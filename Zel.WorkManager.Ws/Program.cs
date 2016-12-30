// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ServiceProcess;

namespace Zel.WorkManager.Ws
{
    internal static class Program
    {
        private static void Main()
        {
            var servicesToRun = new ServiceBase[] {new Service()};
            ServiceBase.Run(servicesToRun);
        }
    }
}