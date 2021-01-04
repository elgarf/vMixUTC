using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class CandlestickData
    {
        public CandlestickSeries lowSeries;
        public CandlestickSeries openSeries;
        public CandlestickSeries closeSeries;
        public CandlestickSeries highSeries;
    }
}