using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class vMixControlNewButtonCommand : ObservableObject, ICloneable
    {
        private vMixNewFunctionReference _action = null;

        /// <summary>
        /// Sets and gets the Action property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public vMixNewFunctionReference Action
        {
            get => _action;
            set => SetProperty(ref _action, value);
        }

        private string _value = "-1";

        /// <summary>
        /// Sets and gets the Parameter property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        private string _selectedIndex = "-1";

        /// <summary>
        /// Sets and gets the Parameter property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SelectedIndex
        {
            get => _selectedIndex;
            set => SetProperty(ref _selectedIndex, value);
        }

        private int _inputNumber = -1;

        /// <summary>
        /// Sets and gets the Input property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int InputNumber
        {
            get => _inputNumber;
            set => SetProperty(ref _inputNumber, value);
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

        private string _duration = "";

        /// <summary>
        /// Sets and gets the StringParameter property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        private string _mix = "0";

        /// <summary>
        /// Sets and gets the StringParameter property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Mix
        {
            get => _mix;
            set => SetProperty(ref _mix, value);
        }

        private string _channel = "";

        /// <summary>
        /// Sets and gets the StringParameter property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Channel
        {
            get => _channel;
            set => SetProperty(ref _channel, value);
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
            }
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

            if (Action.HasInput)
            {
                // ������������ InputKey, ���� �� �����, ����� ���������� Input
                parameters.Add(!string.IsNullOrEmpty(InputKey) ? Escape(InputKey) : InputNumber.ToString());
            }
            if (Action.HasValue)
            {
                parameters.Add(Escape(Value));
            }

            if (Action.HasIndex)
            {
                parameters.Add(Escape(SelectedIndex));
            }

            if (Action.HasDuration)
            {
                parameters.Add(Escape(Duration));
            }
            if (Action.HasChannel)
            {
                parameters.Add(Escape(Channel));
            }
            if (Action.HasMix)
            {
                parameters.Add(Escape(Mix));
            }

            sb.Append(string.Join(",", parameters));
            sb.Append(")");

            return sb.ToString();
        }

        /// <summary>
        /// ������� ������ vMixControlButtonCommand �� ������, ��������� ��������� ������� ��� ����������� �������� ����������.
        /// </summary>
        public static vMixControlNewButtonCommand FromString(string commandString)
        {
            var allFunctions = vMixController.Classes.AppServices.GetRequiredService<MainViewModel>().NewFunctions;
            if (string.IsNullOrWhiteSpace(commandString))
                return new vMixControlNewButtonCommand();

            var cmd = new vMixControlNewButtonCommand();
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
                return new vMixControlNewButtonCommand(); // ������������ ������

            var functionName = remainingString.Substring(0, openParenIndex);
            cmd.Action = allFunctions.FirstOrDefault(f => f.Function.Equals(functionName, StringComparison.OrdinalIgnoreCase));
            if (cmd.Action == null)
                return new vMixControlNewButtonCommand(); // ������� �� �������

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
            if (cmd.Action.HasInput)
            {
                if (currentParamIndex < parameters.Count)
                {
                    var inputParam = Unescape(parameters[currentParamIndex]);
                    // ���� �������� - ����� ��� �������, ������� ��� Input, ����� - InputKey
                    if (int.TryParse(inputParam, out int inputNum) && parameters[currentParamIndex].Trim() == inputParam)
                    {
                        cmd.InputNumber = inputNum;
                        cmd.InputKey = null;
                    }
                    else
                    {
                        cmd.InputKey = inputParam;
                        // ����� ���������� Input � 0 ��� -1 ��� ���������, ��� ������������ ����
                        cmd.InputNumber = -2;
                    }
                    currentParamIndex++;
                }
            }

            if (cmd.Action.HasValue)
            {
                if (currentParamIndex < parameters.Count)
                    cmd.Value = Unescape(parameters[currentParamIndex++]);
            }
            if (cmd.Action.HasIndex)
            {
                if (currentParamIndex < parameters.Count)
                    cmd.SelectedIndex = Unescape(parameters[currentParamIndex++]);
            }
            if (cmd.Action.HasDuration)
            {
                if (currentParamIndex < parameters.Count)
                    cmd.Duration = Unescape(parameters[currentParamIndex++]);
            }

            if (cmd.Action.HasChannel)
            {
                if (currentParamIndex < parameters.Count)
                    cmd.Channel = Unescape(parameters[currentParamIndex++]);
            }

            if (cmd.Action.HasMix)
            {
                if (currentParamIndex < parameters.Count)
                    cmd.Mix = Unescape(parameters[currentParamIndex++]);
            }

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
            return vMixControlNewButtonCommand.FromString(this.ToString());
        }

        public vMixControlNewButtonCommand()
        {
            _action = new vMixNewFunctionReference();

            /*if (_additionalParameters.Count < 10)
                for (int i = 0; i < 10; i++)
                    _additionalParameters.Add("");*/
        }

    }
}



