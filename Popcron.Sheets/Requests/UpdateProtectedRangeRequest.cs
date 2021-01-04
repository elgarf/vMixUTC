using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class UpdateProtectedRangeRequest
    {
        public ProtectedRange protectedRange;
        public string fields;
    }
}