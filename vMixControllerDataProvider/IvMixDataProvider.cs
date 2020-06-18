using System.Collections.Generic;
using System.Windows;

namespace vMixControllerDataProvider
{
    public interface IvMixDataProvider
    {
        /// <summary>
        /// Period of updating data in msec, used for internal data provider needs
        /// </summary>
        int Period { get; set; }
        /// <summary>
        /// TRUE, if data provider has additional properties, which shows when user press "Properties" button.
        /// </summary>
        bool IsProvidingCustomProperties { get; }
        /// <summary>
        /// Data, provided by provider, it can be updated at every property get event.
        /// </summary>
        string[] Values { get; }
        /// <summary>
        /// Shows custom properties window.
        /// </summary>
        /// <param name="owner">Owner of window with custom properties.</param>
        void ShowProperties(Window owner);
        /// <summary>
        /// Custom On-Widget UI.
        /// </summary>
        UIElement CustomUI { get; }
        /// <summary>
        /// Gets provider properties for saving into file.
        /// </summary>
        /// <returns>List of provider properties. Every property must be a serializable value.</returns>
        List<object> GetProperties();
        /// <summary>
        /// Sets properties from list of objects.
        /// </summary>
        /// <param name="props">List of properties, provided by GetProperties method.</param>
        void SetProperties(List<object> props);

        object PreviewKeyUp { get; set; }
        object GotFocus { get; set; }
        object LostFocus { get; set; }
    }
}
