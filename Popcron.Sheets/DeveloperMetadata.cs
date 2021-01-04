using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class DeveloperMetadata
    {
        public int metadataId;
        public string metadataKey;
        public string metadataValue;
        public string location;
        public string visibility;
    }
}