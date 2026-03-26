using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using vMixController.Classes;
using vMixController.Classes.Scripting;
using vMixController.ViewModel;

namespace vMixController.Widgets
{
    internal static class ScriptLoopAnalyzer
    {
        internal enum LinkEdgeKind
        {
            Default = 0,
            DeviceTrigger = 1
        }

        internal sealed class LinkEdge
        {
            public vMixControl Source { get; set; }
            public vMixControl Target { get; set; }
            public LinkEdgeKind Kind { get; set; }
        }

        private sealed class OutgoingLink
        {
            public string Link { get; set; }
            public LinkEdgeKind Kind { get; set; }
        }

        private static readonly Regex QuotedValueRegex = new Regex("['\\\"](?<v>[^'\\\"]+)['\\\"]", RegexOptions.Compiled);

        public static void RefreshPotentialLoopWarnings(IEnumerable<vMixControl> widgets = null, int maxDepth = 10)
        {
            if (maxDepth < 1)
                maxDepth = 1;

            var all = (widgets ?? ResolveWidgets()).Where(x => x != null).Distinct().ToList();
            if (all.Count == 0)
                return;

            var warningWidgets = all
                .Where(x => x is vMixControlButton || x is vMixControlNewButton)
                .ToList();

            var linkTargets = BuildLinkTargets(all);
            var edges = BuildEdges(all, linkTargets);

            foreach (var widget in warningWidgets)
            {
                var cyclePath = FindCyclePath(widget, edges, maxDepth);
                var hasLoop = cyclePath != null && cyclePath.Count > 1;
                var participants = hasLoop
                    ? string.Join(" -> ", cyclePath.Select(GetWidgetLabel))
                    : string.Empty;

                ApplyWarning(widget, hasLoop, participants);
            }
        }

        internal static Dictionary<vMixControl, HashSet<vMixControl>> GetExecLinkEdges(IEnumerable<vMixControl> widgets)
        {
            var all = (widgets ?? ResolveWidgets()).Where(x => x != null).Distinct().ToList();
            if (all.Count == 0)
                return new Dictionary<vMixControl, HashSet<vMixControl>>();

            var linkTargets = BuildLinkTargets(all);
            return BuildEdges(all, linkTargets);
        }

        internal static List<LinkEdge> GetExecLinkEdgesDetailed(IEnumerable<vMixControl> widgets)
        {
            var all = (widgets ?? ResolveWidgets()).Where(x => x != null).Distinct().ToList();
            if (all.Count == 0)
                return new List<LinkEdge>();

            var linkTargets = BuildLinkTargets(all);
            return BuildDetailedEdges(all, linkTargets);
        }

        private static IEnumerable<vMixControl> ResolveWidgets()
        {
            // Avoid forcing MainViewModel creation while it may still be constructing.
            // Calling locator.Main from this path can recurse and overflow stack on startup.
            if (!vMixController.Classes.AppServices.IsRegistered<MainViewModel>())
                return Enumerable.Empty<vMixControl>();

            try
            {
                var main = vMixController.Classes.AppServices.GetRequiredService<MainViewModel>();
                return main?.Widgets ?? Enumerable.Empty<vMixControl>();
            }
            catch
            {
                return Enumerable.Empty<vMixControl>();
            }
        }

        private static Dictionary<string, List<vMixControl>> BuildLinkTargets(IEnumerable<vMixControl> widgets)
        {
            var result = new Dictionary<string, List<vMixControl>>(StringComparer.OrdinalIgnoreCase);
            foreach (var widget in widgets)
            {
                foreach (var hotkey in widget.Hotkey ?? Array.Empty<Hotkey>())
                {
                    if (hotkey == null || !hotkey.Active || string.IsNullOrWhiteSpace(hotkey.Link))
                        continue;

                    var key = hotkey.Link.Trim();
                    if (!result.TryGetValue(key, out var list))
                    {
                        list = new List<vMixControl>();
                        result[key] = list;
                    }

                    if (!list.Contains(widget))
                        list.Add(widget);
                }
            }
            return result;
        }

        private static Dictionary<vMixControl, HashSet<vMixControl>> BuildEdges(
            IEnumerable<vMixControl> widgets,
            IReadOnlyDictionary<string, List<vMixControl>> linkTargets)
        {
            var edges = new Dictionary<vMixControl, HashSet<vMixControl>>();
            foreach (var source in widgets)
            {
                var outgoing = CollectOutgoingLinksDetailed(source)
                    .Select(x => x.Link)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                if (outgoing.Count == 0)
                    continue;

                foreach (var link in outgoing)
                {
                    if (!linkTargets.TryGetValue(link, out var targets))
                        continue;

                    if (!edges.TryGetValue(source, out var set))
                    {
                        set = new HashSet<vMixControl>();
                        edges[source] = set;
                    }

                    foreach (var target in targets)
                        set.Add(target);
                }
            }

            return edges;
        }

        private static List<LinkEdge> BuildDetailedEdges(
            IEnumerable<vMixControl> widgets,
            IReadOnlyDictionary<string, List<vMixControl>> linkTargets)
        {
            var edges = new List<LinkEdge>();
            var dedup = new HashSet<string>(StringComparer.Ordinal);

            foreach (var source in widgets)
            {
                var outgoing = CollectOutgoingLinksDetailed(source);
                foreach (var item in outgoing)
                {
                    if (!linkTargets.TryGetValue(item.Link, out var targets))
                        continue;

                    foreach (var target in targets)
                    {
                        if (target == null)
                            continue;

                        var sourceKey = source?.WidgetId.ToString() ?? "null";
                        var targetKey = target.WidgetId.ToString();
                        var key = $"{sourceKey}|{targetKey}|{(int)item.Kind}";
                        if (!dedup.Add(key))
                            continue;

                        edges.Add(new LinkEdge
                        {
                            Source = source,
                            Target = target,
                            Kind = item.Kind
                        });
                    }
                }
            }

            return edges;
        }

