﻿//-----------------------------------------------------------------------
// <copyright file="DynamicDns.cs" company="Michael Kourlas">
//   namecheap-dynamic-dns
//   Copyright (C) 2017 Michael Kourlas
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
//-----------------------------------------------------------------------

namespace Kourlas.NamecheapDynamicDns
{
    using System;
    using System.IO;
    using System.Net;
    using System.Xml;

    /// <summary>
    /// Utility functions for performing dynamic DNS updates with Namecheap.
    /// </summary>
    public static class DynamicDns
    {
        /// <summary>
        /// Retrieves the current public IPv4 address from Namecheap.
        /// </summary>
        /// <returns>The current public IPv4 address from Namecheap.</returns>
        public static string GetIp()
        {
            var request = WebRequest.CreateHttp(
                "https://dynamicdns.park-your-domain.com/getip");
            return new StreamReader(
                request.GetResponse().GetResponseStream()).ReadToEnd();
        }

        /// <summary>
        /// Performs a dynamic DNS update with Namecheap servers using the 
        /// specified profile.
        /// </summary>
        /// <param name="profile">The specified profile.</param>
        /// <returns>The message to display to the user containing the status 
        /// of the update.</returns>
        public static string PerformDynamicDnsUpdate(Profile profile)
        {
            try
            {
                var ip = profile.IpAddress;
                if (profile.AutoDetectIpAddress)
                {
                    ip = GetIp();
                }

                HttpWebRequest request = WebRequest.CreateHttp(
                    "https://dynamicdns.park-your-domain.com/update?" +
                    "host=" + WebUtility.UrlEncode(profile.Host) +
                    "&domain=" + WebUtility.UrlEncode(profile.Domain) +
                    "&password=" + WebUtility.UrlEncode(
                        profile.DynamicDnsPassword) +
                    "&ip=" + WebUtility.UrlEncode(ip));
                var response = new XmlDocument();
                response.LoadXml(new StreamReader(
                    request.GetResponse().GetResponseStream()).ReadToEnd());
                var errCount = response.GetElementsByTagName("ErrCount");

                if (errCount.Count == 1 && errCount[0].InnerText == "0")
                {
                    return "Update successful (" + ip + ")";
                }
                else
                {
                    return "Update failed (non-zero error count)";
                }
            }
            catch (XmlException)
            {
                return "Update failed (XML parse error)";
            }
            catch (WebException)
            {
                return "Update failed (network error)";
            }
            catch (IOException)
            {
                return "Update failed (network error)";
            }
            catch (Exception)
            {
                return "Update failed (unknown error)";
            }
        }
    }
}
