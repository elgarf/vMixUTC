using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;
using vMixAPI;
using vMixController.Classes;

namespace vMixController.Widgets
{
    [Serializable]
    public partial class vMixControlPlaylist : vMixControl
    {
        private DateTime _pause = DateTime.Now;

        public override string Type => "Playlist";

        private ObservableCollection<string> _items = new ObservableCollection<string>();

        /// <summary>
        /// Sets and gets the Items property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<string> Items
        {
            get => _items;
            set => SetPropertyValue(ref _items, value, nameof(Items));
        }

        private int _selectedIndex = 0;

        /// <summary>
        /// Sets and gets the SelectedIndex property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SetPropertyValue(ref _selectedIndex, value, nameof(SelectedIndex), newValue =>
            {
                _pause = DateTime.Now;

                if (newValue >= 0)
                {
                    State?.SendFunction("Function", "SelectIndex",
                        "Value", (newValue + 1).ToString(),
                        "Input", InputKey);
                }
            });
        }

        private string _inputKey = "";

        /// <summary>
        /// Sets and gets the InputKey property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string InputKey
        {
            get => _inputKey;
            set => SetPropertyValue(ref _inputKey, value, nameof(InputKey));
        }

        private bool _shouldScrollIntoView = false;

        /// <summary>
        /// Sets and gets the ShouldScrollIntoView property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlIgnore]
        public bool ShouldScrollIntoView
        {
            get => _shouldScrollIntoView;
            set => SetPropertyValue(ref _shouldScrollIntoView, value, nameof(ShouldScrollIntoView));
        }

        public List<Input> Inputs { get => _internalState?.Inputs; }

        public vMixControlPlaylist()
        {
            XmlDocumentMessenger.OnDocumentDownloaded += XmlDocumentMessenger_OnDocumentDownloaded;
            Height = 128;
        }

        public override Hotkey[] GetHotkeys()
        {
            return new Classes.Hotkey[] {
                new Classes.Hotkey { Name = "Previous Item" },
                new Classes.Hotkey { Name = "Shuffle" },
                new Classes.Hotkey { Name = "PlayOut" },
                new Classes.Hotkey { Name = "Play/Pause" },
                new Classes.Hotkey { Name = "Next Item" }
            };
        }

        public override void ExecuteHotkey(int index)
        {
            switch (index)
            {
                case 0: PrevItemCommand.Execute(null); break;
                case 1: ShuffleCommand.Execute(null); break;
                case 2: PlayOutCommand.Execute(null); break;
                case 3: PlayCommand.Execute(null); break;
                case 4: NextItemCommand.Execute(null); break;
                default:
                    break;
            }
        }

        private void XmlDocumentMessenger_OnDocumentDownloaded(XmlDocument doc, DateTime timestamp)
        {
            if ((timestamp - _pause).TotalSeconds < 1) return;

            var node = doc?.SelectSingleNode(string.Format("//vmix/inputs/input[@key=\"{0}\"]/list", InputKey));
            Dispatcher.Invoke(() =>
            {
                var oldIndex = _selectedIndex;
                if (node != null)
                {
                    int index = 0;
                    //_updating = true;
                    while (Items.Count > node.ChildNodes.Count)
                        Items.RemoveAt(Items.Count - 1);
                    while (Items.Count < node.ChildNodes.Count)
                        Items.Add(null);

                    foreach (XmlElement item in node.ChildNodes)
                    {
                        Items[index] = Path.GetFileName(item.InnerText);
                        if (item.Attributes["selected"]?.Value.ToLowerInvariant() == "true" && SelectedIndex != index)
                            _selectedIndex = index;
                        index++;
                    }

                    //_updating = false;
                    RaisePropertyChanged(nameof(Items));
                        ShouldScrollIntoView = oldIndex != _selectedIndex;
                    RaisePropertyChanged(nameof(SelectedIndex));
                }
                else
                    Items.Clear();
            });

        }

        public override void BeforePropertiesChanged()
        {
            base.BeforePropertiesChanged();
        }

        public override void AfterPropertiesChanged()
        {
            base.AfterPropertiesChanged();
        }

        protected override void Dispose(bool managed)
        {
            if (_disposed) return;
            XmlDocumentMessenger.OnDocumentDownloaded -= XmlDocumentMessenger_OnDocumentDownloaded;
            base.Dispose(managed);
        }

        [RelayCommand]
        private void RemoveItem(string p)
        {
            State?.SendFunction("Function", "ListRemove",
                "Value", (Items.IndexOf(p) + 1).ToString(),
                "Input", InputKey);
        }

        [RelayCommand]
        private void AddItem()
        {
            Ookii.Dialogs.Wpf.VistaOpenFileDialog opendlg = new Ookii.Dialogs.Wpf.VistaOpenFileDialog
            {
                Filter = "Any File|*.*"
            };
            var result = opendlg.ShowDialog(App.Current.Windows.OfType<vMixWidgetSettingsView>().FirstOrDefault());
            if (result.HasValue && result.Value)
            {
                var fn = opendlg.FileName;
                State?.SendFunction("Function", "ListAdd",
                    "Value", fn,
                    "Input", InputKey);
            }
        }

        [RelayCommand]
        private void NextItem()
        {
            ShouldScrollIntoView = true;
            if (SelectedIndex + 1 < Items.Count)
                SelectedIndex++;
        }

        [RelayCommand]
        private void PrevItem()
        {
            ShouldScrollIntoView = true;
            if (SelectedIndex - 1 >= 0)
                SelectedIndex--;
        }

        [RelayCommand]
        private void Shuffle()
        {
            State?.SendFunction("Function", "ListShuffle",
                "Input", InputKey);
        }

        [RelayCommand]
        private void PlayOut()
        {
            State?.SendFunction("Function", "ListPlayOut",
                "Input", InputKey);
        }

        [RelayCommand]
        private void Play()
        {
            State?.SendFunction("Function", "PlayPause",
                "Input", InputKey);
        }
    }
}
