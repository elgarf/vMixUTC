using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vMixController.Classes.Scripting
{
    public enum NumericValueType
    {
        None,
        Integer,
        Float
    }

    [Serializable]
    public class vMixNewFunctionReference
    {
        public bool Native { get; set; }
        public string Description { get; set; }
        public string Function { get; set; }
        public string Category { get; set; }
        public string ValueName { get; set; }
        public bool MultipleMixes { get; set; }
        public NumericValueType NumericValue { get; set; }
        public string XPath { get; set; }
        public string XPathValue { get; set; }

        public string DirectPath { get; set; }
        public string DirectValueType { get; set; }

        public bool HasInput { get; set; }
        public bool HasValue { get; set; }
        public bool HasMix { get; set; }
        public bool HasDuration { get; set; }
        public bool HasChannel { get; set; }
        public bool HasIndex { get; set; }
        public bool HasSelectedName { get; set; }
        public bool IsBlock { get; set; }

        private string _formatString;

        public string FormatString
        {
            get { return _formatString; }
            set
            {
                HasInput = value.Contains("$Input");
                HasValue = value.Contains("$Value");
                HasChannel = value.Contains("$Channel");
                HasMix = value.Contains("$Mix");
                HasDuration = value.Contains("$Duration");
                HasIndex = value.Contains("$SelectedIndex");
                HasSelectedName = value.Contains("$SelectedName");
                _formatString = value;
            }
        }

        public int Timeout { get; set; } = 5000;

        public override bool Equals(object obj)
        {
            return (obj is vMixNewFunctionReference) && Function == (obj as vMixNewFunctionReference).Function;
        }

        public override int GetHashCode()
        {
            if (Function != null)
                return Function.GetHashCode();
            else
                return 0;
        }

        public vMixNewFunctionReference()
        {

        }

        public string GetSignature()
        {
            var parameters = new List<string>();
            if (HasInput)
                parameters.Add("Input");
            if (HasValue)
                parameters.Add("Value");
            if (HasChannel)
                parameters.Add("Channel");
            if (HasMix)
                parameters.Add("Mix");
            if (HasDuration)
                parameters.Add("Duration");
            if (HasIndex)
                parameters.Add("SelectedIndex");
            if (HasSelectedName)
                parameters.Add("SelectedName");
            return parameters.Count > 0 ? Function + "(" + string.Join(", ", parameters) + ")" : Function;
        }

    }
}
