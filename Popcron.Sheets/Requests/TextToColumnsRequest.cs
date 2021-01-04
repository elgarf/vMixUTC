using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class TextToColumnsRequest
    {
        public GridRange source;
        public string delimeter;
        public string delimeterType;
    }
}