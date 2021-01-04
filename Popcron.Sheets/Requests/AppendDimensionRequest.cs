using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class AppendDimensionRequest
    {
        public int sheetId;
        public string dimension;
        public int length;
    }
}