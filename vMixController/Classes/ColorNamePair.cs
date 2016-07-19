using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace vMixController.Classes
{
    public class ColorNamePair : Pair<Color, string>
    {
        public ColorNamePair(Color a, string b) : base(a, b)
        {
        }
    }
}
