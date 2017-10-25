using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vMixController.Classes;

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

        private ObservableCollection<One<string>> _additionalParameters = new ObservableCollection<One<string>>();

        /// <summary>
        /// Sets and gets the AdditionalParameters property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<One<string>> AdditionalParameters
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

        public vMixControlButtonCommand()
        {
            _action = new vMixFunctionReference();
            /*if (_additionalParameters.Count < 10)
                for (int i = 0; i < 10; i++)
                    _additionalParameters.Add("");*/
        }

    }
}
