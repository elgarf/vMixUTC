using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class GradientRule
    {
        public InterpolationPoint minPoint;
        public InterpolationPoint midPoint;
        public InterpolationPoint maxPoint;
    }
}