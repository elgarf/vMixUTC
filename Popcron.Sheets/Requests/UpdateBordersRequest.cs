using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class UpdateBordersRequest
    {
        public GridRange range;
        public Border top;
        public Border bottom;
        public Border left;
        public Border right;
        public Border innerHorizontal;
        public Border innerVertical;
    }
}