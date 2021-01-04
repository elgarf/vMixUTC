using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class UpdateNamedRangeRequest
    {
        public NamedRange namedRange;
        public string fields;
    }
}