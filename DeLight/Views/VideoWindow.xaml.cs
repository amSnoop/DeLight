using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using DeLight.Models;
using System.IO;
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
        //returns false if a video file cannot be played or is not found, but there is a string provided in the path.

        public void Stop()
        {
            ClearAllMediaElements();
        }

        public new void Hide()
        {
            this.Visibility = Visibility.Hidden;
            ClearAllMediaElements();
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
