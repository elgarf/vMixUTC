using Newtonsoft.Json;
using Popcron.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleSheetsDataProvider
{
    public class JsonSheetsSerializer : SheetsSerializer
    {
        public override T DeserializeObject<T>(string data)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data);
        }

        public override string SerializeObject(object data)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(data);
        }
    }
}
