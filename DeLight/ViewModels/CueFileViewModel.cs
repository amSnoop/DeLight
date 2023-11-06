using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeLight.Interfaces;
using DeLight.Models.Files;
using DeLight.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace DeLight.ViewModels
{
    public abstract partial class CueFileViewModel : ObservableObject
    {
        public ObservableCollection<string> EndActionStrings { get; } = new() {
            "Loop" ,
            "Fade After End" ,
            "Fade Before End" ,
            "Freeze"
        };

        [ObservableProperty]
        protected CueFile file;
        public string FileName => File.FilePath != null ? $" ({System.IO.Path.GetFileNameWithoutExtension(File.FilePath)})" : "";
        public bool DurationVisibility => File is IDurationFile;
        public bool VolumeVisibility => /*File is IAudioFile */ false;//Unused since only one video file per cue means Cue volume is easier to manage.

        public event EventHandler? FileTypeChanged;

        public string Header { get; } = "";
        public bool IsError => File.ErrorState != FileErrorState.None;
        protected void OnFileTypeChanged(object? sender)
        {
            FileTypeChanged?.Invoke(sender, EventArgs.Empty);
        }

        public RelayCommand<Avalonia.Controls.Window> OpenFileDialog { get; }

        protected virtual void OnPathChanged(string s)
        {
            OnPropertyChanged(nameof(FileName));
            OnPropertyChanged(nameof(DurationVisibility));
            OnPropertyChanged(nameof(VolumeVisibility));
            OnPropertyChanged(nameof(IsError));
        }

        public CueFileViewModel(CueFile file)
        {
            this.file = file;
            OpenFileDialog = new(OpenFileDialogAsync);
        }
        public async void OpenFileDialogAsync(Avalonia.Controls.Window? window)
        {

            string startupFolder = Path.GetDirectoryName(File.FilePath) ?? (File is LightFile ? GlobalSettings.Instance.LightShowDirectory : GlobalSettings.Instance.VideoDirectory);
            if (window is null)
                throw new Exception("Parent window is null");
            var st = await window.StorageProvider.TryGetFolderFromPathAsync(startupFolder);
            var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Source File",
                AllowMultiple = false,
                SuggestedStartLocation = st,
            });

            if (files.Count >= 1)
            {
                File.FilePath = Uri.UnescapeDataString(files[0].Path.AbsolutePath);
            }
        }
    }
}
