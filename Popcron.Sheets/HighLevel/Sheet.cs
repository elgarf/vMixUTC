using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class Sheet
    {
        /// <summary>  
        /// Returns the sheetId of the sheet.  
        /// </summary> 
        public int ID
        {
            get
            {
                return raw.properties.sheetId;
            }
        }

        /// <summary>  
        /// Returns the amount of rows are in the spreadsheet.  
        /// </summary> 
        public int Rows
        {
            get
            {
                return rows;//raw.data[0].rowData.Length;
            }
        }

        /// <summary>  
        /// Returns the maximum amount of columns in the spreadsheet.  
        /// </summary> 
        public int Columns
        {
            get
            {
                return columns;
            }
        }

        /// <summary>  
        /// Returns the spreadsheet grid.  
        /// </summary> 
        public Cell[,] Data
        {
            get
            {
                return data;
            }
        }

        /// <summary>  
        /// The name of the sheet.  
        /// </summary> 
        public string Title
        {
            get
            {
                return raw.properties.title;
            }
        }

        /// <summary>  
        /// True if the sheet is hidden in the UI.  
        /// </summary> 
        public bool Hidden
        {
            get
            {
                return raw.properties.hidden;
            }
        }

        /// <summary>  
        /// True if the sheet is an RTL sheet instead of an LTR sheet.  
        /// </summary> 
        public bool RightToLeft
        {
            get
            {
                return raw.properties.rightToLeft;
            }
        }

        private SheetRaw raw;
        public int columns;
        int rows;
        private Cell[,] data;

        public Sheet(SheetRaw raw)
        {
            this.raw = raw;
            rows = raw.data[0].rowData.Length;
            //cache columns
            for (int i = 0; i < raw.data[0].rowData.Length; i++)
            {
                /*int values = 0;
                for (int v = 0; v < raw.data[0].rowData[i].values.Length; v++)
                {
                    if (!string.IsNullOrEmpty(raw.data[0].rowData[i].values[v].formattedValue)) values++;
                }*/
                if ((raw.data[0].rowData[i].values?.Length ?? 0) > columns)
                {
                    columns = raw.data[0].rowData[i].values.Length;
                }
            }

            //cache data
            data = new Cell[Columns, Rows];
            for (int y = 0; y < raw.data[0].rowData.Length; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                        if (raw.data[0].rowData.Length > y && (raw.data[0].rowData[y].values?.Length ?? 0) > x)
                        {
                            data[x, y] = new Cell(raw.data[0].rowData[y].values[x]);
                        }
                        else
                        {
                            data[x, y] = Cell.Empty;
                        }
                }
            }


            for (int y = Rows - 1; y >= 0; y--)
            {
                bool empty = true;
                for (int x = 0; x < Columns; x++)
                    empty = empty && (data[x, y].IsEmpty || string.IsNullOrWhiteSpace(data[x, y].Value));
                if (empty)
                    rows--;
                else
                    break;
            }


        }
    }
}