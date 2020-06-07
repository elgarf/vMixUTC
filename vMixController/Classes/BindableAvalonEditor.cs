using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using vMixController.ViewModel;

namespace vMixController.Classes
{
    /*public class ControllerHighlighter : IHighlightingDefinition
    {
        public string Name => "vMix Controller";

        public HighlightingRuleSet MainRuleSet => new HighlightingRuleSet();

        public IEnumerable<HighlightingColor> NamedHighlightingColors => new List<HighlightingColor>();

        public IDictionary<string, string> Properties => new Dictionary<string, string>();

        public HighlightingColor GetNamedColor(string name)
        {
            throw new NotImplementedException();
        }

        public HighlightingRuleSet GetNamedRuleSet(string name)
        {
            throw new NotImplementedException();
        }

        public ControllerHighlighter()
        {
            
            var r = new Regex(regex.Trim('|'));
            Debug.WriteLine(r.Match("None"));

            var keyw = new HighlightingColor() { Name = "keywords", Foreground = new SimpleHighlightingBrush(Colors.Navy), FontWeight = FontWeights.Bold };
            ((List<HighlightingColor>)NamedHighlightingColors).Add(keyw);
            MainRuleSet.Rules.Add(new HighlightingRule()
            {
                Regex = r,
                Color = keyw

            });
        }
    }*/

    public class BindableAvalonEditor : ICSharpCode.AvalonEdit.TextEditor, INotifyPropertyChanged
    {
        public BindableAvalonEditor() : base()
        {

            if (ViewModelBase.IsInDesignModeStatic) return;

            XshdSyntaxDefinition xshd;
            using (XmlTextReader reader = new XmlTextReader("ControllerHighlight.xml"))
            {
                xshd = HighlightingLoader.LoadXshd(reader);
            }
            var functions = SimpleIoc.Default.GetInstance<MainViewModel>().Functions;
            var regex = @"\b(";
            foreach (var item in functions)
            {
                if (!item.IsGroup)
                    regex += item.Function + "|";
            }
            regex = regex.Trim('|') + @")\b";
            var def = HighlightingLoader.Load(xshd, null);
            def.MainRuleSet.Rules[0].Regex = new Regex(regex);
            SyntaxHighlighting = def;
        }
        /// <summary>
        /// A bindable Text property
        /// </summary>
        public new string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
                RaisePropertyChanged("Text");
            }
        }

        /// <summary>
        /// The bindable text property dependency property
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(BindableAvalonEditor),
                new FrameworkPropertyMetadata
                {
                    DefaultValue = default(string),
                    BindsTwoWayByDefault = true,
                    PropertyChangedCallback = OnDependencyPropertyChanged
                }
            );

        protected static void OnDependencyPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var target = (BindableAvalonEditor)obj;

            if (target.Document != null)
            {
                var caretOffset = target.CaretOffset;
                var newValue = args.NewValue;

                if (newValue == null)
                {
                    newValue = "";
                }

                target.Document.Text = (string)newValue;
                target.CaretOffset = Math.Min(caretOffset, newValue.ToString().Length);
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            if (this.Document != null)
            {
                Text = this.Document.Text;
            }

            base.OnTextChanged(e);
        }

        /// <summary>
        /// Raises a property changed event
        /// </summary>
        /// <param name="property">The name of the property that updates</param>
        public void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
