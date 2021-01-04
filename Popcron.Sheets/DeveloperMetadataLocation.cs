using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class DeveloperMetadataLocation
    {
        public string locationType;
        public bool spreadsheet;
        public int sheetId;
        public DimensionRange dimensionRange;
    }
}