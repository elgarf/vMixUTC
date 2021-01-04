using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class PieChartSpec
    {
        public string legendPosition;
        public ChartData domain;
        public ChartData series;
        public bool threeDimensional;
        public float pieHole;
    }
}