using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GoogleSheetsDataProvider;
using Popcron.Sheets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using vMixControllerDataProvider;
using vMixControllerSkin;

namespace UTCGoogleSheetsDataProvider
{
    public class JsonSheetsSerializer : SheetsSerializer
    {
        //static Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
        public override T DeserializeObject<T>(string data)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data);
        }

        public override string SerializeObject(object data)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(data);
        }
    }
    // ��������� IDisposable ��� ����������� ������������ �������� (������� � ������ ������)
    public partial class GoogleSheetsDataProvider : DependencyObject, vMixControllerDataProvider.IvMixDataProviderTextInput, INotifyPropertyChanged, IDisposable
    {
        // ����������� ���� ��� ����������� ������������� ����� ������������ ����������.
        // ��� �������� ������� �� ����������� � ����������� �������� ����� � ��� �� ������.
        private static readonly ConcurrentDictionary<string, Authorization> _authCache = new ConcurrentDictionary<string, Authorization>();
        private static readonly ConcurrentDictionary<string, Spreadsheet> _spreadsheetCache = new ConcurrentDictionary<string, Spreadsheet>();
        private static readonly ConcurrentDictionary<string, DateTime> _lastModifiedCache = new ConcurrentDictionary<string, DateTime>();

        private readonly DispatcherTimer _webTimer;
        private string[] _valuesCache = Array.Empty<string>();
        private int _period = 5000; // ����������� �������� �� ���������
        private string _apiKey = "";
        private string _sheetKey = "";
        private int _startRow = 0;
        private int _endRow = -1;
        private int _startCol = 0;
        private int _endCol = -1;
        private int _sheet = 0;
        private bool _isTable = true;
        private UIElement _customUI;

        // ������� ��� �������������� �������������� ���������� ���������� �������� �� ���������� ��� ������ ����������.
        // ���������� SemaphoreSlim(1, 1) ��� ����������� ������ lock.
        private readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public ICommand PreviewKeyUp { get; set; }
        public ICommand GotFocus { get; set; }
        public ICommand LostFocus { get; set; }
        public bool IsProvidingCustomProperties => false;

        string _error = string.Empty;
        public string Error
        {
            get => _error;
            set => SetPropertyValue(ref _error, value, nameof(Error));
        }

        public int Period
        {
            get => _period;
            set
            {
                var normalized = Math.Max(1000, value);
                SetPropertyValue(ref _period, normalized, nameof(Period), p =>
                {
                    if (_webTimer != null)
                    {
                        _webTimer.Interval = TimeSpan.FromMilliseconds(p);
                    }
                });
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
                if (PreviewKeyUp != null && PreviewKeyUp.CanExecute(p))
                {
                    PreviewKeyUp.Execute(p);
                }
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

        // �������� Values ������ ������ ���������� �������������� ��������.
        // ��� ������������� ��������� UI, �.�. ������ ����������� ���������.
        public string[] Values => _valuesCache;

        public UIElement CustomUI => _customUI;

        public GoogleSheetsDataProvider()
        {
            SheetsSerializer.Serializer = new JsonSheetsSerializer();
            try
            {
                _customUI = new OnWidgetUI() { DataContext = this };
            }
            catch (Exception e)
            {
                _customUI = new TextBox() { Text = e.ToString(), AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 256, FontWeight = FontWeights.Normal, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            }

            _webTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(Period)
            };
            _webTimer.Tick += WebTimer_Tick;
            _webTimer.Start();

            // ��������� �������������� ���������� ������
            _ = UpdateData();
        }

        // ��������� ����������� ���������� ������ ��� ���������� ����������� ������.
        private async void WebTimer_Tick(object sender, EventArgs e)
        {
            await UpdateData();
        }

        private async Task UpdateData()
        {
            if (string.IsNullOrWhiteSpace(APIKey) || string.IsNullOrWhiteSpace(SheetKey))
            {
                // ���������� ������, ���� ����� �� �����������
                UpdateValuesCache(Array.Empty<string>());
                return;
            }

            // �������� ��������� ������� ��� ��������. ���� �� ��� �����, ������,
            // ���������� ��� ����, � �� ������ �������.
            if (!await _asyncLock.WaitAsync(0))
            {
                return;
            }

            try
            {
                var token = _cancellationTokenSource.Token;
                if (token.IsCancellationRequested) return;

                try
                {
                    // 1. ����������� (���������� ���)
                    if (!_authCache.TryGetValue(APIKey, out var auth))
                    {
                        auth = await Popcron.Sheets.Authorization.Authorize(APIKey);
                        _authCache.TryAdd(APIKey, auth);
                    }

                    token.ThrowIfCancellationRequested();

                    // 2. �������� ������������� ���������� ������
                    _lastModifiedCache.TryGetValue(SheetKey, out var lastModified);
                    if ((DateTime.Now - lastModified).TotalMilliseconds > Period)
                    {
                        //Error = ($"Loading {SheetKey} at {DateTime.Now}");
                        var sst = await Popcron.Sheets.Spreadsheet.Get(SheetKey, auth);
                        _spreadsheetCache[SheetKey] = sst;
                        _lastModifiedCache[SheetKey] = DateTime.Now;
                    }

                    token.ThrowIfCancellationRequested();

                    // 3. ��������� ������ � ���������� UI
                    ProcessAndCacheData();
                }
                catch (OperationCanceledException)
                {
                    // ��� ��������� ���������� ��� ��������, ���������� ���.
                    Error = ("Data update was canceled.");
                }
                catch (Exception ex)
                {
                    Error = ($"Error loading or processing spreadsheet: {ex.Message}");
                    // � ������ ������ ���������� ���, ����� �������� ������ ��������
                    UpdateValuesCache(Array.Empty<string>());
                }
            }
            finally
            {
                // ����������� ������� � ����� ������.
                _asyncLock.Release();
            }
        }

        private void ProcessAndCacheData()
        {
            if (!_spreadsheetCache.TryGetValue(SheetKey, out var sst))
            {
                UpdateValuesCache(Array.Empty<string>());
                return;
            }

            if (sst.Sheets.Count <= SheetIndex)
            {
                UpdateValuesCache(Array.Empty<string>());
                return;
            }

            var sheet = sst.Sheets[SheetIndex];
            var results = new List<string>(); // ���������� ��������� ���������� ��� ������������������
            int maxRows = sheet.Rows;
            int maxCols = sheet.Columns;

            for (int y = Math.Max(StartRow, 0); y < maxRows && (EndRow < 0 || y <= EndRow); y++)
            {
                if (IsTable)
                {
                    var line = new StringBuilder();
                    for (int x = Math.Max(StartCol, 0); x < maxCols && (EndCol < 0 || x <= EndCol); x++)
                    {
                        string val = sheet.Data[x, y].Value ?? "";
                        line.Append('|').Append(val);
                    }

                    if (line.Length > 0)
                    {
                        results.Add(line.ToString(1, line.Length - 1));
                    }
                }
                else
                {
                    for (int x = Math.Max(StartCol, 0); x < maxCols && (EndCol < 0 || x <= EndCol); x++)
                    {
                        results.Add(sheet.Data[x, y].Value ?? "");
                    }
                }
            }

            UpdateValuesCache(results.ToArray());
        }

        // ����� ��� ����������������� ���������� ���� � �������, ��������� � UI.
        private void UpdateValuesCache(string[] newValues)
        {
            // ���������� ��������� ��� ���������� �������, ����������� � UI.
            RunOnUi(() =>
            {
                _valuesCache = newValues ?? Array.Empty<string>();
                OnPropertyChanged(nameof(Values)); // ���������� UI, ��� ������ Values ���������
            });
        }


        #region Properties and Commands

        public string APIKey
        {
            get => _apiKey;
            set => SetPropertyValue(ref _apiKey, value?.Trim() ?? string.Empty, nameof(APIKey), __ => { _ = UpdateData(); });
        }

        public string SheetKey
        {
            get => _sheetKey;
            set
            {
                var normalized = value;
                if (Uri.TryCreate(value, UriKind.Absolute, out var k))
                {
                    normalized = ParseSheetKeyFromUri(k);
                }

                SetPropertyValue(ref _sheetKey, normalized, nameof(SheetKey), __ => { _ = UpdateData(); });
            }
        }

        public int StartRow
        {
            get => _startRow;
            set => SetPropertyValue(ref _startRow, value, nameof(StartRow), _ => ProcessAndCacheData());
        }

        public int EndRow
        {
            get => _endRow;
            set => SetPropertyValue(ref _endRow, value, nameof(EndRow), _ => ProcessAndCacheData());
        }

        public int StartCol
        {
            get => _startCol;
            set => SetPropertyValue(ref _startCol, value, nameof(StartCol), _ => ProcessAndCacheData());
        }

        public int EndCol
        {
            get => _endCol;
            set => SetPropertyValue(ref _endCol, value, nameof(EndCol), _ => ProcessAndCacheData());
        }

        public int SheetIndex
        {
            get => _sheet;
            set => SetPropertyValue(ref _sheet, value, nameof(SheetIndex), _ => ProcessAndCacheData());
        }

        public bool IsTable
        {
            get => _isTable;
            set => SetPropertyValue(ref _isTable, value, nameof(IsTable), _ => ProcessAndCacheData());
        }

        [RelayCommand]
        private void ShowRows()
        {
            new RowsViewer().Bind(this, nameof(Values));
        }

        [RelayCommand]
        private void Reload()
        {
            try
            {
                _cancellationTokenSource.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
            }
            _ = UpdateData();
                }

        #endregion

        #region Serialization and Disposal

        public List<object> GetProperties()
        {
            return new List<object> { APIKey, StartRow, EndRow, StartCol, EndCol, SheetIndex, IsTable, SheetKey, Period };
        }

        public void SetProperties(List<object> props)
        {
            if (props == null) return;

            APIKey = props.ElementAtOrDefault(0) as string ?? "";
            StartRow = (int?)props.ElementAtOrDefault(1) ?? 0;
            EndRow = (int?)props.ElementAtOrDefault(2) ?? -1;
            StartCol = (int?)props.ElementAtOrDefault(3) ?? 0;
            EndCol = (int?)props.ElementAtOrDefault(4) ?? -1;
            SheetIndex = (int?)props.ElementAtOrDefault(5) ?? 0;
            IsTable = (bool?)props.ElementAtOrDefault(6) ?? true;
            SheetKey = props.ElementAtOrDefault(7) as string ?? "";
            Period = (int?)props.ElementAtOrDefault(8) ?? 5000;
        }

        public void ShowProperties(Window owner)
        {
            // Implementation for showing properties window
        }

        public void Dispose()
        {
            _webTimer.Stop();
            _webTimer.Tick -= WebTimer_Tick;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _asyncLock.Dispose();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            // ����������, ��� ������� ���������� � UI-������
            if (Application.Current?.Dispatcher?.CheckAccess() ?? true)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName))));
            }
        }

        #endregion

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

        private static string ParseSheetKeyFromUri(Uri uri)
        {
            if (uri == null)
            {
                return string.Empty;
            }

            var parts = uri.LocalPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return string.Empty;
            }

            var editIndex = Array.IndexOf(parts, "edit");
            if (editIndex > 0)
            {
                return parts[editIndex - 1];
            }

            var dIndex = Array.IndexOf(parts, "d");
            if (dIndex >= 0 && dIndex + 1 < parts.Length)
            {
                return parts[dIndex + 1];
            }

            return parts[parts.Length - 1];
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

