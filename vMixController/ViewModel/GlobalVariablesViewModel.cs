using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using vMixController.Classes;
using vMixController.Messages;

namespace vMixController.ViewModel
{
    public partial class GlobalVariablesViewModel : ViewModelBase
    {

        [ObservableProperty]
        private ObservableCollection<Pair<string, string>> _variables = new ObservableCollection<Pair<string, string>>();
        private static readonly ConcurrentDictionary<string, string> _variablesStore = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);
        private static volatile string[] _indexToName = Array.Empty<string>();

        partial void OnVariablesChanging(ObservableCollection<Pair<string, string>> oldValue, ObservableCollection<Pair<string, string>> newValue)
        {
            DetachCollection(oldValue);
        }

        partial void OnVariablesChanged(ObservableCollection<Pair<string, string>> value)
        {
            AttachCollection(value);
        }

        public GlobalVariablesViewModel()
        {
            AttachCollection(_variables);

            Messenger.Register<SetGlobalVariable>(this, (r, t) =>
            {
                if (t.Index == -1)
                    TrySetStoreValueByName(t.Name, t.Value);
                else
                    TrySetStoreValueByIndex(t.Index, t.Value);

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (t.Index == -1)
                    {
                        var v = Variables.Where(x => x.A == t.Name).FirstOrDefault();
                        if (v != null)
                            v.B = t.Value;
                    }
                    else if (Variables.Count > t.Index)
                        Variables[t.Index].B = t.Value;

                    RebuildStoreFromCollection(Variables);
                }));

            });

            /*Variables.Add(new Pair<string, string>("test", "test"));
            Variables.Add(new Pair<string, string>("test1", "test5"));
            Variables.Add(new Pair<string, string>("test2", "test6"));
            Variables.Add(new Pair<string, string>("test3", "test7"));
            Variables.Add(new Pair<string, string>("test4", "test8"));*/
        }

        public static Dictionary<string, object> GetVariablesSnapshot()
        {
            var snapshot = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach (var item in _variablesStore)
                snapshot[item.Key] = item.Value;
            return snapshot;
        }

        private static bool TrySetStoreValueByName(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;
            if (!_variablesStore.ContainsKey(name))
                return false;

            _variablesStore.AddOrUpdate(name, value ?? "", (k, v) => value ?? "");
            return true;
        }

        private static bool TrySetStoreValueByIndex(int index, string value)
        {
            var map = _indexToName;
            if (index < 0 || map == null || index >= map.Length)
                return false;

            var name = map[index];
            if (string.IsNullOrWhiteSpace(name))
                return false;

            _variablesStore.AddOrUpdate(name, value ?? "", (k, v) => value ?? "");
            return true;
        }

        private void AttachCollection(ObservableCollection<Pair<string, string>> collection)
        {
            if (collection == null)
                return;

            collection.CollectionChanged += Variables_CollectionChanged;
            foreach (var item in collection)
                AttachItem(item);
            RebuildStoreFromCollection(collection);
        }

        private void DetachCollection(ObservableCollection<Pair<string, string>> collection)
        {
            if (collection == null)
                return;

            collection.CollectionChanged -= Variables_CollectionChanged;
            foreach (var item in collection)
                DetachItem(item);
        }

        private void Variables_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (var oldItem in e.OldItems.OfType<Pair<string, string>>())
                    DetachItem(oldItem);
            if (e.NewItems != null)
                foreach (var newItem in e.NewItems.OfType<Pair<string, string>>())
                    AttachItem(newItem);

            RebuildStoreFromCollection(Variables);
        }

        private void AttachItem(Pair<string, string> item)
        {
            if (item == null)
                return;
            item.PropertyChanged += VariableItem_PropertyChanged;
        }

        private void DetachItem(Pair<string, string> item)
        {
            if (item == null)
                return;
            item.PropertyChanged -= VariableItem_PropertyChanged;
        }

        private void VariableItem_PropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RebuildStoreFromCollection(Variables);
        }

        private static void RebuildStoreFromCollection(IEnumerable<Pair<string, string>> source)
        {
            var snapshot = new Dictionary<string, string>(StringComparer.Ordinal);
            var indexMap = new List<string>();

            if (source != null)
            {
                foreach (var item in source)
                {
                    var name = item?.A ?? "";
                    var value = item?.B ?? "";
                    indexMap.Add(name);
                    if (!string.IsNullOrWhiteSpace(name))
                        snapshot[name] = value;
                }
            }

            _variablesStore.Clear();
            foreach (var item in snapshot)
                _variablesStore[item.Key] = item.Value;
            _indexToName = indexMap.ToArray();
        }

        [RelayCommand]
        private void RemoveItem(Pair<string, string> item)
        {
            Variables.Remove(item);
        }

        [RelayCommand]
        private void AddItem()
        {
            Variables.Add(new Pair<string, string>("", ""));
        }

        [RelayCommand]
        private void Ok()
        {
            Messenger.Send(new ValueChangedMessage<bool>(true));
        }
    }
}


