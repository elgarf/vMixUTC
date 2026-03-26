// ��������� �������� ������ �� System.Net.Http
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using vMixControllerDataProvider;
using vMixControllerSkin;

namespace XmlDataProviderNs
{
    public partial class XmlDataProvider : DependencyObject, IvMixDataProviderTextInput, INotifyPropertyChanged
    {
        #region Private Fields

        // 1. ���������� HttpClient. ���� ����������� ��������� ������������� ��� �����������������.
        private static readonly HttpClient _httpClient = new HttpClient();
        // 2. ConcurrentDictionary ��� ����������������� ���� ��� ������ ����������.
        private static readonly ConcurrentDictionary<string, CacheEntry> _cache = new ConcurrentDictionary<string, CacheEntry>();

        // 3. SemaphoreSlim ��� �������������� ������������� �������� ������ � ���� �� �������.
        private readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);

        private List<string> _data = new List<string>();
        private string[] _valuesCache = Array.Empty<string>();
        private int _loadScheduled;
        private readonly DispatcherTimer _refreshTimer;
        private int _currentTimerPeriodMs;

        #endregion

        #region Properties & Commands

        public System.Windows.UIElement CustomUI { get; }
        public bool IsProvidingCustomProperties => true;
        public int Period { get; set; } = 1000; // �� ��������� 1 �������



        public string[] Values
        {
            get
            {
                ScheduleLoadIfNeeded();
                return _valuesCache;
            }
        }

