// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Zel.Helpers;

namespace Zel.Testing
{
    public class TestWebServer
    {
        private const string WEBSERVER_FILE =
            @"C:\Program Files (x86)\Common Files\Microsoft Shared\DevServer\10.0\WebDev.WebServer40.EXE";

        public TestWebServer(string websiteDirectory, short port)
        {
            WebSiteDirectory = websiteDirectory;
            Port = port;
        }

        private bool Loaded { get; set; }
        private string WebServerPort { get; set; }
        private Process WebServerProcess { get; set; }
        private string WebSiteDirectory { get; }
        private short Port { get; }

        private string TestWebSiteDirectory
        {
            get { return Path.Combine(Application.RootDirectory, "website"); }
        }

        public string WebSiteUrl
        {
            get { return string.Format("http://localhost:{0}/", Port); }
        }

        public void Start()
        {
            if (!Loaded)
            {
                //empty website directory
                FileSystemHelper.EmptyDirectory(TestWebSiteDirectory);

                //copy the source into website directory
                foreach (var dirPath in
                    Directory.GetDirectories(WebSiteDirectory, "*", SearchOption.AllDirectories)
                        .Where(x => !x.Contains(".svn")))
                {
                    Directory.CreateDirectory(dirPath.Replace(WebSiteDirectory, TestWebSiteDirectory));
                }

                foreach (var newPath in
                    Directory.GetFiles(WebSiteDirectory, "*.*", SearchOption.AllDirectories)
                        .Where(x => !x.Contains(".svn")))
                {
                    File.Copy(newPath, newPath.Replace(WebSiteDirectory, TestWebSiteDirectory));
                }

                //modify web.config to use sql\unit instead of sql\dev
                var configFile = Path.Combine(TestWebSiteDirectory, "Web.config");
                var config = XDocument.Load(configFile);
                var connectionStrings = config.Descendants("configuration").Descendants("connectionStrings").Elements();
                foreach (var connectionString in connectionStrings)
                {
                    var connection = connectionString.Attribute("connectionString").Value;
                    connectionString.Attribute("connectionString").Value = connection.Replace("SQL\\DEV", "SQL\\UNIT");
                }
                config.Save(configFile);

                if (!MiscHelper.UrlIsListening(new Uri(string.Format("http://localhost:{0}/", Port))))
                {
                    //start webserver
                    var process = new Process();

                    // set the initial properties 
                    process.StartInfo.FileName = WEBSERVER_FILE;
                    process.StartInfo.Arguments = string.Format("/port:{0} /path:\"{1}\"", Port,
                        TestWebSiteDirectory);
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;

                    // start the process
                    process.Start();
                    WebServerProcess = process;
                }
                Loaded = true;
            }
        }
    }
}