using NCalc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace vMixController.Classes.Scripting
{
    #region Вспомогательные классы для передачи данных

    /// <summary>
    /// Неизменяемый класс для передачи аргументов форматирования.
    /// Заменяет тип 'record'.
    /// </summary>
    public class XPathNewFormattingArgs
    {
        public string InputKey { get; }
        public int InputNumber { get; }
        public string Value { get; }
        public string Mix { get; }
        public string Channel { get; }
        public string Duration { get; }
        public string SelectedIndex { get; }
        public string SelectedName { get; }

        public Func<string, string, string> Resolve { get; }
        public XPathNewFormattingArgs(string inputKey, int inputNumber, string value, string index, string selectedName, string mix, string channel, string duration, Func<string, string, string> resolve = null)
        {
            InputKey = inputKey;
            InputNumber = inputNumber;
            Value = value;
            Mix = mix;
            Channel = channel;
            Duration = duration;
            SelectedIndex = index;
            SelectedName = selectedName;
            Resolve = resolve;
        }
    }

    /// <summary>
    /// Класс для хранения результата подготовки аргументов.
    /// Заменяет кортеж (XPathFormattingArgs, bool).
    /// </summary>
    public class NewPrepareArgsResult
    {
        public XPathNewFormattingArgs Args { get; }
        public bool HasError { get; }

        public NewPrepareArgsResult(XPathNewFormattingArgs args, bool hasError)
        {
            Args = args;
            HasError = hasError;
        }
    }

    #endregion
    public static class vMixControlNewButtonHelper
    {
        private static readonly Regex ConditionalBlockRegex = new Regex(@"\$\{\s*if\s+([^{}]+?)\s*\}(.*?)(?:\$\{\s*else\s+if\s+([^{}]+?)\s*\}(.*?))?(?:\$\{\s*else\s*\}(.*?))?\$\{\s*endif\s*\}", RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex PlaceholderRegex = new Regex(@"\$([\w_]+)(:\S+:\d+)?", RegexOptions.Compiled);
        private static readonly Regex FunctionRegex = new Regex(@"\$([\w_]+)\((?<param>[^)]*)\)", RegexOptions.Compiled);
        private static readonly Regex ConditionRegex = new Regex(@"^([\w_]+)\s*(==|!=|<|>|<=|>=)\s*(.+)$", RegexOptions.Compiled);

        public static string ReplacePlaceholdersFromObject(this string template, XPathNewFormattingArgs dataObject)
        {
            if (string.IsNullOrEmpty(template) || dataObject == null)
            {
                return template;
            }

            var replacementDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(XPathNewFormattingArgs.InputKey)] = dataObject.InputKey ?? string.Empty,
                [nameof(XPathNewFormattingArgs.InputNumber)] = dataObject.InputNumber.ToString(CultureInfo.InvariantCulture),
                [nameof(XPathNewFormattingArgs.Value)] = dataObject.Value ?? string.Empty,
                [nameof(XPathNewFormattingArgs.Mix)] = dataObject.Mix ?? string.Empty,
                [nameof(XPathNewFormattingArgs.Channel)] = dataObject.Channel ?? string.Empty,
                [nameof(XPathNewFormattingArgs.Duration)] = dataObject.Duration ?? string.Empty,
                [nameof(XPathNewFormattingArgs.SelectedIndex)] = dataObject.SelectedIndex ?? string.Empty,
                [nameof(XPathNewFormattingArgs.SelectedName)] = dataObject.SelectedName ?? string.Empty
            };

            // 1. Обработка условий (if/else if/else)
            // Этот паттерн будет искать самый внешний блок if/endif и работать с его содержимым.
            // Для вложенных условий потребуется рекурсивный подход или более сложный парсер.
            string result = template;
            MatchCollection matches = ConditionalBlockRegex.Matches(result);

            // Обрабатываем условия от самого внутреннего к самому внешнему, чтобы избежать проблем с вложенностью.
            // Однако текущий Regex не поддерживает вложенность из-за .*?
            // Для полной поддержки вложенности нужен более сложный парсер, возможно, с использованием стека.
            // Пока что, этот код будет работать только с не вложенными блоками if/else.
            // Если вложенность нужна, нужно будет переработать. Для простоты, пока сосредоточимся на не вложенных.

            // Проходим по совпадениям в обратном порядке, чтобы индексы не сбивались
            for (int i = matches.Count - 1; i >= 0; i--)
            {
                Match match = matches[i];
                string fullMatch = match.Value;
                string condition1 = match.Groups[1].Value.Trim();
                string trueBlock1 = match.Groups[2].Value;
                string condition2 = match.Groups[3].Success ? match.Groups[3].Value.Trim() : null;
                string trueBlock2 = match.Groups[4].Success ? match.Groups[4].Value : null;
                string elseBlock = match.Groups[5].Success ? match.Groups[5].Value : string.Empty;

                string evaluatedContent = string.Empty;

                if (EvaluateCondition(condition1, replacementDictionary))
                {
                    evaluatedContent = trueBlock1;
                }
                else if (condition2 != null && EvaluateCondition(condition2, replacementDictionary))
                {
                    evaluatedContent = trueBlock2;
                }
                else
                {
                    evaluatedContent = elseBlock;
                }

                // Заменяем полный блок условия на вычисленное содержимое
                result = result.Remove(match.Index, match.Length).Insert(match.Index, evaluatedContent);
            }

            result = PlaceholderRegex.Replace(result, match =>
            {
                string fullPlaceholder = match.Value;
                string variableName = match.Groups[1].Value;
                string partSpecifier = match.Groups[2].Value;

                if (replacementDictionary.TryGetValue(variableName, out string rawValue))
                {
                    if (!string.IsNullOrEmpty(partSpecifier))
                    {
                        try
                        {
                            string[] parts = partSpecifier.Substring(1).Split(':');
                            if (parts.Length == 2)
                            {
                                char separator = parts[0][0];
                                int index = int.Parse(parts[1]);

                                string[] rawValueParts = rawValue.Split(separator);

                                if (index >= 0 && index < rawValueParts.Length)
                                {
                                    return rawValueParts[index];
                                }
                            }
                        }
                        catch { /* Ошибка парсинга или индекса, возвращаем полный плейсхолдер */ }
                    }
                    return rawValue; // Возвращаем полное значение или плейсхолдер при ошибке части
                }
                return fullPlaceholder; // Переменная не найдена
            });

            // 2. Обработка функций $Function($Parameter)
            // Этот паттерн ищет вызовы функций, включая возможные вложенные скобки для параметров
            result = FunctionRegex.Replace(result, match =>
            {
                string fullFunctionCall = match.Value;
                string functionName = match.Groups[1].Value;
                string parameter = match.Groups["param"].Value; // Получаем параметр

                // Если есть Resolve метод и он не null, пытаемся вызвать его
                if (dataObject.Resolve != null)
                {
                    // Передаем полное имя функции и параметр в Resolve
                    return dataObject.Resolve(functionName, parameter);
                }
                return fullFunctionCall; // Возвращаем как есть, если Resolve не доступен или не справился
            });

            return result;
        }

        // Вспомогательный метод для оценки условия
        private static bool EvaluateCondition(string conditionExpression, Dictionary<string, string> data)
        {
            // Паттерн для условий: PropertyName Operator "Value" или PropertyName Operator Number
            Match match = ConditionRegex.Match(conditionExpression);

            if (!match.Success)
            {
                // Если не соответствует паттерну сравнения, пробуем как булево свойство
                if (data.TryGetValue(conditionExpression, out string boolValueStr))
                {
                    return bool.TryParse(boolValueStr, out bool boolVal) && boolVal;
                }
                return false;
            }

            string propertyName = match.Groups[1].Value;
            string op = match.Groups[2].Value;
            string operand = match.Groups[3].Value.Trim();

            if (!data.TryGetValue(propertyName, out string propertyValueStr))
            {
                return false; // Свойство не найдено
            }

            // Попытка привести операнды к общему типу для сравнения
            // Удаляем кавычки, если операнд строковый
            if (operand.StartsWith("\"") && operand.EndsWith("\""))
            {
                operand = operand.Substring(1, operand.Length - 2);
            }

            // Сравнение строк
            if (op == "==") return string.Equals(propertyValueStr, operand, StringComparison.OrdinalIgnoreCase);
            if (op == "!=") return !string.Equals(propertyValueStr, operand, StringComparison.OrdinalIgnoreCase);

            // Сравнение чисел
            if (double.TryParse(propertyValueStr, out double propVal) && double.TryParse(operand, out double opVal))
            {
                switch (op)
                {
                    case "<": return propVal < opVal;
                    case ">": return propVal > opVal;
                    case "<=": return propVal <= opVal;
                    case ">=": return propVal >= opVal;
                }
            }

            // Если не удалось сравнить как числа или строки, считаем false
            return false;
        }

        public static bool CalculateExpression<T>(string expressionString, Action<SafeExpression> populateVariables, EvaluateFunctionHandler evaluateFunctionHandler, out T result)
            => vMixControlExpressionHelper.CalculateExpression(expressionString, populateVariables, evaluateFunctionHandler, out result);


        /// <summary>
        /// Определяет, зависит ли состояние от XPath-выражений на основе набора команд.
        /// </summary>
        /// <param name="doc">XML-документ для проверки.</param>
        /// <param name="variableExpander">Функция для раскрытия переменных (например, "[VAR]").</param>
        /// <returns>Объект XPathStateResult, содержащий результат и флаг наличия ошибок.</returns>
        public static XPathStateResult CalculateStateDependency(this ObservableCollection<vMixControlNewButtonCommand> _commands, XmlDocument doc, Func<string, string> variableExpander, Action<SafeExpression> PopulateVariables, EvaluateFunctionHandler Exp_EvaluateFunction)
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
                NewPrepareArgsResult preparationResult = PrepareFormattingArgs(doc, command, variableExpander, PopulateVariables, Exp_EvaluateFunction, inputNumberByKey, inputKeyByNumber);
                if (preparationResult.HasError)
                {
                    hasErrors = true;
                    continue; // Пропускаем команду, если не удалось подготовить аргументы
                }
                XPathNewFormattingArgs args = preparationResult.Args;

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
                string expectedValuePattern = command.Action.XPathValue.ReplacePlaceholdersFromObject(args);

                if (CompareValues(actualValue, expectedValuePattern))
                {
                    isStateDependent = true;
                }
            }

            return new XPathStateResult(isStateDependent, hasErrors);
        }

        /// <summary>
        /// Готовит и вычисляет все аргументы, необходимые для форматирования строк XPath и значений.
        /// </summary>
        private static NewPrepareArgsResult PrepareFormattingArgs(XmlDocument doc, vMixControlNewButtonCommand item, Func<string, string> variableExpander, Action<SafeExpression> PopulateVariables, EvaluateFunctionHandler Exp_EvaluateFunction, Dictionary<string, int> inputNumberByKey, Dictionary<int, string> inputKeyByNumber)
        {
            try
            {
                string expandedInputKey = variableExpander(item.InputKey);

                int inputNumber = inputNumberByKey.TryGetValue(expandedInputKey, out var numberByKey)
                    ? numberByKey
                    : -1;

                int intParam = int.MinValue;
                float floatParam = float.MinValue;
                string strParam = String.Empty;
                string value = String.Empty;

                switch (item.Action.NumericValue)
                {
                    case NumericValueType.Integer:
                        CalculateExpression<int>(item.Value, PopulateVariables, Exp_EvaluateFunction, out intParam);
                        value = intParam.ToString(CultureInfo.InvariantCulture);
                        break;
                    case NumericValueType.Float:
                        CalculateExpression<float>(item.Value, PopulateVariables, Exp_EvaluateFunction, out floatParam);
                        value = floatParam.ToString(CultureInfo.InvariantCulture);
                        break;
                    default:
                        CalculateExpression<string>(item.Value, PopulateVariables, Exp_EvaluateFunction, out strParam);
                        value = strParam;
                        break;
                }
                CalculateExpression<string>(item.SelectedIndex, PopulateVariables, Exp_EvaluateFunction, out string index);
                CalculateExpression<string>(item.SelectedName, PopulateVariables, Exp_EvaluateFunction, out string selectedName);
                if (string.IsNullOrEmpty(selectedName))
                    selectedName = item.SelectedName;
                var args = new XPathNewFormattingArgs(expandedInputKey, inputNumber, value, index, selectedName, item.Mix, item.Channel, item.Duration, (function, parameter) =>
                {
                    switch (function)
                    {
                        case "GetInputKey":
                            if (int.TryParse(parameter, NumberStyles.Integer, CultureInfo.InvariantCulture, out int inputNumberFromParameter)
                                && inputKeyByNumber.TryGetValue(inputNumberFromParameter, out var keyByNumber))
                                return keyByNumber;
                            return "-1";
                        case "IntMinus":
                            int val = -1;
                            if (int.TryParse(parameter, out val))
                                return (val - 1).ToString(CultureInfo.InvariantCulture);
                            else
                                return parameter;
                    }
                    return parameter;
                });
                return new NewPrepareArgsResult(args, false);
            }
            catch (Exception) // Ловим ошибки парсинга или вычислений
            {
                return new NewPrepareArgsResult(null, true);
            }
        }

        /// <summary>
        /// Определяет и форматирует итоговый XPath-путь для выполнения.
        /// </summary>
        private static string GetEffectiveXPath(vMixNewFunctionReference action, XPathNewFormattingArgs args)
        {
            string pathTemplate = null;

            if (action == null) return null;

            pathTemplate = action.XPath;

            if (string.IsNullOrWhiteSpace(pathTemplate))
            {
                return null;
            }

            return pathTemplate.ReplacePlaceholdersFromObject(args);
        }

        private enum ComparisonOperator
        {
            Equals,
            Contains,
            NotContains,
            RegExp,
            StartsWith,
            EndsWith,
            NotEquals
        }

        public static bool CompareValues(string actualValue, string expectedValue)
        {
            if (expectedValue == null)
            {
                return actualValue == null;
            }

            ComparisonOperator op = ComparisonOperator.Equals;
            string pattern = expectedValue;
            bool ignoreCase = false;

            string normalizedExpectedValue = expectedValue.Trim();
            if (normalizedExpectedValue.StartsWith("ignore case:", StringComparison.OrdinalIgnoreCase))
            {
                ignoreCase = true;
                pattern = normalizedExpectedValue.Substring("ignore case:".Length).Trim();
            }

            if (normalizedExpectedValue.StartsWith("not:", StringComparison.OrdinalIgnoreCase))
            {
                op = ComparisonOperator.NotEquals;
                pattern = normalizedExpectedValue.Substring("not:".Length).Trim();
            }

            if (normalizedExpectedValue.StartsWith("contains:", StringComparison.OrdinalIgnoreCase))
            {
                op = ComparisonOperator.Contains;
                pattern = normalizedExpectedValue.Substring("contains:".Length).TrimStart();
            }
            else if (normalizedExpectedValue.StartsWith("not contains:", StringComparison.OrdinalIgnoreCase))
            {
                op = ComparisonOperator.NotContains;
                pattern = normalizedExpectedValue.Substring("not contains:".Length).TrimStart();
            }
            else if (normalizedExpectedValue.StartsWith("regexp:", StringComparison.OrdinalIgnoreCase))
            {
                op = ComparisonOperator.RegExp;
                pattern = normalizedExpectedValue.Substring("regexp:".Length).TrimStart();
            }
            else if (normalizedExpectedValue.StartsWith("starts with:", StringComparison.OrdinalIgnoreCase))
            {
                op = ComparisonOperator.StartsWith;
                pattern = normalizedExpectedValue.Substring("starts with:".Length).TrimStart();
            }
            else if (normalizedExpectedValue.StartsWith("ends with:", StringComparison.OrdinalIgnoreCase))
            {
                op = ComparisonOperator.EndsWith;
                pattern = normalizedExpectedValue.Substring("ends with:".Length).TrimStart();
            }

            if (op != ComparisonOperator.Equals && string.IsNullOrEmpty(pattern))
            {
                if (op == ComparisonOperator.Contains || op == ComparisonOperator.NotContains || op == ComparisonOperator.StartsWith || op == ComparisonOperator.EndsWith)
                {
                    if (actualValue == null) return false;
                    if (op == ComparisonOperator.Contains || op == ComparisonOperator.StartsWith || op == ComparisonOperator.EndsWith)
                    {
                        return actualValue.Contains("");
                    }
                    else if (op == ComparisonOperator.NotContains)
                    {
                        return !actualValue.Contains("");
                    }
                }
                if (op == ComparisonOperator.RegExp)
                {
                    return string.IsNullOrEmpty(actualValue);
                }
            }

            if (actualValue == null)
            {
                switch (op)
                {
                    case ComparisonOperator.Equals:
                        return pattern == null;
                    case ComparisonOperator.Contains:
                    case ComparisonOperator.NotContains:
                    case ComparisonOperator.RegExp:
                    case ComparisonOperator.StartsWith:
                    case ComparisonOperator.EndsWith:
                        return false;
                    default:
                        return false;
                }
            }

            StringComparison stringComparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            RegexOptions regexOptions = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;

            switch (op)
            {
                case ComparisonOperator.NotEquals:
                    return !actualValue.Equals(pattern, stringComparison);

                case ComparisonOperator.Equals:
                    return actualValue.Equals(pattern, stringComparison);

                case ComparisonOperator.Contains:
                    return actualValue.IndexOf(pattern, stringComparison) >= 0;

                case ComparisonOperator.NotContains:
                    return actualValue.IndexOf(pattern, stringComparison) < 0;

                case ComparisonOperator.RegExp:
                    try
                    {
                        return Regex.IsMatch(actualValue, pattern, regexOptions);
                    }
                    catch (ArgumentException)
                    {
                        return false;
                    }

                case ComparisonOperator.StartsWith:
                    return actualValue.StartsWith(pattern, stringComparison);

                case ComparisonOperator.EndsWith:
                    return actualValue.EndsWith(pattern, stringComparison);

                default:
                    throw new InvalidOperationException("Unknown comparison operator after parsing.");
            }
        }
    }
}
