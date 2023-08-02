using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Views;
using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using System.Linq;
using Avalonia.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using DeLight.Utilities;
using DeLight.Models;
using System.Windows;
using System.Diagnostics;

namespace DeLight.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly MainWindow _window;
        private List<Screen> _screenObjects;

        public bool VideoIsVisible => VideoWindow.IsVisible;
        public VideoWindow VideoWindow { get; set; } = new();

        private DispatcherTimer _timer = new();
        private int _timerInterval = 50;
        private int _totalTicks = 0;
        private bool foundDuration = false;
        #region Observable Properties

        [ObservableProperty]
        private string selectedMonitor = "";
        [ObservableProperty]
        private List<string> monitors = new();
        [ObservableProperty]
        private CueViewModel previewCueViewModel = new();
        [ObservableProperty]
        private CueViewModel activeCueViewModel = new();
        [ObservableProperty]
        private ShowRunner showRunner;
        [ObservableProperty]
        private double subTitleFontFactor = 1.2;
        [ObservableProperty]
        private double titleFontFactor = 3;
        [ObservableProperty]
        private double cueFontFactor = 1.2;

        #endregion



        #region Font Size Properties

        public double BodyFontSize
        {
            get
            {
                double baseFontSize = 12;
                double scaleFactor = 1 + Math.Log(_window.Bounds.Height / 720.0);
                double maxFontSize = 25;
                double fontSize = Math.Max(Math.Min(baseFontSize * scaleFactor, maxFontSize), baseFontSize);
                return fontSize;
            }
        }

        public double CueFontSize => BodyFontSize * CueFontFactor;
        public double SubtitleFontSize => BodyFontSize * SubTitleFontFactor;

        public double TitleFontSize => BodyFontSize * TitleFontFactor;

        public void UpdateWindowSize()
        {
            OnPropertyChanged(nameof(TitleFontSize));
            OnPropertyChanged(nameof(SubtitleFontSize));
            OnPropertyChanged(nameof(BodyFontSize));
            OnPropertyChanged(nameof(CueFontSize));
        }

        #endregion

        #region Non [ObservableProperty] Properties that send updates to the view

        #endregion

        public MainWindowViewModel(MainWindow window)
        {
            ShowRunner = new(Show.Load(GlobalSettings.Instance.LastShowPath), VideoWindow);
            ShowRunner.PropertyChanged += ActiveCueChanged;
            ShowRunner.ActiveCue = ShowRunner.Show.Cues.FirstOrDefault();
            _window = window;
            _screenObjects = Screen.AllScreens.ToList();
            Monitors = _screenObjects.Select((s, i) => $"Monitor {i + 1}: {s.Bounds.Width}x{s.Bounds.Height}").ToList();
            if (Screen.PrimaryScreen == null)
            {
                SelectedMonitor = Monitors.FirstOrDefault() ?? "";
                VideoWindow.SetScreen(_screenObjects.First());
            }
            else
            {
                SelectedMonitor = Monitors[_screenObjects.IndexOf(Screen.PrimaryScreen)];
                VideoWindow.SetScreen(Screen.PrimaryScreen);
            }
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            VideoWindow.GotFocus += VideoWindow_GotFocus;
            _timer.Interval = TimeSpan.FromMilliseconds(_timerInterval);
        }
        public void MainWindowLoaded()
        {
            _window.DefaultCueDisplay.DataContext = GlobalSettings.Instance.DefaultCue;
            _window.SettingsDisplay.DataContext = GlobalSettings.Instance;
            _window.CueList.SelectionChanged += CueList_SelectionChanged;
        }
        private void CueList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_window.CueEditorWindow.DataContext is CueEditorViewModel vm && !vm.IsSaved)
            {
                var result = System.Windows.MessageBox.Show("You have unsaved changes. Would you like to save them?", "Unsaved Changes", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    vm.Save();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    _window.CueList.SelectedItem = e.RemovedItems[0];
                }
            }
        }

        #region Other Event Listeners

        private void ActiveCueChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ShowRunner.ActiveCue))
            {
                ActiveCueViewModel.CurrentCue = ShowRunner.ActiveCue;
            }
            if (e.PropertyName == nameof(ShowRunner.SelectedCue))
            {
                PreviewCueViewModel.CurrentCue = ShowRunner.SelectedCue;
                if(ShowRunner.SelectedCue != null)
                    _window.CueEditorWindow.DataContext = new CueEditorViewModel(ShowRunner.SelectedCue);
            }
        }

        //Volume property of active cue changed, send it to video window.
        private void ActiveCue_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Cue.Volume))
            {
            }
        }

        //Monitor selection changed, send it to video window.
        partial void OnSelectedMonitorChanged(string value)
        {
            if (value == null)
            {
                Debug.WriteLine("Selected monitor is null");
                return;
            }
            var screen = _screenObjects[Monitors.IndexOf(SelectedMonitor)];
            VideoWindow.SetScreen(screen);
        }

        //refocus the main window if the video window is clicked [Not working]
        public void VideoWindow_GotFocus(object sender, EventArgs e)
        {
            _window.Activate();
            _window.Focus();
        }

        //A new monitor is plugged in or display settings are changed, update the list of monitors TODO: Test
        private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
        {
            var screen = _screenObjects[_window.MonitorSelector.SelectedIndex];
            _screenObjects = Screen.AllScreens.ToList();
            Monitors = _screenObjects.Select((s, i) => $"Monitor {i + 1}: {s.Bounds.Width}x{s.Bounds.Height}").ToList();
            try
            {
                SelectedMonitor = Monitors[_screenObjects.IndexOf(screen)];
            }
            catch (Exception)
            {
                SelectedMonitor = Monitors.FirstOrDefault() ?? "";
            }
        }


        #endregion


        public void HideVideoPlayback()
        {
            VideoWindow.Hide();
            ShowRunner.Stop();
            _window.Focus();
        }

        public void PlayNextCue()
        {
            // The show is over and no cue is selected
            ShowRunner.Play();
            _window.Activate();
            _window.Focus();
        }
        public void Stop()
        {
            ShowRunner.Stop();
            VideoWindow.Stop();
            _window.Activate();
            _window.Focus();
        }

    }
}