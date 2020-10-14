using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vMixController.Messages
{
    public struct LoadingMessage
    {
        public bool Loading { get; set; }
    }
    public class LIVEToggleMessage
    {
        public int State { get; set; }
    }
}