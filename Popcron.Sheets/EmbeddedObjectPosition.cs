using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class EmbeddedObjectPosition
    {
        public int sheetId;
        public OverlayPosition overlayPosition;
        public bool newSheet;
    }
}