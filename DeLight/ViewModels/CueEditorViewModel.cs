using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Models;
using DeLight.Models.Files;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.ConstrainedExecution;

namespace DeLight.ViewModels
{

    public partial class CueEditorViewModel : ObservableObject
    {
        public static List<string> EndActionStrings { get; } = new() {
            "Loop" ,
            "Fade After End" ,
            "Fade Before End" ,
            "Freeze"
        };

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Visible))]
        private Cue? cue;

        [ObservableProperty]
        private double duration;

        public bool Visible => Cue != null;

        //only values that need validation are created here, all others directly bind to the cue.
        [ObservableProperty]
        private int number;
        [ObservableProperty]
        private string durationString;
        [ObservableProperty]
        private int volume;
        [ObservableProperty]
        private LightFileViewModel? lightFile;
        [ObservableProperty]
        private ScreenFileViewModel? screenFile;

        [ObservableProperty]
        private ObservableCollection<CueFileViewModel> cues = new();
        public CueEditorViewModel(Cue? cue)
        {
            if (cue is null)
            {
                this.cue = cue;
                durationString = "";

                return;
            }
            this.cue = cue;
            number = cue.Number;
            durationString = cue.Duration.ToString();
            duration = cue.Duration;
            volume = (int)Math.Round(cue.Volume * 100);
            lightFile = new(cue.LightFile);
            screenFile = new(cue.ScreenFile);
            lightFile.FileTypeChanged += (s, e) => OnFileTypeChanged(((LightFileViewModel)s!).File);
            screenFile.FileTypeChanged += (s, e) => OnFileTypeChanged(((ScreenFileViewModel)s!).File);
            cues.Add(lightFile);
            cues.Add(screenFile);
            OnPropertyChanged(nameof(Visible));
        }

        private void OnFileTypeChanged(CueFile f)
        {
            if (f is LightFile lf)
                Cue?.SetLightFile(lf);
            else
                Cue?.SetScreenFile((ScreenFile)f);
        }
        public bool Validate(string propName, object value)
        {
            switch (propName)
            {
                case nameof(Number):
                    if ((int)value < 0)
                        return false;
                    Number = (int)value;
                    break;
                case nameof(DurationString):
                    var d = (double)value;
                    if (d < 0)
                        return false;
                    Duration = d;
                    break;
                case nameof(Volume):
                    Volume = (int)value;
                    break;
            }
            return true;
        }


        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (Cue is not null)
                switch (e.PropertyName)
                {
                    case nameof(Number):
                        Cue.Number = Number;
                        break;
                    case nameof(Duration):
                        Cue.Duration = Duration;
                        break;
                    case nameof(Volume):
                        Cue.Volume = Math.Clamp(Volume / 100.0, 0, 1);
                        break;
                }
        }
    }
}

