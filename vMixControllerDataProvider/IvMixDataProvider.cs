using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace vMixControllerDataProvider
{
    public interface IvMixDataProvider
    {
        /// <summary>
        /// Period of updating data in msec, used for internal data provider needs
        /// </summary>
        int Period { get; set; }
        /// <summary>
        /// TRUE, if data provider has additional properties, which shows when 
        /// </summary>
        bool IsProvidingCustomProperties { get; }
        string[] Values { get; }
        void ShowProperties(Window owner);
        UIElement CustomUI { get; }
        List<object> GetProperties();
        void SetProperties(List<object> props);
    }
}
