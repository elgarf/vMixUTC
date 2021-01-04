using System;

namespace Popcron.Sheets
{
    [Serializable]
    public abstract class SheetsSerializer
    {
        public abstract T DeserializeObject<T>(string data);
        public abstract string SerializeObject(object data);

        public static SheetsSerializer Serializer { get; set; } = null;
    }
}