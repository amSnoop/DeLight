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
using System.Threading.Tasks;

namespace DeLight.Utilities
{
    public partial class ShowRunner : ObservableObject
    {
        [ObservableProperty]
        private Show show;

        public event EventHandler? OnLoaded, Sorted;

        [ObservableProperty]
        private Cue? activeCue;
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
            if (cue == ActiveCue)
            {
                Stop();
            }
            Show.Cues.Remove(cue);
        }

        #endregion


        #region Playback Commands


        //Load a new cue and play it
        public async Task Go(Cue? cue)
        {
            ActiveCue = cue;
            await LightingManager.UpdateCue(cue);
            await VideoManager.UpdateCue(cue);
        }

        public void Stop()
        {
            ActiveCue = null;
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
            VideoManager.SeekTo(time, play);
            LightingManager.SeekTo(time, play);
        }

        #endregion






        #region Internal Methods


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

        #endregion

    }
}
