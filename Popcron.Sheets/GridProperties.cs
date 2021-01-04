using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class GridProperties
    {
        public int rowCount;
        public int columnCount;
        public int frozenRowCount;
        public int frozenColumnCount;
        public bool hideGridlines;
        public bool rowGroupControlAfter;
        public bool columnGroupControlAfter;
    }
}