using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class DuplicateSheetRequest
    {
        public int sourceSheetId;
        public int insertSheetIndex;
        public int newSheetId;
        public string newSheetName;
    }
}