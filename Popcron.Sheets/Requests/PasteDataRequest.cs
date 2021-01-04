using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class PasteDataRequest
    {
        public GridCoordinate coordinate;
        public string data;
        public string type;
        public object kind;

        public void Set(string delimeter)
        {
            kind = delimeter;
        }

        public void Set(bool html)
        {
            kind = html;
        }
    }
}