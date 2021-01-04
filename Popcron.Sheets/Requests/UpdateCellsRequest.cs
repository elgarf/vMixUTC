using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class UpdateCellsRequest
    {
        public RowData[] rows;
        public string fields;
        public object area;

        public void Set(object area)
        {
            if (area.GetType() == typeof(GridCoordinate) || area.GetType() == typeof(GridRange))
            {
                this.area = area;
            }
            else
            {
                throw new Exception("Unsupported data type " + area.GetType());
            }
        }
    }
}