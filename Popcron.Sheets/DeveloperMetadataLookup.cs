using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class DeveloperMetadataLookup
    {
        public string locationType;
        public DeveloperMetadataLocation metadataLocation;
        public string locationMatchingStrategy;
        public int metadataId;
        public string metadataKey;
        public string metadataValue;
        public string visibility;
    }
}