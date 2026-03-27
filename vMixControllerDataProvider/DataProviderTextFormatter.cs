using System;
using System.Collections.Generic;
using System.Linq;

namespace vMixControllerDataProvider
{
    public static class DataProviderTextFormatter
    {
        public static string JoinWithPipe(IEnumerable<string> values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            return string.Join("|", values.Select(v => v ?? string.Empty));
        }

        public static List<string> GroupByPipe(IReadOnlyList<string> values, int groupBy)
        {
            var normalizedGroupBy = Math.Max(1, groupBy);
            if (values == null || values.Count == 0)
            {
                return new List<string>();
            }

            if (normalizedGroupBy <= 1)
            {
                return values.Select(v => v ?? string.Empty).ToList();
            }

            var grouped = new List<string>((values.Count + normalizedGroupBy - 1) / normalizedGroupBy);
            for (int i = 0; i < values.Count; i += normalizedGroupBy)
            {
                int count = Math.Min(normalizedGroupBy, values.Count - i);
                var slice = new string[count];
                for (int j = 0; j < count; j++)
                {
                    slice[j] = values[i + j] ?? string.Empty;
                }

                grouped.Add(JoinWithPipe(slice));
            }

            return grouped;
        }
    }
}
