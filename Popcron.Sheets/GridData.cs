using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class GridData
    {
        public int startRow;
        public int startColumn;
        public RowData[] rowData;
        public DimensionProperties[] rowMetadata;
        public DimensionProperties[] columnMedata;
    }
}