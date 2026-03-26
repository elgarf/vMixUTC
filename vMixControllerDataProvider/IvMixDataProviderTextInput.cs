using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace vMixControllerDataProvider
{
    public interface IvMixDataProviderTextInput: IvMixDataProvider
    {
        ICommand PreviewKeyUp { get; set; }
        ICommand GotFocus { get; set; }
        ICommand LostFocus { get; set; }
    }
}
