using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vMixController.Classes
{
    [Serializable]
    public class vMixFunctionReference
    {
        public string Description { get; set; }
        public string Function { get; set; }
        public bool Native { get; set; }
        private string _formatString;
        //Input - 0, Int Value - 1, String Value - 2
        public string FormatString { get { return _formatString; } set {
                _formatString = value;
                if (!string.IsNullOrWhiteSpace(_formatString))
                {
                    HasInputProperty = _formatString.Contains("{0}");
                    HasIntProperty = _formatString.Contains("{1}");
                    HasStringProperty = _formatString.Contains("{2}");
                    if (!HasInputProperty)
                        InputDescription = "N/A";
                    if (!HasIntProperty)
                        IntDescription = "N/A";
                    if (!HasStringProperty)
                        StringDescription = "N/A";
                }
            } }

        public bool HasInputProperty { get; set; }
        public bool HasStringProperty { get; set; }
        public bool HasIntProperty { get; set; }

        public string ActiveStatePath { get; set; }
        public string ActiveStateValue { get; set; }

        public bool StateDirect { get; set; }
        public string StatePath { get; set; }
        public string StateValue { get; set; } 

        public int TransitionNumber { get; set; }


        public string IntDescription { get; set; }
        public string StringDescription { get; set; }
        public string InputDescription { get; set; }

        public int[] IntValues { get; set; }
        public string[] StringValues { get; set; }

        public override bool Equals(object obj)
        {
            return (obj is vMixFunctionReference) && Function == (obj as vMixFunctionReference).Function;
        }

        public override int GetHashCode()
        {
            return Function.GetHashCode();
        }

        public vMixFunctionReference()
        {
            InputDescription = "Input";
            IntDescription = "Integer";
            StringDescription = "String";
        }

    }
}
