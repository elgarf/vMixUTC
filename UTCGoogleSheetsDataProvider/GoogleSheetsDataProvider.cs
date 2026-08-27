using GalaSoft.MvvmLight.CommandWpf;
using GoogleSheetsDataProvider;
using Popcron.Sheets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
    // Реализуем IDisposable для корректного освобождения ресурсов (таймера и токена отмены)
    public class GoogleSheetsDataProvider : DependencyObject, vMixControllerDataProvider.IvMixDataProviderTextInput, INotifyPropertyChanged, IDisposable
    {
        // Статические кэши для совместного использования между экземплярами провайдера.
        // Это экономит запросы на авторизацию и дублирующую загрузку одних и тех же таблиц.
        private static readonly ConcurrentDictionary<string, Authorization> _authCache = new ConcurrentDictionary<string, Authorization>();
        private static readonly ConcurrentDictionary<string, Spreadsheet> _spreadsheetCache = new ConcurrentDictionary<string, Spreadsheet>();
        private static readonly ConcurrentDictionary<string, DateTime> _lastModifiedCache = new ConcurrentDictionary<string, DateTime>();

        private readonly DispatcherTimer _webTimer;
        private string[] _cached = Array.Empty<string>();
        private int _period = 5000; // Установлено значение по умолчанию
        private string _apiKey = "";
        private string _sheetKey = "";
        private int _startRow = 0;
        private int _endRow = -1;
        private int _startCol = 0;
        private int _endCol = -1;
        private int _sheet = 0;
        private bool _isTable = true;
        private int _rowsCount = 0;
        private UIElement _customUI;
        private RelayCommand _showRowsCommand;
        private RelayCommand _reloadCommand;

        // Семафор для предотвращения одновременного выполнения нескольких запросов на обновление для одного экземпляра.
        // Используем SemaphoreSlim(1, 1) как асинхронный аналог lock.
        private readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public object PreviewKeyUp { get; set; }
        public object GotFocus { get; set; }
        public object LostFocus { get; set; }
        public bool IsProvidingCustomProperties => false;

        string _error = string.Empty;
        public string Error
        {
            get => _error;
            set
            {
                if (_error == value) return;
                _error = value;
                OnPropertyChanged(nameof(Error));
            }
        }

        public int Period
        {
            get => _period;
            set
            {
                if (_period == value) return;
                // Период не должен быть слишком маленьким, чтобы не превысить квоты Google API.
                _period = Math.Max(1000, value);
                if (_webTimer != null)
                {
                    _webTimer.Interval = TimeSpan.FromMilliseconds(_period);
                }
                OnPropertyChanged(nameof(Period));
            }
        }

        // Свойство Values теперь просто возвращает закэшированное значение.
        // Это предотвращает зависание UI, т.к. геттер выполняется мгновенно.
        public string[] Values => _cached;

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

            // Запускаем первоначальное обновление данных
            _ = UpdateData();
        }

        // Запускает асинхронное обновление данных без блокировки вызывающего потока.
        private async void WebTimer_Tick(object sender, EventArgs e)
        {
            await UpdateData();
        }

        private async Task UpdateData()
        {
            if (string.IsNullOrWhiteSpace(APIKey) || string.IsNullOrWhiteSpace(SheetKey))
            {
                // Сбрасываем данные, если ключи не установлены
                UpdateCachedData(Array.Empty<string>());
                return;
            }

            // Пытаемся захватить семафор без ожидания. Если он уже занят, значит,
            // обновление уже идет, и мы просто выходим.
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
                    // 1. Авторизация (используем кэш)
                    if (!_authCache.TryGetValue(APIKey, out var auth))
                    {
                        auth = await Popcron.Sheets.Authorization.Authorize(APIKey);
                        _authCache.TryAdd(APIKey, auth);
                    }

                    token.ThrowIfCancellationRequested();

                    // 2. Проверка необходимости обновления данных
                    _lastModifiedCache.TryGetValue(SheetKey, out var lastModified);
                    if ((DateTime.Now - lastModified).TotalMilliseconds > Period)
                    {
                        //Error = ($"Loading {SheetKey} at {DateTime.Now}");
                        var sst = await Popcron.Sheets.Spreadsheet.Get(SheetKey, auth);
                        _spreadsheetCache[SheetKey] = sst;
                        _lastModifiedCache[SheetKey] = DateTime.Now;
                    }

                    token.ThrowIfCancellationRequested();

                    // 3. Обработка данных и обновление UI
                    ProcessAndCacheData();
                }
                catch (OperationCanceledException)
                {
                    // Это ожидаемое исключение при закрытии, игнорируем его.
                    Error = ("Data update was canceled.");
                }
                catch (Exception ex)
                {
                    Error = ($"Error loading or processing spreadsheet: {ex.Message}");
                    // В случае ошибки сбрасываем кэш, чтобы показать пустое значение
                    UpdateCachedData(Array.Empty<string>());
                }
            }
            finally
            {
                // Освобождаем семафор в любом случае.
                _asyncLock.Release();
            }
        }

        private void ProcessAndCacheData()
        {
            if (!_spreadsheetCache.TryGetValue(SheetKey, out var sst))
            {
                UpdateCachedData(Array.Empty<string>());
                return;
            }

            if (sst.Sheets.Count <= SheetIndex)
            {
                UpdateCachedData(Array.Empty<string>());
                return;
            }

            var sheet = sst.Sheets[SheetIndex];
            var results = new List<string>(); // Используем локальную переменную для потокобезопасности
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

            UpdateCachedData(results.ToArray());
        }

        // Метод для потокобезопасного обновления кэша и свойств, связанных с UI.
        private void UpdateCachedData(string[] newCache)
        {
            // Используем диспетчер для обновления свойств, привязанных к UI.
            RunOnUi(() =>
            {
                _cached = newCache;
                RowsCount = _cached.Length;
                OnPropertyChanged(nameof(Values)); // Уведомляем UI, что массив Values изменился
                OnPropertyChanged(nameof(Cached)); // Также уведомляем для RowsViewer
            });
        }


        #region Properties and Commands

        public string APIKey
        {
            get => _apiKey;
            set
            {
                if (_apiKey == value) return;
                _apiKey = value.Trim();
                OnPropertyChanged(nameof(APIKey));
                _ = UpdateData(); // Запускаем обновление при смене ключа
            }
        }

        public string SheetKey
        {
            get => _sheetKey;
            set
            {
                if (_sheetKey == value) return;
                if (Uri.TryCreate(value, UriKind.Absolute, out var k))
                {
                    _sheetKey = ParseSheetKeyFromUri(k);
                }
                else
                    _sheetKey = value;
                OnPropertyChanged(nameof(SheetKey));
                _ = UpdateData(); // Запускаем обновление при смене ключа
            }
        }

        public int StartRow
        {
            get => _startRow;
            set
            {
                if (_startRow == value) return;
                _startRow = value;
                OnPropertyChanged(nameof(StartRow));
                ProcessAndCacheData(); // Пересчитываем данные из кэша, не загружая заново
            }
        }

        public int EndRow
        {
            get => _endRow;
            set
            {
                if (_endRow == value) return;
                _endRow = value;
                OnPropertyChanged(nameof(EndRow));
                ProcessAndCacheData();
            }
        }

        public int StartCol
        {
            get => _startCol;
            set
            {
                if (_startCol == value) return;
                _startCol = value;
                OnPropertyChanged(nameof(StartCol));
                ProcessAndCacheData();
            }
        }

        public int EndCol
        {
            get => _endCol;
            set
            {
                if (_endCol == value) return;
                _endCol = value;
                OnPropertyChanged(nameof(EndCol));
                ProcessAndCacheData();
            }
        }

        public int SheetIndex
        {
            get => _sheet;
            set
            {
                if (_sheet == value) return;
                _sheet = value;
                OnPropertyChanged(nameof(SheetIndex));
                ProcessAndCacheData();
            }
        }

        public bool IsTable
        {
            get => _isTable;
            set
            {
                if (_isTable == value) return;
                _isTable = value;
                OnPropertyChanged(nameof(IsTable));
                ProcessAndCacheData();
            }
        }

        public int RowsCount
        {
            get => _rowsCount;
            set
            {
                if (_rowsCount == value) return;
                _rowsCount = value;
                OnPropertyChanged(nameof(RowsCount));
            }
        }

        public RelayCommand ShowRowsCommand => _showRowsCommand ?? (_showRowsCommand = new RelayCommand(() => new RowsViewer().Bind(this, nameof(Cached))));

        public RelayCommand ReloadCommand => _reloadCommand ?? (_reloadCommand = new RelayCommand(() =>
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
            _lastModifiedCache.TryRemove(SheetKey, out _);
            _spreadsheetCache.TryRemove(SheetKey, out _);
            _ = UpdateData();
        }));

        public string[] Cached
        {
            get => _cached;
            private set // Сеттер делаем приватным
            {
                _cached = value;
                OnPropertyChanged(nameof(Cached));
            }
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
            // Убеждаемся, что событие вызывается в UI-потоке
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
    }
}
