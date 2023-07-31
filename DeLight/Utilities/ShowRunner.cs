using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Models;
using DeLight.Models.Files;
using DeLight.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeLight.Utilities
{
    public partial class ShowRunner : ObservableObject
    {
        public Show Show { get; set; }

        [ObservableProperty]
        private Cue? selectedCue;
        [ObservableProperty]
        private Cue? activeCue;

        public bool ShowComplete = false;

        public VideoWindow VideoWindow { get; set; }

        public List<CueRunner> ActiveCues { get; set; } = new();

        public ShowRunner(Show show, VideoWindow videoWindow)
        {
            Show = show;
            VideoWindow = videoWindow;
            Prepare();
        }

        public void Prepare()
        {
            foreach (Cue cue in Show.Cues)
            {
                cue.Ready = true;
                if (cue.LightScene is BlackoutLightFile blf && blf.Reason != BlackoutReason.EmptyPath)
                    cue.Ready = false;
                foreach (ScreenFile screenFile in cue.ScreenFiles.Values)
                    if (screenFile is BlackoutScreenFile bsf && bsf.Reason != BlackoutReason.EmptyPath)
                        cue.Ready = false;
            }
        }

        public void Play()
        {
            if (ShowComplete && SelectedCue == null)
                return;
            if (ActiveCue != null)
            {
                ActiveCue.IsActive = false;
                foreach (CueRunner cueRunner in ActiveCues)
                    cueRunner.End();
            }

            // The show was over, but a new cue is manually selected
            ShowComplete = ShowComplete && SelectedCue == null;

            SelectedCue ??= Show.Cues.FirstOrDefault();

            if (SelectedCue != null)
            {
                ActiveCue = SelectedCue;
                int index = Show.Cues.IndexOf(SelectedCue);

                SelectedCue = index < Show.Cues.Count - 1 ? Show.Cues[index + 1] : null;
                ShowComplete = SelectedCue == null;

                VideoWindow.Show();
                CueRunner cueRunner = new(ActiveCue, VideoWindow);
                ActiveCues.Add(cueRunner);
                cueRunner.Play();
                cueRunner.FadedOut += CueFadedOut;
                ActiveCue.IsActive = true;
            }
        }
        public void CueFadedOut(object? sender, EventArgs e)
        {
            if (sender != null)
            {
                CueRunner cueRunner = (CueRunner)sender;
                cueRunner.FadedOut -= CueFadedOut;
                ActiveCues.Remove(cueRunner);
                cueRunner.Cue.IsActive = false;
            }
        }
        public void Stop()
        {
            foreach (CueRunner cueRunner in ActiveCues)
                cueRunner.Stop();
            ActiveCues.Clear();
        }

    }
}
