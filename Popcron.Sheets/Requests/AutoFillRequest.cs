using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class AutoFillRequest
    {
        public bool useAlternateSeries;
        public object area;

        public void Set(GridRange range)
        {
            area = range;
        }

        public void Set(SourceAndDestination sourceAndDestination)
        {
            area = sourceAndDestination;
        }
    }
}