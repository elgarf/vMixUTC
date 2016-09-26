using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vMixController.Classes
{
    public class SharedData : Singleton<SharedData>
    {
        public Func<string, string, IList<string>> GetData { get; set; }
        public Func<string, object> GetDataSource { get; set; }
        public Func<IList<string>> GetDataSources { get; set; }
        public Func<string, IList<string>> GetDataSourceProps { get; set; }

        private SharedData()
        {

        }
    }
}
