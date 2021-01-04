using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class UpdateDimensionPropertiesRequest
    {
        public DimensionRange range;
        public DimensionProperties properties;
        public string fields;
    }
}