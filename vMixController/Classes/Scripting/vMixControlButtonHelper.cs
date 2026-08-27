using NCalc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace vMixController.Classes.Scripting
{
    #region Вспомогательные классы для передачи данных

    /// <summary>
    /// Класс для хранения результата выполнения основной функции.
    /// Заменяет кортеж (bool, bool).
    /// </summary>
    public class XPathStateResult
    {
        public bool IsStateDependent { get; }
        public bool HasErrors { get; }

        public XPathStateResult(bool isStateDependent, bool hasErrors)
        {
            IsStateDependent = isStateDependent;
            HasErrors = hasErrors;
        }
    }

    /// <summary>
    /// Неизменяемый класс для передачи аргументов форматирования.
    /// Заменяет тип 'record'.
    /// </summary>
    public class XPathFormattingArgs
    {
        public string InputKey { get; }
        public int InputNumber { get; }
        public int IntParameter { get; }
        public string FloatParameter { get; }
        public object StringParameter { get; }
        public string KeyByInt { get; }
        public string KeyByString { get; }

        public XPathFormattingArgs(string inputKey, int inputNumber, int intParameter, object stringParameter, string keyByInt, string keyByString, string floatParameter)
        {
            InputKey = inputKey;
            InputNumber = inputNumber;
            IntParameter = intParameter;
            StringParameter = stringParameter;
            KeyByInt = keyByInt;
            KeyByString = keyByString;
            FloatParameter = floatParameter;
        }
    }

    /// <summary>
    /// Класс для хранения результата подготовки аргументов.
    /// Заменяет кортеж (XPathFormattingArgs, bool).
    /// </summary>
    public class PrepareArgsResult
    {
        public XPathFormattingArgs Args { get; }
        public bool HasError { get; }

        public PrepareArgsResult(XPathFormattingArgs args, bool hasError)
        {
            Args = args;
            HasError = hasError;
        }
    }

    #endregion
    public static class vMixControlButtonHelper
    {
        public static bool CalculateExpression<T>(string expressionString, Action<SafeExpression> populateVariables, EvaluateFunctionHandler evaluateFunctionHandler, out T result)
            => vMixControlExpressionHelper.CalculateExpression(expressionString, populateVariables, evaluateFunctionHandler, out result);

        /// <summary>
        /// Определяет, зависит ли состояние от XPath-выражений на основе набора команд.
        /// </summary>
        /// <param name="doc">XML-документ для проверки.</param>
        /// <param name="variableExpander">Функция для раскрытия переменных (например, "[VAR]").</param>
        /// <returns>Объект XPathStateResult, содержащий результат и флаг наличия ошибок.</returns>
        public static XPathStateResult CalculateStateDependency(this ObservableCollection<vMixControlButtonCommand> _commands, XmlDocument doc, Func<string, string> variableExpander, Action<SafeExpression> PopulateVariables, EvaluateFunctionHandler Exp_EvaluateFunction)
        {
            if (doc == null || variableExpander == null)
            {
                return new XPathStateResult(false, true); // Возвращаем ошибку, если входные данные неверны
            }

            bool isStateDependent = false;
            bool hasErrors = false;
            vMixControlExpressionHelper.BuildInputMaps(doc, out var inputNumberByKey, out var inputKeyByNumber);

            foreach (var command in _commands.ToList())
            {
                if (!command.UseInActiveState)
                {
                    continue;
                }

                // 1. Подготовка всех необходимых аргументов
                PrepareArgsResult preparationResult = PrepareFormattingArgs(doc, command, variableExpander, PopulateVariables, Exp_EvaluateFunction, inputNumberByKey, inputKeyByNumber);
                if (preparationResult.HasError)
                {
                    hasErrors = true;
                    continue; // Пропускаем команду, если не удалось подготовить аргументы
                }
                XPathFormattingArgs args = preparationResult.Args;

                // 2. Получение и форматирование XPath-пути
                string xpath = GetEffectiveXPath(command.Action, args);
                if (string.IsNullOrWhiteSpace(xpath))
                {
                    continue;
                }

                // 3. Извлечение фактического значения из XML
                string actualValue = vMixControlExpressionHelper.GetNodeValue(doc, xpath);
                if (actualValue == null) // Узел не найден
                {
                    continue;
                }

                // 4. Форматирование ожидаемого значения и сравнение
                string expectedValuePattern = string.Format(command.Action.ActiveStateValue,
                    args.InputNumber, args.IntParameter, args.StringParameter, args.IntParameter - 1,
                    args.InputNumber, "", args.KeyByInt, args.KeyByString);

                if (ValuesMatch(actualValue, expectedValuePattern))
                {
                    isStateDependent = true;
                }
            }

            return new XPathStateResult(isStateDependent, hasErrors);
        }

        /// <summary>
        /// Готовит и вычисляет все аргументы, необходимые для форматирования строк XPath и значений.
        /// </summary>
        private static PrepareArgsResult PrepareFormattingArgs(XmlDocument doc, vMixControlButtonCommand item, Func<string, string> variableExpander, Action<SafeExpression> PopulateVariables, EvaluateFunctionHandler Exp_EvaluateFunction, Dictionary<string, int> inputNumberByKey, Dictionary<int, string> inputKeyByNumber)
        {
            try
            {
                string expandedInputKey = variableExpander(item.InputKey);

                int inputNumber = inputNumberByKey.TryGetValue(expandedInputKey, out var numberByKey)
                    ? numberByKey
                    : -1;

                CalculateExpression<int>(item.Parameter, PopulateVariables, Exp_EvaluateFunction, out int intParam);
                CalculateExpression<object>(item.StringParameter, PopulateVariables, Exp_EvaluateFunction, out object strParam);
                CalculateExpression<float>(item.Parameter, PopulateVariables, Exp_EvaluateFunction, out float floatParam);

                int strParamAsInt;
                // Использование 'out var' доступно в C# 7.0, поддерживаемом .NET 4.7.2
                if (strParam == null || !int.TryParse(strParam.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out strParamAsInt))
                {
                    strParamAsInt = int.MinValue;
                }

                inputKeyByNumber.TryGetValue(intParam, out string keyByInt);
                inputKeyByNumber.TryGetValue(strParamAsInt, out string keyByString);

                var fps = floatParam.ToString(CultureInfo.InvariantCulture);

                var args = new XPathFormattingArgs(expandedInputKey, inputNumber, intParam, strParam, keyByInt, keyByString, item.Action.CommaFloatDelimiter ? fps.Replace('.', ',') : fps);
                return new PrepareArgsResult(args, false);
            }
            catch (Exception) // Ловим ошибки парсинга или вычислений
            {
                return new PrepareArgsResult(null, true);
            }
        }

        /// <summary>
        /// Определяет и форматирует итоговый XPath-путь для выполнения.
        /// </summary>
        private static string GetEffectiveXPath(vMixFunctionReference action, XPathFormattingArgs args)
        {
            string pathTemplate = null;

            if (action == null) return null;

            if (action.ActiveStateXPathIntDependence != null && args.IntParameter >= 0 && args.IntParameter < action.ActiveStateXPathIntDependence.Length)
            {
                pathTemplate = action.ActiveStateXPathIntDependence[args.IntParameter];
            }
            else if (!string.IsNullOrWhiteSpace(action.ActiveStateXPath))
            {
                pathTemplate = action.ActiveStateXPath;
            }

            if (string.IsNullOrWhiteSpace(pathTemplate))
            {
                return null;
            }

            return string.Format(pathTemplate,
                args.InputKey, args.IntParameter, args.StringParameter, args.IntParameter - 1,
                args.InputNumber, "", args.KeyByInt, args.KeyByString, args.FloatParameter);
        }

        /// <summary>
        /// Сравнивает фактическое значение с ожидаемым шаблоном, учитывая операторы.
        /// </summary>
        private static bool ValuesMatch(string actualValue, string expectedValuePattern)
        {
            if (expectedValuePattern == null) return false;

            char operatorChar = expectedValuePattern.Length > 0 ? expectedValuePattern[0] : ' ';
            bool isNegated = operatorChar == '!';

            if (isNegated)
            {
                expectedValuePattern = expectedValuePattern.Substring(1);
                operatorChar = expectedValuePattern.Length > 0 ? expectedValuePattern[0] : ' ';
            }

            string valueToCompare = expectedValuePattern;
            if (operatorChar == '~' || operatorChar == '`')
            {
                valueToCompare = expectedValuePattern.Substring(1);
            }

            bool result;
            switch (operatorChar)
            {
                case '~': // contains
                    result = actualValue.IndexOf(valueToCompare, StringComparison.Ordinal) >= 0;
                    break;
                case '`': // not contains
                    result = actualValue.IndexOf(valueToCompare, StringComparison.Ordinal) < 0;
                    break;
                default:
                    if (expectedValuePattern == "*") // wildcard
                        result = true;
                    else if (expectedValuePattern == "-") // check for null/whitespace
                        result = string.IsNullOrWhiteSpace(actualValue);
                    else // direct equality
                        result = actualValue == expectedValuePattern;
                    break;
            }

            return isNegated ? !result : result;
        }
    }
}