        private static List<OutgoingLink> CollectOutgoingLinksDetailed(vMixControl widget)
        {
            var rawLinks = new List<OutgoingLink>();

            if (widget is vMixControlButton button)
            {
                foreach (var cmd in button.Commands ?? Enumerable.Empty<vMixControlButtonCommand>())
                {
                    if (cmd?.Action?.Function == NativeFunctions.EXECLINK && !string.IsNullOrWhiteSpace(cmd.StringParameter))
                        rawLinks.Add(new OutgoingLink { Link = cmd.StringParameter, Kind = LinkEdgeKind.Default });
                }
            }
            else if (widget is vMixControlNewButton newButton)
            {
                foreach (var cmd in newButton.Commands ?? Enumerable.Empty<vMixControlNewButtonCommand>())
                {
                    if (cmd?.Action?.Function == NewNativeFunctions.EXECLINK && !string.IsNullOrWhiteSpace(cmd.Value))
                        rawLinks.Add(new OutgoingLink { Link = cmd.Value, Kind = LinkEdgeKind.Default });
                }
            }
            else if (widget is vMixControlTimer timer)
            {
                foreach (var link in timer.Links ?? Array.Empty<string>())
                {
                    if (!string.IsNullOrWhiteSpace(link))
                        rawLinks.Add(new OutgoingLink { Link = link, Kind = LinkEdgeKind.Default });
                }
            }
            else if (widget is vMixControlMidiInterface midi)
            {
                foreach (var key in midi.Midis ?? Enumerable.Empty<MidiInterfaceKey>())
                {
                    if (!string.IsNullOrWhiteSpace(key?.C))
                        rawLinks.Add(new OutgoingLink { Link = key.C, Kind = LinkEdgeKind.DeviceTrigger });
                }
            }
            else if (widget is vMixControlStreamDeck streamDeck)
            {
                foreach (var key in streamDeck.Keys ?? Enumerable.Empty<StreamDeckKey>())
                {
                    if (!string.IsNullOrWhiteSpace(key?.B))
                        rawLinks.Add(new OutgoingLink { Link = key.B, Kind = LinkEdgeKind.DeviceTrigger });
                }
            }

            var result = new List<OutgoingLink>();
            var dedup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var raw in rawLinks)
            {
                foreach (var candidate in ExpandLinkCandidates(raw.Link))
                {
                    var key = $"{candidate}|{(int)raw.Kind}";
                    if (!dedup.Add(key))
                        continue;

                    result.Add(new OutgoingLink { Link = candidate, Kind = raw.Kind });
                }
            }

            return result;
        }

        private static IEnumerable<string> ExpandLinkCandidates(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                yield break;

            var trimmed = raw.Trim();
            if (trimmed.Length == 0)
                yield break;

            yield return trimmed;

            if ((trimmed.StartsWith("\"") && trimmed.EndsWith("\"")) ||
                (trimmed.StartsWith("'") && trimmed.EndsWith("'")))
            {
                var unquoted = trimmed.Substring(1, trimmed.Length - 2).Trim();
                if (!string.IsNullOrWhiteSpace(unquoted))
                    yield return unquoted;
            }

            foreach (Match match in QuotedValueRegex.Matches(trimmed))
            {
                var value = match.Groups["v"].Value?.Trim();
                if (!string.IsNullOrWhiteSpace(value))
                    yield return value;
            }
        }

        private static List<vMixControl> FindCyclePath(
            vMixControl start,
            IReadOnlyDictionary<vMixControl, HashSet<vMixControl>> edges,
            int maxDepth)
        {
            var path = new List<vMixControl> { start };
            var visitedOnPath = new HashSet<vMixControl> { start };
            return FindCyclePathCore(start, start, edges, maxDepth, path, visitedOnPath, 0)
                ? path
                : null;
        }

        private static bool FindCyclePathCore(
            vMixControl start,
            vMixControl current,
            IReadOnlyDictionary<vMixControl, HashSet<vMixControl>> edges,
            int maxDepth,
            List<vMixControl> path,
            HashSet<vMixControl> visitedOnPath,
            int depth)
        {
            if (depth >= maxDepth)
                return false;

            if (!edges.TryGetValue(current, out var nexts) || nexts.Count == 0)
                return false;

            foreach (var next in nexts)
            {
                if (next == null)
                    continue;

                if (next == start && path.Count >= 1)
                {
                    path.Add(start);
                    return true;
                }

                if (visitedOnPath.Contains(next))
                    continue;

                visitedOnPath.Add(next);
                path.Add(next);
                if (FindCyclePathCore(start, next, edges, maxDepth, path, visitedOnPath, depth + 1))
                    return true;
                path.RemoveAt(path.Count - 1);
                visitedOnPath.Remove(next);
            }

            return false;
        }

        private static string GetWidgetLabel(vMixControl widget)
        {
            if (widget == null)
                return "Unknown";

            var name = string.IsNullOrWhiteSpace(widget.Name) ? widget.Type : widget.Name;
            return $"{name} [{widget.Type}]";
        }

        private static void ApplyWarning(vMixControl widget, bool warning, string participants)
        {
            if (widget is vMixControlButton button)
            {
                button.PotentialLoopWarning = warning;
                button.PotentialLoopParticipants = participants;
            }
            else if (widget is vMixControlNewButton newButton)
            {
                newButton.PotentialLoopWarning = warning;
                newButton.PotentialLoopParticipants = participants;
            }
        }
    }
}



