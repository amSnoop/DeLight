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
using DeLight.Utilities.VideoOutput;
using System.Threading.Tasks;
using System.IO;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Microsoft.CodeAnalysis.CSharp;

namespace DeLight.ViewModels
{

    public partial class MainWindowViewModel : ObservableObject
    {

        private List<Screen> _screenObjects;

        private Screen? selectedScreen;

        private ShowRunner showRunner;

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
        private ObservableCollection<string> monitors = new();

        [ObservableProperty]
        private CuePlaybackViewModel? cuePlaybackViewModel;

        [ObservableProperty]
        private ObservableCollection<CueListCueViewModel> cues;

        [ObservableProperty]
        public CueListCueViewModel? selectedCue;
        public string WindowTitle => "DeLight" + ((showRunner?.Show?.Name ?? "") == "" ? "" : " - " + showRunner!.Show.Name);

        public MainWindowViewModel(ShowRunner runner)
        {
            _screenObjects = new();
            showRunner = runner;
            ConfigureMonitorDisplay();
            Cues = new();
            showRunner.OnLoaded += ShowRunner_OnLoaded;
            showRunner.PrepareCues();
            GlobalSettings.Instance.LastShowPath = showRunner.Show.Path;
        }
        public void LoadShow(string filepath)
        {
            showRunner.Stop();
            SaveShow();
            showRunner = new(Show.Load(filepath));
            showRunner.OnLoaded += ShowRunner_OnLoaded;
            showRunner.PrepareCues();
            GlobalSettings.Instance.LastShowPath = showRunner.Show.Path;
            OnPropertyChanged(nameof(WindowTitle));
        }
        public void SaveShow()
        {
            Show.Save(showRunner.Show);
            GlobalSettings.Instance.LastShowPath = showRunner.Show.Path;
        }
        public void NewShow(string path)
        {
            showRunner.Stop();
            SaveShow();
            showRunner = new(new Show() { Path = path, Name = string.Join(".", Path.GetFileName(path).Split('.')[..1]) });
            showRunner.OnLoaded += ShowRunner_OnLoaded;
            showRunner.PrepareCues();
            OnPropertyChanged(nameof(WindowTitle));
        }
        partial void OnSelectedCueChanging(CueListCueViewModel? value)
        {
            if (SelectedCue != null)
                SelectedCue.Selected = false;
            if (value != null)
                value.Selected = true;
        }

        public void ShowRunner_Sorted(object? sender, EventArgs e)
        {
            var selCue = SelectedCue?.Cue;
            Cues.Clear();
            foreach (var cue in showRunner.Show.Cues)
            {
                Cues.Add(new CueListCueViewModel(cue));
            }
            SelectedCue = Cues.FirstOrDefault(c => c.Cue == selCue);
        }
        public void ShowRunner_OnLoaded(object? sender, EventArgs e)
        {
            Cues.Clear();
            foreach (var cue in showRunner.Show.Cues)
            {
                Cues.Add(new CueListCueViewModel(cue));
            }
            CuePlaybackViewModel = new CuePlaybackViewModel(showRunner.Show.Cues.FirstOrDefault());
            showRunner.Sorted += ShowRunner_Sorted;
        }

        #region Monitor Selection
        private void ConfigureMonitorDisplay()
        {
            _screenObjects = Screen.AllScreens.ToList();
            Monitors = new(_screenObjects.Select((s, i) => $"Monitor {i + 1}: {s.Bounds.Width}x{s.Bounds.Height}"));
            if (GlobalSettings.Instance.Screen == null)
                if (Screen.PrimaryScreen == null)
                {
                    SelectedMonitor = Monitors.FirstOrDefault() ?? "";
                    VideoManager.SetVideoScreen(_screenObjects.First());
                }
                else
                {
                    SelectedMonitor = Monitors[_screenObjects.IndexOf(Screen.PrimaryScreen)];
                    VideoManager.SetVideoScreen(Screen.PrimaryScreen);
                }
            else
            {
                SelectedMonitor = Monitors[_screenObjects.IndexOf(GlobalSettings.Instance.Screen)];
                VideoManager.SetVideoScreen(GlobalSettings.Instance.Screen);
            }
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
        }

        private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
        {
            var screen = selectedScreen;
            _screenObjects = Screen.AllScreens.ToList();
            Monitors = new(_screenObjects.Select((s, i) => $"Monitor {i + 1}: {s.Bounds.Width}x{s.Bounds.Height}"));
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
            VideoManager.SetVideoScreen(screen);
            selectedScreen = screen;
            GlobalSettings.Instance.Screen = screen;
        }

        #endregion

        public async Task PlayCue()
        {
            SelectedCue ??= Cues.FirstOrDefault();
            var curCue = Cues.FirstOrDefault(c => c.Cue == showRunner?.ActiveCue);
            if (curCue != null)
                curCue.Active = false;
            if (SelectedCue != null && SelectedCue.Cue != null)
            {
                await showRunner.Go(SelectedCue.Cue);
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
            return;
        }
        public void StopCue()
        {
            foreach (var cue in Cues)
                cue.Active = false;
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
        public void SeekTo(double time, bool play)
        {
            showRunner.SeekTo(time, play);
        }

        public void HideVideoWindow()
        {
            SaveShow();
            foreach (var cue in Cues)
                cue.Active = false;
            showRunner.Stop();
            VideoManager.HideVideoWindow();
        }
        public void ShowVideoWindow()
        {
            VideoManager.ShowVideoWindow();
        }

        public void DeleteCue(Cue? cue)
        {
            if (cue != null)
                showRunner.DeleteCue(cue);
        }

        public void InsertCue(Cue? cue)
        {
            if (cue != null)
                showRunner.AddCue(cue);
        }
    }

}