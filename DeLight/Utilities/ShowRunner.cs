using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Metadata;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Models;
using DeLight.Models.Files;
using DeLight.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace DeLight.Utilities
{

    public class CueChangedEventArgs : EventArgs
    {
        public enum ChangeType
        {
            Added,
            Deleted,
            Modified//unused
        }
        public Cue Cue { get; set; }
        public ChangeType ActionTaken { get; set; }

        public int Index { get; set; }
        public CueChangedEventArgs(Cue cue, ChangeType actionTaken, int index)
        {
            Cue = cue;
            ActionTaken = actionTaken;
            Index = index;
        }
    }
    public partial class ShowRunner : ObservableObject
    {
        [ObservableProperty]
        private Show show;
        [ObservableProperty]
        private ObservableCollection<CueRunner> oldCues;

        [ObservableProperty]
        private CueRunner? activeCue;
        [ObservableProperty]
        private VideoWindow? videoWindow;//TODO: REMOVE THIS LATER. WILL MAKE SOME KIND OF VIDEO WINDOW MANAGER

        public event EventHandler? OnLoaded, Sorted;

        public event EventHandler<CueChangedEventArgs>? CueChanged;

        public ShowRunner(Show show)
        {
            Show = show;
            oldCues = new();
            if (!Design.IsDesignMode)
                VideoWindow = new VideoWindow();
            else
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    VideoWindow = new VideoWindow();
                });
            }
        }

        public void PrepareCues()
        {
            foreach (Cue cue in Show.Cues)
            {
                cue.LightFile = LightFile.CheckLightFile(cue.LightFile);
                cue.ScreenFile =  ScreenFile.ConvertCueFile(cue.ScreenFile);
                cue.PropertyChanged += Cue_PropertyChanged;
            }
            OnLoaded?.Invoke(this, EventArgs.Empty);
        }

        private void Cue_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(sender is Cue && (e.PropertyName==nameof(Cue.Number) || e.PropertyName == nameof(Cue.Letter)))
            {
                Sort();
            }
        }

        public void Play(Cue cue)
        {
            ShowVideoWindow();
            CueRunner newCueRunner = new(cue, VideoWindow);
            ActiveCue = newCueRunner;
            newCueRunner.FadedIn += FadedIn;
            newCueRunner.FadedOut += (sender, e) => OldCues.Remove((CueRunner)sender!);
            newCueRunner.Play();
        }


        public void Stop()
        {
            ActiveCue?.Stop();
            foreach (CueRunner cueRunner in OldCues.ToList())
                cueRunner.Stop();
            OldCues.Clear();
        }

        public void Pause()
        {
            ActiveCue?.Pause();
            foreach (CueRunner cueRunner in OldCues.ToList())
                cueRunner.Pause();
        }
        public void Unpause()
        {
            ActiveCue?.Unpause();
            foreach (CueRunner cueRunner in OldCues.ToList())
                cueRunner.Unpause();
        }
        public void SeekTo(int tick)
        {
            foreach (CueRunner cueRunner in OldCues.ToList())//temporary fix for seeking
            {
                OldCues.Remove(cueRunner);
                cueRunner.Stop();
            }
            ActiveCue?.SeekTo(tick);
        }

        public void AddCue(Cue cue)
        {
            Show.Cues.Add(cue);
            Sort();
        }

        public void Sort()
        {
            Show.Cues = new(Show.Cues.OrderBy(c => c.Number).ThenBy(c => c.Letter));
            Sorted?.Invoke(this, EventArgs.Empty);
        }
        public void DeleteCue(Cue cue)
        {
            if (ActiveCue?.Cue == cue)
            {
                ActiveCue.Stop();
                ActiveCue = null;
            }
            else
                foreach (var cueRunner in OldCues.ToList())
                {
                    if (cueRunner.Cue == cue)
                    {
                        cueRunner.Stop();
                        OldCues.Remove(cueRunner);
                    }
                }
            Show.Cues.Remove(cue);
        }

        public void HideVideoWindow()
        {
            VideoWindow?.Hide();
            Stop();
        }

        public void ShowVideoWindow()
        {
            VideoWindow?.Show();
        }


        public void FadedIn(object? sender, EventArgs e)
        {
            if (sender is CueRunner cueRunner)
            {
                cueRunner.FadedIn -= FadedIn;
                foreach (CueRunner activeCue in OldCues.ToList())
                    if (activeCue != cueRunner)
                        activeCue.Stop();
                OldCues.Clear();
                OldCues.Add(cueRunner);
            }
        }
        public void SetVideoScreen(Screen screen)
        {
            VideoWindow?.SetScreen(screen);
        }
    }
}