        public List<string> Data
        {
            get => _data;
            private set // ������ ������ ���������, ����� ������ �������� ������ ������ ������
            {
                var newData = value ?? new List<string>();
                if (!_data.SequenceEqual(newData))
                {
                    _data = newData;
                    _valuesCache = _data.ToArray();
                    OnPropertyChanged(nameof(Values));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Data)));
                }
            }
        }

        public ICommand PreviewKeyUp { get; set; }
        public ICommand GotFocus { get; set; }
        public ICommand LostFocus { get; set; }

        [RelayCommand]
        private void HandlePreviewKeyUp(KeyEventArgs p)
        {
            if (p == null)
            {
                return;
            }

            if (!(p.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) && p.Key == Key.Return))
            {
                if (PreviewKeyUp is ICommand command && command.CanExecute(p))
                {
                    command.Execute(p);
                }
            }
        }

        [RelayCommand]
        private void HandlePreviewKeyDown(KeyEventArgs p)
        {
            if (p == null)
            {
                return;
            }

            if (p.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) && p.Key == Key.Return)
            {
                p.Handled = true;
                if (p.Source is TextBox sender)
                {
                    int lastLocation = sender.SelectionStart;
                    sender.Text = sender.Text.Insert(lastLocation, Environment.NewLine);
                    sender.SelectionStart = lastLocation + Environment.NewLine.Length;
                }
            }
            else if (p.Key == Key.Return)
            {
                p.Handled = true;
            }
        }

        [RelayCommand]
        private void HandleGotFocus(RoutedEventArgs p)
        {
            if (p == null)
            {
                return;
            }

            if (GotFocus != null && GotFocus.CanExecute(p))
            {
                GotFocus.Execute(p);
            }
        }

        [RelayCommand]
        private void HandleLostFocus(RoutedEventArgs p)
        {
            if (p == null)
            {
                return;
            }

            if (LostFocus != null && LostFocus.CanExecute(p))
            {
                LostFocus.Execute(p);
            }
        }

        [RelayCommand]
        private void ShowRows()
        {
            new RowsViewer().Bind(this, nameof(Data));
        }

        [RelayCommand]
        private void Reload()
        {
            _ = ForceReloadAsync();
        }
        #endregion

        #region Async Data Loading

        private async Task LoadDataIfNeededAsync()
        {
            try
            {
                var url = Url; // �������� �������� �� DependencyProperty
                if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(XPath))
                {
                    Data = new List<string>();
                    SetError(string.Empty);
                    return;
                }

                // �������� ����
                if (_cache.TryGetValue(url, out var cacheEntry) && (DateTime.UtcNow - cacheEntry.LastUpdated).TotalMilliseconds < Period)
                {
                    // ���� ������ � ���� ���������, ������ ������� ������� ��������� �� ���
                    UpdateDataFromCache(cacheEntry.Document);
                    return;
                }

                // ������ � �������, ����� ������ ���� ����� ��� ��������� ������
                await _asyncLock.WaitAsync();
                try
                {
                    // ��������� �������� ���� ����� ����� � �������.
                    // ��������, ������ ����� ��� ������� ������, ���� �� �����.
                    if (_cache.TryGetValue(url, out cacheEntry) && (DateTime.UtcNow - cacheEntry.LastUpdated).TotalMilliseconds < Period)
                    {
                        UpdateDataFromCache(cacheEntry.Document);
                        return;
                    }

                    // �������� � ��������� ������
                    var doc = await FetchXmlAsync(url);
                    if (doc != null)
                    {
                        _cache[url] = new CacheEntry { Document = doc, LastUpdated = DateTime.UtcNow };
                        UpdateDataFromCache(doc);
                    }
                }
                finally
                {
                    _asyncLock.Release();
                }
            }
            finally
            {
                Interlocked.Exchange(ref _loadScheduled, 0);
            }
        }

        private async Task<XDocument> FetchXmlAsync(string url)
        {
            SetError(string.Empty);
            try
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                    return null;

                if (uri.Scheme == Uri.UriSchemeFile)
                {
                    using (var stream = File.Open(uri.LocalPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        return XDocument.Load(stream, LoadOptions.None);
                }
                else
                    using (var response = await _httpClient.GetAsync(url))
                    {
                        response.EnsureSuccessStatusCode();
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            // 5. ����������� �������� � XDocument
                            return XDocument.Load(stream, LoadOptions.None);
                        }
                    }
            }
            catch (Exception ex)
            {
                SetError($"Error retrieving XML data from {url}: {ex.Message}");
                return null;
            }
        }

        private void UpdateDataFromCache(XDocument doc)
        {
            if (doc == null) return;
            try
            {
                var nsManager = new XmlNamespaceManager(new NameTable());
                var namespaces = NameSpaces; // �������� �������� �� DependencyProperty
                if (!string.IsNullOrEmpty(namespaces))
                {
                    foreach (var item in namespaces.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var parts = item.Trim().Split(new[] { ' ' }, 2);
                        if (parts.Length == 2)
                        {
                            nsManager.AddNamespace(parts[0], parts[1]);
                        }
                    }
                }

                // 6. ���������� XPathSelectElements �� LINQ to XML
                var nodes = (IEnumerable<object>)doc.XPathEvaluate(XPath, nsManager);
                var groupBy = GroupBy;
                var maxItems = 100 * (groupBy <= 0 ? 1 : groupBy);
                var extracted = new List<string>(maxItems);

                foreach (var node in nodes.OfType<XObject>())
                {
                    if (extracted.Count >= maxItems)
                    {
                        break;
                    }

                    switch (node.NodeType)
                    {
                        case XmlNodeType.Element:
                            extracted.Add(((XElement)node).Value);
                            break;
                        case XmlNodeType.Text:
                            extracted.Add(((XText)node).Value);
                            break;
                        case XmlNodeType.Attribute:
                            extracted.Add(((XAttribute)node).Value);
                            break;
                        default:
                            extracted.Add(string.Empty);
                            break;
                    }
                }

                List<string> newData;
                if (groupBy > 1)
                {
                    newData = new List<string>((extracted.Count + groupBy - 1) / groupBy);
                    var builder = new StringBuilder();
                    for (int i = 0; i < extracted.Count; i++)
                    {
                        if (i > 0 && i % groupBy == 0)
                        {
                            newData.Add(builder.ToString().TrimEnd('|'));
                            builder.Clear();
                        }

                        builder.Append(extracted[i]).Append('|');
                    }

                    if (builder.Length > 0)
                    {
                        newData.Add(builder.ToString().TrimEnd('|'));
                    }
                }
                else
                {
                    newData = extracted;
                }

                RunOnUi(() =>
                {
                    SetError(string.Empty);
                    Data = newData;
                });
            }
            catch (Exception ex)
            {
                RunOnUi(() =>
                {
                    SetError($"Error updating data from XML: {ex.Message}");
                    Data = new List<string>();
                });
            }
        }


        #endregion

        #region Dependency Properties

        public string Url
        {
            get => (string)GetValue(UrlProperty);
            set => SetValue(UrlProperty, value);
        }
        public static readonly DependencyProperty UrlProperty =
            DependencyProperty.Register(nameof(Url), typeof(string), typeof(XmlDataProvider), new PropertyMetadata(""));

        public string XPath
        {
            get => (string)GetValue(XPathProperty);
            set => SetValue(XPathProperty, value);
        }
        public static readonly DependencyProperty XPathProperty =
            DependencyProperty.Register(nameof(XPath), typeof(string), typeof(XmlDataProvider), new PropertyMetadata(""));

        public string NameSpaces
        {
            get => (string)GetValue(NameSpacesProperty);
            set => SetValue(NameSpacesProperty, value);
        }
        public static readonly DependencyProperty NameSpacesProperty =
            DependencyProperty.Register(nameof(NameSpaces), typeof(string), typeof(XmlDataProvider), new PropertyMetadata(""));

        public string Error
        {
            get => (string)GetValue(ErrorProperty);
            set => SetValue(ErrorProperty, value);
        }
        public static readonly DependencyProperty ErrorProperty =
            DependencyProperty.Register(nameof(Error), typeof(string), typeof(XmlDataProvider), new PropertyMetadata(""));

        public int GroupBy
        {
            get => (int)GetValue(GroupByProperty);
            set => SetValue(GroupByProperty, value);
        }
        public static readonly DependencyProperty GroupByProperty =
            DependencyProperty.Register(nameof(GroupBy), typeof(int), typeof(XmlDataProvider), new PropertyMetadata(1));

        #endregion

        #region Interface Implementations & Constructor

        public List<object> GetProperties()
        {
            return new List<object> { Url, XPath, NameSpaces, GroupBy };
        }

        public void SetProperties(List<object> props)
        {
            if (props == null) return;

            Url = props.ElementAtOrDefault(0) as string;
            XPath = props.ElementAtOrDefault(1) as string;
            NameSpaces = props.ElementAtOrDefault(2) as string;
            GroupBy = (int)(props.ElementAtOrDefault(3) ?? 1);
        }

        public void ShowProperties(Window owner)
        {
            var properties = new PropertiesWindow { Owner = owner, DataContext = this };
            var previous = GetProperties();
            if (properties.ShowDialog() != true)
            {
                SetProperties(previous);
            }
        }

        public XmlDataProvider()
        {
            try
            {
                CustomUI = new OnWidgetUI { DataContext = this };
            }
            catch (Exception e)
            {
                CustomUI = new TextBox { Text = e.ToString(), AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 256, FontWeight = FontWeights.Normal, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            }

            _currentTimerPeriodMs = Math.Max(250, Period);
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(_currentTimerPeriodMs)
            };
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();
            ScheduleLoadIfNeeded();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // ��������������� ����� ��� ����
        private class CacheEntry
        {
            public XDocument Document { get; set; }
            public DateTime LastUpdated { get; set; }
        }

        #endregion

        // ���������� ���� � ������, ������� ������ �� �����
        // private static int _maxid = 0;
        // private readonly int _id = _maxid++;
        // private bool _retrievingData = false;
        // private string _url, _xpath, _namespaces;
        // private int _groupBy;
        // PropertyChangedCallback ������ �� �����, �.�. �� ������ �������� �������� �� DP.

        private void ScheduleLoadIfNeeded()
        {
            if (Interlocked.CompareExchange(ref _loadScheduled, 1, 0) != 0)
            {
                return;
            }

            _ = LoadDataIfNeededAsync();
        }

        private async Task ForceReloadAsync()
        {
            if (!string.IsNullOrWhiteSpace(Url))
            {
                _cache.TryRemove(Url, out _);
            }

            await LoadDataIfNeededAsync();
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetError(string error)
        {
            if (Application.Current?.Dispatcher?.CheckAccess() ?? true)
            {
                Error = error;
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => Error = error));
            }
        }

        private static void RunOnUi(Action action)
        {
            if (action == null) return;

            if (Application.Current?.Dispatcher?.CheckAccess() ?? true)
            {
                action();
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(action);
            }
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            var targetPeriod = Math.Max(250, Period);
            if (targetPeriod != _currentTimerPeriodMs)
            {
                _currentTimerPeriodMs = targetPeriod;
                _refreshTimer.Interval = TimeSpan.FromMilliseconds(_currentTimerPeriodMs);
            }

            ScheduleLoadIfNeeded();
        }
    }
}

