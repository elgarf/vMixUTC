using GalaSoft.MvvmLight.CommandWpf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using vMixControllerDataProvider;
using vMixControllerSkin;

namespace JsonDataProviderNs
{
    public class JsonDataProvider : IvMixDataProviderTextInput, INotifyPropertyChanged
    {

        JToken _document;
        DateTime _previousQuery;

        List<string> _data = new List<string>();
        bool _retrivingData = false;

        string _url = "";
        string _jsonPath = "";
        int _groupBy = 1;
        UIElement _ui;

        public event PropertyChangedEventHandler PropertyChanged;

        public object PreviewKeyUp { get; set; }
        public object GotFocus { get; set; }
        public object LostFocus { get; set; }
        public int Period { get; set; }

        private RelayCommand<KeyEventArgs> _previewKeyUpCommand;

        /// <summary>
        /// Gets the PreviewKeyUpCommand.
        /// </summary>
        public RelayCommand<KeyEventArgs> PreviewKeyUpCommand
        {
            get
            {
                return _previewKeyUpCommand
                    ?? (_previewKeyUpCommand = new RelayCommand<KeyEventArgs>(
                    p =>
                    {
                        if (!(p.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) && p.Key == Key.Return))
                            ((RelayCommand<KeyEventArgs>)PreviewKeyUp).Execute(p);
                    }));
            }
        }

        private RelayCommand<KeyEventArgs> _previewKeyDown;

        /// <summary>
        /// Gets the MyCommand.
        /// </summary>
        public RelayCommand<KeyEventArgs> PreviewKeyDownCommand
        {
            get
            {
                return _previewKeyDown
                    ?? (_previewKeyDown = new RelayCommand<KeyEventArgs>(
                    p =>
                    {
                        if (p.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) && p.Key == Key.Return)
                        {
                            p.Handled = true;
                            TextBox sender = (TextBox)p.Source;
                            Int32 lastLocation = sender.SelectionStart;
                            sender.Text = sender.Text.Insert(lastLocation, Environment.NewLine);
                            sender.SelectionStart = lastLocation + Environment.NewLine.Length;
                        }
                        else if (p.Key == Key.Return)
                            p.Handled = true;

                    }));
            }
        }

        public bool IsProvidingCustomProperties => false;

        public string[] Values
        {
            get
            {
                try
                {

                    if ((DateTime.Now - _previousQuery).TotalMilliseconds >= Period)
                    {
                        Uri uri = null;
                        if (Uri.TryCreate(Url, UriKind.Absolute, out uri))
                        {
                            WebRequest req = WebRequest.Create(uri);
                            if (!_retrivingData)
                                req.BeginGetResponse(new AsyncCallback(BeginGetResponseCallback), req);
                            _retrivingData = true;
                        }
                    }
                    else
                        UpdateData();
                }
                catch (Exception)
                {

                }
                return Data.ToArray();
            }
        }

        private void BeginGetResponseCallback(IAsyncResult ar)
        {

            _retrivingData = true;
            try
            {
                using (var stream = (ar.AsyncState as WebRequest).EndGetResponse(ar).GetResponseStream())
                using (StreamReader sr = new StreamReader(stream))
                {
                    if (_document != null)
                    {
                        _document = null;
                        GC.Collect();
                    }
                    var text = sr.ReadToEnd();
                    _document = JToken.Parse(text);
                    UpdateData();
                }
            }
            catch (Exception) {

            }
            _previousQuery = DateTime.Now;
            _retrivingData = false;

        }

        void UpdateData()
        {
            if (_document != null)
            {
                _data = _document.SelectTokens(JsonPath.Replace("\r", "").Replace("\n", "")).Select(x => x.ToString()).ToList();
                if (_groupBy > 1)
                {
                    List<string> groupedData = new List<string>();
                    string grouped = "";
                    for (int i = 0; i < _data.Count; i++)
                    {
                        if (i % _groupBy == 0)
                        {
                            if (!string.IsNullOrWhiteSpace(grouped))
                                groupedData.Add(grouped.TrimEnd('|'));
                            grouped = "";
                        }
                        grouped += _data[i] + "|";
                    }
                    if (!string.IsNullOrWhiteSpace(grouped))
                        groupedData.Add(grouped.TrimEnd('|'));
                    Data = groupedData;
                }
                else
                    Data = _data;
            }
        }
        
        public UIElement CustomUI => _ui;

        public string Url
        {
            get => _url;
            set
            {
                _url = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Url)));
            }
        }

        public string JsonPath
        {
            get => _jsonPath;
            set
            {
                _jsonPath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JsonPath)));
            }
        }

        public List<string> Data
        {
            get => _data;
            set
            {
                _data = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Data)));
            }
        }

        public int GroupBy
        {
            get => _groupBy;
            set
            {
                _groupBy = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GroupBy)));
            }
        }

        public List<object> GetProperties()
        {
            return new List<object>() { Url, JsonPath, GroupBy };
        }

        public void SetProperties(List<object> props)
        {
            Url = (string)(props?.ElementAt(0) ?? "");
            JsonPath = (string)(props?.ElementAt(1) ?? "");
            GroupBy = (int)(props?.ElementAt(2) ?? 1);
        }

        private RelayCommand _showRowsCommand;

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
                        new RowsViewer().Bind(this, "Data");
                    }));
            }
        }

        public JsonDataProvider()
        {
            try
            {
                _ui = new OnWidgetUI() { DataContext = this };
            }
            catch (Exception e)
            {
                _ui = new TextBox() { Text = e.ToString(), AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 256, FontWeight = FontWeights.Normal, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            }
        }

        public void ShowProperties(Window owner)
        {
            throw new NotImplementedException();
        }
    }
}
