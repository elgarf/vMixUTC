using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Popcron.Sheets
{
    [Serializable]
    public class RequestBatchUpdate
    {
        public Request[] requests = { };
        public bool includeSpreadsheetInResponse;
        public string[] responseRanges = { };
        public bool responseIncludeGridData;
    }
}
