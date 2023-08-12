using Avalonia.Controls;
using DeLight.ViewModels;
using System;
using DeLight.Utilities;
using System.Windows.Input;
using Avalonia.Input;

namespace DeLight.Views
{
    public partial class MainWindow : Window
    {
        private int count;

        private bool isDragging;

        //private IDeviceNotifier usbDeviceNotifier;

        private readonly int vendor_id = 0x1069;
        private readonly int product_id = 0x1040;

        public MainWindow()
        {
            WindowState = WindowState.Normal;
            Position = new(GlobalSettings.Instance.LastScreenLeft, GlobalSettings.Instance.LastScreenTop);
            InitializeComponent();
            DefaultCueDisplay.DataContext = GlobalSettings.Instance.DefaultCue;
            SettingsDisplay.DataContext = GlobalSettings.Instance;
            CueList.SelectionChanged += CueList_SelectionChanged;
            //usbDeviceNotifier = DeviceNotifier.OpenDeviceNotifier();
            //usbDeviceNotifier.OnDeviceNotify += OnDeviceNotifyEvent;
            KeyDown += OnKeyDown;
            SizeChanged += MainWindow_OnSizeChanged;
            PointerPressed += MainWindow_MouseDown;
            Loaded += MainWindow_Loaded;
            CueList.Focusable = false;
        }

        public void CueList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (CueEditorWindow.IsVisible && CueEditorWindow.DataContext is CueEditorViewModel cevm && !cevm.IsSaved)
            {
                var result = System.Windows.MessageBox.Show("You have unsaved changes. Would you like to save them?", "Unsaved Changes", System.Windows.MessageBoxButton.YesNoCancel);
                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    cevm.Save();
                }
                else if (result == System.Windows.MessageBoxResult.Cancel)
                {
                   CueList.SelectedItem = e.RemovedItems[0];
                }
            }
        }


        public void MainWindow_Loaded(object? sender, EventArgs e)
        {
            WindowState = GlobalSettings.Instance.WindowState;
            PlaybackBar.DataContext = (DataContext as MainWindowViewModel)!.CuePlaybackViewModel;
        }
        public void MainWindow_MouseDown(object? sender, PointerPressedEventArgs e)
        {
            TopLevel.GetTopLevel(this)?.FocusManager?.ClearFocus();
            Activate();
            Focus();
        }

        //private void OnDeviceNotifyEvent(object? sender, DeviceNotifyEventArgs e)
        //{
        //    if (e.Device.IdVendor == vendor_id && e.Device.IdProduct == product_id)
        //    {
        //        if (e.EventType == EventType.DeviceArrival)
        //        {
        //            Console.WriteLine("Device connected.");
        //            // handle connection event
        //        }
        //        else if (e.EventType == EventType.DeviceRemoveComplete)
        //        {
        //            Console.WriteLine("Device disconnected.");
        //            // handle disconnection event
        //        }
        //    }
        //}

        protected override void OnClosed(EventArgs e)
        {
            GlobalSettings.Instance.LastScreenTop = Position.Y;
            GlobalSettings.Instance.LastScreenLeft = Position.X;
            GlobalSettings.Instance.WindowState = WindowState;
            GlobalSettings.Save();
            base.OnClosed(e);
            (DataContext as MainWindowViewModel)?.HideVideoWindow();

        }
        protected void OnKeyDown(object? sender, Avalonia.Input.KeyEventArgs e)

        {
            base.OnKeyDown(e);
            if (e.Key == Avalonia.Input.Key.Escape && Keyboard.FocusedElement is not TextBox)
            {
                count++;
                if (count == 2)
                {
                    (DataContext as MainWindowViewModel)?.HideVideoWindow();
                    count = 0;
                }
                e.Handled = true;
            }
            else if (e.Key == Avalonia.Input.Key.Space && DataContext is MainWindowViewModel vm && Keyboard.FocusedElement is not TextBox)
            {
                vm.PlayCue();
                e.Handled = true;
            }
            else
                count = 0;
        }

        //private void Play_Button_Clicked(object sender, RoutedEventArgs e)
        //{
        //    if (DataContext is MainWindowViewModel viewModel)
        //        viewModel.Play();
        //}
        //private void Stop_Button_Clicked(object sender, RoutedEventArgs e)
        //{
        //    if (DataContext is MainWindowViewModel viewModel)
        //        viewModel.Stop();
        //}

        private void MainWindow_OnSizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.UpdateWindowSize(Bounds.Height);
            }
        }
    }
}