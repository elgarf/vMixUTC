using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using vMixController.Classes;
using vMixController.Controls;
using vMixController.Extensions;
using vMixControllerSkin.Localization;
using vMixController.Messages;
using vMixController.ViewModel;

namespace vMixController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IMessenger Messenger => AppServices.IsRegistered<IMessenger>()
            ? AppServices.GetRequiredService<IMessenger>()
            : WeakReferenceMessenger.Default;

        bool _loading = false;

        private bool _isCanvasPanning;
        private Point _canvasPanStart;
        private Matrix _canvasPanStartMatrix;
        private const double CanvasZoomMin = 0.1;
        private const double CanvasZoomMax = 10.0;
        MatrixTransform _canvasTransform = new MatrixTransform();
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            DataContext = AppServices.GetRequiredService<MainViewModel>();
            InitializeComponent();
            Closing += (s, e) =>
            {
                Ookii.Dialogs.Wpf.TaskDialog dialog = new Ookii.Dialogs.Wpf.TaskDialog();
                dialog.Buttons.Add(new Ookii.Dialogs.Wpf.TaskDialogButton(Ookii.Dialogs.Wpf.ButtonType.Yes));
                dialog.Buttons.Add(new Ookii.Dialogs.Wpf.TaskDialogButton(Ookii.Dialogs.Wpf.ButtonType.No));
                dialog.WindowTitle = LocalizationManager.Instance["Dialog.ExitConfirmation.Title"];
                dialog.MainIcon = Ookii.Dialogs.Wpf.TaskDialogIcon.Warning;
                dialog.MainInstruction = LocalizationManager.Instance["Dialog.ExitConfirmation.MainInstruction"];
                if (dialog.ShowDialog(this).ButtonType == Ookii.Dialogs.Wpf.ButtonType.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    vMixController.Properties.Settings.Default.Save();
                    if (AppServices.IsRegistered<MainViewModel>())
                    {
                        AppServices.GetRequiredService<MainViewModel>().Cleanup();
                    }
                    App.Current.Shutdown();
                }
            };

            Messenger.Register<LoadingMessage>(this, (r, msg) =>
            {
                var fadein = (Storyboard)FindResource("StoryboardFadeIn");
                var fadeout = (Storyboard)FindResource("StoryboardFadeOut");

                if (msg.Loading && !_loading)
                {
                    fadein.Begin(Loading);
                    _loading = true;
                }
                else if (!msg.Loading && _loading)
                {
                    fadeout.Begin(Loading);
                    _loading = false;
                }
            });

        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void UpdateMatrix()
        {
            if (CanvasContent.RenderTransform is MatrixTransform mtx)
                _canvasTransform.Matrix = mtx.Matrix;
        }

        private void LayoutGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle ||
                (e.ChangedButton == MouseButton.Left && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control))
            {
                if (!vMixController.Classes.AppServices.GetRequiredService<MainViewModel>().WindowSettings.UseInfiniteCanvas) return;
                UpdateMatrix();


                _isCanvasPanning = true;

                var fadein = (Storyboard)FindResource("StoryboardFadeIn");
                fadein.Begin(MiniMapBorder);

                _canvasPanStart = e.GetPosition(LayoutGrid);
                _canvasPanStartMatrix = _canvasTransform?.Matrix ?? Matrix.Identity;
                LayoutGrid.CaptureMouse();
                Mouse.OverrideCursor = Cursors.Hand;
                CanvasContent.SetCurrentValue(Grid.RenderTransformProperty, _canvasTransform);
                e.Handled = true;
            }
        }

        private void LayoutGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isCanvasPanning)
                return;

            var current = e.GetPosition(LayoutGrid);
            var delta = current - _canvasPanStart;

            var m = _canvasPanStartMatrix;
            m.Translate(delta.X, delta.Y);
            if (_canvasTransform != null)
                _canvasTransform.Matrix = m;
            CanvasContent.SetCurrentValue(Grid.RenderTransformProperty, _canvasTransform);
            ConnectionsOverlay?.RequestRender();
            e.Handled = true;
        }

        private void LayoutGrid_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isCanvasPanning)
                return;

            var fadeout = (Storyboard)FindResource("StoryboardFadeOut");
            fadeout.Begin(MiniMapBorder);

            if (e.ChangedButton == MouseButton.Middle || e.ChangedButton == MouseButton.Left)
            {
                _isCanvasPanning = false;
                if (LayoutGrid.IsMouseCaptured)
                    LayoutGrid.ReleaseMouseCapture();
                Mouse.OverrideCursor = null;
                e.Handled = true;
            }
        }

        private void LayoutGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Keep Shift+Wheel behavior (horizontal scrolling) handled by the outer ScrollViewer.
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                return;

            if (_canvasTransform == null)
                return;

            if (!vMixController.Classes.AppServices.GetRequiredService<MainViewModel>().WindowSettings.UseInfiniteCanvas ||
                !vMixController.Classes.AppServices.GetRequiredService<MainViewModel>().IsHotkeysEnabled ||
                ((DependencyObject)sender).FindChild<WheelControlledScrollViewer>().Where(x => x.IsMouseOver).Count() > 0) return;

            var container = ((DependencyObject)sender).FindChild<vMixControlContainerDummy>().Where(x => x.IsMouseOver).FirstOrDefault();
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                 container != null)
            {
                if (e.Delta > 0)
                    container.ScaleUpCommand.Execute(container.Control);
                else
                    container.ScaleDownCommand.Execute(container.Control);
                e.Handled = true;
                return;
            }

            UpdateMatrix();

            var m = _canvasTransform.Matrix;
            var scale = m.M11;
            var zoomFactor = e.Delta > 0 ? 1.1 : (1.0 / 1.1);
            var newScale = scale * zoomFactor;

            if (newScale < CanvasZoomMin)
                zoomFactor = CanvasZoomMin / scale;
            else if (newScale > CanvasZoomMax)
                zoomFactor = CanvasZoomMax / scale;

            var p = e.GetPosition(LayoutGrid);
            m.ScaleAt(zoomFactor, zoomFactor, p.X, p.Y);
            _canvasTransform.Matrix = m;

            CanvasContent.SetCurrentValue(Grid.RenderTransformProperty, _canvasTransform);
            ConnectionsOverlay?.RequestRender();
            e.Handled = true;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var wheel = ((DependencyObject)sender).FindChild<WheelControlledScrollViewer>().Where(x => x.IsMouseOver);

            // Let the Layout canvas handle wheel zoom/pan; don't hijack the wheel for horizontal scrolling here.
            if (LayoutGrid != null && LayoutGrid.IsMouseOver && (Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift &&
                (vMixController.Classes.AppServices.GetRequiredService<MainViewModel>().WindowSettings.UseInfiniteCanvas && wheel.Count() == 0))
                return;

            foreach (var wheelControl in wheel)
            {
                var e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = UIElement.MouseWheelEvent
                };
                wheelControl.RaiseEvent(e2);
            }
            if (wheel.Count() > 0)
            {
                e.Handled = true;
                return;
            }


            var scrollViewer = (ScrollViewer)sender;

            if (scrollViewer == null || (scrollViewer.Tag is bool && (bool)scrollViewer.Tag))
                return;

            if (Keyboard.Modifiers != ModifierKeys.Shift && scrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible)
                return;

            if (e.Delta < 0)
                scrollViewer.LineRight();
            else
                scrollViewer.LineLeft();

            e.Handled = true;
        }

        public Point ToCanvasContentPoint(Point layoutGridPoint, bool infiniteCanvas = true)
        {
            if (!infiniteCanvas) return layoutGridPoint;

            var m = _canvasTransform?.Matrix ?? Matrix.Identity;
            if (m.HasInverse)
            {
                m.Invert();
                return m.Transform(layoutGridPoint);
            }
            return layoutGridPoint;
        }

        private void Layout_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            /*if (vMixController.Classes.AppServices.GetRequiredService<MainViewModel>().WindowSettings.Locked)
            {
                (sender as ListView).ContextMenu.IsOpen = false;
                e.Handled = true;
            }*/
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            App.CloseSplash();
            Keyboard.Focus(Layout);
        }

        private void LanguageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is MenuItem menuItem && menuItem.Tag is CultureInfo cultureName)
                LocalizationManager.Instance.SetCulture(cultureName.Name);
        }
    }
}


