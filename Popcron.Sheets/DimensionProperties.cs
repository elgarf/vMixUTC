using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class DimensionProperties
    {
        public bool hiddenByFilter;
        public bool hiddenByUser;
        public int pixelSize;
        public DeveloperMetadata[] developerMetadata;
    }
}