using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Models;
using DeLight.Utilities;
using System;
using System.Collections.Generic;

namespace DeLight.ViewModels
{
    public partial class CueViewModel : ObservableObject
    {

        [ObservableProperty]
        private Cue? currentCue;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FormattedDuration))]
        private double realDuration;

        public string FormattedDuration => " / " + TimeSpan.FromSeconds(Duration).ToString(@"hh\:mm\:ss");

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FormattedCurrentTime))]
        private double currentTime;

        public string FormattedCurrentTime => TimeSpan.FromSeconds(CurrentTime).ToString(@"hh\:mm\:ss");


        public string Title => CurrentCue == null ? "No Cue Selected" : "Settings for Cue #" + CurrentCue.Number;

        public string Note => CurrentCue?.Note ?? "";

        public string FormattedNumber => CurrentCue == null ? "" : "#" + CurrentCue.Number + ": ";

        #region Verified Properties
        public double FadeInTime {
            get => CurrentCue?.FadeInTime ?? GlobalSettings.Instance.DefaultCue.FadeInTime;
            set
            {
                if(value < 0)
                    value = 0;
                if (CurrentCue != null) {
                    CurrentCue.FadeInTime = value;
                    OnPropertyChanged(nameof(FadeInTime));
                }
            }
        }
        public double FadeOutTime {
            get => CurrentCue?.FadeOutTime ?? GlobalSettings.Instance.DefaultCue.FadeOutTime;
            set
            {
                if (value < 0)
                    value = 0;
                if (CurrentCue != null) {
                    CurrentCue.FadeOutTime = value;
                    OnPropertyChanged(nameof(FadeOutTime));
                }
            }
        }

        public double Volume {
              get => CurrentCue?.Volume ?? GlobalSettings.Instance.DefaultCue.Volume;
            set
            {
                if (value < 0)
                    value = 0;
                if(value > 1)
                    value = 1;
                if (CurrentCue != null) {
                    CurrentCue.Volume = value;
                    OnPropertyChanged(nameof(Volume));
                }
            }
        }

        public double Duration
        {
            get => CurrentCue?.Duration ?? GlobalSettings.Instance.DefaultCue.Duration;
            set
            {
                if (value < 0)
                    value = 0;
                if (CurrentCue != null)
                {
                    CurrentCue.Duration = value;
                    OnPropertyChanged(nameof(Duration));
                }
            }
        }

        #endregion

        partial void OnCurrentCueChanged(Cue? value)
        {
            OnPropertyChanged(nameof(FadeInTime));
            OnPropertyChanged(nameof(FadeOutTime));
            OnPropertyChanged(nameof(Volume));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Note));
            OnPropertyChanged(nameof(FormattedNumber));
        }
    }
}
