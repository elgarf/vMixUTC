using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class FindReplaceResponse
    {
        public int valuesChanged;
        public int formulasChanged;
        public int rowsChanged;
        public int sheetsChanged;
        public int occurrencesChanged;
    }
}
