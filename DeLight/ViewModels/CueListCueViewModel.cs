using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeLight.Models;
using DeLight.Models.Files;
using System;
using System.ComponentModel;
using System.Linq;

namespace DeLight.ViewModels
{
    public class CueListContextMenuButtonClickedEventArgs : EventArgs
    {
        public enum CueListContextMenuButton
        {
            Edit,
            Delete,
            Duplicate,
            Disable,
            Move
        }

        public CueListContextMenuButton Action { get; set; }
        
        public CueListContextMenuButtonClickedEventArgs(CueListContextMenuButton action)
        {
            Action = action;
        }
    }


    public partial class CueListCueViewModel : CueViewModel
    {

        [ObservableProperty]
        private bool selected;
        [ObservableProperty]
        private bool active;
        [ObservableProperty]
        private bool error;
        public event EventHandler<CueListContextMenuButtonClickedEventArgs>? ButtonClicked;

        public bool Disabled => Cue?.Disabled ?? true;

        public string Number => Cue?.Number.ToString() + Cue?.Letter ?? "!";

        public CueListCueViewModel(Cue? cue) : base(cue)
        {
            if (Cue != null)
                Cue.PropertyChanged += OnCuePropertyChanged;
            Error = CheckCueErrorState();
        }

        [RelayCommand]
        public void EditButtonClicked()
        {
            ButtonClicked?.Invoke(this, new(CueListContextMenuButtonClickedEventArgs.CueListContextMenuButton.Edit));

        }
        [RelayCommand]
        public void DeleteButtonClicked()
        {
            ButtonClicked?.Invoke(this, new(CueListContextMenuButtonClickedEventArgs.CueListContextMenuButton.Delete));
        }
        [RelayCommand]
        public void DuplicateButtonClicked()
        {
            ButtonClicked?.Invoke(this, new(CueListContextMenuButtonClickedEventArgs.CueListContextMenuButton.Duplicate));
        }
        [RelayCommand]
        public void DisableButtonClicked()
        {
            ButtonClicked?.Invoke(this, new(CueListContextMenuButtonClickedEventArgs.CueListContextMenuButton.Disable));
            if(Cue != null)
                Cue.Disabled = !Cue.Disabled;
        }
        [RelayCommand]
        public void MoveButtonClicked()
        {
            ButtonClicked?.Invoke(this, new(CueListContextMenuButtonClickedEventArgs.CueListContextMenuButton.Move));
        }

        private bool CheckCueErrorState()
        {
            if (Cue?.LightFile.ErrorState != FileErrorState.None || Cue?.ScreenFile.ErrorState != FileErrorState.None)
                return true;
            return false;
        }
        public override void OnCuePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender == Cue)
                if (e.PropertyName == nameof(Cue.Disabled))
                    OnPropertyChanged(nameof(Disabled));
            else if (e.PropertyName == nameof(Cue.Number) || e.PropertyName == nameof(Cue.Letter))
                    OnPropertyChanged(nameof(Number));
            Error = CheckCueErrorState();
        }
    }
}
