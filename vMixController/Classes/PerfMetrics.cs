using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace vMixController.Classes
{
    public static class PerfMetrics
    {
        private sealed class Metric
        {
            public long Count;
            public long TotalTicks;
            public long MaxTicks;
            public long Errors;
        }

        private sealed class MeasureScope : IDisposable
        {
            private readonly string _name;
            private readonly Stopwatch _sw;
            private bool _disposed;

            public MeasureScope(string name)
            {
                _name = name;
                _sw = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;
                _sw.Stop();
                Record(_name, _sw.ElapsedTicks);
            }
        }

        private sealed class EmptyScope : IDisposable
        {
            public static readonly EmptyScope Instance = new EmptyScope();
            public void Dispose()
            {
            }
        }

        private static readonly ConcurrentDictionary<string, Metric> Metrics =
            new ConcurrentDictionary<string, Metric>(StringComparer.Ordinal);

        public static IDisposable Measure(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return EmptyScope.Instance;

            return new MeasureScope(name);
        }

        public static void Increment(string name, int delta = 1)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            var metric = Metrics.GetOrAdd(name, _ => new Metric());
            Interlocked.Add(ref metric.Count, delta);
        }

        public static void Error(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            var metric = Metrics.GetOrAdd(name, _ => new Metric());
            Interlocked.Increment(ref metric.Errors);
        }

        public static string SnapshotAndReset()
        {
            var snapshot = Metrics.ToArray();
            Metrics.Clear();

            if (snapshot.Length == 0)
                return "No metrics";

            var sb = new StringBuilder();
            foreach (var pair in snapshot.OrderBy(p => p.Key, StringComparer.Ordinal))
            {
                var key = pair.Key;
                var m = pair.Value;

                var count = Volatile.Read(ref m.Count);
                var totalTicks = Volatile.Read(ref m.TotalTicks);
                var maxTicks = Volatile.Read(ref m.MaxTicks);
                var errors = Volatile.Read(ref m.Errors);

                var avgMs = count > 0 ? TimeSpan.FromTicks(totalTicks / count).TotalMilliseconds : 0d;
                var maxMs = TimeSpan.FromTicks(maxTicks).TotalMilliseconds;

                sb.Append(key)
                  .Append(": count=").Append(count)
                  .Append(", avgMs=").Append(avgMs.ToString("0.###"))
                  .Append(", maxMs=").Append(maxMs.ToString("0.###"))
                  .Append(", errors=").Append(errors)
                  .AppendLine();
            }

            return sb.ToString().TrimEnd();
        }

        private static void Record(string name, long elapsedTicks)
        {
            var metric = Metrics.GetOrAdd(name, _ => new Metric());
            Interlocked.Increment(ref metric.Count);
            Interlocked.Add(ref metric.TotalTicks, elapsedTicks);

            long initial;
            do
            {
                initial = Volatile.Read(ref metric.MaxTicks);
                if (initial >= elapsedTicks)
                    break;
            } while (Interlocked.CompareExchange(ref metric.MaxTicks, elapsedTicks, initial) != initial);
        }
    }
}
