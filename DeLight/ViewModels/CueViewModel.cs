using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Models;
using System.ComponentModel;

namespace DeLight.ViewModels
{
    public partial class CueViewModel : ObservableObject
    {

        private Cue? cue;

        public Cue? Cue
        {
            get => cue;
            set
            {
                if(cue != null)
                    cue.PropertyChanged -= OnCuePropertyChanged;
                SetProperty(ref cue, value);
            }
        }
        public CueViewModel()
        {
        }
        public CueViewModel(Cue? cue)
        {
            this.cue = cue;
        }

        public virtual void OnCuePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
        }
    }
}