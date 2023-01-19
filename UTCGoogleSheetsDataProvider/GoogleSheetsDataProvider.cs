using GalaSoft.MvvmLight.CommandWpf;
using GoogleSheetsDataProvider;
using Popcron.Sheets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Windows.Threading;
using vMixControllerSkin;

namespace UTCGoogleSheetsDataProvider
{
    public class GoogleSheetsDataProvider : DependencyObject, vMixControllerDataProvider.IvMixDataProviderTextInput, INotifyPropertyChanged, IDataErrorInfo
    {
        /*private struct SheetDataArg
        {
            public string SheetKey;
            public string APIKey;
            public int Period;
            public int SheetIndex;
            public int StartRow;
            public int EndRow;
            public int StartCol;
            public int EndCol;
            public bool IsTable;
        }*/

        static ConcurrentDictionary<string, Authorization> _authCache = new ConcurrentDictionary<string, Authorization>();
        static ConcurrentDictionary<string, Spreadsheet> _spreadsheetCache = new ConcurrentDictionary<string, Spreadsheet>();
        static ConcurrentDictionary<string, int> _sheetDownloading = new ConcurrentDictionary<string, int>();

        //static Dictionary<string, object> _lockers = new Dictionary<string, object>();
        static object _lock = new object();

        public object PreviewKeyUp { get; set; }

        public object GotFocus { get; set; }

        public object LostFocus { get; set; }

        public int Period
        {
            get => _period;
            set
            {
                _period = value;
                _webTimer.Interval = TimeSpan.FromMilliseconds(_period);
            }
        }

        public bool IsProvidingCustomProperties => false;

        private DateTime _lastModified = DateTime.MinValue;

        private string[] _cached = Array.Empty<string>();

        private bool _hasError = false;

        DispatcherTimer _webTimer = new DispatcherTimer();

        DateTime _lastRequest = DateTime.MinValue;

        List<string> results = new List<string>();

        public string[] Values
        {
            get
            {
                var p = (int)((DateTime.Now - _lastRequest).TotalMilliseconds);
                Period = p < 100 ? 100 : (p > 36000 ? 36000 : p);
                _lastRequest = DateTime.Now;
                string[] result = Array.Empty<string>();
                results.Clear();
                Spreadsheet sst = null;
                StringBuilder line = new StringBuilder();
                if (_spreadsheetCache.ContainsKey(SheetKey))
                {
                    sst = _spreadsheetCache[SheetKey];
                    if (sst.Sheets.Count > SheetIndex)
                    {
                        var sheet = sst.Sheets[SheetIndex];
                        for (int y = 0; y < sheet.Rows; y++)
                        {
                            line.Clear();
                            for (int x = 0; x < sheet.Columns; x++)
                            {
                                if (y >= StartRow && (y <= EndRow || EndRow < 0))
                                    if (x >= StartCol && (x <= EndCol || EndCol < 0))
                                    {
                                        var val = sheet.Data[x, y].Value;
                                        val = string.IsNullOrEmpty(val) ? "" : val;
                                        if (IsTable)
                                        {
                                            line.Append("|");
                                            line.Append(val);
                                        }
                                        else
                                            results.Add(val);
                                    }
                            }
                            if (line.Length != 0)
                            {
                                line.Remove(0, 1);
                                results.Add(String.Intern(line.ToString()));
                            }
                        }
                        try
                        {
                            Cached = results.ToArray();
                            RowsCount = Cached.Length;
                            sheet = null;
                        } catch {
                            Cached = Array.Empty<string>();
                            RowsCount = 0;
                        }
                        return Cached;
                    }
                }
                return result;
            }
        }


        System.Windows.UIElement _customUI;

        public System.Windows.UIElement CustomUI { get { return _customUI; } }

        public GoogleSheetsDataProvider()
        {
            try
            {
                _customUI = new OnWidgetUI() { DataContext = this };
            }
            catch (Exception e)
            {
                _customUI = new TextBox() { Text = e.ToString(), AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 256, FontWeight = FontWeights.Normal, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            }

            _webTimer.Interval = TimeSpan.FromMilliseconds(1000);
            _webTimer.Start();
            _webTimer.Tick += _webTimer_Tick;

        }

        private void _webTimer_Tick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(APIKey) || string.IsNullOrWhiteSpace(SheetKey))
                return;
            int downloadings = 0;
            _sheetDownloading.TryGetValue(SheetKey, out downloadings);
            if (!_sheetDownloading.ContainsKey(SheetKey) || downloadings <= 0)

