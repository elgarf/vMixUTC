using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.VisualBasic;
using vMixController.Messages;
using vMixController.Classes;
using vMixController.ViewModel;
using vMixController.Widgets;

namespace vMixController.Controls
{
    public partial class WidgetLinksOverlay : UserControl
    {
        private IMessenger Messenger => AppServices.IsRegistered<IMessenger>()
            ? AppServices.GetRequiredService<IMessenger>()
            : WeakReferenceMessenger.Default;

        private readonly HashSet<vMixControl> _subscribedWidgets = new HashSet<vMixControl>();
        private readonly List<vMixControl> _subscriptionRemovedBuffer = new List<vMixControl>();
        private readonly List<vMixControl> _subscriptionAddedBuffer = new List<vMixControl>();
        private readonly Dictionary<vMixControl, Size> _actualWidgetSizes = new Dictionary<vMixControl, Size>();
        private readonly Popup _legendPopup;
        private INotifyCollectionChanged _itemsCollection;
        private bool _renderScheduled;
        private Window _ownerWindow;
        private vMixControl _hoveredWidget;
        private static readonly Pen LinkPen = CreatePen(128, 255, 255, 255);
        private static readonly Pen DeviceLinkPen = CreatePen(128, 0, 255, 0);
        private static readonly DoubleCollection LinkDashArray = CreateDashArray();
        private static readonly SolidColorBrush CrossPageCycleBrush = CreateBrush(128, 255, 0, 0);

        public WidgetLinksOverlay()
        {
            InitializeComponent();
            _legendPopup = new Popup
            {
                AllowsTransparency = true,
                Placement = PlacementMode.Relative,
                StaysOpen = true,
                IsHitTestVisible = false
            };
            Loaded += (_, __) =>
            {
                AttachOwnerWindowHandlers();
                SubscribeCollection(Items);
                UpdateWidgetSubscriptions(Items);
                UpdateLegendPopupPresentation();
                Messenger.Register<HoveredWidgetMessage>(this, (r, m) => OnHoveredWidgetChanged(m));
                ScheduleRender();
            };
            SizeChanged += (_, __) => ScheduleRender();
            IsVisibleChanged += (_, __) => UpdateLegendPopupPresentation();
            IsEnabledChanged += (_, __) => UpdateLegendPopupPresentation();
            Unloaded += (_, __) => DisposeSubscriptions();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == OpacityProperty || e.Property == VisibilityProperty)
                UpdateLegendPopupPresentation();
        }

        public IEnumerable<vMixControl> Items
        {
            get => (IEnumerable<vMixControl>)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public int CurrentPage
        {
            get => (int)GetValue(CurrentPageProperty);
            set => SetValue(CurrentPageProperty, value);
        }

        public FrameworkElement PopupHost
        {
            get => (FrameworkElement)GetValue(PopupHostProperty);
            set => SetValue(PopupHostProperty, value);
        }

        public ScrollViewer ScrollHost
        {
            get => (ScrollViewer)GetValue(ScrollHostProperty);
            set => SetValue(ScrollHostProperty, value);
        }

        public FrameworkElement TransformSource
        {
            get => (FrameworkElement)GetValue(TransformSourceProperty);
            set => SetValue(TransformSourceProperty, value);
        }

        public string Mode
        {
            get => (string)GetValue(ModeProperty);
            set => SetValue(ModeProperty, value);
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(
                nameof(Items),
                typeof(IEnumerable<vMixControl>),
                typeof(WidgetLinksOverlay),
                new PropertyMetadata(null, ItemsPropertyChanged));

        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register(
                nameof(CurrentPage),
                typeof(int),
                typeof(WidgetLinksOverlay),
                new PropertyMetadata(0, CurrentPagePropertyChanged));

        public static readonly DependencyProperty PopupHostProperty =
            DependencyProperty.Register(
                nameof(PopupHost),
                typeof(FrameworkElement),
                typeof(WidgetLinksOverlay),
                new PropertyMetadata(null, PopupHostChanged));

        public static readonly DependencyProperty ScrollHostProperty =
            DependencyProperty.Register(
                nameof(ScrollHost),
                typeof(ScrollViewer),
                typeof(WidgetLinksOverlay),
                new PropertyMetadata(null, ScrollHostChanged));

        public static readonly DependencyProperty TransformSourceProperty =
            DependencyProperty.Register(
                nameof(TransformSource),
                typeof(FrameworkElement),
                typeof(WidgetLinksOverlay),
                new PropertyMetadata(null, TransformSourceChanged));

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register(
                nameof(Mode),
                typeof(string),
                typeof(WidgetLinksOverlay),
                new PropertyMetadata(vMixController.Classes.Constants.LINKS_NONE, ModeChanged));

        private static void ModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WidgetLinksOverlay overlay)
                overlay.ScheduleRender();
        }

