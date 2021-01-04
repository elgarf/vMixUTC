using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class IterativeCalculationSettings
    {
        public int maxIterations;
        public int convergenceThreshold;
    }
}