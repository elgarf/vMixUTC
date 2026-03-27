using CommunityToolkit.Mvvm.Input;
using Json.More;
using Json.Path;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using vMixControllerDataProvider;
using vMixControllerSkin;

namespace JsonDataProviderNs
{
    public partial class JsonDataProvider : PollingTextInputDataProviderBase
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        private readonly object _pathLock = new object();
        private readonly object _ctsLock = new object();

        private JsonDocument _document;
        private Json.Path.JsonPath _compiledPath;
        private string _compiledPathSource = string.Empty;
        private CancellationTokenSource _cancellationTokenSource;

        private List<string> _data = new List<string>();
        private string _url = string.Empty;
        private string _jsonPath = string.Empty;
        private string _headers = string.Empty;
        private string _error = string.Empty;
        private int _groupBy = 1;

        protected override int MinPeriodMs => 250;

        public JsonDataProvider()
        {
            Period = 5000;

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

        public string Url
        {
            get => _url;
            set => SetPropertyValue(ref _url, value ?? string.Empty, nameof(Url), _ => ScheduleRefresh());
        }

        public string JsonPath
        {
            get => _jsonPath;
            set
            {
                SetPropertyValue(ref _jsonPath, value ?? string.Empty, nameof(JsonPath), _ =>
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
            set => SetPropertyValue(ref _headers, value ?? string.Empty, nameof(Headers), _ => ScheduleRefresh());
        }

        public string Error
        {
            get => _error;
            set => SetPropertyValue(ref _error, value ?? string.Empty, nameof(Error));
        }

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

        public int GroupBy
        {
            get => _groupBy;
            set => SetPropertyValue(ref _groupBy, value <= 0 ? 1 : value, nameof(GroupBy), _ => UpdateData());
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

            ScheduleRefresh();
        }

        public override List<object> GetProperties()
        {
            return new List<object> { Url, JsonPath, GroupBy, Headers };
        }

        public override void SetProperties(List<object> props)
        {
            Url = (string)(props?.ElementAtOrDefault(0) ?? string.Empty);
            JsonPath = (string)(props?.ElementAtOrDefault(1) ?? string.Empty);
            GroupBy = (int)(props?.ElementAtOrDefault(2) ?? 1);
            Headers = (string)(props?.ElementAtOrDefault(3) ?? string.Empty);
        }

        public override void ShowProperties(Window owner)
        {
        }

        protected override async Task RefreshValuesAsync()
        {
            await RetrieveDataAsync();
        }

        public override void Dispose()
        {
            lock (_ctsLock)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }

            _document?.Dispose();
            base.Dispose();
        }

        private async Task RetrieveDataAsync()
        {
            Error = string.Empty;

            if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri))
            {
                Data = new List<string>();
                return;
            }

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

                if (uri.Scheme == Uri.UriSchemeFile)
                {
                    using (var stream = File.Open(uri.LocalPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        newDocument = await JsonDocument.ParseAsync(stream, default, token);
                    }
                }
                else
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                    {
                        AddHeadersFromString(request.Headers, Headers);
                        using (var response = await HttpClient.SendAsync(request, token))
                        {
                            response.EnsureSuccessStatusCode();
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            {
                                newDocument = await JsonDocument.ParseAsync(stream, default, token);
                            }
                        }
                    }
                }

                RunOnUi(() =>
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    _document?.Dispose();
                    _document = newDocument;
                    UpdateData();
                });
            }
            catch (OperationCanceledException)
            {
                Error = "JSON data request was cancelled.";
            }
            catch (HttpRequestException ex)
            {
                Error = $"HTTP request error: {ex.Message}";
                Data = new List<string>();
            }
            catch (Exception ex)
            {
                Error = $"Error retrieving or parsing JSON data: {ex.Message}";
                Data = new List<string>();
            }
        }

        private void UpdateData()
        {
            Error = string.Empty;
            if (_document == null)
            {
                Data = new List<string>();
                return;
            }

            try
            {
                var path = GetOrParseJsonPath();
                if (path == null)
                {
                    Data = new List<string>();
                    return;
                }

                int groupBy = GroupBy <= 0 ? 1 : GroupBy;
                var results = path.Evaluate(_document.RootElement.AsNode())
                    .Matches
                    .Take(100 * groupBy)
                    .Select(x => x.Value.ToString())
                    .ToList();

                Data = DataProviderTextFormatter.GroupByPipe(results, groupBy);
            }
            catch (Exception ex)
            {
                Error = $"Error updating data with JSONPath: {ex.Message}";
                Data = new List<string>();
            }
        }

        private Json.Path.JsonPath GetOrParseJsonPath()
        {
            var normalized = (JsonPath ?? string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);
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

        private static void AddHeadersFromString(System.Net.Http.Headers.HttpRequestHeaders headers, string headersString)
        {
            if (string.IsNullOrWhiteSpace(headersString))
            {
                return;
            }

            var lines = headersString.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var colonIndex = line.IndexOf(':');
                if (colonIndex <= 0)
                {
                    continue;
                }

                var key = line.Substring(0, colonIndex).Trim();
                var value = line.Substring(colonIndex + 1).Trim();
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    headers.TryAddWithoutValidation(key, value);
                }
            }
        }
    }
}
