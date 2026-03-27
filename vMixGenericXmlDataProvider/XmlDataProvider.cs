using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using vMixControllerDataProvider;
using vMixControllerSkin;

namespace XmlDataProviderNs
{
    public partial class XmlDataProvider : PollingTextInputDataProviderBase
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        private static readonly ConcurrentDictionary<string, CacheEntry> Cache = new ConcurrentDictionary<string, CacheEntry>();

        private List<string> _data = new List<string>();

        protected override int MinPeriodMs => 250;

        public XmlDataProvider()
        {
            Period = 1000;

            try
            {
                CustomUI = new OnWidgetUI { DataContext = this };
            }
            catch (Exception e)
            {
                CustomUI = new TextBox
                {
                    Text = e.ToString(),
                    AcceptsReturn = true,
                    TextWrapping = TextWrapping.Wrap,
                    Height = 256,
                    FontWeight = FontWeights.Normal,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                };
            }

            StartPolling(runImmediately: true);
        }

        public override bool IsProvidingCustomProperties => true;

        public string Url
        {
            get => (string)GetValue(UrlProperty);
            set => SetValue(UrlProperty, value);
        }

        public static readonly DependencyProperty UrlProperty =
            DependencyProperty.Register(nameof(Url), typeof(string), typeof(XmlDataProvider),
                new PropertyMetadata(string.Empty, OnSourcePropertyChanged));

        public string XPath
        {
            get => (string)GetValue(XPathProperty);
            set => SetValue(XPathProperty, value);
        }

        public static readonly DependencyProperty XPathProperty =
            DependencyProperty.Register(nameof(XPath), typeof(string), typeof(XmlDataProvider),
                new PropertyMetadata(string.Empty, OnSourcePropertyChanged));

        public string NameSpaces
        {
            get => (string)GetValue(NameSpacesProperty);
            set => SetValue(NameSpacesProperty, value);
        }

        public static readonly DependencyProperty NameSpacesProperty =
            DependencyProperty.Register(nameof(NameSpaces), typeof(string), typeof(XmlDataProvider),
                new PropertyMetadata(string.Empty, OnSourcePropertyChanged));

        public string Error
        {
            get => (string)GetValue(ErrorProperty);
            set => SetValue(ErrorProperty, value);
        }

        public static readonly DependencyProperty ErrorProperty =
            DependencyProperty.Register(nameof(Error), typeof(string), typeof(XmlDataProvider), new PropertyMetadata(string.Empty));

        public int GroupBy
        {
            get => (int)GetValue(GroupByProperty);
            set => SetValue(GroupByProperty, value);
        }

        public static readonly DependencyProperty GroupByProperty =
            DependencyProperty.Register(nameof(GroupBy), typeof(int), typeof(XmlDataProvider),
                new PropertyMetadata(1, OnSourcePropertyChanged));

        public List<string> Data
        {
            get => _data;
            private set
            {
                _data = value ?? new List<string>();
                SetValuesCache(_data.ToArray(), skipIfEqual: false);
                RaisePropertyChanged(nameof(Data));
            }
        }

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
            if (!string.IsNullOrWhiteSpace(Url))
            {
                Cache.TryRemove(Url, out _);
            }

            ScheduleRefresh();
        }

        public override List<object> GetProperties()
        {
            return new List<object> { Url, XPath, NameSpaces, GroupBy };
        }

        public override void SetProperties(List<object> props)
        {
            if (props == null)
            {
                return;
            }

            Url = props.ElementAtOrDefault(0) as string;
            XPath = props.ElementAtOrDefault(1) as string;
            NameSpaces = props.ElementAtOrDefault(2) as string;
            GroupBy = (int)(props.ElementAtOrDefault(3) ?? 1);
        }

        public override void ShowProperties(Window owner)
        {
            var properties = new PropertiesWindow { Owner = owner, DataContext = this };
            var previous = GetProperties();
            if (properties.ShowDialog() != true)
            {
                SetProperties(previous);
            }
        }

        protected override async Task RefreshValuesAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var url = Url;
            var xPath = XPath;

            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(xPath))
            {
                Data = new List<string>();
                SetError(string.Empty);
                return;
            }

            if (Cache.TryGetValue(url, out var cacheEntry) && (DateTime.UtcNow - cacheEntry.LastUpdated).TotalMilliseconds < Period)
            {
                UpdateDataFromCache(cacheEntry.Document, xPath);
                return;
            }

            var doc = await FetchXmlAsync(url);
            if (doc != null)
            {
                Cache[url] = new CacheEntry { Document = doc, LastUpdated = DateTime.UtcNow };
                UpdateDataFromCache(doc, xPath);
            }
        }

        private async Task<XDocument> FetchXmlAsync(string url)
        {
            SetError(string.Empty);
            try
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    return null;
                }

                if (uri.Scheme == Uri.UriSchemeFile)
                {
                    using (var stream = File.Open(uri.LocalPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        return XDocument.Load(stream, LoadOptions.None);
                    }
                }

                using (var response = await HttpClient.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
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

        private void UpdateDataFromCache(XDocument doc, string xPath)
        {
            if (doc == null)
            {
                return;
            }

            try
            {
                var nsManager = new XmlNamespaceManager(new NameTable());
                var namespaces = NameSpaces;
                if (!string.IsNullOrWhiteSpace(namespaces))
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

                var nodes = (IEnumerable<object>)doc.XPathEvaluate(xPath, nsManager);
                int groupBy = GroupBy <= 0 ? 1 : GroupBy;
                int maxItems = 100 * groupBy;
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

                List<string> newData = DataProviderTextFormatter.GroupByPipe(extracted, groupBy);

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

        private void SetError(string error)
        {
            RunOnUi(() => Error = error ?? string.Empty);
        }

        private static void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is XmlDataProvider provider)
            {
                provider.ScheduleRefresh();
            }
        }

        private sealed class CacheEntry
        {
            public XDocument Document { get; set; }
            public DateTime LastUpdated { get; set; }
        }
    }
}
