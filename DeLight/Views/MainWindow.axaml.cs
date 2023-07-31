using Avalonia.Controls;
using DeLight.ViewModels;
using System;
using DeLight.Utilities;
using Avalonia.Input;
using System.Windows.Input;

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
            DataContext = new MainWindowViewModel(this);
            InitializeComponent();
            ((MainWindowViewModel)DataContext).MainWindowLoaded();

            //usbDeviceNotifier = DeviceNotifier.OpenDeviceNotifier();
            //usbDeviceNotifier.OnDeviceNotify += OnDeviceNotifyEvent;
            KeyDown += OnKeyDown;
            SizeChanged += MainWindow_OnSizeChanged;
            //MouseDown += MainWindow_MouseDown;
            Loaded += MainWindow_Loaded;
        }

        public void MainWindow_Loaded(object? sender, EventArgs e)
        {
            WindowState = GlobalSettings.Instance.WindowState;
        }
        public void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
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
            (DataContext as MainWindowViewModel)?.HideVideoPlayback();

        }
        protected void OnKeyDown(object? sender, Avalonia.Input.KeyEventArgs e)

        {
            base.OnKeyDown(e);
            if (e.Key == Avalonia.Input.Key.Escape && ((DataContext as MainWindowViewModel)?.VideoIsVisible ?? false) && Keyboard.FocusedElement is not TextBox)
            {
                count++;
                if (count == 2)
                {
                    (DataContext as MainWindowViewModel)?.HideVideoPlayback();
                    count = 0;
                }
                e.Handled = true;
            }
            else if (e.Key == Avalonia.Input.Key.Space && DataContext is MainWindowViewModel vm && Keyboard.FocusedElement is not TextBox)
            {
                vm.PlayNextCue();
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
                vm.UpdateWindowSize();
            }
        }
    }
}