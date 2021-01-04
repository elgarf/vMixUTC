using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class SetDataValidationRequest
    {
        public GridRange range;
        public DataValidationRule rule;
    }
}