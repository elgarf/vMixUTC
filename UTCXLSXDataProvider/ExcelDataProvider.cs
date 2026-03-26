using CommunityToolkit.Mvvm.Input;
using ExcelDataReader;
using ExcelDataReader.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using vMixControllerSkin;

namespace UTCGoogleSheetsDataProvider
{
    public partial class ExcelDataProvider : DependencyObject, vMixControllerDataProvider.IvMixDataProviderTextInput, INotifyPropertyChanged, IDataErrorInfo
    {
        private static readonly Regex ColumnRegex = new Regex("^[A-Z]+$", RegexOptions.Compiled);

        private readonly DispatcherTimer _refreshTimer;
        private readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);

        private DateTime _lastModifiedUtc = DateTime.MinValue;
        private string[] _valuesCache = Array.Empty<string>();
        private bool _hasError;
        private int _refreshScheduled;
        private int _period = 1000;
        private UIElement _customUI;

        private string _filePath = "";
        private int _startRow;
        private int _endRow = -1;
        private string _startCol = "0";
        private string _endCol = "-1";
        private string _sheet = "0";
        private bool _isTable = true;

        public ICommand PreviewKeyUp { get; set; }
        public ICommand GotFocus { get; set; }
        public ICommand LostFocus { get; set; }

        public bool IsProvidingCustomProperties => false;
        public string Error => null;
        public UIElement CustomUI => _customUI;
        public string[] Values => _valuesCache;

        public int Period
        {
            get => _period;
            set
            {
                var normalized = Math.Max(250, value);
                SetPropertyValue(ref _period, normalized, nameof(Period), p =>
                {
                    _refreshTimer.Interval = TimeSpan.FromMilliseconds(p);
                    InvalidateAndScheduleRefresh();
                });
            }
        }

        public string FilePath
        {
            get => _filePath;
            set => SetPropertyValue(ref _filePath, value, nameof(FilePath), _ => InvalidateAndScheduleRefresh());
        }

        public int StartRow
        {
            get => _startRow;
            set => SetPropertyValue(ref _startRow, value, nameof(StartRow), _ => InvalidateAndScheduleRefresh());
        }

        public int EndRow
        {
            get => _endRow;
            set => SetPropertyValue(ref _endRow, value, nameof(EndRow), _ => InvalidateAndScheduleRefresh());
        }

        public string StartCol
        {
            get => _startCol;
            set => SetPropertyValue(ref _startCol, value, nameof(StartCol), _ => InvalidateAndScheduleRefresh());
        }

        public string EndCol
        {
            get => _endCol;
            set => SetPropertyValue(ref _endCol, value, nameof(EndCol), _ => InvalidateAndScheduleRefresh());
        }

        public string SheetIndex
        {
            get => _sheet;
            set => SetPropertyValue(ref _sheet, value, nameof(SheetIndex), _ => InvalidateAndScheduleRefresh());
        }

        public bool IsTable
        {
            get => _isTable;
            set => SetPropertyValue(ref _isTable, value, nameof(IsTable), _ => InvalidateAndScheduleRefresh());
        }

        public ExcelDataProvider()
        {
            try
            {
                _customUI = new OnWidgetUI { DataContext = this };
            }
            catch (Exception e)
            {
                _customUI = new TextBox
                {
                    Text = e.ToString(),
                    AcceptsReturn = true,
                    TextWrapping = TextWrapping.Wrap,
                    Height = 256,
                    FontWeight = FontWeights.Normal,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                };
            }

            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(Period)
            };
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();

            ScheduleRefresh();
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

        [RelayCommand]
        private void ShowRows()
        {
            new RowsViewer().Bind(this, nameof(Values));
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            ScheduleRefresh();
        }

        private void InvalidateAndScheduleRefresh()
        {
            _lastModifiedUtc = DateTime.MinValue;
            ScheduleRefresh();
        }

        private void ScheduleRefresh()
        {
            if (Interlocked.CompareExchange(ref _refreshScheduled, 1, 0) != 0)
            {
                return;
            }

            _ = RefreshValuesAsync();
        }

        private async Task RefreshValuesAsync()
        {
            try
            {
                await _asyncLock.WaitAsync();
                LoadValuesCore();
            }
            finally
            {
                _asyncLock.Release();
                Interlocked.Exchange(ref _refreshScheduled, 0);
            }
        }

        private void LoadValuesCore()
        {
            _hasError = false;

            try
            {
                if (string.IsNullOrWhiteSpace(FilePath) || !File.Exists(FilePath))
                {
                    _hasError = true;
                    UpdateValuesCache(Array.Empty<string>());
                    return;
                }

                var fileInfo = new FileInfo(FilePath);
                if (fileInfo.LastWriteTimeUtc <= _lastModifiedUtc)
                {
                    return;
                }

                using (var xls = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = ExcelReaderFactory.CreateReader(xls))
                {
                    var results = ReadRows(reader);
                    _lastModifiedUtc = fileInfo.LastWriteTimeUtc;
                    UpdateValuesCache(results.ToArray());
                }
            }
            catch (ExcelReaderException ex)
            {
                _hasError = true;
                UpdateValuesCache(Array.Empty<string>());
                Debug.Print($"Error reading Excel file: {ex.Message}");
            }
            catch (Exception ex)
            {
                _hasError = true;
                UpdateValuesCache(Array.Empty<string>());
                Debug.Print($"Unexpected error: {ex.Message}");
            }
        }

