using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class Cell
    {
        /// <summary>  
        /// Short hand for creating an invalid cell.  
        /// </summary>
        public static Cell Empty
        {
            get
            {
                return new Cell();
            }
        }

        /// <summary>  
        /// Checks if the cell is empty.  
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return raw == null;
            }
        }

        /// <summary>  
        /// Returns the formatted value of this cell.  
        /// </summary>
        public string Value
        {
            get
            {
                if (raw != null)
                {
                    return raw.formattedValue;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>  
        /// Returns the effective value of this cell.  
        /// </summary>
        public ExtendedValue ExtendedValue
        {
            get
            {
                if (raw != null)
                {
                    return raw.effectiveValue;
                }
                else
                {
                    return null;
                }
            }
        }

        private CellData raw;

        internal Cell()
        {

        }

        internal Cell(CellData raw)
        {
            this.raw = raw;
        }
    }
}