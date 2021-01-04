using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class EmbeddedChart
    {
        public int chardId;
        public ChartSpec spec;
        public EmbeddedObjectPosition position;
    }
}