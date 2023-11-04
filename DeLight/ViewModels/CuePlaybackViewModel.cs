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


        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FormattedCurrentTime))]
        private double currentTime;

        public CuePlaybackViewModel(Cue? cue) : base(cue)
        {
            if (cue != null)
                cue.PropertyChanged += OnCuePropertyChanged;
            PropertyChanged += CueObjectChanged;
        }

        private void CueObjectChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Cue) && Cue != null)
            {
                OnCuePropertyChanged(Cue, new PropertyChangedEventArgs(""));
                Cue.PropertyChanged += OnCuePropertyChanged;
            }
        }

        public string FormattedCurrentTime => TimeSpan.FromSeconds(CurrentTime).ToString(@"hh\:mm\:ss");
        public string Note => Cue?.Note ?? "";
        public string Title => Cue == null ? "No Cue Selected" : "Settings for Cue #" + Cue.Number;
        public string FormattedNumber => Cue == null ? "" : "#" + Cue.Number + ": ";
        public double FadeInTime => Cue?.FadeInTime ?? GlobalSettings.Instance.DefaultCue.FadeInTime;
        public double FadeOutTime => Cue?.FadeOutTime ?? GlobalSettings.Instance.DefaultCue.FadeOutTime;
        public string FormattedDuration => " / " + TimeSpan.FromSeconds(RealDuration).ToString(@"hh\:mm\:ss");
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

        public override void OnCuePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(FadeInTime));
            OnPropertyChanged(nameof(FadeOutTime));
            OnPropertyChanged(nameof(Volume));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Note));
            OnPropertyChanged(nameof(FormattedNumber));
            OnPropertyChanged(nameof(FormattedDuration));
        }

        
    }
}
