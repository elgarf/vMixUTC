using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class ExtendedValue
    {
        public double numberValue;
        public string stringValue;
        public bool boolValue;
        public string formulaValue;
        public ErrorValue errorValue;
    }
}