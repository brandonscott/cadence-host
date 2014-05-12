// ---------------------------------------------------------------------------------------
//  <copyright file="Cadence.cs" company="Cadence">
//      Copyright © 2013-2014 by Brandon Scott and Christopher Franklin.
// 
//      Permission is hereby granted, free of charge, to any person obtaining a copy of
//      this software and associated documentation files (the "Software"), to deal in
//      the Software without restriction, including without limitation the rights to
//      use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//      of the Software, and to permit persons to whom the Software is furnished to do
//      so, subject to the following conditions:
// 
//      The above copyright notice and this permission notice shall be included in all
//      copies or substantial portions of the Software.
// 
//      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//      IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//      FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//      AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
//      WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
//      CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
//  </copyright>
//  ---------------------------------------------------------------------------------------

#region

using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using PubNubMessaging.Core;

#endregion

namespace CadenceHost.Helpers
{
    /// <summary>
    /// Sends and retrieves data from Cadence
    /// </summary>
    public static class Cadence
    {
        public static readonly String ServerUri = "http://cadence-bu.cloudapp.net/servers";
        public static readonly String PulseUri = "http://cadence-bu.cloudapp.net/pulses";
        public static readonly String ServerUriUpdate = "http://cadence-bu.cloudapp.net/servers/" + Properties.Settings.Default.ServerID;
        public static readonly String Username = "brandon@brandonscott.co.uk";
        public static readonly String Password = "Cadenc3!";

        /// <summary>
        /// Creates a Cadence server instance
        /// </summary>
        /// <param name="kvpData">The server data passed to Cadence</param>
        public static int CreateServer(NameValueCollection kvpData)
        {
            return PostRequest(kvpData, ServerUri);
        }

        /// <summary>
        /// Sends a pulse for the current server
        /// </summary>
        /// <param name="kvpData">The pulse data to pass to Cadence</param>
        public static int SendPulse(NameValueCollection kvpData)
        {
            return PostRequest(kvpData, PulseUri);
        }

        /// <summary>
        /// Updates the current Cadence server instance
        /// </summary>
        /// <param name="kvpData">The server data to update for the current server</param>
        public static void UpdateServer(NameValueCollection kvpData)
        {
            PutRequest(kvpData, ServerUriUpdate);
        }

        /// <summary>
        /// The base Put web request to send to the server
        /// </summary>
        /// <param name="kvpData">The data to send to the server</param>
        /// <param name="serverUriUpdate"></param>
        private static void PutRequest(NameValueCollection kvpData, string serverUriUpdate)
        {
            var nc = new NetworkCredential("brandon@brandonscott.co.uk", "Cadenc3!");
            string responseJson;
            using (var wb = new WebClient { Credentials = nc })
            {
                var response = wb.UploadValues(serverUriUpdate, "PUT", kvpData);
            }
        }

        /// <summary>
        /// The base Post web request to send to the server
        /// </summary>
        /// <param name="kvpData">The data to send to the server</param>
        /// <param name="uri">The URI to send the request to</param>
        private static int PostRequest(NameValueCollection kvpData, String uri)
        {
            var nc = new NetworkCredential("brandon@brandonscott.co.uk", "Cadenc3!");
            string responseJson;
            using (var wb = new WebClient {Credentials = nc})
            {
                var response = wb.UploadValues(uri, "POST", kvpData);
                responseJson = wb.Encoding.GetString(response);
            }
            return JObject.Parse(responseJson).GetValue("id").Value<int>();
        }
    }
}