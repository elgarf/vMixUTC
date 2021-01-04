using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class UpdateBandingRequest
    {
        public BandedRange bandedRange;
        public string fields;
    }
}