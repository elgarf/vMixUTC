using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class FindReplaceRequest
    {
        public string find;
        public string replacement;
        public bool matchCase;
        public bool matchEntireCell;
        public bool searchByRegex;
        public bool includeFormulas;
        public object scope;

        public void Set(GridRange scope)
        {
            this.scope = scope;
        }

        public void Set(int scope)
        {
            this.scope = scope;
        }

        public void Set(bool scope)
        {
            this.scope = scope;
        }
    }
}