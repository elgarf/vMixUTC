using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Xml.Serialization;
using vMixController.Classes;
using vMixController.Classes.Scripting;
using vMixController.Messages;

namespace vMixController.Widgets
{
    public class vMixControlStreamDeck : vMixControl
    {
        public override int MaxCount => 1;

        private string _context = "[NO BUTTON PRESSED]";

        /// <summary>
        /// Sets and gets the DeviceCaps property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Context
        {
            get => _context;
            set => SetPropertyValue(ref _context, value, nameof(Context));
        }

        private ObservableCollection<StreamDeckKey> _keys = new ObservableCollection<StreamDeckKey>();

        /// <summary>
        /// Sets and gets the Midis property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<StreamDeckKey> Keys
        {
            get => _keys;
            set => SetPropertyValue(ref _keys, value, nameof(Keys));
        }

        //private static vMixStreamDeckLibrary.StreamDeckAPI device = null;
        private static StreamDeckConnector _connector = new StreamDeckConnector();

        public override string Type
        {
            get
            {
                return "Stream Deck Device";
            }
        }

        public vMixControlStreamDeck()
        {



            _connector.OnStreamDeckEvent += _connector_OnStreamDeckEvent;
            Learn = new Func<StreamDeckKey>(() =>
            {

                var wnd = new StreamDeckLearnWindow(_connector);
                var result = wnd.ShowDialog();
                if (result ?? true)
                {
                    var k = wnd.Key;
                    wnd.Close();
                    return k;
                }

                return null;
            });
        }

        private void _connector_OnStreamDeckEvent(object sender, StreamDeckEvent e)
        {
            Dispatcher.Invoke(() =>
            {
                //A - context
                //B - execLink
                //if (e.Button != null)
                {
                    foreach (var item in Keys)
                    {
                        if (item.A == e.Context && e.Type == vMixStreamDeckLibrary.StreamDeckEvent.KeyUp)
                            Messenger.Send(new HotkeyLinkMessage() { Link = item.B, Parameter = ScriptExecutionDispatchRuntime.CreateOutgoingParameter(null) });
                    }
                    Context = e.Context.ToUpper();
                }
                //Debug.WriteLine(e.Message);
            });
        }

        [XmlIgnore]
        public Func<StreamDeckKey> Learn { get; set; }

        public override void BeforePropertiesChanged()
        {
            base.BeforePropertiesChanged();
        }

        public override void AfterPropertiesChanged()
        {
            base.AfterPropertiesChanged();
        }

        public override void Dispose()
        {
            /*if (device != null)
            {
                device.Dispose();
                device = null;
            }*/
            base.Dispose();
        }

        protected override void Dispose(bool managed)
        {
            base.Dispose(managed);
        }

    }

    public class StreamDeckKey : Quadriple<string, string, int, int>, ICloneable
    {
        new public object Clone()
        {
            return new StreamDeckKey() { A = (string)(A?.Clone() ?? ""), B = (string)(B?.Clone() ?? ""), C = C, D = D };
        }

    }
}


