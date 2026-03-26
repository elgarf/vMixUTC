using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading; // ��� CancellationTokenSource
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using vMixControllerDataProvider;
using vMixControllerSkin;
using Json.Path;
using Json.More;
using System.Net.Http;
using System.IO;

namespace JsonDataProviderNs
{
    public partial class JsonDataProvider : IvMixDataProviderTextInput, INotifyPropertyChanged, IDisposable
    {
        // ������ ��������: ���� ��������� HttpClient �� �� ����������
        private static readonly HttpClient _httpClient = new HttpClient();

        private JsonDocument _document;
        private DateTime _previousQuery;

        private List<string> _data = new List<string>();
        private string[] _valuesCache = Array.Empty<string>();
        private int _retrievingData = 0;
        private readonly object _pathLock = new object();
        private readonly object _ctsLock = new object();
        private Json.Path.JsonPath _compiledPath;
        private string _compiledPathSource = string.Empty;

        // �������� ������� ��� ������ ����������� �������
        private CancellationTokenSource _cancellationTokenSource;

        private string _url = "";
        private string _jsonPath = "";
        private string _headers = "";
        private string _error = "";
        private int _groupBy = 1;
        private int _period = 5000;
        private UIElement _ui;
        private readonly DispatcherTimer _refreshTimer;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand PreviewKeyUp { get; set; }
        public ICommand GotFocus { get; set; }
        public ICommand LostFocus { get; set; }
        public int Period
        {
            get => _period;
            set
            {
                var normalized = Math.Max(250, value);
                if (_period == normalized)
                {
                    return;
                }

                _period = normalized;
                if (_refreshTimer != null)
                {
                    _refreshTimer.Interval = TimeSpan.FromMilliseconds(_period);
                }

                _ = RetrieveDataAsync();
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

        public bool IsProvidingCustomProperties => false;

        public string[] Values
        {
            get
            {
                return _valuesCache;
            }
        }

        private static void AddHeadersFromString(System.Net.Http.Headers.HttpRequestHeaders headers, string headersString)
        {
            if (string.IsNullOrWhiteSpace(headersString))
                return;

            var lines = headersString.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var colonIndex = line.IndexOf(':');
                if (colonIndex > 0)
                {
                    var key = line.Substring(0, colonIndex).Trim();
                    var value = line.Substring(colonIndex + 1).Trim();

                    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                    {
                        headers.TryAddWithoutValidation(key, value);
                    }
                }
            }
        }

        /// <summary>
        /// ���������� �������� � ������������ JSON ������.
        /// �������� ���������� ������������� ������.
        /// </summary>
        private async Task RetrieveDataAsync()
        {
            Error = "";
            if (Interlocked.Exchange(ref _retrievingData, 1) == 1)
            {
                return;
            }

            _previousQuery = DateTime.Now;

            // ���� URL ����������, ������ �������
            if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri))
            {
                Interlocked.Exchange(ref _retrievingData, 0);
                return;
            }

            // �������� ���������� ��������, ���� ��� ����, � ������� ����� CancellationTokenSource
            CancellationTokenSource cts;
            lock (_ctsLock)
            {
                var previous = _cancellationTokenSource;
                cts = new CancellationTokenSource();
                _cancellationTokenSource = cts;
                previous?.Cancel();
                previous?.Dispose();
            }
            var token = cts.Token;

