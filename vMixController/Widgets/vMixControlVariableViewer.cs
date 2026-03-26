using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using vMixController.Classes;
using vMixController.ViewModel;

namespace vMixController.Widgets
{
    public class vMixControlVariableViewer : vMixControl
    {
        public override bool IsResizeableVertical => true;
        
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(vMixControlVariableViewer), new PropertyMetadata("", InternalPropertyChanged));

        public bool ShowVariableName
        {
            get { return (bool)GetValue(ShowVariableNameProperty); }
            set { SetValue(ShowVariableNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowVariableName.  
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowVariableNameProperty =
            DependencyProperty.Register(nameof(ShowVariableName), typeof(bool), typeof(vMixControlVariableViewer), new PropertyMetadata(true, InternalPropertyChanged));

        private static void InternalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private string _variable = "";//Basic, Basketball, American Football

        /// <summary>
        /// Sets and gets the Style property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Variable
        {
            get => _variable;
            set => SetPropertyValue(ref _variable, value, nameof(Variable), _ =>
            {
                BindingOperations.ClearBinding(this, TextProperty);
                var globalSettings = AppServices.IsRegistered<GlobalVariablesViewModel>()
                    ? AppServices.GetRequiredService<GlobalVariablesViewModel>()
                    : null;
                if (globalSettings == null)
                    return;

                Binding b = new Binding("B");
                foreach (var variable in globalSettings.Variables)
                    if (variable.A == _variable)
                        b.Source = variable;
                BindingOperations.SetBinding(this, TextProperty, b);
            });
        }

        public List<string> VariableList
        {
            get
            {
                var globalSettings = AppServices.IsRegistered<GlobalVariablesViewModel>()
                    ? AppServices.GetRequiredService<GlobalVariablesViewModel>()
                    : null;
                return globalSettings?.Variables.Select(x => x.A).ToList() ?? new List<string>();
            }
        }

        public override string Type
        {
            get
            {
                return "VariableViewer";
            }
        }

        public override void BeforePropertiesChanged()
        {
            base.BeforePropertiesChanged();
        }

        public override void AfterPropertiesChanged()
        {

            base.AfterPropertiesChanged();
        }
    }
}
