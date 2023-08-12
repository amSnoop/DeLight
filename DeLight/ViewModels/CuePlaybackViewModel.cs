using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Models;
using DeLight.Utilities;
using System;
using System.ComponentModel;

namespace DeLight.ViewModels
{
    public partial class CuePlaybackViewModel : CueViewModel
    {

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FormattedDuration))]
        private double realDuration;

        public string FormattedDuration => " / " + TimeSpan.FromSeconds(Duration).ToString(@"hh\:mm\:ss");

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FormattedCurrentTime))]
        private double currentTime;

        public CuePlaybackViewModel(Cue cue) : base(cue)
        {
            cue.PropertyChanged += OnCuePropertyChanged;
            PropertyChanged += CueObjectChanged;

        }

        private void CueObjectChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Cue))
            {
                OnCuePropertyChanged(Cue, new PropertyChangedEventArgs(""));
                Cue.PropertyChanged += OnCuePropertyChanged;
            }
        }

        public string FormattedCurrentTime => TimeSpan.FromSeconds(CurrentTime).ToString(@"hh\:mm\:ss");

        public string Note => Cue?.Note ?? "";
        public string Title => Cue == null ? "No Cue Selected" : "Settings for Cue #" + Cue.Number;


        public string FormattedNumber => Cue == null ? "" : "#" + Cue.Number + ": ";

        #region Verified Properties
        public double FadeInTime {
            get => Cue?.FadeInTime ?? GlobalSettings.Instance.DefaultCue.FadeInTime;
            set
            {
                if(value < 0)
                    value = 0;
                if (Cue != null) {
                    Cue.FadeInTime = value;
                    OnPropertyChanged(nameof(FadeInTime));
                }
            }
        }
        public double FadeOutTime {
            get => Cue?.FadeOutTime ?? GlobalSettings.Instance.DefaultCue.FadeOutTime;
            set
            {
                if (value < 0)
                    value = 0;
                if (Cue != null) {
                    Cue.FadeOutTime = value;
                    OnPropertyChanged(nameof(FadeOutTime));
                }
            }
        }

        public double Volume {
              get => Cue?.Volume ?? GlobalSettings.Instance.DefaultCue.Volume;
            set
            {
                if (value < 0)
                    value = 0;
                if(value > 1)
                    value = 1;
                if (Cue != null) {
                    Cue.Volume = value;
                    OnPropertyChanged(nameof(Volume));
                }
            }
        }

        public double Duration
        {
            get => Cue?.Duration ?? GlobalSettings.Instance.DefaultCue.Duration;
            set
            {
                if (value < 0)
                    value = 0;
                if (Cue != null)
                {
                    Cue.Duration = value;
                    OnPropertyChanged(nameof(Duration));
                }
            }
        }

        #endregion

        public override void OnCuePropertyChanged(object? sender, PropertyChangedEventArgs e)
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
