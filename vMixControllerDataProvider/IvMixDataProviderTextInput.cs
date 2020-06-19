using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vMixControllerDataProvider
{
    public interface IvMixDataProviderTextInput: IvMixDataProvider
    {
        object PreviewKeyUp { get; set; }
        object GotFocus { get; set; }
        object LostFocus { get; set; }
    }
}
