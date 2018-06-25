using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using vMixControllerDataProvider;
using System.Windows;

namespace vMixWeatherExternalDataProvider
{
    public class WeatherProvider : DependencyObject, IvMixDataProvider
    {
        List<string> data = new List<string>();
        bool _retrivingData = false;

        public string[] Values
        {
            get
            {

                
                WebRequest req = WebRequest.Create(string.Format("http://export.yandex.ru/weather-ng/forecasts/{0}.xml", _city));
                if (!_retrivingData)
                    req.BeginGetResponse(new AsyncCallback(BeginGetResponseCallback), req);

                return data.ToArray();
            }
        }

        private void BeginGetResponseCallback(IAsyncResult ar)
        {

            _retrivingData = true;
            data.Clear();
            try
            {
                XmlDocument doc = new XmlDocument();

                doc.Load((ar.AsyncState as WebRequest).EndGetResponse(ar).GetResponseStream());
                XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
                ns.AddNamespace("yandex", "http://weather.yandex.ru/forecast");

                var nodes = doc.SelectNodes("//yandex:day_part[@typeid=5]", ns);
                foreach (var day in nodes.OfType<XmlElement>())
                {
                    data.Add(day.SelectSingleNode("yandex:temperature", ns).InnerText);
                    data.Add(day.SelectSingleNode("yandex:weather_type", ns).InnerText);
                    data.Add(string.Format("http://yandex.st/weather/1.1.78/i/icons/48x48/{0}.png", day.SelectSingleNode("yandex:image-v3", ns).InnerText));
                }
            }
            catch (Exception)
            {

            }
            _retrivingData = false;

        }

        public string Country
        {
            get { return (string)GetValue(CountryProperty); }
            set { SetValue(CountryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Country.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CountryProperty =
            DependencyProperty.Register("Country", typeof(string), typeof(WeatherProvider), new PropertyMetadata("Россия"));


        internal string _city = "37099";
        public string City
        {
            get { return (string)GetValue(CityProperty); }
            set { SetValue(CityProperty, value); }
        }

        UIElement _customUI;

        public UIElement CustomUI
        {
            get
            {
                return _customUI??(_customUI = new OnWidgetUI() { Provider = this });
            }
        }

        public bool IsProvidingCustomProperties
        {
            get
            {
                return false;
            }
        }

        public int Period
        {
            get;

            set;
        }

        // Using a DependencyProperty as the backing store for City.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CityProperty =
            DependencyProperty.Register("City", typeof(string), typeof(WeatherProvider), new PropertyMetadata("37099", CityChanged));

        private static void CityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as WeatherProvider)._city = (string)e.NewValue;
        }

        public List<object> GetProperties()
        {
            return new List<object>() { Country, City };
        }

        public void SetProperties(List<object> props)
        {
            if (props != null && props.Count == 2)
            {
                Country = (string)props[0];
                City = (string)props[1];
            }
        }



        public void ShowProperties(System.Windows.Window owner)
        {
            PropertiesWindow _properties = new PropertiesWindow();
            _properties.Owner = owner;
            _properties.Provider = this;
            var previous = GetProperties();
            var result = _properties.ShowDialog();
            if (!(result.HasValue && result.Value))
                SetProperties(previous);
        }
    }
}
