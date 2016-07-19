using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Security.Cryptography;

namespace Licensing.WPF.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class LicenseWindowViewModel : ViewModelBase
    {
        public RSAParameters Parameters { get; set; }

        /// <summary>
            /// The <see cref="PreText" /> property's name.
            /// </summary>
        public const string PreTextPropertyName = "PreText";

        private string _preText = null;

        /// <summary>
        /// Sets and gets the PreText property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string PreText
        {
            get
            {
                return _preText;
            }

            set
            {
                if (_preText == value)
                {
                    return;
                }

                _preText = value;
                RaisePropertyChanged(PreTextPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="EMail" /> property's name.
        /// </summary>
        public const string EMailPropertyName = "EMail";

        private string _email = "info@multimedia74";

        /// <summary>
        /// Sets and gets the EMail property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string EMail
        {
            get
            {
                return _email;
            }

            set
            {
                if (_email == value)
                {
                    return;
                }

                _email = value;
                RaisePropertyChanged(EMailPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ActivationKey" /> property's name.
        /// </summary>
        public const string ActivationKeyPropertyName = "ActivationKey";

        private string _activationKey = "";

        /// <summary>
        /// Sets and gets the ActivationKey property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ActivationKey
        {
            get
            {
                return _activationKey;
            }

            set
            {
                if (_activationKey == value)
                {
                    return;
                }

                _activationKey = value;
                RaisePropertyChanged(ActivationKeyPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Salt" /> property's name.
        /// </summary>
        public const string SaltPropertyName = "Salt";

        private string _salt = "";

        /// <summary>
        /// Sets and gets the Salt property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Salt
        {
            get
            {
                return _salt;
            }

            set
            {
                if (_salt == value)
                {
                    return;
                }

                _salt = value;
                ActivationKey = LicenseManager.GenerateActivationKey();
                ApplySerialNumber.RaiseCanExecuteChanged();
                RaisePropertyChanged(SaltPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SerialKey" /> property's name.
        /// </summary>
        public const string SerialNumberPropertyName = "SerialNumber";

        private string _serialNumber = "";

        /// <summary>
        /// Sets and gets the SerialKey property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SerialNumber
        {
            get
            {
                return _serialNumber;
            }

            set
            {
                if (_serialNumber == value)
                {
                    return;
                }

                _serialNumber = value;
                ApplySerialNumber.RaiseCanExecuteChanged();
                RaisePropertyChanged(SerialNumberPropertyName);
            }
        }

        private RelayCommand _applySerialNumber;

        /// <summary>
        /// Gets the ApplySerialNumber.
        /// </summary>
        public RelayCommand ApplySerialNumber
        {
            get
            {
                return _applySerialNumber
                    ?? (_applySerialNumber = new RelayCommand(
                    () =>
                    {
                        if (!ApplySerialNumber.CanExecute(null))
                        {
                            return;
                        }


                    },
                    () => LicenseManager.VerifyKey(ActivationKey, SerialNumber, Parameters)));
            }
        }

        RSACryptoServiceProvider _provider;

        public void LoadParameters(string parameters)
        {
            _provider.FromXmlString(parameters);
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public LicenseWindowViewModel()
        {
            _provider = new RSACryptoServiceProvider();
            Parameters = _provider.ExportParameters(false);
            Salt = "test";
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}