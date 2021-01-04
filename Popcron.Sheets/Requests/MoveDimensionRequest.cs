using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class MoveDimensionRequest
    {
        public DimensionRange source;
        public int destinationIndex;
    }
}