                ThreadPool.QueueUserWorkItem(async (state) =>
                {
                    if (!_sheetDownloading.ContainsKey(SheetKey))
                        _sheetDownloading.TryAdd(SheetKey, 1);
                    else
                    {
                        int d = 0;
                        _sheetDownloading.TryGetValue(SheetKey, out d);
                        if (d > 0) return;
                        _sheetDownloading.TryUpdate(SheetKey, d + 1, d);
                    }

                    Debug.WriteLine("Loading {0} at {1}", SheetKey, DateTime.Now);
                    //lock (_lock)
                    {
                        SheetsSerializer.Serializer = new JsonSheetsSerializer();
                        Authorization auth = null;
                        if (!_authCache.ContainsKey(APIKey))
                        {
                            try
                            {
                                auth = await Popcron.Sheets.Authorization.Authorize(APIKey);
                                _authCache.TryAdd(APIKey, auth);
                            }
                            catch (Exception)
                            {
                                return;
                            }
                        }
                        else
                            auth = _authCache[APIKey];

                        List<string> results = new List<string>();

                        try
                        {
                            int d = 0;
                            _sheetDownloading.TryGetValue(SheetKey, out d);
                            if (d > 1) return;
                            Spreadsheet sst = null;
                            if ((DateTime.Now - _lastModified).TotalMilliseconds > Period)
                            {
                                sst = await Popcron.Sheets.Spreadsheet.Get(SheetKey, auth);

                                if (!_spreadsheetCache.ContainsKey(SheetKey))
                                    _spreadsheetCache.TryAdd(SheetKey, sst);
                                else
                                {
                                    Spreadsheet previous = null;
                                    _spreadsheetCache.TryGetValue(SheetKey, out previous);
                                    _spreadsheetCache.TryUpdate(SheetKey, sst, previous);
                                    previous = null;
                                }
                                _lastModified = DateTime.Now;

                            }
                            else
                                sst = _spreadsheetCache[SheetKey];
                            _sheetDownloading.TryUpdate(SheetKey, d - 1, d);

                            sst = null;
                            GC.Collect();
                            return;

                        }
                        catch (Exception)
                        {
                            int d = 0;
                            _sheetDownloading.TryGetValue(SheetKey, out d);
                            _sheetDownloading.TryUpdate(SheetKey, d - 1, d);
                            return;
                        }
                    }
                });

        }



        /// <summary>
        /// The <see cref="APIKey" /> property's name.
        /// </summary>
        public const string APIKeyPropertyName = "APIKey";

        private string _APIKey = "";

