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
        private int m_Timeout;

        public int Timeout
        {
            get
            {
                return this.m_Timeout;
            }
            set
            {
                this.m_Timeout = value;
            }
        }

        public vMixWebClient()
        {
            this.m_Timeout = 5000;
            this.Encoding = Encoding.UTF8;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest webRequest = base.GetWebRequest(address);
            webRequest.Timeout = this.m_Timeout;
            return webRequest;
        }
    }
}
