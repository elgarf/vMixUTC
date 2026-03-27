using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace vMixAPI
{
    public class vMixWebClient : WebClient
    {
        //public static int _requests = 0;
        public int Requests { get; set; }
        public int Timeout { get; set; }
        public string Tag { get; set; }
        public bool Working { get; set; }
        private int _requestsCount = 0;
        private string _cache = null;

        public vMixWebClient()
        {
            this.Timeout = 5000;
            this.Encoding = Encoding.UTF8;
            this.DownloadStringCompleted += VMixWebClient_DownloadStringCompleted;
        }

        private void VMixWebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
                _cache = e.Result;
            Working = false;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest webRequest = base.GetWebRequest(address);
            ((HttpWebRequest)webRequest).KeepAlive = false;
            webRequest.Timeout = Timeout;
            //Debug.WriteLine(++_requests);
            return webRequest;
        }

        protected override void Dispose(bool disposing)
        {
            //Debug.WriteLine(--_requests);
            base.Dispose(disposing);
        }

        public new void DownloadStringAsync(Uri address, object userToken)
        {
            if (!Working)
            {
                Working = true;
                base.DownloadStringAsync(address, userToken);
            }
            Requests++;
            _requestsCount++;
            //Debug.WriteLine("Requesting {0} : {1}", Tag, _requestsCount);
        }
    }
}
