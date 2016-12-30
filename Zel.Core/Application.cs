// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Principal;

namespace Zel
{
    public enum EnvironmentType
    {
        Production,
        Test,
        Development
    }

    /// <summary>
    ///     Application  class
    /// </summary>
    public static class Application
    {
        #region Constructors

        /// <summary>
        ///     Static constructor
        /// </summary>
        static Application()
        {
            //set application directory
            RootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var windowsIdentity = WindowsIdentity.GetCurrent();
            if (windowsIdentity != null)
            {
                ApplicationUserName = windowsIdentity.Name;
            }
            if (ApplicationUserName == null)
            {
                ApplicationUserName = Environment.UserName;
            }

            if (!Directory.Exists(OutDirectory))
            {
                Directory.CreateDirectory(OutDirectory);
            }

            MachineName = Environment.MachineName;
        }

        #endregion

        public static bool HasInternetConnection()
        {
            if (!(DateTime.UtcNow.Subtract(_lastInternetConnectionCheck).TotalSeconds > 0))
            {
                return _hasInternetConnection;
            }
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return false;
            }

            _lastInternetConnectionCheck = DateTime.UtcNow;
            try
            {
                var tcpClient = new TcpClient("www.google.com", 80);
                tcpClient.Close();
                _hasInternetConnection = true;
                return _hasInternetConnection;
            }
            catch (Exception)
            {
                _hasInternetConnection = false;
                return _hasInternetConnection;
            }
        }

        #region Internals

        private static DateTime _lastInternetConnectionCheck;
        private static bool _hasInternetConnection;

        #endregion

        #region Properties

        /// <summary>
        ///     Current application's root directory
        /// </summary>
        public static string RootDirectory { get; }

        /// <summary>
        ///     Current application's bin directory
        /// </summary>
        public static string BinDirectory
        {
            get
            {
                var binDirectory = Path.Combine(RootDirectory, "bin");
                return Directory.Exists(binDirectory) ? binDirectory : RootDirectory;
            }
        }

        /// <summary>
        ///     Current application's out directory
        /// </summary>
        public static string OutDirectory
        {
            get { return Path.Combine(RootDirectory, "out"); }
        }

        /// <summary>
        ///     The username the  application is running under
        /// </summary>
        public static string ApplicationUserName { get; set; }

        /// <summary>
        ///     Current machine's name
        /// </summary>
        public static string MachineName { get; private set; }

        /// <summary>
        ///     Current application enviroment type
        /// </summary>
        public static EnvironmentType EnvironmentType { get; private set; }

        #endregion
    }
}