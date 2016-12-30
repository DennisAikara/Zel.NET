// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Web;
using Zel.Classes;

namespace Zel
{
    public static class Asp
    {
        public static string GetUrl()
        {
            return HasHttpContext() ? HttpContext.Current.Request.Url.ToString() : null;
        }

        public static string GetUrlReferrer()
        {
            if (!HasHttpContext())
            {
                return null;
            }
            return HttpContext.Current.Request.UrlReferrer != null
                ? HttpContext.Current.Request.UrlReferrer.ToString()
                : null;
        }

        public static string GetRequestIdentifier()
        {
            if (!HasHttpContext())
            {
                return null;
            }

            var httpContextItem = GetHttpContextItem("RequestIdentifier");
            if (httpContextItem == null)
            {
                var requestIdentifier = Guid.NewGuid().ToString("N");
                SetHttpContextItem("RequestIdentifier", requestIdentifier);
                return requestIdentifier;
            }

            return httpContextItem.ToString();
        }

        /// <summary>
        ///     Indicates if the current thread has an http context
        /// </summary>
        /// <returns></returns>
        public static bool HasHttpContext()
        {
            return HttpContext.Current != null;
        }

        /// <summary>
        ///     Gets the specified item from the current http context
        /// </summary>
        /// <param name="itemName">Item name</param>
        /// <returns>Item if exist, else null</returns>
        public static object GetHttpContextItem(string itemName)
        {
            if (itemName == null)
            {
                throw new ArgumentNullException("itemName");
            }

            if (!HasHttpContext())
            {
                throw new HttpContextDoesNotExistException();
            }

            return HttpContext.Current.Items.Contains(itemName) ? HttpContext.Current.Items[itemName] : null;
        }

        /// <summary>
        ///     Sets the specified item in the current http context
        /// </summary>
        /// <param name="itemName">Item name</param>
        /// <param name="item">Item</param>
        public static void SetHttpContextItem(string itemName, object item)
        {
            if (itemName == null)
            {
                throw new ArgumentNullException("itemName");
            }


            if (!HasHttpContext())
            {
                throw new HttpContextDoesNotExistException();
            }

            HttpContext.Current.Items[itemName] = item;
        }

        /// <summary>
        ///     Disposes all the disposable items from the current http context
        /// </summary>
        public static void DisposeContextItems()
        {
            if (!HasHttpContext())
            {
                throw new HttpContextDoesNotExistException();
            }

            foreach (DictionaryEntry dictionaryEntry in HttpContext.Current.Items)
            {
                var disposable = dictionaryEntry.Value as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        /// <summary>
        ///     Gets the host code of the specified host, if no host is specified return the host code of the current request host
        /// </summary>
        /// <param name="host">Host</param>
        /// <returns>Host code</returns>
        public static int GetHostCode(string host = null)
        {
            if (string.IsNullOrWhiteSpace(host) && HasHttpContext())
            {
                host = HttpContext.Current.Request.Url.Host;
            }


            if (!string.IsNullOrWhiteSpace(host))
            {
                if (host.Length > 30)
                {
                    host = host.Substring(0, 15) + host.Substring(host.Length - 15);
                }

                var code = 0;
                foreach (var c in host)
                {
                    if (c%2 == 1)
                    {
                        code = code + c;
                    }
                    else
                    {
                        code = code - c;
                    }
                }

                code *= host[0] + host[host.Length/2] + host[host.Length - 1]*8080;
                code /= host[host.Length - 1];
                return code;
            }

            return 0;
        }
    }
}