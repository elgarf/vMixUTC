using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class CutPasteRequest
    {
        public GridRange source;
        public GridCoordinate destination;
        public string pasteType;
    }
}