        private List<string> ReadRows(IExcelDataReader reader)
        {
            var results = new List<string>();
            int startColIndex = ParseExcelColumn(StartCol);
            int endColIndex = ParseExcelColumn(EndCol);
            int sheet = 0;

            do
            {
                int row = 0;
                int parsedSheetIndex;
                bool sheetMatches = (int.TryParse(SheetIndex, out parsedSheetIndex) && sheet == parsedSheetIndex) || reader.Name == SheetIndex;

                if (sheetMatches)
                {
                    while (reader.Read())
                    {
                        if (row >= StartRow)
                        {
                            var safeStartCol = Math.Max(0, startColIndex);
                            var endExclusive = endColIndex >= 0
                                ? Math.Min(reader.FieldCount, endColIndex + 1)
                                : reader.FieldCount;

                            if (IsTable)
                            {
                                var lineBuilder = new StringBuilder();
                                for (int i = safeStartCol; i < endExclusive; i++)
                                {
                                    var value = reader.GetValue(i)?.ToString() ?? string.Empty;
                                    if (lineBuilder.Length > 0)
                                    {
                                        lineBuilder.Append('|');
                                    }
                                    lineBuilder.Append(value);
                                }
                                results.Add(lineBuilder.ToString());
                            }
                            else
                            {
                                for (int i = safeStartCol; i < endExclusive; i++)
                                {
                                    results.Add(reader.GetValue(i)?.ToString() ?? string.Empty);
                                }
                            }
                        }

                        row++;
                        if (EndRow >= 0 && row > EndRow)
                        {
                            break;
                        }
                    }
                }

                sheet++;
            }
            while (reader.NextResult());

            return results;
        }

        private void UpdateValuesCache(string[] newValues)
        {
            var normalized = newValues ?? Array.Empty<string>();
            if (_valuesCache.SequenceEqual(normalized))
            {
                return;
            }

            _valuesCache = normalized;
            RaisePropertyChanged(nameof(Values));
        }

        private int ParseExcelColumn(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return -1;
            }

            input = input.Trim();
            if (int.TryParse(input, out int number))
            {
                return number;
            }

            input = input.ToUpperInvariant();
            if (!ColumnRegex.IsMatch(input))
            {
                return -1;
            }

            int result = 0;
            foreach (char c in input)
            {
                result = result * 26 + (c - 'A' + 1);
            }

            return result - 1;
        }

        public string this[string columnName]
        {
            get
            {
                string error = string.Empty;
                switch (columnName)
                {
                    case nameof(FilePath):
                        if (_hasError)
                        {
                            error = "File not found or is not a valid excel file!";
                        }
                        break;
                    case nameof(StartCol):
                        if (!int.TryParse(StartCol, out _) && !ColumnRegex.IsMatch(StartCol?.ToUpperInvariant() ?? string.Empty))
                        {
                            error = "Start column is in wrong format!";
                        }
                        break;
                    case nameof(EndCol):
                        if (!int.TryParse(EndCol, out _) && !ColumnRegex.IsMatch(EndCol?.ToUpperInvariant() ?? string.Empty))
                        {
                            error = "End column is in wrong format!";
                        }
                        break;
                }

                return error;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetPropertyValue<T>(ref T field, T value, string propertyName, Action<T> onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            onChanged?.Invoke(value);
            RaisePropertyChanged(propertyName);
            return true;
        }

        public List<object> GetProperties()
        {
            return new List<object> { FilePath, StartRow, EndRow, StartCol, EndCol, SheetIndex, IsTable };
        }

        public void SetProperties(List<object> props)
        {
            if (props == null)
            {
                return;
            }

            FilePath = props.ElementAtOrDefault(0) as string ?? string.Empty;
            StartRow = (int?)props.ElementAtOrDefault(1) ?? 0;
            EndRow = (int?)props.ElementAtOrDefault(2) ?? -1;

            if (props.ElementAtOrDefault(3) is int)
            {
                StartCol = ((int?)props.ElementAtOrDefault(3) ?? 0).ToString();
            }
            else
            {
                StartCol = (string)props.ElementAtOrDefault(3) ?? "0";
            }

            if (props.ElementAtOrDefault(4) is int)
            {
                EndCol = ((int?)props.ElementAtOrDefault(4) ?? -1).ToString();
            }
            else
            {
                EndCol = (string)props.ElementAtOrDefault(4) ?? "-1";
            }

            if (props.ElementAtOrDefault(5) is int)
            {
                SheetIndex = ((int?)props.ElementAtOrDefault(5) ?? 0).ToString();
            }
            else
            {
                SheetIndex = (string)props.ElementAtOrDefault(5) ?? "0";
            }

            IsTable = (bool?)props.ElementAtOrDefault(6) ?? true;
        }

        public void ShowProperties(Window owner)
        {
        }
    }
}