        private static void ItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WidgetLinksOverlay overlay)
                overlay.OnItemsChanged(e.OldValue as IEnumerable<vMixControl>, e.NewValue as IEnumerable<vMixControl>);
        }

        private static void CurrentPagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WidgetLinksOverlay overlay)
                overlay.ScheduleRender();
        }

        private static void PopupHostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WidgetLinksOverlay overlay)
                overlay.ScheduleRender();
        }

        private static void ScrollHostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is WidgetLinksOverlay overlay))
                return;

            if (e.OldValue is ScrollViewer oldScroll)
                oldScroll.ScrollChanged -= overlay.ScrollHostScrollChanged;

            if (e.NewValue is ScrollViewer newScroll)
                newScroll.ScrollChanged += overlay.ScrollHostScrollChanged;

            overlay.ScheduleRender();
        }

        private static void TransformSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is WidgetLinksOverlay overlay))
                return;

            if (e.OldValue is FrameworkElement oldSource)
                oldSource.LayoutUpdated -= overlay.TransformSourceLayoutUpdated;

            if (e.NewValue is FrameworkElement newSource)
                newSource.LayoutUpdated += overlay.TransformSourceLayoutUpdated;

            overlay.ScheduleRender();
        }

        private void OnItemsChanged(IEnumerable<vMixControl> oldItems, IEnumerable<vMixControl> newItems)
        {
            SubscribeCollection(newItems);
            UpdateWidgetSubscriptions(newItems);
            ScheduleRender();
        }

        private void SubscribeCollection(IEnumerable<vMixControl> items)
        {
            var nextCollection = items as INotifyCollectionChanged;
            if (ReferenceEquals(_itemsCollection, nextCollection))
                return;

            if (_itemsCollection != null)
                _itemsCollection.CollectionChanged -= ItemsCollectionChanged;

            _itemsCollection = nextCollection;
            if (_itemsCollection != null)
                _itemsCollection.CollectionChanged += ItemsCollectionChanged;
        }

        private void UnsubscribeCollection(IEnumerable<vMixControl> items)
        {
            var oldCollection = items as INotifyCollectionChanged ?? _itemsCollection;
            if (oldCollection != null)
                oldCollection.CollectionChanged -= ItemsCollectionChanged;

            _itemsCollection = null;
        }

        private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateWidgetSubscriptions(Items);
            ScheduleRender();
        }

        private void UpdateWidgetSubscriptions(IEnumerable<vMixControl> items)
        {
            var next = new HashSet<vMixControl>((items ?? Enumerable.Empty<vMixControl>()).Where(x => x != null));

            _subscriptionRemovedBuffer.Clear();
            foreach (var existing in _subscribedWidgets)
            {
                if (!next.Contains(existing))
                    _subscriptionRemovedBuffer.Add(existing);
            }

            foreach (var removed in _subscriptionRemovedBuffer)
            {
                removed.PropertyChanged -= WidgetPropertyChanged;
                _subscribedWidgets.Remove(removed);
            }

            _subscriptionAddedBuffer.Clear();
            foreach (var candidate in next)
            {
                if (!_subscribedWidgets.Contains(candidate))
                    _subscriptionAddedBuffer.Add(candidate);
            }

            foreach (var added in _subscriptionAddedBuffer)
            {
                added.PropertyChanged += WidgetPropertyChanged;
                _subscribedWidgets.Add(added);
            }
        }

        private void WidgetPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ScheduleRender();
        }

        private void ScrollHostScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScheduleRender();
        }

        private void TransformSourceLayoutUpdated(object sender, EventArgs e)
        {
            ScheduleRender();
        }

        private void ScheduleRender()
        {
            if (_renderScheduled)
                return;

            _renderScheduled = true;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _renderScheduled = false;
                RenderLines();
            }), DispatcherPriority.Render);
        }

        public void RequestRender()
        {
            ScheduleRender();
        }

        private void RenderLines()
        {
            PART_Path.Data = null;
            PART_Arrows.Data = null;
            PART_DevicePath.Data = null;
            PART_DeviceArrows.Data = null;
            PART_DynamicLayer.Children.Clear();

            if (Mode == vMixController.Classes.Constants.LINKS_NONE)
                return;

            RefreshActualSizeCache();

            var widgets = (Items ?? Enumerable.Empty<vMixControl>())
                .Where(x => x != null)
                .ToList();
            if (widgets.Count == 0)
            {
                HideLegendPopup();
                return;
            }

            var edges = ScriptLoopAnalyzer.GetExecLinkEdgesDetailed(widgets);
            if (edges.Count == 0)
            {
                HideLegendPopup();
                return;
            }

            var widgetOrder = widgets
                .Select((widget, index) => new { widget, index })
                .ToDictionary(x => x.widget, x => x.index);
            var visibleEdges = new List<ScriptLoopAnalyzer.LinkEdge>();
            
            if ((Mode == vMixController.Classes.Constants.LINKS_HOVER && _hoveredWidget != null) ||
                 Mode == vMixController.Classes.Constants.LINKS_ALL)
                visibleEdges = edges
                    .Where(e => e.Source != null && e.Target != null && (e.Source.Page == CurrentPage || e.Target.Page == CurrentPage))
                    .ToList();

            if (visibleEdges.Count == 0)
            {
                HideLegendPopup();
                return;
            }

            var hovered = _hoveredWidget;
            
            if (Mode == vMixController.Classes.Constants.LINKS_ALL)
                hovered = null;

            if (hovered != null && !widgets.Contains(hovered))
                hovered = null;

            if (hovered != null)
            {
                var focusedEdges = visibleEdges
                    .Where(e => e.Source == hovered || e.Target == hovered)
                    .ToList();
                if (focusedEdges.Count > 0 || Mode == vMixController.Classes.Constants.LINKS_HOVER)
                    visibleEdges = focusedEdges;
            }

            var cycleEdges = BuildCycleEdgeSet(edges);

            var pageFlow = BuildCrossPageFlow(visibleEdges);
            var legendEntries = BuildLegendEntries(pageFlow.Keys);
            var legendAnchors = RenderLegend(legendEntries, pageFlow);

            var lineGeometry = new StreamGeometry();
            var arrowGeometry = new StreamGeometry();
            var deviceLineGeometry = new StreamGeometry();
            var deviceArrowGeometry = new StreamGeometry();
            var cycleLineGeometry = new StreamGeometry();
            var cycleArrowGeometry = new StreamGeometry();

            using (var context = lineGeometry.Open())
            using (var arrowContext = arrowGeometry.Open())
            using (var deviceContext = deviceLineGeometry.Open())
            using (var deviceArrowContext = deviceArrowGeometry.Open())
            using (var cycleContext = cycleLineGeometry.Open())
            using (var cycleArrowContext = cycleArrowGeometry.Open())
            {
                foreach (var edge in visibleEdges)
                {
                    var source = edge.Source;
                    var target = edge.Target;
                    if (source == null || target == null)
                        continue;
                    var isCycle = cycleEdges.Contains(GetEdgeKey(source, target));
                    var crossPage = source.Page != target.Page;

                    if (crossPage)
                    {
                        var isOutgoing = source.Page == CurrentPage;
                        var otherPage = isOutgoing ? target.Page : source.Page;
                        if (!legendAnchors.TryGetValue(otherPage, out var pageAnchor))
                            continue;

                        var start = isOutgoing
                            ? GetConnectionPoint(source, pageAnchor)
                            : pageAnchor;
                        var end = isOutgoing
                            ? pageAnchor
                            : GetConnectionPoint(target, pageAnchor);

                        var brush = isCycle
                            ? (Brush)CrossPageCycleBrush
                            : (edge.Kind == ScriptLoopAnalyzer.LinkEdgeKind.DeviceTrigger ? DeviceLinkPen.Brush : LinkPen.Brush);
                        var side = GetCrossPageSide(otherPage, isOutgoing);

                        DrawDynamicBezier(start, end, side, brush);
                        continue;
                    }

                    var lineTargetContext = isCycle
                        ? cycleContext
                        : (edge.Kind == ScriptLoopAnalyzer.LinkEdgeKind.DeviceTrigger ? deviceContext : context);
                    var arrowTargetContext = isCycle
                        ? cycleArrowContext
                        : (edge.Kind == ScriptLoopAnalyzer.LinkEdgeKind.DeviceTrigger ? deviceArrowContext : arrowContext);

                    var p1 = GetConnectionPoint(source, GetCenter(target));
                    var p2 = GetConnectionPoint(target, GetCenter(source));

                    if (source == target)
                    {
                        var bounds = GetWidgetBounds(source);
                        const double loopRadius = 6.0;
                        const double loopGap = 4.0;
                        var loopCenter = new Point(
                            bounds.Left + (bounds.Width * 0.5),
                            bounds.Top - loopGap - loopRadius);

                        lineTargetContext.BeginFigure(new Point(loopCenter.X + loopRadius, loopCenter.Y), false, true);
                        lineTargetContext.ArcTo(new Point(loopCenter.X - loopRadius, loopCenter.Y), new Size(loopRadius, loopRadius), 0, false, SweepDirection.Clockwise, true, false);
                        lineTargetContext.ArcTo(new Point(loopCenter.X + loopRadius, loopCenter.Y), new Size(loopRadius, loopRadius), 0, false, SweepDirection.Clockwise, true, false);
                        AppendArrow(arrowTargetContext, new Point(loopCenter.X, loopCenter.Y - loopRadius), new Vector(1, 0));
                        continue;
                    }

                    lineTargetContext.BeginFigure(p1, false, false);
                    Vector endDirection;
                    AppendNodeStyleCurve(lineTargetContext, source, target, p1, p2, widgetOrder, out endDirection);
                    AppendArrow(arrowTargetContext, p2, endDirection);
                }
            }

            lineGeometry.Freeze();
            arrowGeometry.Freeze();
            deviceLineGeometry.Freeze();
            deviceArrowGeometry.Freeze();
            cycleLineGeometry.Freeze();
            cycleArrowGeometry.Freeze();
            PART_Path.Data = lineGeometry;
            PART_Path.StrokeThickness = LinkPen.Thickness;
            PART_Path.Stroke = LinkPen.Brush;
            PART_Path.StrokeDashArray = LinkDashArray;
            PART_Arrows.Data = arrowGeometry;
            PART_Arrows.Fill = LinkPen.Brush;
            PART_Arrows.Stroke = LinkPen.Brush;

            PART_DevicePath.Data = deviceLineGeometry;
            PART_DevicePath.StrokeThickness = DeviceLinkPen.Thickness;
            PART_DevicePath.Stroke = DeviceLinkPen.Brush;
            PART_DevicePath.StrokeDashArray = LinkDashArray;
            PART_DeviceArrows.Data = deviceArrowGeometry;
            PART_DeviceArrows.Fill = DeviceLinkPen.Brush;
            PART_DeviceArrows.Stroke = DeviceLinkPen.Brush;

            if (!cycleLineGeometry.Bounds.IsEmpty)
            {
                PART_DynamicLayer.Children.Add(new Path
                {
                    Data = cycleLineGeometry,
                    Stroke = CrossPageCycleBrush,
                    StrokeThickness = 1.6,
                    StrokeDashArray = LinkDashArray,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round,
                    Fill = Brushes.Transparent,
                    IsHitTestVisible = false
                });
            }

            if (!cycleArrowGeometry.Bounds.IsEmpty)
            {
                PART_DynamicLayer.Children.Add(new Path
                {
                    Data = cycleArrowGeometry,
                    Fill = CrossPageCycleBrush,
                    Stroke = CrossPageCycleBrush,
                    StrokeThickness = 1.0,
                    IsHitTestVisible = false
                });
            }
        }

        private Dictionary<int, (bool Outgoing, bool Incoming)> BuildCrossPageFlow(IEnumerable<ScriptLoopAnalyzer.LinkEdge> visibleEdges)
        {
            var result = new Dictionary<int, (bool Outgoing, bool Incoming)>();
            foreach (var edge in visibleEdges)
            {
                if (edge.Source == null || edge.Target == null || edge.Source.Page == edge.Target.Page)
                    continue;

                var isOutgoing = edge.Source.Page == CurrentPage;
                var isIncoming = edge.Target.Page == CurrentPage;
                var otherPage = isOutgoing ? edge.Target.Page : edge.Source.Page;

                if (!result.TryGetValue(otherPage, out var flow))
                    flow = (false, false);

                flow.Outgoing = flow.Outgoing || isOutgoing;
                flow.Incoming = flow.Incoming || isIncoming;
                result[otherPage] = flow;
            }
            return result;
        }

        private List<(int Page, string Name, SolidColorBrush Brush)> BuildLegendEntries(IEnumerable<int> pages)
        {
            return pages
                .Distinct()
                .OrderBy(x => x)
                .Select(p => (p, ResolvePageName(p), GetPageBrush(p)))
                .ToList();
        }

        private Dictionary<int, Point> RenderLegend(
            List<(int Page, string Name, SolidColorBrush Brush)> entries,
            IReadOnlyDictionary<int, (bool Outgoing, bool Incoming)> pageFlow)
        {
            var anchors = new Dictionary<int, Point>();
            var host = PopupHost ?? Window.GetWindow(this) as FrameworkElement;
            if (entries.Count == 0 || host == null)
            {
                HideLegendPopup();
                return anchors;
            }

            const double rowHeight = 18;
            const double legendWidth = 250;
            const double marginRight = 16;
            const double marginTop = 12;
            const double padding = 8;

            var legendHeight = (entries.Count * rowHeight) + (padding * 2);
            var x = Math.Max(0, host.ActualWidth - legendWidth - marginRight);
            var y = marginTop;

            var panel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(padding)
            };

            var border = new Border
            {
                Width = legendWidth,
                Height = legendHeight,
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(Color.FromArgb(120, 20, 24, 30)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(120, 90, 100, 110)),
                BorderThickness = new Thickness(1),
                Child = panel,
                IsHitTestVisible = false
            };

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var rowY = y + padding + (i * rowHeight);
                var markerCenterY = rowY + 8.0;

                var row = new DockPanel
                {
                    LastChildFill = true,
                    Height = rowHeight
                };

                var marker = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = entry.Brush,
                    Stroke = entry.Brush,
                    StrokeThickness = 1,
                    Margin = new Thickness(0, 3, 6, 0),
                    VerticalAlignment = VerticalAlignment.Top
                };
                DockPanel.SetDock(marker, Dock.Left);
                row.Children.Add(marker);

                var flow = pageFlow.TryGetValue(entry.Page, out var f) ? f : (false, false);
                var dir = flow.Item1 && flow.Item2 ? "\u2194" : flow.Item1 ? "\u2192" : flow.Item2 ? "\u2190" : "\u00B7";

                var text = new TextBlock
                {
                    Foreground = new SolidColorBrush(Color.FromArgb(210, 235, 240, 246)),
                    FontSize = 11,
                    Text = $"{dir} {entry.Name}"
                };
                row.Children.Add(text);
                panel.Children.Add(row);

                var anchorWindowPoint = new Point(x + padding, markerCenterY);
                anchors[entry.Page] = ToOverlayPoint(host, anchorWindowPoint);
            }

            _legendPopup.PlacementTarget = host;
            _legendPopup.HorizontalOffset = x;
            _legendPopup.VerticalOffset = y;
            _legendPopup.Child = border;
            _legendPopup.IsOpen = true;
            UpdateLegendPopupPresentation();

            return anchors;
        }

        private Point ToOverlayPoint(Visual source, Point pointInSource)
        {
            var screen = source.PointToScreen(pointInSource);
            return PointFromScreen(screen);
        }

        private void DrawDynamicBezier(Point start, Point end, double side, Brush brush)
        {
            var geometry = new StreamGeometry();
            var arrow = new StreamGeometry();
            using (var ctx = geometry.Open())
            using (var arrowCtx = arrow.Open())
            {
                ctx.BeginFigure(start, false, false);
                Vector endDirection;
                AppendSimpleBezier(ctx, start, end, side, out endDirection);
                AppendArrow(arrowCtx, end, endDirection);
            }
            geometry.Freeze();
            arrow.Freeze();

            var path = new Path
            {
                Data = geometry,
                Stroke = brush,
                StrokeThickness = 1.6,
                StrokeDashArray = LinkDashArray,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Fill = Brushes.Transparent,
                IsHitTestVisible = false
            };
            PART_DynamicLayer.Children.Add(path);

            var arrowPath = new Path
            {
                Data = arrow,
                Fill = brush,
                Stroke = brush,
                StrokeThickness = 1.0,
                IsHitTestVisible = false
            };
            PART_DynamicLayer.Children.Add(arrowPath);
        }

        private static void AppendSimpleBezier(StreamGeometryContext context, Point start, Point end, double side, out Vector endDirection)
        {
            var dx = end.X - start.X;
            var dy = end.Y - start.Y;
            var distance = Math.Sqrt((dx * dx) + (dy * dy));
            if (distance < 0.001)
            {
                context.LineTo(end, true, false);
                endDirection = new Vector(1, 0);
                return;
            }

            var dirX = dx / distance;
            var dirY = dy / distance;
            var perpX = -dirY;
            var perpY = dirX;
            var bend = 8.0 * side;

            var c1 = new Point(
                start.X + (dx * 0.33) + (perpX * bend),
                start.Y + (dy * 0.33) + (perpY * bend));

            var c2 = new Point(
                start.X + (dx * 0.66) + (perpX * bend),
                start.Y + (dy * 0.66) + (perpY * bend));

            context.BezierTo(c1, c2, end, true, false);
            endDirection = new Vector(end.X - c2.X, end.Y - c2.Y);
            if (endDirection.LengthSquared < 0.0001)
                endDirection = new Vector(dirX, dirY);
            else
                endDirection.Normalize();
        }

        private static double GetCrossPageSide(int page, bool outgoing)
        {
            var baseSide = (page % 2 == 0) ? 1.0 : -1.0;
            return outgoing ? baseSide : -baseSide;
        }

        private static SolidColorBrush GetPageBrush(int page)
        {
            try
            {
                var colors = vMixWidgetSettingsViewModel.Colors;
                if (colors != null && colors.Count > 0)
                {
                    var filtered = colors
                        .Select(x => x.A)
                        .Where(clr => (0.2126f * (clr.R / 255f) + 0.7152f * (clr.G / 255f) + 0.0722f * (clr.B / 255f)) >= 0.33f)
                        .ToList();

                    var palette = filtered.Count > 0 ? filtered : colors.Select(x => x.A).ToList();
                    var color = palette[Math.Abs(page) % palette.Count];
                    return CreateBrush(200, color.R, color.G, color.B);
                }
            }
            catch
            {
                // ignore and fallback
            }

            return CreateBrush(200, 91, 142, 201);
        }

        private static string ResolvePageName(int pageIndex)
        {
            try
            {
                var mainViewModel = vMixController.Classes.AppServices.IsRegistered<vMixController.ViewModel.MainViewModel>()
                    ? vMixController.Classes.AppServices.GetRequiredService<vMixController.ViewModel.MainViewModel>()
                    : null;
                var pages = mainViewModel?.WindowSettings?.Pages;
                if (pages != null && pageIndex >= 0 && pageIndex < pages.Count && !string.IsNullOrWhiteSpace(pages[pageIndex]))
                    return pages[pageIndex];
            }
            catch
            {
                // ignore and fallback
            }

            return $"Page {pageIndex + 1}";
        }

        private HashSet<string> BuildCycleEdgeSet(List<ScriptLoopAnalyzer.LinkEdge> edges)
        {
            var result = new HashSet<string>(StringComparer.Ordinal);
            if (edges == null || edges.Count == 0)
                return result;

            var nodes = edges
                .SelectMany(e => new[] { e.Source, e.Target })
                .Where(w => w != null)
                .Distinct()
                .ToList();
            if (nodes.Count == 0)
                return result;

            var indexOf = nodes
                .Select((w, i) => new { w, i })
                .ToDictionary(x => x.w, x => x.i);

            var adj = new List<int>[nodes.Count];
            for (var i = 0; i < adj.Length; i++)
                adj[i] = new List<int>();

            foreach (var edge in edges)
            {
                if (edge.Source == null || edge.Target == null)
                    continue;

                var a = indexOf[edge.Source];
                var b = indexOf[edge.Target];
                if (!adj[a].Contains(b))
                    adj[a].Add(b);
            }

            var index = 0;
            var stack = new Stack<int>();
            var onStack = new bool[nodes.Count];
            var idx = Enumerable.Repeat(-1, nodes.Count).ToArray();
            var low = new int[nodes.Count];
            var compId = Enumerable.Repeat(-1, nodes.Count).ToArray();
            var compCount = 0;

            void StrongConnect(int v)
            {
                idx[v] = index;
                low[v] = index;
                index++;
                stack.Push(v);
                onStack[v] = true;

                foreach (var w in adj[v])
                {
                    if (idx[w] == -1)
                    {
                        StrongConnect(w);
                        low[v] = Math.Min(low[v], low[w]);
                    }
                    else if (onStack[w])
                    {
                        low[v] = Math.Min(low[v], idx[w]);
                    }
                }

                if (low[v] == idx[v])
                {
                    while (true)
                    {
                        var w = stack.Pop();
                        onStack[w] = false;
                        compId[w] = compCount;
                        if (w == v)
                            break;
                    }
                    compCount++;
                }
            }

            for (var v = 0; v < nodes.Count; v++)
            {
                if (idx[v] == -1)
                    StrongConnect(v);
            }

            var compSizes = new int[compCount];
            for (var i = 0; i < compId.Length; i++)
                compSizes[compId[i]]++;

            foreach (var edge in edges)
            {
                if (edge.Source == null || edge.Target == null)
                    continue;

                var a = indexOf[edge.Source];
                var b = indexOf[edge.Target];
                var sameComp = compId[a] == compId[b];
                var isCycle = edge.Source == edge.Target || (sameComp && compSizes[compId[a]] > 1);
                if (isCycle)
                    result.Add(GetEdgeKey(edge.Source, edge.Target));
            }

            return result;
        }

        private static string GetEdgeKey(vMixControl source, vMixControl target)
        {
            var sourceKey = source?.WidgetId.ToString() ?? "null";
            var targetKey = target?.WidgetId.ToString() ?? "null";
            return $"{sourceKey}|{targetKey}";
        }

        private Point GetCenter(vMixControl control)
        {
            var bounds = GetWidgetBounds(control);
            return new Point(bounds.Left + (bounds.Width * 0.5), bounds.Top + (bounds.Height * 0.5));
        }

        private static void AppendNodeStyleCurve(
            StreamGeometryContext context,
            vMixControl source,
            vMixControl target,
            Point start,
            Point end,
            IReadOnlyDictionary<vMixControl, int> widgetOrder,
            out Vector endDirection)
        {
            var dx = end.X - start.X;
            var dy = end.Y - start.Y;
            var distance = Math.Sqrt((dx * dx) + (dy * dy));
            if (distance < 0.001)
            {
                context.LineTo(end, true, false);
                endDirection = new Vector(1, 0);
                return;
            }

            var dirX = dx / distance;
            var dirY = dy / distance;
            var perpX = -dirY;
            var perpY = dirX;

            var sourceIndex = widgetOrder.TryGetValue(source, out var s) ? s : 0;
            var targetIndex = widgetOrder.TryGetValue(target, out var t) ? t : 0;

            // Keep one stable side for an unordered widget pair.
            // This makes A->B and B->A bend to opposite sides instead of overlapping.
            var minIndex = Math.Min(sourceIndex, targetIndex);
            var maxIndex = Math.Max(sourceIndex, targetIndex);
            var side = ((minIndex + maxIndex) % 2 == 0) ? 1.0 : -1.0;
            var bend = 8.0 * side;
            var c1 = new Point(
                start.X + (dx * 0.33) + (perpX * bend),
                start.Y + (dy * 0.33) + (perpY * bend));

            var c2 = new Point(
                start.X + (dx * 0.66) + (perpX * bend),
                start.Y + (dy * 0.66) + (perpY * bend));

            context.BezierTo(c1, c2, end, true, false);

            endDirection = new Vector(end.X - c2.X, end.Y - c2.Y);
            if (endDirection.LengthSquared < 0.0001)
                endDirection = new Vector(dirX, dirY);
            else
                endDirection.Normalize();
        }

        private Point GetConnectionPoint(vMixControl control, Point toward)
        {
            var bounds = GetWidgetBounds(control);
            var center = new Point(bounds.Left + (bounds.Width * 0.5), bounds.Top + (bounds.Height * 0.5));
            var halfWidth = Math.Max(1.0, bounds.Width * 0.5);
            var halfHeight = Math.Max(1.0, bounds.Height * 0.5);

            var vx = toward.X - center.X;
            var vy = toward.Y - center.Y;

            if (Math.Abs(vx) < 0.001 && Math.Abs(vy) < 0.001)
                return center;

            var tx = Math.Abs(vx) / halfWidth;
            var ty = Math.Abs(vy) / halfHeight;
            var t = Math.Max(tx, ty);
            if (t < 0.001)
                return center;

            return new Point(
                center.X + (vx / t),
                center.Y + (vy / t));
        }

        private Rect GetWidgetBounds(vMixControl control)
        {
            var width = SafeDimension(control?.Width);
            var height = GetFallbackHeight(control);

            if (control != null && _actualWidgetSizes.TryGetValue(control, out var actual))
            {
                width = SafeDimension(actual.Width);
                height = SafeDimension(actual.Height);
            }

            var left = control?.Left ?? 0.0;
            var top = control?.Top ?? 0.0;
            return new Rect(left, top, width, height);
        }

        private static double GetFallbackHeight(vMixControl control)
        {
            var contentHeight = SafeDimension(control?.Height);
            var scale = control != null ? Math.Max(0.01, control.Scale) : 1.0;
            var captionHeight = control != null && control.IsCaptionOn ? SafeDimension(control.CaptionHeight) : 0.0;
            return Math.Max(1.0, (contentHeight * scale) + captionHeight);
        }

        private static double SafeDimension(double? value)
        {
            if (!value.HasValue)
                return 1.0;

            var v = value.Value;
            if (double.IsNaN(v) || double.IsInfinity(v))
                return 1.0;

            return Math.Max(1.0, v);
        }

        private void RefreshActualSizeCache()
        {
            _actualWidgetSizes.Clear();

            var root = Window.GetWindow(this);
            if (root == null)
                return;

            var queue = new Queue<DependencyObject>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (node is vMixControlContainerDummy container && container.Control != null)
                {
                    var w = container.ActualWidth;
                    var h = container.ActualHeight;
                    if (w > 0.5 && h > 0.5)
                        _actualWidgetSizes[container.Control] = new Size(w, h);
                }

                var count = VisualTreeHelper.GetChildrenCount(node);
                for (var i = 0; i < count; i++)
                    queue.Enqueue(VisualTreeHelper.GetChild(node, i));
            }
        }

        private static void AppendArrow(StreamGeometryContext context, Point tip, Vector direction)
        {
            if (direction.LengthSquared < 0.0001)
                return;

            direction.Normalize();
            var perp = new Vector(-direction.Y, direction.X);

            const double arrowLength = 10;
            const double arrowWidth = 7;

            var baseCenter = new Point(
                tip.X - (direction.X * arrowLength),
                tip.Y - (direction.Y * arrowLength));

            var left = new Point(
                baseCenter.X + (perp.X * (arrowWidth * 0.5)),
                baseCenter.Y + (perp.Y * (arrowWidth * 0.5)));

            var right = new Point(
                baseCenter.X - (perp.X * (arrowWidth * 0.5)),
                baseCenter.Y - (perp.Y * (arrowWidth * 0.5)));

            context.BeginFigure(tip, true, true);
            context.LineTo(left, true, false);
            context.LineTo(right, true, false);
        }

        private void DisposeSubscriptions()
        {
            HideLegendPopup();
            Messenger.UnregisterAll(this);
            DetachOwnerWindowHandlers();
            if (ScrollHost != null)
                ScrollHost.ScrollChanged -= ScrollHostScrollChanged;

            if (TransformSource != null)
                TransformSource.LayoutUpdated -= TransformSourceLayoutUpdated;

            UnsubscribeCollection(Items);

            foreach (var widget in _subscribedWidgets.ToList())
            {
                widget.PropertyChanged -= WidgetPropertyChanged;
            }

            _subscribedWidgets.Clear();
        }

        private void AttachOwnerWindowHandlers()
        {
            var nextWindow = Window.GetWindow(this);
            if (ReferenceEquals(_ownerWindow, nextWindow))
                return;

            DetachOwnerWindowHandlers();
            _ownerWindow = nextWindow;
            if (_ownerWindow == null)
                return;

            _ownerWindow.Activated += OwnerWindowActivationChanged;
            _ownerWindow.Deactivated += OwnerWindowActivationChanged;
            _ownerWindow.StateChanged += OwnerWindowActivationChanged;
            _ownerWindow.LocationChanged += OwnerWindowLayoutChanged;
            _ownerWindow.SizeChanged += OwnerWindowLayoutChanged;
        }

        private void DetachOwnerWindowHandlers()
        {
            if (_ownerWindow == null)
                return;

            _ownerWindow.Activated -= OwnerWindowActivationChanged;
            _ownerWindow.Deactivated -= OwnerWindowActivationChanged;
            _ownerWindow.StateChanged -= OwnerWindowActivationChanged;
            _ownerWindow.LocationChanged -= OwnerWindowLayoutChanged;
            _ownerWindow.SizeChanged -= OwnerWindowLayoutChanged;
            _ownerWindow = null;
        }

        private void OwnerWindowLayoutChanged(object sender, EventArgs e)
        {
            ScheduleRender();
        }

        private void OwnerWindowLayoutChanged(object sender, SizeChangedEventArgs e)
        {
            ScheduleRender();
        }

        private void OwnerWindowActivationChanged(object sender, EventArgs e)
        {
            UpdateLegendPopupPresentation();
        }

        private void OnHoveredWidgetChanged(HoveredWidgetMessage msg)
        {
            var next = msg?.Widget;
            if (ReferenceEquals(_hoveredWidget, next))
                return;

            _hoveredWidget = next;
            ScheduleRender();
        }

        private void HideLegendPopup()
        {
            _legendPopup.IsOpen = false;
            _legendPopup.Child = null;
        }

        private void UpdateLegendPopupPresentation()
        {
            if (!(_legendPopup.Child is UIElement child))
                return;

            AttachOwnerWindowHandlers();
            var ownerActive = _ownerWindow == null || (_ownerWindow.IsActive && _ownerWindow.WindowState != WindowState.Minimized);
            var canShow = IsVisible && IsEnabled && Visibility == Visibility.Visible && Opacity > 0.001 && ownerActive;
            _legendPopup.IsOpen = canShow;
            child.Opacity = Math.Max(0.0, Math.Min(1.0, Opacity));
        }

        private static Pen CreatePen()
        {
            return CreatePen(180, 74, 92, 112);
        }

        private static Pen CreatePen(byte a, byte r, byte g, byte b)
        {
            var brush = CreateBrush(a, r, g, b);
            var pen = new Pen(brush, 1.4);
            pen.Freeze();
            return pen;
        }

        private static SolidColorBrush CreateBrush(byte a, byte r, byte g, byte b)
        {
            var brush = new SolidColorBrush(Color.FromArgb(a, r, g, b));
            brush.Freeze();
            return brush;
        }

        private static DoubleCollection CreateDashArray()
        {
            var dash = new DoubleCollection { 6, 4 };
            dash.Freeze();
            return dash;
        }
    }
}


