using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace DeLight.Views
{
    public partial class VideoWindow : Window
    {

        public VideoWindow(Screen? screen)
        {
            InitializeComponent();
            Top = screen?.Bounds.Top ?? Screen.PrimaryScreen?.Bounds.Top ?? 0;
            Left = screen?.Bounds.Left ?? Screen.PrimaryScreen?.Bounds.Left ?? 0;
        }

        public VideoWindow()
        {
            InitializeComponent();
        }
        protected override void OnClosed(EventArgs e)
        {
            Stop();
            base.OnClosed(e);
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
        }
        

        public void Stop()
        {
            ClearAllMediaElements();
        }

        public new void Hide()
        {
            Visibility = Visibility.Hidden;
            ClearAllMediaElements();
        }

        public void RemoveMediaElement(UIElement mediaElement)
        {
            Container.Children.Remove(mediaElement);
        }

        private void ClearAllMediaElements()
        {
            while (Container.Children.Count > 0)
            {
                var c = Container.Children[0];
                if(c is MediaElement mediaElement)
                    mediaElement.Stop();
                Container.Children.Remove(c);
            }
        }
        public void SetScreen(Screen screen)
        {
            Dispatcher.Invoke(() => {
                WindowState = WindowState.Normal;
                Top = screen.Bounds.Top;
                Left = screen.Bounds.Left;
                WindowState = WindowState.Maximized;
            });
        }
    }
}
