using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class UpdateEmbeddedObjectPositionRequest
    {
        public int objectId;
        public EmbeddedObjectPosition newPosition;
        public string fields;
    }
}