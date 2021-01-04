using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class TextFormat
    {
        public Color foregroundColor;
        public string fontFamily;
        public int fontSize;
        public bool bold;
        public bool italic;
        public bool strikethrough;
        public bool underline;
    }
}