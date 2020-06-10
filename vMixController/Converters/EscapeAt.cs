using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vMixController.Converters
{
    public static class Helpers
    {
        public static string EscapeSymbol = "~";

        public static string EscapeAt(string val)
        {
            if (val == null) return "";
            return val.Replace(EscapeSymbol + EscapeSymbol, string.Format("{0}", (char)0));
        }
        public static string UnescapeAt(string val)
        {
            if (val == null) return "";
            return val.Replace(string.Format("{0}", (char)0), EscapeSymbol);
        }
    }
}
