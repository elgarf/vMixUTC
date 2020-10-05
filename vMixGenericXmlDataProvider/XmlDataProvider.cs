using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;
using vMixControllerDataProvider;
using vMixControllerSkin;

namespace XmlDataProviderNs
{
    public class XmlDataProvider : DependencyObject, IvMixDataProviderTextInput, INotifyPropertyChanged
    {
        //Internal variables for caching xml results
        private static int _maxid = 0;
        private int _id = 0;

        private static Dictionary<string, CacheStats> _cahce = new Dictionary<string, CacheStats>();
        private string _url;
        private string _xpath;
        private string _namespaces;
        private int _groupBy;

        public System.Windows.UIElement CustomUI { get; }

        public bool IsProvidingCustomProperties
        {
            get
            {
                return true;
            }
        }

        List<string> _data = new List<string>();
        bool _retrivingData = false;

        /// <summary>
        /// Returns cached or new XmlData
        /// </summary>
        public string[] Values
        {
            get
            {

                try
                {
                    if (_cahce.ContainsKey(_url ?? "") && (DateTime.Now - _cahce[_url ?? ""].LastUpdated).TotalMilliseconds < Period)
                    {
                        UpdateData(_cahce[_url].Document);
                    }
                    else
                    {
                        Uri uri = null;
                        if (Uri.TryCreate(_url, UriKind.Absolute, out uri))
                        {
                            WebRequest req = WebRequest.Create(uri);
                            if (!_retrivingData)
                                req.BeginGetResponse(new AsyncCallback(BeginGetResponseCallback), req);
                        }
                    }
                }
                catch (Exception)
                {

                }
                return _data.ToArray();
            }
        }

        /// <summary>
        /// Retrieving data callback
        /// </summary>
        /// <param name="ar"></param>
        private void BeginGetResponseCallback(IAsyncResult ar)
        {

            _retrivingData = true;
            _data.Clear();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load((ar.AsyncState as WebRequest).EndGetResponse(ar).GetResponseStream());
                if (_cahce.ContainsKey(_url))
                {
                    _cahce[_url].Document = doc;
                    _cahce[_url].LastId = _id;
                    _cahce[_url].LastUpdated = DateTime.Now;
                }
                else
                    _cahce.Add(_url, new CacheStats() { Document = doc, LastId = _id, LastUpdated = DateTime.Now });

                UpdateData(doc);

            }
            catch (Exception)
            {

            }
            _retrivingData = false;

        }

        /// <summary>
        /// Updating data values from selected XmlDocument nodes.
        /// </summary>
        /// <param name="doc"></param>
        private void UpdateData(XmlDocument doc)
        {
            //Adding namespaces
            XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
            foreach (var item in _namespaces.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()))
            {
                try
                {
                    var nsn = item.Split(' ');
                    ns.AddNamespace(nsn[0], nsn[1]);
                }
                catch (Exception) { }
            }
            //Selecting nodes
            var nodes = doc.SelectNodes(_xpath, ns);
            //We need only inner text property value or attribute value
            var _data = nodes.OfType<XmlNode>().Select(x =>
            {
                switch (x)
                {
                    case XmlElement element:
                        return element.InnerText;
                    case XmlAttribute attr:
                        return attr.Value;
                    case null:
                    default:
                        return "";
                }
            }).ToList();

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

        //Url property of data provider
        public string Url
        {
            get { return (string)GetValue(UrlProperty); }
            set { SetValue(UrlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Url.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UrlProperty =
            DependencyProperty.Register("Url", typeof(string), typeof(XmlDataProvider), new PropertyMetadata("", propchanged));

        //Namespaces property of data provider
        public string NameSpaces
        {
            get { return (string)GetValue(NameSpacesProperty); }
            set { SetValue(NameSpacesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NameSpaces.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameSpacesProperty =
            DependencyProperty.Register("NameSpaces", typeof(string), typeof(XmlDataProvider), new PropertyMetadata("", propchanged));



        public int GroupBy
        {
            get { return (int)GetValue(GroupByProperty); }
            set { SetValue(GroupByProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GroupBy.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GroupByProperty =
            DependencyProperty.Register("GroupBy", typeof(int), typeof(XmlDataProvider), new PropertyMetadata(1, propchanged));




        //Updating private variables on dependency property changed
        private static void propchanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Url")
                (d as XmlDataProvider)._url = (string)e.NewValue;
            if (e.Property.Name == "XPath")
                (d as XmlDataProvider)._xpath = (string)e.NewValue;
            if (e.Property.Name == "NameSpaces")
                (d as XmlDataProvider)._namespaces = (string)e.NewValue;
            if (e.Property.Name == "GroupBy")
                (d as XmlDataProvider)._groupBy = (int)e.NewValue;
        }

        //XPath property of data provider
        public string XPath
        {
            get { return (string)GetValue(XPathProperty); }
            set { SetValue(XPathProperty, value); }
        }

        public int Period
        {
            get;

            set;
        }
        //Internal data values
        public List<string> Data
        {
            get
            {
                return _data;
            }

            set
            {
                _data = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Data"));
            }
        }

        public object PreviewKeyUp { get; set; }
        public object GotFocus { get; set; }
        public object LostFocus { get; set; }

        // Using a DependencyProperty as the backing store for XPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty XPathProperty =
            DependencyProperty.Register("XPath", typeof(string), typeof(XmlDataProvider), new PropertyMetadata("", propchanged));

        public event PropertyChangedEventHandler PropertyChanged;


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

        /// <summary>
        /// Provide data provider properties.
        /// </summary>
        /// <returns>Url, XPath and NameSpaces for saving into file</returns>
        public List<object> GetProperties()
        {
            return new List<object>() { Url, XPath, NameSpaces, GroupBy };
        }

        /// <summary>
        /// Recovers properties from properties list
        /// </summary>
        /// <param name="props"></param>
        public void SetProperties(List<object> props)
        {
            if (props == null || props.Count == 0)
                return;
            Url = (string)props[0];
            XPath = (string)props[1];
            if (props.Count > 2)
                NameSpaces = (string)props[2];
            if (props.Count > 3)
                GroupBy = (int)props[3];
        }

        /// <summary>
        /// Shows custom properties window
        /// </summary>
        /// <param name="owner"></param>
        public void ShowProperties(System.Windows.Window owner)
        {
            PropertiesWindow _properties = new PropertiesWindow
            {
                Owner = owner,
                DataContext = this
            };
            var previous = GetProperties();
            var result = _properties.ShowDialog();
            if (!(result.HasValue && result.Value))
                SetProperties(previous);
        }

        public XmlDataProvider()
        {
            _id = _maxid++;
            try
            {
                CustomUI = new OnWidgetUI() { DataContext = this };
            }
            catch (Exception e)
            {
                CustomUI = new TextBox() { Text = e.ToString(), AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 256, FontWeight = FontWeights.Normal, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            }
            
            _namespaces = "";

        }

    }
}
