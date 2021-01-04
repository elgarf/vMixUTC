using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class UpdateDimensionGroupRequest
    {
        public DimensionGroup dimensionGroup;
        public string fields;
    }
}