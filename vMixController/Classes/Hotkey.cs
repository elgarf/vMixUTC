using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace vMixController.Classes
{
    [Serializable]
    public class Hotkey
    {
        public string Name { get; set; }
        public string Link { get; set; }
        public Key Key { get; set; }
        public bool Ctrl { get; set; }
        public bool Alt { get; set; }
        public bool Shift { get; set; }
        public bool Active { get; set; }
    }
}
