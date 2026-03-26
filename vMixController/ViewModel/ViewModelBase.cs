using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel;
using System.Windows;
using vMixController.Classes;

namespace vMixController.ViewModel
{
    public class ViewModelBase : ObservableRecipient
    {
        protected ViewModelBase()
            : base(AppServices.IsRegistered<IMessenger>()
                ? AppServices.GetRequiredService<IMessenger>()
                : WeakReferenceMessenger.Default)
        {
        }

        public static bool IsInDesignModeStatic => DesignerProperties.GetIsInDesignMode(new DependencyObject());

        public bool IsInDesignMode => IsInDesignModeStatic;

        public virtual void Cleanup()
        {
            Messenger.UnregisterAll(this);
        }
    }
}
