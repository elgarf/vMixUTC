using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class ProtectedRange
    {
        public int protectedRangeId;
        public GridRange range;
        public string namedRangeId;
        public string description;
        public bool warningOnly;
        public bool requestingUserCanEdit;
        public GridRange[] unprotectedRanges;
        public Editors editors;
    }
}