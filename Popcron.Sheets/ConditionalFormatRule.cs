using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class ConditionalFormatRule
    {
        public GridRange[] ranges;
        public BooleanRule booleanRule;
        public GradientRule gradientRule;
    }
}