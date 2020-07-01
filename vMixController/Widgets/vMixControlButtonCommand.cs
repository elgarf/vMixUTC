using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using vMixController.Classes;
using vMixController.ViewModel;

namespace vMixController.Widgets
{
    [Serializable]
    public class vMixControlButtonCommand : ObservableObject
    {
        /// <summary>
        /// The <see cref="Action" /> property's name.
        /// </summary>
        public const string ActionPropertyName = "Action";

        private vMixFunctionReference _action = null;

        /// <summary>
        /// Sets and gets the Action property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public vMixFunctionReference Action
        {
            get
            {
                return _action;
            }

            set
            {
                if (_action == value)
                {
                    return;
                }

                _action = value;
                RaisePropertyChanged(ActionPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Parameter" /> property's name.
        /// </summary>
        public const string ParameterPropertyName = "Parameter";

        private string _parameter = "-1";

        /// <summary>
        /// Sets and gets the Parameter property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Parameter
        {
            get
            {
                return _parameter;
            }

            set
            {
                if (_parameter == value)
                {
                    return;
                }

                _parameter = value;
                RaisePropertyChanged(ParameterPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Input" /> property's name.
        /// </summary>
        public const string InputPropertyName = "Input";

        private int _input = -1;

        /// <summary>
        /// Sets and gets the Input property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int Input
        {
            get
            {
                return _input;
            }

            set
            {
                if (_input == value)
                {
                    return;
                }

                _input = value;
                RaisePropertyChanged(InputPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="InputKey" /> property's name.
        /// </summary>
        public const string InputKeyPropertyName = "InputKey";

        private string _inputKey = null;

        /// <summary>
        /// Sets and gets the InputKey property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string InputKey
        {
            get
            {
                return _inputKey;
            }

            set
            {
                if (_inputKey == value)
                {
                    return;
                }

                _inputKey = value;
                RaisePropertyChanged(InputKeyPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="StringParameter" /> property's name.
        /// </summary>
        public const string StringParameterPropertyName = "StringParameter";

        private string _stringParameter = "";

        /// <summary>
        /// Sets and gets the StringParameter property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string StringParameter
        {
            get
            {
                return _stringParameter;
            }

            set
            {
                if (_stringParameter == value)
                {
                    return;
                }

                _stringParameter = value;
                RaisePropertyChanged(StringParameterPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="AdditionalParameters" /> property's name.
        /// </summary>
        public const string AdditionalParametersPropertyName = "AdditionalParameters";

        private List<One<string>> _additionalParameters = new List<One<string>>();

        /// <summary>
        /// Sets and gets the AdditionalParameters property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public List<One<string>> AdditionalParameters
        {
            get
            {
                return _additionalParameters;
            }

            set
            {
                if (_additionalParameters == value)
                {
                    return;
                }

                _additionalParameters = value;
                RaisePropertyChanged(AdditionalParametersPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Collapsed" /> property's name.
        /// </summary>
        public const string CollapsedPropertyName = "Collapsed";

        private bool _collapsed = false;

        /// <summary>
        /// Sets and gets the Collapsed property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Collapsed
        {
            get
            {
                return _collapsed;
            }

            set
            {
                if (_collapsed == value)
                {
                    return;
                }

                _collapsed = value;
                RaisePropertyChanged(CollapsedPropertyName);
                RaisePropertyChanged(AdditionalParametersPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="NoInputAssigned" /> property's name.
        /// </summary>
        public const string NoInputAssignedPropertyName = "NoInputAssigned";

        private bool _noInputAssigned = false;

        /// <summary>
        /// Sets and gets the NoInputAssigned property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool NoInputAssigned
        {
            get
            {
                return _noInputAssigned;
            }

            set
            {
                if (_noInputAssigned == value)
                {
                    return;
                }

                _noInputAssigned = value;
                RaisePropertyChanged(NoInputAssignedPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Ident" /> property's name.
        /// </summary>
        public const string IdentPropertyName = "Ident";
        [NonSerialized]
        private Thickness _ident = new Thickness(0);

        /// <summary>
        /// Sets and gets the Collapsed property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Thickness Ident
        {
            get
            {
                return _ident;
            }

            set
            {
                if (_ident == value)
                {
                    return;
                }

                _ident = value;
                RaisePropertyChanged(IdentPropertyName);
            }
        }

        private string Enquote(string par)
        {
            if (par == null) return par;
            int commas = 0;
            int quotes = 0;
            for (int i = 0; i<par.Length; i++)
            {
                if (par[i] == '\'')
                    quotes++;
                if (par[i] == ',' && quotes % 2 == 0)
                    commas++;
            }
            if (commas > 0)
                par = string.Format("'{0}'", par);
            return par;
        }

        public override string ToString()
        {
            var result = new String('\t', (int)(Ident.Left / 8));
            if (Collapsed)
                result += ">";

            if (Action == null) return "";

            result += Action.Function + "(";
            if (Action.HasInputProperty)
                result += Enquote(InputKey) + ", ";
            if (Action.HasIntProperty)
                result += Enquote(Parameter) + ", ";
            if (Action.HasStringProperty)
                result += Enquote(StringParameter) + ", ";

            var lastMean = 0;
            for (int i = 0; i < AdditionalParameters.Count; i++)
                if (!string.IsNullOrWhiteSpace(AdditionalParameters[i].A))
                    lastMean = i;

            foreach (var item in AdditionalParameters)
            {
                lastMean--;
                if (!string.IsNullOrWhiteSpace(item.A) || lastMean - 1 > 0)
                    result += Enquote(item.A) + ", ";
            }

            if (result.EndsWith(", "))
                result = result.Substring(0, result.Length - 2);

            result += ")";

            return result;
        }

        public static vMixControlButtonCommand FromString(string s)
        {
            var result = new vMixControlButtonCommand();

            for (int i = 0; i < 10; i++)
                result.AdditionalParameters.Add(new One<string>() { A = "" });

            var functions = SimpleIoc.Default.GetInstance<MainViewModel>().Functions;
            result.Action = functions.Where(x => x.Function == "None").FirstOrDefault();

            
            var bracketIndex = s.IndexOf('(');
            if (bracketIndex <= 0)
            {
                var action = functions.Where(x => x.Function == s.Substring(0, s.Length).Trim().TrimStart('>')).FirstOrDefault();
                if (action != null)
                    result.Action = action;
                return result;
            }
            var act = functions.Where(x => x.Function == s.Substring(0, bracketIndex).Trim().TrimStart('>')).FirstOrDefault();
            if (act == null)
                return result;
            else
                result.Action = act;
            int ident = 0;
            for (int i = 0; i < s.Length; i++)
            {
                bool brk = false;
                switch (s[i])
                {
                    case '\t':
                        ident += 8;
                        break;
                    case '>':
                        result.Collapsed = true;
                        break;
                    default:
                        brk = true;
                        break;
                }
                if (brk) break;
            }
            result.Ident = new Thickness(ident, 0, 0, 0);
            if (s.Length - bracketIndex - 2 > 0)
                s = s.Substring(bracketIndex + 1, s.Length - bracketIndex - 2);
            else
                return result;

            List<string> arguments = new List<string>();

            var arg = "";
            var br = 0;
            var bc = 0;
            var str = 0;
            for (int i = 0; i < s.Length; i++)
            {
                switch (s[i])
                {
                    case '\'':
                        str++;
                        break;
                    case '(':
                        br++;
                        break;
                    case ')':
                        br--;
                        break;
                    case '[':
                        bc++;
                        break;
                    case ']':
                        bc--;
                        break;
                    case ',':
                        if (str % 2 == 0)
                        {
                            arguments.Add(arg.Trim());
                            arg = "";
                            continue;
                        }
                        break;
                    default:
                        break;
                }
                arg += s[i];
            }
            arguments.Add(arg.Trim());

            if (act.HasInputProperty)
            {
                result.InputKey = arguments[0];
                arguments.RemoveAt(0);
            }
            if (act.HasIntProperty)
            {
                result.Parameter = arguments[0];
                arguments.RemoveAt(0);
            }
            if (act.HasStringProperty)
            {
                result.StringParameter = arguments[0];
                arguments.RemoveAt(0);
            }
            var parameter = 0;

            while (arguments.Count > 0)
            {
                result.AdditionalParameters[parameter++].A = arguments[0];
                arguments.RemoveAt(0);
            }
            return result;
        }

        public vMixControlButtonCommand()
        {
            _action = new vMixFunctionReference();

            /*if (_additionalParameters.Count < 10)
                for (int i = 0; i < 10; i++)
                    _additionalParameters.Add("");*/
        }

    }
}