            try
            {
                JsonDocument newDocument;

                // ���� (������������ �� ������� .NET):
                // using (var stream = await _httpClient.GetStreamAsync(uri, token))

                // ����� (���������� � ���������):
                // 1. ��������� GET ������ � ������� ������
                if (uri.Scheme == Uri.UriSchemeFile)
                {
                    using (var stream = File.Open(uri.LocalPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        newDocument = await JsonDocument.ParseAsync(stream, default, token);
                }
                else
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                    {
                        AddHeadersFromString(request.Headers, Headers);
                        using (var response = await _httpClient.SendAsync(request, token))
                        {
                            response.EnsureSuccessStatusCode();
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            {
                                newDocument = await JsonDocument.ParseAsync(stream, default, token);
                            }
                        }
                    }
                }
                // ����� ��������� ��������� � ��������, ��������� ������ � ������ UI
                RunOnUi(() =>
                {
                    // ���������, �� ���� �� �������� ��������, ���� �� ����� ���������
                    if (token.IsCancellationRequested) return;

                    _document?.Dispose(); // ����������� ������ �� ������� ���������
                    _document = newDocument;
                    UpdateData();
                });

            }
            catch (OperationCanceledException)
            {
                // ��� ��������� ���������� ��� ������ �������. �������� ��� �������.
                Error = ("JSON data request was cancelled.");
            }
            catch (HttpRequestException ex)
            {
                // ��� ���������� ����� ������� EnsureSuccessStatusCode ��� ������ (����. 404, 500)
                Error = ($"HTTP request error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // �������� ������ ������ (�������, �������� � �.�.)
                Error = ($"Error retrieving or parsing JSON data: {ex.Message}");
            }
            finally
            {
                // ��� ����������� �� ����������, ���������� ����
                Interlocked.Exchange(ref _retrievingData, 0);
            }
        }


        private void UpdateData()
        {
            Error = "";
            if (_document == null) return;

            try
            {
                var path = GetOrParseJsonPath();
                if (path == null)
                {
                    Data = new List<string>();
                    return;
                }
                var results = path.Evaluate(_document.RootElement.AsNode()).Matches.Take(100 * (_groupBy <= 0 ? 1 : _groupBy)).Select(x => x.Value.ToString()).ToList();

                if (_groupBy > 1)
                {
                    var groupedData = new List<string>();
                    var grouped = new StringBuilder();
                    for (int i = 0; i < results.Count; i++)
                    {
                        if (i > 0 && i % _groupBy == 0)
                        {
                            groupedData.Add(grouped.ToString().TrimEnd('|'));
                            grouped.Clear();
                        }
                        grouped.Append(results[i]).Append("|");
                    }
                    if (grouped.Length > 0)
                    {
                        groupedData.Add(grouped.ToString().TrimEnd('|'));
                    }
                    Data = groupedData;
                }
                else
                {
                    Data = results;
                }
            }
            catch (Exception ex)
            {
                Error = ($"Error updating data with JSONPath: {ex.Message}");
            }
        }

        public UIElement CustomUI => _ui;

        public string Url
        {
            get => _url;
            set => SetPropertyValue(ref _url, value, nameof(Url), __ => { _ = RetrieveDataAsync(); });
        }

        public string JsonPath
        {
            get => _jsonPath;
            set
            {
                SetPropertyValue(ref _jsonPath, value, nameof(JsonPath), _ =>
                {
                    lock (_pathLock)
                    {
                        _compiledPath = null;
                        _compiledPathSource = string.Empty;
                    }

                    UpdateData();
                });
            }
        }

        public string Headers
        {
            get => _headers;
            set => SetPropertyValue(ref _headers, value, nameof(Headers), _ => UpdateData());
        }

        public string Error
        {
            get => _error;
            set => SetPropertyValue(ref _error, value, nameof(Error));
        }

        public List<string> Data
        {
            get => _data;
            set
            {
                _data = value ?? new List<string>();
                _valuesCache = _data.ToArray();
                OnPropertyChanged(nameof(Data));
                OnPropertyChanged(nameof(Values));
            }
        }

        public int GroupBy
        {
            get => _groupBy;
            set => SetPropertyValue(ref _groupBy, value, nameof(GroupBy), _ => UpdateData());
        }

        public List<object> GetProperties()
        {
            return new List<object> { Url, JsonPath, GroupBy, Headers };
        }

        public void SetProperties(List<object> props)
        {
            Url = (string)(props?.ElementAtOrDefault(0) ?? "");
            JsonPath = (string)(props?.ElementAtOrDefault(1) ?? "");
            GroupBy = (int)(props?.ElementAtOrDefault(2) ?? 1);
            Headers = (string)(props?.ElementAtOrDefault(3) ?? "");
        }

        [RelayCommand]
        private void ShowRows()
        {
            new RowsViewer().Bind(this, nameof(Data));
        }

        [RelayCommand]
        private void Reload()
        {
            lock (_ctsLock)
            {
                _cancellationTokenSource?.Cancel();
            }
            _ = RetrieveDataAsync();
        }

        public JsonDataProvider()
        {
            try
            {
                _ui = new OnWidgetUI { DataContext = this };
            }
            catch (Exception e)
            {
                _ui = new TextBox { Text = e.ToString(), AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 256, FontWeight = FontWeights.Normal, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            }

            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(Period)
            };
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();
            _ = RetrieveDataAsync();
        }

        public void ShowProperties(Window owner)
        {
            // Implementation for showing properties window
            // For now, it's not implemented
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            // ��������� IDisposable ��� ���������� ������� ��������
            _refreshTimer.Stop();
            _refreshTimer.Tick -= RefreshTimer_Tick;
            lock (_ctsLock)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
            _document?.Dispose();
        }

        private Json.Path.JsonPath GetOrParseJsonPath()
        {
            var normalized = (JsonPath ?? string.Empty).Replace("\r", "").Replace("\n", "");
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return null;
            }

            lock (_pathLock)
            {
                if (_compiledPath != null && string.Equals(_compiledPathSource, normalized, StringComparison.Ordinal))
                {
                    return _compiledPath;
                }

                _compiledPath = Json.Path.JsonPath.Parse(normalized);
                _compiledPathSource = normalized;
                return _compiledPath;
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
            _ = RetrieveDataAsync();
        }

        private bool SetPropertyValue<T>(ref T field, T value, string propertyName, Action<T> onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            onChanged?.Invoke(value);
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}

