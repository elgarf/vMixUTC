using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class UpdateFilterViewRequest
    {
        public FilterView filter;
        public string fields;
    }
}