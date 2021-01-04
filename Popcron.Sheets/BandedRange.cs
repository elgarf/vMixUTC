using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class BandedRange
    {
        public int bandedRangeId;
        public GridRange range;
        public BandingProperties rowProperties;
        public BandingProperties columnProperties;
    }
}