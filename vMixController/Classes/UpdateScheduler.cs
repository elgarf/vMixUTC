using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using vMixController.Classes;

namespace vMixController.Classes
{
    /// <summary>
    /// Coalesces frequent updates by key and executes only the latest work item.
    /// </summary>
    public static class UpdateScheduler
    {
        private sealed class WorkState
        {
            public int Running;
            public int Pending;
            public Action LatestWork;
        }

        private static readonly ConcurrentDictionary<string, WorkState> _states =
            new ConcurrentDictionary<string, WorkState>(StringComparer.Ordinal);

        public static void ScheduleLatest(string key, Action work)
        {
            if (string.IsNullOrWhiteSpace(key) || work == null)
                return;

            var metricKey = GetMetricKey(key);
            PerfMetrics.Increment("scheduler.enqueue." + metricKey);

            var state = _states.GetOrAdd(key, _ => new WorkState());
            state.LatestWork = work;
            Interlocked.Exchange(ref state.Pending, 1);

            if (Interlocked.CompareExchange(ref state.Running, 1, 0) != 0)
                return;

            _ = Task.Run(() =>
            {
                try
                {
                    while (Interlocked.Exchange(ref state.Pending, 0) == 1)
                    {
                        PerfMetrics.Increment("scheduler.run." + metricKey);
                        using (PerfMetrics.Measure("scheduler.run.time." + metricKey))
                        {
                            var action = state.LatestWork;
                            action?.Invoke();
                        }
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref state.Running, 0);
                    if (Interlocked.Exchange(ref state.Pending, 0) == 1 &&
                        Interlocked.CompareExchange(ref state.Running, 1, 0) == 0)
                    {
                        ScheduleLatest(key, state.LatestWork);
                    }
                }
            });
        }

        private static string GetMetricKey(string key)
        {
            var idx = key.IndexOf(':');
            if (idx > 0)
                return key.Substring(0, idx);
            return key;
        }
    }
}

