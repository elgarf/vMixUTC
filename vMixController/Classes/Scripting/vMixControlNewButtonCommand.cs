using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
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
                RaisePropertyChanged(nameof(Action));
            }
        }

        private string _value = "-1";

        /// <summary>
        /// Sets and gets the Parameter property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Value
        {
            get
            {
                return _value;
            }

            set
            {
                if (_value == value)
                {
                    return;
                }

                _value = value;
                RaisePropertyChanged(nameof(Value));
            }
        }

        private string _selectedIndex = "-1";

        /// <summary>
        /// Sets and gets the Parameter property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }

            set
            {
                if (_selectedIndex == value)
                {
                    return;
                }

                _selectedIndex = value;
                RaisePropertyChanged(nameof(SelectedIndex));
            }
        }

        private string _selectedName = "";

        /// <summary>
        /// Sets and gets the SelectedName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SelectedName
        {
            get
            {
                return _selectedName;
            }

            set
            {
                if (_selectedName == value)
                {
                    return;
                }

                _selectedName = value;
                RaisePropertyChanged(nameof(SelectedName));
            }
        }

        private int _inputNumber = -1;

        /// <summary>
        /// Sets and gets the Input property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int InputNumber
        {
            get
            {
                return _inputNumber;
            }

            set
            {
                if (_inputNumber == value)
                {
                    return;
                }

                _inputNumber = value;
                RaisePropertyChanged(nameof(InputNumber));
            }
        }

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
                RaisePropertyChanged(nameof(InputKey));
            }
        }

        private string _duration = "";

        /// <summary>
        /// Sets and gets the StringParameter property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Duration
        {
            get
            {
                return _duration;
            }

            set
            {
                if (_duration == value)
                {
                    return;
                }

                _duration = value;
                RaisePropertyChanged(nameof(Duration));
            }
        }

        private string _mix = "0";

        /// <summary>
        /// Sets and gets the StringParameter property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Mix
        {
            get
            {
                return _mix;
            }

            set
            {
                if (_mix == value)
                {
                    return;
                }

                _mix = value;
                RaisePropertyChanged(nameof(Mix));
            }
        }

        private string _channel = "";

        /// <summary>
        /// Sets and gets the StringParameter property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Channel
        {
            get
            {
                return _channel;
            }

            set
            {
                if (_channel == value)
                {
                    return;
                }

                _channel = value;
                RaisePropertyChanged(nameof(Channel));
            }
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
                RaisePropertyChanged(nameof(Collapsed));
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
                RaisePropertyChanged(nameof(Ident));
            }
        }

        private bool _useInActiveState = true;

        /// <summary>
        /// Sets and gets the UseInActiveState property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool UseInActiveState
        {
            get
            {
                return _useInActiveState;
            }

            set
            {
                if (_useInActiveState == value)
                {
                    return;
                }

                _useInActiveState = value;
                RaisePropertyChanged(nameof(UseInActiveState));
            }
        }

        private bool _isExecutable = true;

        /// <summary>
        /// Sets and gets the IsExecutable property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsExecutable
        {
            get
            {
                return _isExecutable;
            }

            set
            {
                if (_isExecutable == value)
                {
                    return;
                }

                _isExecutable = value;
                RaisePropertyChanged(nameof(IsExecutable));
            }
        }

        /// <summary>
        /// Сериализует команду в строку, учитывая сигнатуру функции для включения только необходимых параметров.
        /// Формат: [атрибуты] Функция(параметры)
        /// </summary>
        public override string ToString()
        {
            if (Action == null || string.IsNullOrWhiteSpace(Action.Function))
                return string.Empty;

            var sb = new StringBuilder();

            // 1. Атрибуты (добавляем только отличающиеся от дефолтных)
            if (Collapsed) sb.Append("[C] ");
            if (!IsExecutable) sb.Append("[!E] ");
            if (!UseInActiveState) sb.Append("[!S] ");

            // 2. Имя функции
            sb.Append(Action.Function);
            sb.Append("(");

            // 3. Параметры (добавляем только те, что определены в сигнатуре Action)
            var parameters = new List<string>();

            if (Action.HasInput)
            {
                // Предпочитаем InputKey, если он задан, иначе используем Input
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

            if (Action.HasSelectedName)
            {
                parameters.Add(Escape(SelectedName));
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
        /// Создает объект vMixControlButtonCommand из строки, используя сигнатуру функции для корректного парсинга параметров.
        /// </summary>
        public static vMixControlNewButtonCommand FromString(string commandString)
        {
            var allFunctions = SimpleIoc.Default.GetInstance<MainViewModel>().NewFunctions;
            if (string.IsNullOrWhiteSpace(commandString))
                return new vMixControlNewButtonCommand();

            var cmd = new vMixControlNewButtonCommand();
            var remainingString = commandString.Trim();

            // 1. Парсинг атрибутов
            bool attributesParsed = true;
            while (attributesParsed)
            {
                attributesParsed = false;
                if (remainingString.StartsWith("[C] ")) { cmd.Collapsed = true; remainingString = remainingString.Substring(4); attributesParsed = true; }
                if (remainingString.StartsWith("[!E] ")) { cmd.IsExecutable = false; remainingString = remainingString.Substring(5); attributesParsed = true; }
                if (remainingString.StartsWith("[!S] ")) { cmd.UseInActiveState = false; remainingString = remainingString.Substring(5); attributesParsed = true; }
            }

            // 2. Парсинг имени функции и поиск Action
            var openParenIndex = remainingString.IndexOf('(');
            var closeParenIndex = remainingString.LastIndexOf(')');
            if (openParenIndex == -1 || closeParenIndex == -1 || closeParenIndex < openParenIndex)
                return new vMixControlNewButtonCommand(); // Некорректный формат

            var functionName = remainingString.Substring(0, openParenIndex);
            cmd.Action = allFunctions.FirstOrDefault(f => f.Function.Equals(functionName, StringComparison.OrdinalIgnoreCase));
            if (cmd.Action == null)
                return new vMixControlNewButtonCommand(); // Функция не найдена

            // 3. Парсинг параметров
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

            // 4. Распределение параметров по свойствам согласно сигнатуре Action
            if (cmd.Action.HasInput)
            {
                if (currentParamIndex < parameters.Count)
                {
                    var inputParam = Unescape(parameters[currentParamIndex]);
                    // Если параметр - число без кавычек, считаем его Input, иначе - InputKey
                    if (int.TryParse(inputParam, out int inputNum) && parameters[currentParamIndex].Trim() == inputParam)
                    {
                        cmd.InputNumber = inputNum;
                        cmd.InputKey = null;
                    }
                    else
                    {
                        cmd.InputKey = inputParam;
                        // Можно установить Input в 0 или -1 как индикатор, что используется ключ
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
            if (cmd.Action.HasSelectedName)
            {
                if (currentParamIndex < parameters.Count)
                    cmd.SelectedName = Unescape(parameters[currentParamIndex++]);
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
            // Оборачиваем в кавычки, если содержит запятую, пробел или уже является строкой в кавычках
            if (s.Contains(",") || s.Contains(" ") || s.StartsWith("\"") || s.Length == 0)
                return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
            return s; // Числа и простые строки можно не оборачивать
        }

        private static string Unescape(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s.StartsWith("\"") && s.EndsWith("\""))
            {
                string inner = s.Substring(1, s.Length - 2);
                return inner.Replace("\\\"", "\"").Replace("\\\\", "\\");
            }
            return s; // Возвращаем как есть, если это неэкранированная строка (например, число)
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
