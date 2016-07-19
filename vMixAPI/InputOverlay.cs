using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;

namespace vMixAPI
{
    [Serializable]
    public class InputOverlay: InputBase
    {

        public override State ControlledState
        {
            get
            {
                return base.ControlledState;
            }

            set
            {
                foreach (var item in Elements)
                {
                    item.ControlledState = value;
                }
                base.ControlledState = value;
            }
        }

        [XmlAttribute("index")]
        public int Index { get; set; }
        [XmlAttribute("key")]
        public string Key { get; set; }
        [XmlElement(typeof(InputText), ElementName = "text"),
            XmlElement(typeof(InputOverlay), ElementName = "overlay"),
            XmlElement(typeof(InputPosition), ElementName = "position"),]
        public ObservableCollection<InputBase> Elements { get; set; }

        public InputOverlay()
        {
            Elements = new ObservableCollection<InputBase>();
            Elements.CollectionChanged += Elements_CollectionChanged;
        }

        private void Elements_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.NewItems.OfType<InputBase>())
            {
                item.ControlledState = ControlledState;
            }
        }
    }
}
