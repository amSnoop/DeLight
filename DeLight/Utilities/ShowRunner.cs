using Avalonia.Metadata;
using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Models;
using DeLight.Models.Files;
using DeLight.Views;
using System;
using System.Collections.ObjectModel;
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
        private VideoWindow videoWindow;//REMOVE THIS LATER. WILL MAKE SOME KIND OF VIDEO WINDOW MANAGER

        public event EventHandler? OnLoaded;

        public event EventHandler<CueChangedEventArgs>? CueChanged;

        public ShowRunner(Show show)
        {
            Show = show;
            oldCues = new();
            VideoWindow = new VideoWindow();
        }

        public void PrepareCues()
        {
            foreach (Cue cue in Show.Cues)
            {
                cue.LightScene = LightFile.CheckLightFile(cue.LightScene);
                foreach (var projFilePair in cue.ScreenFiles.ToList())
                {
                    cue.ScreenFiles[projFilePair.Key] = (ScreenFile)CueFile.ConvertCueFile(projFilePair.Value);//will always be a screen file, despite the class
                }
            }
            OnLoaded?.Invoke(this, EventArgs.Empty);
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
            CueChanged?.Invoke(this, new CueChangedEventArgs(cue, CueChangedEventArgs.ChangeType.Added, Show.Cues.Count - 1));//TODO: insert into list based on Cue Number
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
                    cueRunner.Stop();
                    OldCues.Remove(cueRunner);
                }
            Show.Cues.Remove(cue);
            CueChanged?.Invoke(this, new CueChangedEventArgs(cue, CueChangedEventArgs.ChangeType.Deleted, 0));
        }

        public void HideVideoWindow()
        {
            VideoWindow.Hide();
            Stop();
        }

        public void ShowVideoWindow()
        {
            VideoWindow.Show();
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
            VideoWindow.SetScreen(screen);
        }
    }
}
