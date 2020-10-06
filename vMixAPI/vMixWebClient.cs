using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace vMixAPI
{
    public class vMixWebClient : WebClient
    {
        public int Timeout { get; set; }

        public vMixWebClient()
        {
            this.Timeout = 5000;
            this.Encoding = Encoding.UTF8;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest webRequest = base.GetWebRequest(address);
            ((HttpWebRequest)webRequest).KeepAlive = false;
            webRequest.Timeout = Timeout;
            return webRequest;
        }
    }
}
