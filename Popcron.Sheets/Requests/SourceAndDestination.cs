using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class SourceAndDestination
    {
        public GridRange source;
        public string dimension;
        public int fillLength;
    }
}