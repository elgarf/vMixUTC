using System;
using System.Text.RegularExpressions;

namespace NewTek.NDI
{
    public class Source
    {
        // Really only useful for disconnects or for default values
        public Source()
        {
        }

        // Construct from NDIlib.source_t
        public Source(NDIlib.source_t source_t)
        {
            Name = UTF.Utf8ToString(source_t.p_ndi_name);

            _ipAddress = UTF.Utf8ToString(source_t.p_ip_address);
        }

        // Construct from strings
        public Source(String name, String ipAddress)
        {
            Name = name;
            _ipAddress = ipAddress;
        }

        // Copy constructor.
        public Source(Source previousSource)
        {
            Name = previousSource.Name;
            _ipAddress = previousSource._ipAddress;
            _uri = previousSource._uri;
        }

        // These are purposely 'public get' only because
        // they should not change during the life of a source.
        public String Name
        {
            get { return _name; }
            private set
            {
                _name = value;
                
                int parenIdx = _name.IndexOf(" (");
                _computerName = _name.Substring(0, parenIdx);
                
                _sourceName = Regex.Match(_name, @"(?<=\().+?(?=\))").Value;

                String uriString = String.Format("ndi://{0}/{1}", _computerName, System.Net.WebUtility.UrlEncode(_sourceName));

                if (!Uri.TryCreate(uriString, UriKind.Absolute, out _uri))
                    _uri = null;
                
            }
        }

        public String ComputerName
        {
            get { return _computerName; }
        }

        public String SourceName
        {
            get { return _sourceName; }
        }

        public String IpAddress
        {
            get { return _ipAddress; }
        }

        public Uri Uri
        {
            get { return _uri; }
        }

        public override string ToString()
        {
            return Name;
        }

        private String _name = String.Empty;
        private String _computerName = String.Empty;
        private String _sourceName = String.Empty;
        private String _ipAddress = String.Empty;
        private Uri _uri = null;
    }
}
