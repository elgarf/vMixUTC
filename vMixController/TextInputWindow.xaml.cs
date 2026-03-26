using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace vMixController
{
    public enum InputTextWindowMode
    {
        Default = 0,
        NewPassword = 1,
        Password = 2
    }
    /// <summary>
    /// Логика взаимодействия для TextInputWindow.xaml
    /// </summary>
    public partial class TextInputWindow : Window, INotifyPropertyChanged
    {
        public TextInputWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>
        /// The <see cref="Text" /> property's name.
        /// </summary>
        public const string TextPropertyName = "Text";

        private string _text = "";

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets and gets the Text property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Text
        {
            get => _text;
            set => SetPropertyValue(ref _text, value, nameof(Text));
        }

        // Добавляем свойства Password, PasswordConfirmation, Title

        /// <summary>
        /// The <see cref="Password" /> property's name.
        /// </summary>
        public const string PasswordPropertyName = "Password";

        private string _password = "";

        /// <summary>
        /// Sets and gets the Password property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Password
        {
            get => _password;
            set => SetPropertyValue(ref _password, value, nameof(Password));
        }

        /// <summary>
        /// The <see cref="PasswordConfirmation" /> property's name.
        /// </summary>
        public const string PasswordConfirmationPropertyName = "PasswordConfirmation";

        private string _passwordConfirmation = "";

        /// <summary>
        /// Sets and gets the PasswordConfirmation property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string PasswordConfirmation
        {
            get => _passwordConfirmation;
            set => SetPropertyValue(ref _passwordConfirmation, value, nameof(PasswordConfirmation));
        }

        private string _title = "Input Page Name Text";

        /// <summary>
        /// Sets and gets the Title property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string InputTitle
        {
            get => _title;
            set => SetPropertyValue(ref _title, value, nameof(InputTitle));
        }

        InputTextWindowMode _mode = InputTextWindowMode.Default;
        public InputTextWindowMode Mode
        {
            get => _mode;
            set => SetPropertyValue(ref _mode, value, nameof(Mode));
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetPropertyValue<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void Me_Activated(object sender, EventArgs e)
        {
            //tb.Focus();
            //tb.SelectAll();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
