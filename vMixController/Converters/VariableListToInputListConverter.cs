using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using vMixAPI;
using vMixController.Classes;
using vMixController.ViewModel;

namespace vMixController.Converters
{
    /// <summary>
    /// Convert from list of variables into list of inputs
    /// </summary>
    [ValueConversion(typeof(Pair<string>), typeof(List<SampleInput>))]
    public class VariableListToInputListConverter : MarkupExtension, IValueConverter
    {

        private static IValueConverter _instance;

        /// <summary>
        /// Static instance of this converter.
        /// </summary>
        public static IValueConverter Instance => _instance ?? (_instance = new VariableListToInputListConverter());

        private const string VARIABLEPREFIX = "@";
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var vals = (ObservableCollection<Pair<string, string>>)value;
            List<SampleInput> result = new List<SampleInput>();

            foreach (var val in vals)
            {
                var v = (Pair<string, string>)val;

                if (!v.A.StartsWith(VARIABLEPREFIX)) continue;

                var state = ServiceLocator.Current.GetInstance<MainViewModel>().Model;
                if (state == null) break;

                SampleInput input = new SampleInput() { Key = v.A, Title = "[VAR] " + v.A, Number = -2, Elements = new System.Collections.ObjectModel.ObservableCollection<InputBase>() };

                var baseInput = state.Inputs.Where(x =>
                {
                    int inumber = -1;
                    return x.Key == v.B || x.Title == v.B || (int.TryParse(v.B, out inumber) && x.Number == inumber);
                }).FirstOrDefault();

                if (baseInput != null)
                {
                    foreach (var item in baseInput.Elements)
                    {
                        var element = ((InputBase)item.GetType().GetConstructor(new Type[] { }).Invoke(new object[] { }));
                        element.Name = item.Name;
                        element.Index = item.Index;

                        input.Elements.Add(element);
                    }
                }

                result.Add(input);
            }
            

            return result;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }
    }
}
