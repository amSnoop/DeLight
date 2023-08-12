using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Models;
using DeLight.Models.Files;
using System.ComponentModel;
using System.Linq;

namespace DeLight.ViewModels
{
    public partial class CueListCueViewModel : CueViewModel
    {

        [ObservableProperty]
        private bool selected;
        [ObservableProperty]
        private bool active;
        [ObservableProperty]
        private bool error;

        public bool Disabled => Cue.Disabled;

        public CueListCueViewModel(Cue cue) : base(cue)
        {
            Cue.PropertyChanged += OnCuePropertyChanged;
            Error = CheckCueErrorState();
        }


        private bool CheckCueErrorState()
        {
            if (Cue.LightScene.ErrorState != FileErrorState.None)
                return true;
            foreach (var file in Cue.ScreenFiles.Values.ToList())
                if (file.ErrorState != FileErrorState.None)
                    return true;
            return false;
        }
        private void OnCuePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is Cue)
            {
                if (e.PropertyName == nameof(Cue.Disabled))
                {
                    OnPropertyChanged(nameof(Disabled));
                }
            }
        }
    }
}
