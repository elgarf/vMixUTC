using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;
using vMixController.Classes;
using vMixController.ViewModel;

namespace vMixController.Classes.Scripting
{
    [Serializable]
    public class vMixControlButtonCommand : ObservableObject, ICloneable
    {
        private vMixFunctionReference _action = null;

        /// <summary>
        /// Sets and gets the Action property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public vMixFunctionReference Action
        {
            get => _action;
            set => SetProperty(ref _action, value);
        }

        private string _parameter = "-1";

        /// <summary>
        /// Sets and gets the Parameter property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Parameter
        {
            get => _parameter;
            set => SetProperty(ref _parameter, value);
        }


        private string _floatParameter = "-1";

        /// <summary>
        /// Sets and gets the FloatParameter property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string FloatParameter
        {
            get => _floatParameter;
            set => SetProperty(ref _floatParameter, value);
        }

        private int _input = -1;

        /// <summary>
        /// Sets and gets the Input property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int Input
        {
            get => _input;
            set => SetProperty(ref _input, value);
        }

        private string _inputKey = null;

        /// <summary>
        /// Sets and gets the InputKey property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string InputKey
        {
            get => _inputKey;
            set => SetProperty(ref _inputKey, value);
        }

        private string _stringParameter = "";

        /// <summary>
        /// Sets and gets the StringParameter property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string StringParameter
        {
            get => _stringParameter;
            set => SetProperty(ref _stringParameter, value);
        }

        private List<One<string>> _additionalParameters = new List<One<string>>();

