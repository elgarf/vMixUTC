using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class CopyPasteRequest
    {
        public GridRange source;
        public GridCoordinate destination;
        public string pasteType;
        public string pasteOrientation;
    }
}