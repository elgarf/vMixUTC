using ExcelDataReader;
using ExcelDataReader.Exceptions;
using GalaSoft.MvvmLight.CommandWpf;
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
using System.Windows.Threading;
using vMixControllerSkin;

namespace UTCGoogleSheetsDataProvider
{
    public class ExcelDataProvider : DependencyObject, vMixControllerDataProvider.IvMixDataProviderTextInput, INotifyPropertyChanged, IDataErrorInfo
    {
        private DateTime _lastModified = DateTime.MinValue;
        private string[] _cached = Array.Empty<string>();
        private bool _hasError = false;
        private System.Windows.UIElement _customUI;
        private static readonly Regex ColumnRegex = new Regex("^[A-Z]+$", RegexOptions.Compiled);
        private readonly DispatcherTimer _refreshTimer;
        private int _period = 1000;
        private readonly SemaphoreSlim _reloadLock = new SemaphoreSlim(1, 1);

        public object PreviewKeyUp { get; set; }
        public object GotFocus { get; set; }
        public object LostFocus { get; set; }
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

                _lastModified = DateTime.MinValue;
            }
        }
        public bool IsProvidingCustomProperties => false;

        private int ParseExcelColumn(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return -1;

            input = input.Trim();

            // Если входная строка состоит только из цифр - это номер столбца
            if (int.TryParse(input, out int number))
            {
                /*if (number <= 0)
                    throw new ArgumentException("Column number must be positive");*/
                return number;
            }

            // Если есть буквы - это буквенное обозначение
            input = input.ToUpper();

            if (!ColumnRegex.IsMatch(input))
                return -1;//throw new ArgumentException("Input must contain only letters A-Z or only digits");

            int result = 0;

            foreach (char c in input)
            {
                result = result * 26 + (c - 'A' + 1);
            }

            return result - 1;
        }

        public string[] Values
        {
            get
            {
                return Cached;
            }
        }

        private void ReloadInBackground()
        {
            if (string.IsNullOrWhiteSpace(FilePath))
                return;

            if (!File.Exists(FilePath))
            {
                _hasError = true;
                Cached = Array.Empty<string>();
                RowsCount = 0;
                RaisePropertyChanged(nameof(Values));
                return;
            }

            var fileInfo = new FileInfo(FilePath);
            if (fileInfo.LastWriteTimeUtc <= _lastModified)
                return;

            if (!_reloadLock.Wait(0))
                return;

            var timestamp = fileInfo.LastWriteTimeUtc;
            Task.Run(() =>
            {
                string[] result = null;
                try
                {
                    result = ReloadValues();
                }
                catch
                {
                    result = null;
                }
                finally
                {
                    _reloadLock.Release();
                }

                var dispatcher = Application.Current?.Dispatcher;
                if (dispatcher == null)
                    return;

                Action apply = () =>
                {
                    if (result != null)
                    {
                        _lastModified = timestamp;
                        _hasError = false;
                        Cached = result;
                        RowsCount = result.Length;
                    }
                    else
                    {
                        _hasError = true;
                        Cached = Array.Empty<string>();
                        RowsCount = 0;
                    }
                    RaisePropertyChanged(nameof(Values));
                };

                if (dispatcher.CheckAccess())
                    apply();
                else
                    dispatcher.BeginInvoke(apply);
            });
        }

        private string[] ReloadValues()
        {
            try
            {
                using (var xls = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    try
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(xls))
                        {
                            List<string> results = new List<string>();
                            int sheet = 0;
                            int startColIndex = ParseExcelColumn(StartCol);
                            int endColIndex = ParseExcelColumn(EndCol);
                            do
                            {
                                int row = 0;
                                int sheetIndex = 0;
                                if ((int.TryParse(SheetIndex, out sheetIndex) && sheet == sheetIndex) || reader.Name == SheetIndex)
                                    while (reader.Read())
                                    {
                                        if (row >= StartRow)
                                        {
                                            var safeStartCol = Math.Max(0, startColIndex);
                                            var endExclusive = endColIndex >= 0
                                                ? Math.Min(reader.FieldCount, endColIndex + 1)
                                                : reader.FieldCount;
                                            StringBuilder lineBuilder = IsTable ? new StringBuilder() : null;
                                            for (int i = safeStartCol; i < endExclusive; i++)
                                            {
                                                var value = reader.GetValue(i)?.ToString() ?? "";
                                                if (IsTable)
                                                {
                                                    if (lineBuilder.Length > 0)
                                                    {
                                                        lineBuilder.Append('|');
                                                    }
                                                    lineBuilder.Append(value);
                                                }
                                                else
                                                {
                                                    results.Add(value);
                                                }
                                            }
                                            if (IsTable)
                                                results.Add(lineBuilder?.ToString() ?? string.Empty);
                                        }
                                        row++;
                                        if (EndRow >= 0 && row > EndRow)
                                            break;
                                    }
                                sheet++;
                            }
                            while (reader.NextResult());

                            return results.ToArray();
                        }
                    }
                    catch (ExcelReaderException ex)
                    {
                        Debug.Print($"Error reading Excel file: {ex.Message}");
                        return null;
                    }
                    catch (Exception ex)
                    {
                        Debug.Print($"Unexpected error: {ex.Message}");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"An error occurred: {ex.Message}");
                return null;
            }
        }


        public System.Windows.UIElement CustomUI => _customUI;

        public ExcelDataProvider()
        {
            try
            {
                _customUI = new OnWidgetUI() { DataContext = this };
            }
            catch (Exception e)
            {
                _customUI = new TextBox() { Text = e.ToString(), AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 256, FontWeight = FontWeights.Normal, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            }

            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(Period)
            };
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();
        }

        public string FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath == value)
                {
                    return;
                }

                _filePath = value;
                _lastModified = DateTime.MinValue;
                RaisePropertyChanged(nameof(FilePath));
            }
        }
        private string _filePath = "";

        public int StartRow
        {
            get => _startRow;
            set
            {
                if (_startRow == value)
                {
                    return;
                }

                _startRow = value;
                _lastModified = DateTime.MinValue;
                RaisePropertyChanged(nameof(StartRow));
            }
        }
        private int _startRow = 0;

        public int EndRow
        {
            get => _endRow;
            set
            {
                if (_endRow == value)
                {
                    return;
                }

                _endRow = value;
                _lastModified = DateTime.MinValue;
                RaisePropertyChanged(nameof(EndRow));
            }
        }
        private int _endRow = -1;

        public string StartCol
        {
            get => _startCol;
            set
            {
                if (_startCol == value)
                {
                    return;
                }

                _startCol = value;
                _lastModified = DateTime.MinValue;
                RaisePropertyChanged(nameof(StartCol));
            }
        }
        private string _startCol = "0";

        public string EndCol
        {
            get => _endCol;
            set
            {
                if (_endCol == value)
                {
                    return;
                }

                _endCol = value;
                _lastModified = DateTime.MinValue;
                RaisePropertyChanged(nameof(EndCol));
            }
        }
        private string _endCol = "-1";

        public string SheetIndex
        {
            get => _sheet;
            set
            {
                if (_sheet == value)
                {
                    return;
                }

                _sheet = value;
                _lastModified = DateTime.MinValue;
                RaisePropertyChanged(nameof(SheetIndex));
            }
        }
        private string _sheet = "0";

        public bool IsTable
        {
            get => _isTable;
            set
            {
                if (_isTable == value)
                {
                    return;
                }

                _isTable = value;
                _lastModified = DateTime.MinValue;
                RaisePropertyChanged(nameof(IsTable));
            }
        }
        private bool _isTable = true;

        public int RowsCount
        {
            get => _rowsCount;
            set
            {
                if (_rowsCount == value)
                {
                    return;
                }

                _rowsCount = value;
                RaisePropertyChanged(nameof(RowsCount));
            }
        }
        private int _rowsCount = 0;

        public string Error => null; // Not implemented, no general error for the entire object

        private RelayCommand _showRowsCommand;
        public RelayCommand ShowRowsCommand => _showRowsCommand ?? (_showRowsCommand = new RelayCommand(() => new RowsViewer().Bind(this, nameof(Cached))));

        public string[] Cached
        {
            get => _cached;
            set
            {
                _cached = value;
                RaisePropertyChanged(nameof(Cached));
            }
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
                            error = "File not found or is not a valid excel file!";
                        break;
                    case nameof(StartCol):
                        if (!int.TryParse(StartCol, out _) && !ColumnRegex.IsMatch(StartCol?.ToUpperInvariant() ?? string.Empty))
                            error = "Start column is in wrong format!";
                        break;
                    case nameof(EndCol):
                        if (!int.TryParse(EndCol, out _) && !ColumnRegex.IsMatch(EndCol?.ToUpperInvariant() ?? string.Empty))
                            error = "End column is in wrong format!";
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

        public List<object> GetProperties()
        {
            return new List<object> { FilePath, StartRow, EndRow, StartCol, EndCol, SheetIndex, IsTable };
        }

        public void SetProperties(List<object> props)
        {
            if (props == null) return;

            FilePath = props.ElementAtOrDefault(0) as string ?? "";
            StartRow = (int?)props.ElementAtOrDefault(1) ?? 0;
            EndRow = (int?)props.ElementAtOrDefault(2) ?? -1;


            if (props.ElementAtOrDefault(3) is int)
                StartCol = ((int?)props.ElementAtOrDefault(3) ?? 0).ToString();
            else
                StartCol = (string)props.ElementAtOrDefault(3) ?? "0";

            if (props.ElementAtOrDefault(4) is int)
                EndCol = ((int?)props.ElementAtOrDefault(4) ?? -1).ToString();
            else
                EndCol = (string)props.ElementAtOrDefault(4) ?? "-1";
            if (props.ElementAtOrDefault(5) is int)
                SheetIndex = ((int?)props.ElementAtOrDefault(5) ?? 0).ToString();
            else
                SheetIndex = (string)props.ElementAtOrDefault(5) ?? "0";

            IsTable = (bool?)props.ElementAtOrDefault(6) as bool? ?? true;
        }

        public void ShowProperties(System.Windows.Window owner)
        {
            // Consider implementing a custom properties window if needed.
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            ReloadInBackground();
        }
    }
}
