using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class UpdateDeveloperMetadataRequest
    {
        public DataFilter[] dataFilters;
        public DeveloperMetadata developerMetadata;
        public string fields;
    }
}