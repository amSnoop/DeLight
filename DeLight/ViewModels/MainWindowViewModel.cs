using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Windows.Forms;
using DeLight.Utilities;
using System.Diagnostics;
using System.Collections.ObjectModel;
using DeLight.Models;

namespace DeLight.ViewModels
{

    public partial class MainWindowViewModel : ObservableObject
    {

        private List<Screen> _screenObjects;

        private Screen? selectedScreen;

        private readonly ShowRunner showRunner;

        #region Font Size Properties
        [ObservableProperty]
        private double bodyFontSize = 12;
        [ObservableProperty]
        private double cueFontSize = 16;
        [ObservableProperty]
        private double subtitleFontSize = 16;
        [ObservableProperty]
        private double titleFontSize = 30;



        public void UpdateWindowSize(double height)
        {
            double baseFontSize = 12;
            double scaleFactor = 1 + Math.Log(height / 720.0);
            double maxFontSize = 25;
            double fontSize = Math.Max(Math.Min(baseFontSize * scaleFactor, maxFontSize), baseFontSize);
            double subTitleFontFactor = 1.5;
            double titleFontFactor = 3;
            double cueFontFactor = 1.2;
            BodyFontSize = fontSize;
            CueFontSize = BodyFontSize * cueFontFactor;
            SubtitleFontSize = BodyFontSize * subTitleFontFactor;
            TitleFontSize = BodyFontSize * titleFontFactor;
        }

        #endregion

        [ObservableProperty]
        private string selectedMonitor = "";
        [ObservableProperty]
        private List<string> monitors = new();

        [ObservableProperty]
        private CuePlaybackViewModel? cuePlaybackViewModel;
        public ObservableCollection<CueListCueViewModel> Cues { get; set; }

        [ObservableProperty]
        public CueListCueViewModel? selectedCue;

        public MainWindowViewModel(ShowRunner runner)
        {
            _screenObjects = new();
            showRunner = runner;
            ConfigureMonitorDisplay();
            Cues = new();
            showRunner.OnLoaded += ShowRunner_OnLoaded;
            showRunner.CueChanged += ShowRunner_CueChanged;
            showRunner.PrepareCues();
        }

        partial void OnSelectedCueChanging(CueListCueViewModel? value)
        {
            if (SelectedCue != null)
                SelectedCue.Selected = false;
            if (value != null)
                value.Selected = true;
        }


        public void ShowRunner_OnLoaded(object? sender, EventArgs e)
        {
            Cues.Clear();
            foreach (var cue in showRunner.Show.Cues)
            {
                Cues.Add(new CueListCueViewModel(cue));
            }
            CuePlaybackViewModel = new CuePlaybackViewModel(showRunner.Show.Cues.FirstOrDefault());
        }

        public void ShowRunner_CueChanged(object? sender, CueChangedEventArgs e)
        {
            if (e.ActionTaken == CueChangedEventArgs.ChangeType.Added)
            {
                if (e.Index < Cues.Count)
                {
                    Cues.Insert(e.Index, new CueListCueViewModel(e.Cue));
                }
                else
                {
                    Cues.Add(new CueListCueViewModel(e.Cue));
                }
            }
            else if (e.ActionTaken == CueChangedEventArgs.ChangeType.Deleted)
            {
                foreach (var cue in Cues)
                {
                    if (cue.Cue == e.Cue)
                    {
                        Cues.Remove(cue);
                        break;
                    }
                }
            }
        }

        #region Monitor Selection
        private void ConfigureMonitorDisplay()
        {
            _screenObjects = Screen.AllScreens.ToList();
            Monitors = _screenObjects.Select((s, i) => $"Monitor {i + 1}: {s.Bounds.Width}x{s.Bounds.Height}").ToList();
            if (Screen.PrimaryScreen == null)
            {
                SelectedMonitor = Monitors.FirstOrDefault() ?? "";
                showRunner.SetVideoScreen(_screenObjects.First());
            }
            else
            {
                SelectedMonitor = Monitors[_screenObjects.IndexOf(Screen.PrimaryScreen)];
                showRunner.SetVideoScreen(Screen.PrimaryScreen);
            }
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
        }

        private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
        {
            var screen = selectedScreen;
            _screenObjects = Screen.AllScreens.ToList();
            Monitors = _screenObjects.Select((s, i) => $"Monitor {i + 1}: {s.Bounds.Width}x{s.Bounds.Height}").ToList();
            if (screen != null)
                SelectedMonitor = Monitors[_screenObjects.IndexOf(screen)];
            else
                SelectedMonitor = Monitors.FirstOrDefault() ?? "";
        }

        partial void OnSelectedMonitorChanged(string value)
        {
            if (value == null)
            {
                Debug.WriteLine("Selected monitor is null");
                return;
            }
            var screen = _screenObjects[Monitors.IndexOf(SelectedMonitor)];
            showRunner.SetVideoScreen(screen);
            selectedScreen = screen;
        }

        #endregion

        public void PlayCue()
        {
            SelectedCue ??= Cues.FirstOrDefault();
            var curCue = Cues.FirstOrDefault(c => c.Cue == showRunner?.ActiveCue?.Cue);
            if (curCue != null)
                curCue.Active = false;
            if (SelectedCue != null && SelectedCue.Cue != null)
            {
                showRunner.Play(SelectedCue.Cue);
                SelectedCue.Active = true;
                if (CuePlaybackViewModel == null)
                    CuePlaybackViewModel = new CuePlaybackViewModel(SelectedCue.Cue);
                else
                    CuePlaybackViewModel.Cue = SelectedCue.Cue;

                for (int i = Cues.IndexOf(SelectedCue) + 1; i < Cues.Count; i++)
                    if (!Cues[i].Disabled)
                    {
                        SelectedCue = Cues[i];
                        return;
                    }
                SelectedCue = null;
            }
        }
        public void StopCue()
        {
            showRunner.Stop();
        }
        public void PauseCue()
        {
            showRunner.Pause();
        }
        public void ResumeCue()
        {
            showRunner.Unpause();
        }
        public void SeekTo(int tick)
        {
            showRunner.SeekTo(tick);
        }

        public void HideVideoWindow()
        {
            showRunner.Stop();
            showRunner.HideVideoWindow();
        }
        public void ShowVideoWindow()
        {
            showRunner.ShowVideoWindow();
        }

        public void DeleteCue(Cue? cue)
        {
            if (cue != null)
                showRunner.DeleteCue(cue);
        }

        public void InsertCue(Cue? cue, bool useLetters)
        {
            if (cue != null)
                showRunner.AddCue(cue, useLetters);
        }
        public void UpdateCue(Cue? cue, bool useLetters)
        {
            if (cue != null)
                showRunner.UpdateCue(cue, useLetters);
        }
    }

}