        /// <summary>
        /// Sets and gets the AdditionalParameters property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public List<One<string>> AdditionalParameters
        {
            get => _additionalParameters;
            set => SetProperty(ref _additionalParameters, value);
        }

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
                OnPropertyChanged(nameof(Collapsed));
                OnPropertyChanged(nameof(AdditionalParameters));
            }
        }

        private bool _noInputAssigned = false;

        /// <summary>
        /// Sets and gets the NoInputAssigned property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool NoInputAssigned
        {
            get => _noInputAssigned;
            set => SetProperty(ref _noInputAssigned, value);
        }

        [NonSerialized]
        private Thickness _ident = new Thickness(0);

        /// <summary>
        /// Sets and gets the Collapsed property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Thickness Ident
        {
            get => _ident;
            set => SetProperty(ref _ident, value);
        }

        private bool _useInActiveState = true;

        /// <summary>
        /// Sets and gets the UseInActiveState property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool UseInActiveState
        {
            get => _useInActiveState;
            set => SetProperty(ref _useInActiveState, value);
        }

        private bool _isExecutable = true;

        /// <summary>
        /// Sets and gets the IsExecutable property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsExecutable
        {
            get => _isExecutable;
            set => SetProperty(ref _isExecutable, value);
        }

        /// <summary>
        /// ����������� ������� � ������, �������� ��������� ������� ��� ��������� ������ ����������� ����������.
        /// ������: [��������] �������(���������)
        /// </summary>
        public override string ToString()
        {
            if (Action == null || string.IsNullOrWhiteSpace(Action.Function))
                return string.Empty;

            var sb = new StringBuilder();

            // 1. �������� (��������� ������ ������������ �� ���������)
            if (Collapsed) sb.Append("[C] ");
            if (!IsExecutable) sb.Append("[!E] ");
            if (!UseInActiveState) sb.Append("[!S] ");

            // 2. ��� �������
            sb.Append(Action.Function);
            sb.Append("(");

            // 3. ��������� (��������� ������ ��, ��� ���������� � ��������� Action)
            var parameters = new List<string>();

            if (Action.HasInputProperty && !NoInputAssigned)
            {
                // ������������ InputKey, ���� �� �����, ����� ���������� Input
                parameters.Add(!string.IsNullOrEmpty(InputKey) ? Escape(InputKey) : Input.ToString());
            }
            if (Action.HasIntProperty)
            {
                parameters.Add(Escape(Parameter));
            }
            if (Action.HasStringProperty)
            {
                parameters.Add(Escape(StringParameter));
            }
            if (Action.HasFloatProperty)
            {
                parameters.Add(Escape(FloatParameter));
            }
            if (Action.AdditionalCount > 0 && AdditionalParameters != null)
            {
                parameters.AddRange(AdditionalParameters.Take(Action.AdditionalCount).Select(p => Escape(p.A)));
            }

            sb.Append(string.Join(",", parameters));
            sb.Append(")");

            return sb.ToString();
        }

        /// <summary>
        /// ������� ������ vMixControlButtonCommand �� ������, ��������� ��������� ������� ��� ����������� �������� ����������.
        /// </summary>
        public static vMixControlButtonCommand FromString(string commandString)
        {
            var allFunctions = vMixController.Classes.AppServices.GetRequiredService<MainViewModel>().Functions;
            if (string.IsNullOrWhiteSpace(commandString))
                return new vMixControlButtonCommand();

            var cmd = new vMixControlButtonCommand();
            var remainingString = commandString.Trim();

            // 1. ������� ���������
            bool attributesParsed = true;
            while (attributesParsed)
            {
                attributesParsed = false;
                if (remainingString.StartsWith("[C] ")) { cmd.Collapsed = true; remainingString = remainingString.Substring(4); attributesParsed = true; }
                if (remainingString.StartsWith("[!E] ")) { cmd.IsExecutable = false; remainingString = remainingString.Substring(5); attributesParsed = true; }
                if (remainingString.StartsWith("[!S] ")) { cmd.UseInActiveState = false; remainingString = remainingString.Substring(5); attributesParsed = true; }
            }

            // 2. ������� ����� ������� � ����� Action
            var openParenIndex = remainingString.IndexOf('(');
            var closeParenIndex = remainingString.LastIndexOf(')');
            if (openParenIndex == -1 || closeParenIndex == -1 || closeParenIndex < openParenIndex)
                return new vMixControlButtonCommand(); // ������������ ������

            var functionName = remainingString.Substring(0, openParenIndex);
            cmd.Action = allFunctions.FirstOrDefault(f => f.Function.Equals(functionName, StringComparison.OrdinalIgnoreCase));
            if (cmd.Action == null)
                return new vMixControlButtonCommand(); // ������� �� �������

            // 3. ������� ����������
            var paramsString = remainingString.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1);
            List<string> parameters = new List<string>();
            if (!string.IsNullOrEmpty(paramsString))
            {
                parameters = Regex.Matches(paramsString, @"(""[^""\\]*(?:\\.[^""\\]*)*""|[^,]+)")
                                  .Cast<Match>()
                                  .Select(m => m.Value.Trim())
                                  .ToList();
            }

            int currentParamIndex = 0;

            // 4. ������������� ���������� �� ��������� �������� ��������� Action
            if (cmd.Action.HasInputProperty && !cmd.NoInputAssigned)
            {
                if (currentParamIndex < parameters.Count)
                {
                    var inputParam = Unescape(parameters[currentParamIndex]);
                    // ���� �������� - ����� ��� �������, ������� ��� Input, ����� - InputKey
                    if (int.TryParse(inputParam, out int inputNum) && parameters[currentParamIndex].Trim() == inputParam)
                    {
                        cmd.Input = inputNum;
                        cmd.InputKey = null;
                    }
                    else
                    {
                        cmd.InputKey = inputParam;
                        // ����� ���������� Input � 0 ��� -1 ��� ���������, ��� ������������ ����
                        cmd.Input = 0;
                    }
                    currentParamIndex++;
                }
            }

            if (cmd.Action.HasIntProperty)
            {
                if (currentParamIndex < parameters.Count)
                    cmd.Parameter = Unescape(parameters[currentParamIndex++]);
            }

            if (cmd.Action.HasStringProperty)
            {
                if (currentParamIndex < parameters.Count)
                    cmd.StringParameter = Unescape(parameters[currentParamIndex++]);
            }

            if (cmd.Action.HasFloatProperty)
            {
                if (currentParamIndex < parameters.Count)
                    cmd.FloatParameter = Unescape(parameters[currentParamIndex++]);
            }

            if (cmd.Action.AdditionalCount > 0)
            {
                cmd.AdditionalParameters = new List<One<string>>();
                for (int i = 0; i < cmd.Action.AdditionalCount; i++)
                {
                    if (currentParamIndex < parameters.Count)
                        cmd.AdditionalParameters.Add(new One<string>() { A = Unescape(parameters[currentParamIndex++]) });
                    else
                        break; // ���������� � ������ ������, ��� ������� �������
                }
            }

            while (cmd.AdditionalParameters.Count < 10)
                cmd.AdditionalParameters.Add(new One<string>() { A = "" });

            return cmd;
        }

        private static string Escape(string s)
        {
            if (s == null) return "\"\"";
            // ����������� � �������, ���� �������� �������, ������ ��� ��� �������� ������� � ��������
            if (s.Contains(",") || s.Contains(" ") || s.StartsWith("\"") || s.Length == 0)
                return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
            return s; // ����� � ������� ������ ����� �� �����������
        }

        private static string Unescape(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s.StartsWith("\"") && s.EndsWith("\""))
            {
                string inner = s.Substring(1, s.Length - 2);
                return inner.Replace("\\\"", "\"").Replace("\\\\", "\\");
            }
            return s; // ���������� ��� ����, ���� ��� ���������������� ������ (��������, �����)
        }

        public object Clone()
        {
            return vMixControlButtonCommand.FromString(this.ToString());
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



