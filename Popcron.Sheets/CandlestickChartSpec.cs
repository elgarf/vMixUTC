using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class CandlestickChartSpec
    {
        public CandlestickDomain domain;
        public CandlestickData[] data;
    }
}