        /// <summary>
        /// Sets and gets the FilePath property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string APIKey
        {
            get
            {
                return _APIKey;
            }

            set
            {
                if (_APIKey == value)
                {
                    return;
                }

                _APIKey = value;
                _lastModified = DateTime.MinValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(APIKeyPropertyName));
            }
        }


        /// <summary>
        /// The <see cref="SheetKey" /> property's name.
        /// </summary>
        public const string SheetKeyPropertyName = "SheetKey";

        private string _sheetKey = "";

        /// <summary>
        /// Sets and gets the SheetKey property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SheetKey
        {
            get
            {
                return _sheetKey;
            }

            set
            {
                if (_sheetKey == value)
                {
                    return;
                }

                _sheetKey = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(SheetKeyPropertyName));
            }
        }

        /// <summary>
        /// The <see cref="StartRow" /> property's name.
        /// </summary>
        public const string StartRowPropertyName = "StartRow";

        private int _startRow = 0;

        /// <summary>
        /// Sets and gets the StartRow property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int StartRow
        {
            get
            {
                return _startRow;
            }

            set
            {
                if (_startRow == value)
                {
                    return;
                }

                _startRow = value;
                _lastModified = DateTime.MinValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(StartRowPropertyName));
            }
        }

        /// <summary>
        /// The <see cref="EndRow" /> property's name.
        /// </summary>
        public const string EndRowPropertyName = "EndRow";

        private int _endRow = -1;

        /// <summary>
        /// Sets and gets the EndRow property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int EndRow
        {
            get
            {
                return _endRow;
            }

            set
            {
                if (_endRow == value)
                {
                    return;
                }

                _endRow = value;
                _lastModified = DateTime.MinValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(EndRowPropertyName));
            }
        }


        /// <summary>
        /// The <see cref="StartCol" /> property's name.
        /// </summary>
        public const string StartColPropertyName = "StartCol";

        private int _startCol = 0;

        /// <summary>
        /// Sets and gets the StartCol property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int StartCol
        {
            get
            {
                return _startCol;
            }

            set
            {
                if (_startCol == value)
                {
                    return;
                }

                _startCol = value;
                _lastModified = DateTime.MinValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(StartColPropertyName));
            }
        }

        /// <summary>
        /// The <see cref="EndCol" /> property's name.
        /// </summary>
        public const string EndColPropertyName = "EndCol";

        private int _endCol = -1;

        /// <summary>
        /// Sets and gets the EndCol property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int EndCol
        {
            get
            {
                return _endCol;
            }

            set
            {
                if (_endCol == value)
                {
                    return;
                }

                _endCol = value;
                _lastModified = DateTime.MinValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(EndColPropertyName));
            }
        }

        /// <summary>
        /// The <see cref="SheetIndex" /> property's name.
        /// </summary>
        public const string SheetIndexPropertyName = "SheetIndex";

        private int _sheet = 0;

        /// <summary>
        /// Sets and gets the SheetIndex property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int SheetIndex
        {
            get
            {
                return _sheet;
            }

            set
            {
                if (_sheet == value)
                {
                    return;
                }

                _sheet = value;
                _lastModified = DateTime.MinValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(SheetIndexPropertyName));
            }
        }

        /// <summary>
        /// The <see cref="IsTable" /> property's name.
        /// </summary>
        public const string IsTablePropertyName = "IsTable";

        private bool _isTable = true;

        /// <summary>
        /// Sets and gets the IsTable property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsTable
        {
            get
            {
                return _isTable;
            }

            set
            {
                if (_isTable == value)
                {
                    return;
                }

                _isTable = value;
                _lastModified = DateTime.MinValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IsTablePropertyName));
            }
        }

        /// <summary>
        /// The <see cref="RowsCount" /> property's name.
        /// </summary>
        public const string RowsCountPropertyName = "RowsCount";

        private int _rowsCount = 0;

        /// <summary>
        /// Sets and gets the RowsCount property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int RowsCount
        {
            get
            {
                return _rowsCount;
            }

            set
            {
                if (_rowsCount == value)
                {
                    return;
                }

                _rowsCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(RowsCountPropertyName));
            }
        }

        public string Error => throw new NotImplementedException();


        private RelayCommand _showRowsCommand;
        private int _period;

        /// <summary>
        /// Gets the ShowRowsCommand.
        /// </summary>
        public RelayCommand ShowRowsCommand
        {
            get
            {
                return _showRowsCommand
                    ?? (_showRowsCommand = new RelayCommand(
                    () =>
                    {
                        new RowsViewer().Bind(this, "Cached");
                    }));
            }
        }

        public string[] Cached
        {
            get => _cached;
            set
            {
                _cached = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Cached)));
            }
        }

        public string this[string columnName]
        {
            get
            {
                string error = string.Empty;
                switch (columnName)
                {
                    case "FilePath":
                        if (_hasError)
                            error = "File not found or is not excel file!";
                        break;
                }
                return error;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public List<object> GetProperties()
        {
            return new List<object> { APIKey, StartRow, EndRow, StartCol, EndCol, SheetIndex, IsTable, SheetKey };
            //throw new NotImplementedException();
        }

        public void SetProperties(List<object> props)
        {
            APIKey = (string)props?.ElementAt(0) ?? "";
            StartRow = (int?)props?.ElementAt(1) ?? 0;
            EndRow = (int?)props?.ElementAt(2) ?? -1;
            StartCol = (int?)props?.ElementAt(3) ?? 0;
            EndCol = (int?)props?.ElementAt(4) ?? -1;
            SheetIndex = (int?)props?.ElementAt(5) ?? 0;
            IsTable = (bool?)props?.ElementAt(6) ?? true;
            SheetKey = (string)props?.ElementAt(7) ?? "";
            //throw new NotImplementedException();
        }

        public void ShowProperties(System.Windows.Window owner)
        {
            //throw new NotImplementedException();
        }
    }
}
