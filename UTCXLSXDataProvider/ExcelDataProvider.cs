using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace UTCExcelDataProvider
{
    public class ExcelDataProvider : DependencyObject, vMixControllerDataProvider.IvMixDataProviderTextInput, INotifyPropertyChanged, IDataErrorInfo
    {
        public object PreviewKeyUp { get; set; }

        public object GotFocus { get; set; }

        public object LostFocus { get; set; }

        public int Period { get; set; }

        public bool IsProvidingCustomProperties => false;

        private DateTime _lastModified = DateTime.MinValue;

        private string[] _cached = Array.Empty<string>();

        private bool _hasError = false;

        public string[] Values
        {
            get
            {
                _hasError = false;
                if (File.Exists(FilePath))
                {
                    var fileInfo = new FileInfo(FilePath);
                    if (fileInfo.LastWriteTimeUtc > _lastModified)
                    {
                        _lastModified = fileInfo.LastWriteTimeUtc;

                        using (var xls = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            try
                            {
                                var reader = ExcelDataReader.ExcelReaderFactory.CreateReader(xls);
                                List<string> results = new List<string>();
                                int row = 0;
                                int sheet = 0;
                                do
                                {
                                    if (sheet == SheetIndex)
                                        while (reader.Read())
                                        {
                                            if (row >= StartRow)
                                            {
                                                string line = "";
                                                for (int i = StartCol; i < (EndCol >= 0 ? Math.Min(reader.FieldCount, EndCol) : reader.FieldCount); i++)
                                                    if (IsTable)
                                                        line += "|" + (reader.GetValue(i)?.ToString() ?? "");
                                                    else
                                                        results.Add((reader.GetValue(i)?.ToString() ?? ""));
                                                if (IsTable)
                                                    results.Add(line.Substring(1));
                                            }
                                            row++;
                                            if (EndRow >= 0 && row >= EndRow)
                                                break;
                                        }
                                    sheet++;
                                }
                                while (reader.NextResult());
                                _cached = results.ToArray();
                                RowsCount = _cached.Length;
                                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(FilePathPropertyName));
                                return _cached;
                            }
                            catch (ExcelDataReader.Exceptions.ExcelReaderException)
                            {
                                _hasError = true;
                                RowsCount = 0;
                                return Array.Empty<string>();
                            }
                        }
                    }
                    else
                        return _cached;

                }
                _hasError = true;
                RowsCount = 0;
                return Array.Empty<string>();
            }
        }

        public System.Windows.UIElement CustomUI => new OnWidgetUI() { DataContext = this };


        /// <summary>
        /// The <see cref="FilePath" /> property's name.
        /// </summary>
        public const string FilePathPropertyName = "FilePath";

        private string _filePath = "";

        /// <summary>
        /// Sets and gets the FilePath property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string FilePath
        {
            get
            {
                return _filePath;
            }

            set
            {
                if (_filePath == value)
                {
                    return;
                }

                _filePath = value;
                _lastModified = DateTime.MinValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(FilePathPropertyName));
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
            return new List<object> { FilePath, StartRow, EndRow, StartCol, EndCol, SheetIndex, IsTable };
            //throw new NotImplementedException();
        }

        public void SetProperties(List<object> props)
        {
            FilePath = (string)props?.ElementAt(0) ?? "";
            StartRow = (int?)props?.ElementAt(1) ?? 0;
            EndRow = (int?)props?.ElementAt(2) ?? -1;
            StartCol = (int?)props?.ElementAt(3) ?? 0;
            EndCol = (int?)props?.ElementAt(4) ?? -1;
            SheetIndex = (int?)props?.ElementAt(5) ?? 0;
            IsTable = (bool?)props?.ElementAt(6) ?? true;
            //throw new NotImplementedException();
        }

        public void ShowProperties(System.Windows.Window owner)
        {
            //throw new NotImplementedException();
        }
    }
}
