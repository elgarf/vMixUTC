using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class DataFilter
    {
        public DeveloperMetadataLookup developerMetadataLookup;
        public string a1Range;
        public GridRange gridRange;
    }
}