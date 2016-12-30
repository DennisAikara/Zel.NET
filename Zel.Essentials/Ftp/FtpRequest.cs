// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Zel.Ftp
{
    public class FtpRequest
    {
        public FtpRequest(string url, FtpCredentials credentials)
        {
            if (url.EndsWith("/"))
            {
                throw new NotSupportedException("Url cannot end with '/'.");
            }

            Credentials = credentials;
            Url = url;
        }

        public FtpCredentials Credentials { get; set; }
        public string Url { get; set; }

        private string FullPath(string path)
        {
            return Url + path;
        }

        private FtpWebRequest CreateFtpWebRequest(string path, string method)
        {
            var ftpWebRequest = (FtpWebRequest) WebRequest.Create(FullPath(path));
            ftpWebRequest.Method = method;
            ftpWebRequest.Credentials = new NetworkCredential(Credentials.UserName, Credentials.Password);
            return ftpWebRequest;
        }

        private List<FtpFile> ParseFileList(List<string> fileList, List<string> fileDetailList)
        {
            var ftpFileList = new List<FtpFile>();

            fileList = (from fl in fileList
                orderby fl.Length descending
                select fl).ToList();
            fileDetailList = (from fdl in fileDetailList
                orderby fdl.Length descending
                select fdl).ToList();


            foreach (var file in fileList)
            {
                foreach (var fileDetail in fileDetailList)
                {
                    if (fileDetail.EndsWith(" " + file))
                    {
                        var lowerFileDetail = fileDetail.ToLower();
                        if (lowerFileDetail.StartsWith("d") || lowerFileDetail.Contains(" <dir> "))
                        {
                            ftpFileList.Add(new FtpFile
                            {
                                Name = file,
                                Type = FtpFileType.Directory
                            });
                            fileDetailList.Remove(fileDetail);
                            break;
                        }
                        ftpFileList.Add(new FtpFile
                        {
                            Name = file,
                            Type = FtpFileType.File
                        });
                        fileDetailList.Remove(fileDetail);
                        break;
                    }
                }
            }

            ftpFileList = ftpFileList.OrderBy(x => x.Type).ThenBy(x => x.Name).ToList();
            return ftpFileList;
        }


        public List<FtpFile> GetFileList(string path)
        {
            if (!path.StartsWith("/") || !path.EndsWith("/"))
            {
                throw new NotSupportedException("Path must begin and end with '/'.");
            }

            var ftpWebRequest = CreateFtpWebRequest(path, WebRequestMethods.Ftp.ListDirectory);
            var ftpWebResponse = (FtpWebResponse) ftpWebRequest.GetResponse();

            var fileList = new List<string>();
            var fileDetailList = new List<string>();

            using (var ftpStreamReader = new StreamReader(ftpWebResponse.GetResponseStream(), Encoding.UTF8))
            {
                var file = ftpStreamReader.ReadLine();
                while (file != null)
                {
                    fileList.Add(file);
                    file = ftpStreamReader.ReadLine();
                }
            }

            ftpWebRequest = CreateFtpWebRequest(path, WebRequestMethods.Ftp.ListDirectoryDetails);
            ftpWebResponse = (FtpWebResponse) ftpWebRequest.GetResponse();
            using (var ftpStreamReader = new StreamReader(ftpWebResponse.GetResponseStream(), Encoding.UTF8))
            {
                var fileDetail = ftpStreamReader.ReadLine();
                while (fileDetail != null)
                {
                    fileDetailList.Add(fileDetail);
                    fileDetail = ftpStreamReader.ReadLine();
                }
            }
            ftpWebResponse.Close();

            return ParseFileList(fileList, fileDetailList);
        }

        public void EmptyDirectory(string path)
        {
            if (!path.StartsWith("/") || !path.EndsWith("/"))
            {
                throw new NotSupportedException("Path must begin and end with '/'.");
            }

            var ftpFileList = GetFileList(path);

            var files = ftpFileList.Where(x => x.Type == FtpFileType.File).ToList();
            foreach (var file in files)
            {
                DeleteFile(path + file.Name);
            }

            var directories = ftpFileList.Where(x => x.Type == FtpFileType.Directory).ToList();
            foreach (var directory in directories)
            {
                EmptyDirectory(path + directory + "/");
                DeleteDirectory(path + directory + "/");
            }
        }

        public void DeleteDirectory(string path)
        {
            if (!path.StartsWith("/") || !path.EndsWith("/"))
            {
                throw new NotSupportedException("Path must begin and end with '/'.");
            }

            EmptyDirectory(path);

            var ftpWebRequest = CreateFtpWebRequest(path, WebRequestMethods.Ftp.RemoveDirectory);
            var ftpWebResponse = (FtpWebResponse) ftpWebRequest.GetResponse();
            ftpWebResponse.Close();
        }

        public void DeleteFile(string path)
        {
            if (!path.StartsWith("/"))
            {
                throw new NotSupportedException("Path must begin with '/'.");
            }

            var ftpWebRequest = CreateFtpWebRequest(path, WebRequestMethods.Ftp.DeleteFile);
            var ftpWebResponse = (FtpWebResponse) ftpWebRequest.GetResponse();
            ftpWebResponse.Close();
        }

        public void CreateDirectory(string path)
        {
            if (!path.StartsWith("/") || !path.EndsWith("/"))
            {
                throw new NotSupportedException("Path must begin and end with '/'.");
            }

            var ftpWebRequest = CreateFtpWebRequest(path, WebRequestMethods.Ftp.MakeDirectory);
            var ftpWebResponse = (FtpWebResponse) ftpWebRequest.GetResponse();
            ftpWebResponse.Close();
        }

        public void UploadFile(string path, string localPath)
        {
            if (!path.StartsWith("/"))
            {
                throw new NotSupportedException("Path must begin with '/'.");
            }

            var webClient = new WebClient();
            webClient.Credentials = new NetworkCredential(Credentials.UserName, Credentials.Password);
            webClient.UploadFile(FullPath(path), localPath);
            webClient.Dispose();
        }

        public void DownloadFile(string path, string localPath)
        {
            if (!path.StartsWith("/"))
            {
                throw new NotSupportedException("Path must begin with '/'.");
            }

            var webClient = new WebClient();
            webClient.Credentials = new NetworkCredential(Credentials.UserName, Credentials.Password);
            webClient.DownloadFile(FullPath(path), localPath);
            webClient.Dispose();
        }
    }
}