// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net.Sockets;

namespace Zel.Helpers
{
    public static class MiscHelper
    {
        #region Constructor

        /// <summary>
        ///     Static constructor
        /// </summary>
        static MiscHelper()

        {
            _sequentialGuidSeed = new byte[4];
            _sequentialNumber = DateTime.Now.Ticks;
            Array.Copy(BitConverter.GetBytes(GetSequentialLong()), 0, _sequentialGuidSeed, 0, 4);

            var machineName = Application.MachineName;

            _sequentialGuidMachineNameHashCode = machineName.GetHashCode();
        }

        #endregion

        #region Internals

        /// <summary>
        ///     Current sequential number
        /// </summary>
        private static long _sequentialNumber;

        /// <summary>
        ///     Sequential guid seed
        /// </summary>
        private static readonly byte[] _sequentialGuidSeed;

        /// <summary>
        ///     Sequential guid machine name as hash code
        /// </summary>
        private static readonly int _sequentialGuidMachineNameHashCode;

        /// <summary>
        ///     Synchronization object
        /// </summary>
        private static readonly object _sequentialGuidSynchronization = new object();

        #endregion

        #region Methods

        /// <summary>
        ///     Gets the sequential number as an integer
        /// </summary>
        /// <returns>Sequential number as integer</returns>
        public static int GetSequentialInteger()
        {
            return Convert.ToInt32(GetSequentialLong().ToString().Substring(10));
        }

        /// <summary>
        ///     Gets the sequential number
        /// </summary>
        /// <returns>Sequential number</returns>
        public static long GetSequentialLong()
        {
            lock (_sequentialGuidSynchronization)
            {
                return ++_sequentialNumber;
            }
        }

        /// <summary>
        ///     Generate a sequenctial guid
        /// </summary>
        /// <returns>Sequential guid</returns>
        public static Guid GetSequentialGuid()

        {
            var guidBinary = new byte[16];

            var time = DateTime.Now;
            Array.Copy(BitConverter.GetBytes(_sequentialGuidMachineNameHashCode), 0, guidBinary, 0, 4);
            Array.Copy(BitConverter.GetBytes(time.Year), 0, guidBinary, 5, 2);
            guidBinary[7] = (byte) time.Month;
            Array.Copy(_sequentialGuidSeed, 0, guidBinary, 8, 4);
            Array.Copy(BitConverter.GetBytes(GetSequentialLong()), 0, guidBinary, 12, 4);

            return new Guid(guidBinary);
        }


        /// <summary>
        ///     Converts the specified DateTime to its relative date.
        /// </summary>
        /// <param name="dateTime">The DateTime to convert.</param>
        /// <returns>
        ///     A string value based on the relative date
        ///     of the datetime as compared to the current date.
        /// </returns>
        public static string ToRelativeDate(DateTime dateTime)
        {
            var timeSpan = DateTime.Now.Subtract(dateTime);

            // span is less than or equal to 60 seconds, measure in seconds.
            if (timeSpan < TimeSpan.FromSeconds(60))
            {
                return timeSpan.Seconds + " second(s) ago";
            }
            // span is less than or equal to 60 minutes, measure in minutes.
            if (timeSpan < TimeSpan.FromMinutes(60))
            {
                return timeSpan.Minutes > 1 ? "about " + timeSpan.Minutes + " minutes ago" : "about a minute ago";
            }
            // span is less than or equal to 24 hours, measure in hours.
            if (timeSpan < TimeSpan.FromHours(24))
            {
                return timeSpan.Hours > 1 ? "about " + timeSpan.Hours + " hours ago" : "about an hour ago";
            }
            // span is less than or equal to 30 days (1 month), measure in days.
            if (timeSpan < TimeSpan.FromDays(30))
            {
                return timeSpan.Days > 1 ? "about " + timeSpan.Days + " days ago" : "about a day ago";
            }
            // span is less than or equal to 365 days (1 year), measure in months.
            if (timeSpan < TimeSpan.FromDays(365))
            {
                return timeSpan.Days > 31 ? "about " + timeSpan.Days/30 + " month(s) ago" : "about a month ago";
            }

            // span is greater than 365 days (1 year), measure in years.
            return timeSpan.Days > 365 ? "about " + timeSpan.Days/365 + " year(s) ago" : "about a year ago";
        }

        /// <summary>
        ///     Checks if the specified url is listenting
        /// </summary>
        /// <param name="url">Url</param>
        /// <returns>True if url is listening, else false</returns>
        public static bool UrlIsListening(Uri url)
        {
            try
            {
                var tcpClient = new TcpClient(url.Host, url.Port);
                tcpClient.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     Copies content of specified source stream to target
        /// </summary>
        /// <param name="source">Source stream</param>
        /// <param name="target">Target stream</param>
        public static void CopyStream(Stream source, Stream target)
        {
            var buffer = new byte[2000];
            int len;
            while ((len = source.Read(buffer, 0, 2000)) > 0)
            {
                target.Write(buffer, 0, len);
            }
            target.Flush();
        }

        #endregion
    }
}