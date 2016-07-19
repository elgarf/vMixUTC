using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.IO;
using System.Security.Cryptography;

namespace Licensing.KeyGenerator.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        RSACryptoServiceProvider _rsa;

        private RelayCommand _generateKeysCommand;

        /// <summary>
        /// Gets the GenerateKeysCommand.
        /// </summary>
        public RelayCommand GenerateKeysCommand
        {
            get
            {
                return _generateKeysCommand
                    ?? (_generateKeysCommand = new RelayCommand(
                    () =>
                    {
                        if (_rsa != null)
                            _rsa.Dispose();
                        _rsa = new RSACryptoServiceProvider();
                        Properties.Settings.Default.LoadedKeys = _rsa.ToXmlString(true);
                        ActivationKey = ActivationKey;
                    }));
            }
        }

        private RelayCommand _saveKeysCommand;

        /// <summary>
        /// Gets the SaveKeysCommand.
        /// </summary>
        public RelayCommand SaveKeysCommand
        {
            get
            {
                return _saveKeysCommand
                    ?? (_saveKeysCommand = new RelayCommand(
                    () =>
                    {
                        Ookii.Dialogs.Wpf.VistaSaveFileDialog fd = new Ookii.Dialogs.Wpf.VistaSaveFileDialog();
                        fd.Filter = "RSA ключи|*.xml";
                        fd.DefaultExt = "xml";
                        if (fd.ShowDialog(App.Current.MainWindow) ?? false)
                        {
                            var keys = _rsa.ToXmlString(true);
                            File.WriteAllText(fd.FileName, keys);
                            Properties.Settings.Default.LoadedKeys = keys;
                        }
                    }));
            }
        }

        private RelayCommand _loadKeysCommand;

        /// <summary>
        /// Gets the LoadKeysCommand.
        /// </summary>
        public RelayCommand LoadKeysCommand
        {
            get
            {
                return _loadKeysCommand
                    ?? (_loadKeysCommand = new RelayCommand(
                    () =>
                    {
                        Ookii.Dialogs.Wpf.VistaOpenFileDialog fd = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
                        fd.Filter = "RSA ключи|*.xml";
                        fd.DefaultExt = "xml";
                        if (fd.ShowDialog(App.Current.MainWindow) ?? false)
                        {
                            var keys = File.ReadAllText(fd.FileName);
                            _rsa.FromXmlString(keys);
                            Properties.Settings.Default.LoadedKeys = keys;
                            ActivationKey = ActivationKey;
                        }
                        
                    }));
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
                /*if (_activationKey == value)
                {
                    return;
                }*/

                _activationKey = value;

                SerialNumber = LicenseManager.SignKey(value, _rsa.ExportParameters(true));

                Verified = LicenseManager.VerifyKey(value, SerialNumber, _rsa.ExportParameters(false));


                var lic = new License() { Customer = "elgarf@outlook.com", MachineID = _activationKey };
                lic.Features.Add(new Feature() { Name = "Max Widgets", Value = 256 });
                var data = LicenseManager.SignLicense(lic, _rsa.ExportParameters(true));
                //data[5] = 2;
                var result = LicenseManager.VerifyLicense(data, _activationKey, _rsa.ExportParameters(false));

                RaisePropertyChanged(ActivationKeyPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SerialNumber" /> property's name.
        /// </summary>
        public const string SerialNumberPropertyName = "SerialNumber";

        private string _serialNumber = "";

        /// <summary>
        /// Sets and gets the SerialNumber property.
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
                RaisePropertyChanged(SerialNumberPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Verified" /> property's name.
        /// </summary>
        public const string VerifiedPropertyName = "Verified";

        private bool _verified = false;

        /// <summary>
        /// Sets and gets the Verified property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Verified
        {
            get
            {
                return _verified;
            }

            set
            {
                if (_verified == value)
                {
                    return;
                }

                _verified = value;
                RaisePropertyChanged(VerifiedPropertyName);
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            _rsa = new RSACryptoServiceProvider();
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LoadedKeys))
                _rsa.FromXmlString(Properties.Settings.Default.LoadedKeys);
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}