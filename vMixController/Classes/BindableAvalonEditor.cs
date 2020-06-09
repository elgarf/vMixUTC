using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using vMixController.ViewModel;

namespace vMixController.Classes
{
    /// Implements AvalonEdit ICompletionData interface to provide the entries in the
    /// completion drop down.
    public class MyCompletionData : ICompletionData
    {

        private string _imagePath = @"pack://application:,,,/vMixController;component/Images/Function.png";

        public MyCompletionData(string text, string description, string image = @"pack://application:,,,/vMixController;component/Images/Function.png")
        {
            this.Text = text;
            this.RealText = text;
            this.Description = description;
            this.Length = 0;
            _imagePath = image;
        }

        public MyCompletionData(string realtext, string text, string description, string image = @"pack://application:,,,/vMixController;component/Images/Function.png") : this(text, description, image)
        {
            this.RealText = realtext;
        }


        public System.Windows.Media.ImageSource Image
        {
            get
            {

                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(_imagePath, UriKind.Absolute);
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();

                return src;
            }
        }

        public string Text { get; private set; }

        public string RealText { get; set; }

        public int Length { get; set; }

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content
        {
            get
            {
                return this.Text;
            }
        }

        public object Description
        {
            get; set;
        }

        public double Priority => 1.0;

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.RealText);
            textArea.Caret.Location = new TextLocation(textArea.Caret.Location.Line, textArea.Caret.Location.Column - Length);
        }
    }

    public class BindableAvalonEditor : ICSharpCode.AvalonEdit.TextEditor, INotifyPropertyChanged
    {
        CompletionWindow completionWindow;

        public BindableAvalonEditor() : base()
        {

            if (ViewModelBase.IsInDesignModeStatic) return;

            this.TextArea.KeyDown += TextArea_KeyDown;
            this.TextArea.PreviewKeyDown += TextArea_PreviewKeyDown;

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

        private void TextArea_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var area = sender as TextArea;
            if (e.Key == System.Windows.Input.Key.Space && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
            {
                // Open code completion after the user has pressed dot:
                int start = area.Caret.Offset;
                int end = area.Caret.Offset;
                while (start > 0 && char.IsLetter(area.Document.GetCharAt(start - 1)))
                    start--;

                area.Caret.Offset = start;
                completionWindow = new CompletionWindow(area);
                completionWindow.EndOffset = end;

                //completionWindow.CompletionList.IsFiltering = false;
                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                var Main = SimpleIoc.Default.GetInstance<MainViewModel>();
                foreach (var item in Main.Functions.Where(x => !x.IsGroup))
                {
                    var sig = item.GetSignature();
                    data.Add(new MyCompletionData(sig, item.Function, item.Description, @"pack://application:,,,/vMixController;component/Images/Function.png") { Length = sig.Length - item.Function.Length - 1 });
                }
                if (Main.Model != null)
                    foreach (var item in Main.Model.Inputs)
                    {
                        data.Add(new MyCompletionData(item.Key, string.Format("{1} [{0}]", item.Number, item.Title), item.Title, @"pack://application:,,,/vMixController;component/Images/Input.png"));
                    }
                completionWindow.Show();
                area.Caret.Offset = end;
                completionWindow.Closed += delegate
                {
                    completionWindow = null;
                };
                e.Handled = true;

            }
            if (e.Key == Key.Tab)
            {
                int loopCount = 0;
                int caret = area.Caret.Offset;
                int hash = -1;
                char c = '\0';
                int mul = 1;
                if (area.Selection.IsEmpty || !area.Selection.GetText().StartsWith("#"))
                    mul = -1;
                if (area.Selection.GetText().StartsWith("#"))
                    caret += 2;
                while (loopCount <= 1)
                {
                    bool brk = false;
                    while (caret > 0 && caret < area.Document.TextLength && (c = area.Document.GetCharAt(caret - 1)) != '\r' && c != '\n' && (c != '(' || loopCount > 0))
                    {
                        if (c == '#')
                        {
                            hash = caret - 1;
                            brk = true;
                            break;
                        }
                        if (c == ',' && mul == -1)
                            mul *= -1;
                        caret += mul;
                    }
                    if (c == '(')
                        mul *= -1;

                    if (brk) break;
                    //mul *= -1;
                    caret = mul == 1 ? area.Document.GetLineByOffset(caret).Offset + 1 : area.Document.TextLength - 1;
                    loopCount++;
                }


                if (hash >= 0)
                {
                    caret = hash + 2;
                    while (caret < area.Document.TextLength && char.IsLetterOrDigit(c = area.Document.GetCharAt(caret - 1)))
                        caret++;

                    area.Caret.Offset = hash;
                    var startp = area.Document.GetLocation(hash);
                    var endp = area.Document.GetLocation(caret - 1);
                    area.Selection = new RectangleSelection(area, new TextViewPosition(startp.Line, startp.Column), new TextViewPosition(endp.Line, endp.Column));

                    e.Handled = true;
                }
            }
        }

        private void TextArea_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }



        /// <summary>
        /// A bindable Text property
        /// </summary>
        /*public new string Text
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
        }*/

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
