using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Models;
using DeLight.Models.Files;
using DeLight.Utilities.LightingOutput;
using DeLight.Utilities.VideoOutput;
using DeLight.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DeLight.Utilities
{
    public partial class ShowRunner : ObservableObject
    {
        [ObservableProperty]
        private Show show;

        public event EventHandler? OnLoaded, Sorted;

        public ShowRunner(Show show)
        {
            Show = show;
            Messenger.SeekTo += SeekTo;
        }


        #region Setup Commands

        //Check files for errors and set up cue property change notifcations
        public void PrepareCues()
        {
            foreach (Cue cue in Show.Cues)
            {
                cue.SetLightFile(LightFile.CheckLightFile(cue.LightFile));
                cue.SetScreenFile(ScreenFile.ConvertCueFile(cue.ScreenFile));
                cue.PropertyChanged += Cue_PropertyChanged;
            }
            OnLoaded?.Invoke(this, EventArgs.Empty);
        }

        public void AddCue(Cue cue)
        {
            Show.Cues.Add(cue);
            Sort();
        }

        public void DeleteCue(Cue cue)
        {
            if (ActiveCue?.Cue == cue)
            {
                Stop(ActiveCue);
                ActiveCue = null;
            }
            if (OldCue?.Cue == cue)
            {
                Stop(OldCue);
                OldCue = null;
            }
            Show.Cues.Remove(cue);
        }

        #endregion


        #region Playback Commands


        //Load a new cue and play it
        public void Go(Cue cue)
        {
            LightCue lc = new(cue.LightFile);
            IRunnableScreenCue sc;
            var sf = cue.ScreenFile;
            if (sf.ErrorState != FileErrorState.None)
            {
                sc = new BlackoutScreenCue(new() { FadeInDuration = sf.FadeInDuration });
            }
            else if (sf is VideoFile vf)
            {
                sc = new VideoMediaElement(vf);
            }
            else if (sf is ImageFile imgf)
            {
                sc = new ImageMediaElement(imgf);
            }
            else
            {
                sc = new BlackoutScreenCue(new() { FadeInDuration = sf.FadeInDuration });
            }
            LightingManager.UpdateCue(lc);
            VideoManager.UpdateCue(sc);
        }

        public void Stop()
        {
            VideoManager.Stop();
            LightingManager.Stop();
        }
        public void Pause()
        {
            LightingManager.Pause();
            VideoManager.Pause();
        }
        public void Unpause()
        {
            VideoManager.Play();
            LightingManager.Play();
        }
        public void SeekTo(double time, bool play)
        {
            //This should allow the old cue to be properly shown and play during hte FadeIn sequence of the new cue.
            OldCue?.SeekTo(time, play);
            ActiveCue?.SeekTo(time, play);
        }

        #endregion






        #region Internal Methods

        private void Stop(CueRunner? cr)
        {
            if (cr is not null)
            {
                cr.Stop();
                VideoWindow?.Container.Children.Remove(cr.ScreenFile.GetUIElement());
            }
        }


        //Sort the list if a cue's number or letter changed
        private void Cue_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is Cue && (e.PropertyName == nameof(Cue.Number) || e.PropertyName == nameof(Cue.Letter)))
            {
                Sort();
            }
        }

        private void Sort()
        {
            Show.Cues = new(Show.Cues.OrderBy(c => c.Number).ThenBy(c => c.Letter));
            Sorted?.Invoke(this, EventArgs.Empty);
        }
        private void OnCueRunnerFadedIn(object? sender, EventArgs e)
        {
            if (sender is CueRunner cueRunner)
            {
                OldCue?.Pause();
            }
        }
        #endregion

    }
}
