using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class OverlayPosition
    {
        public GridCoordinate anchorCell;
        public int offsetXPixels;
        public int offsetYPixels;
        public int widthPixels;
        public int heightPixels;
    }
}