using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeLight.Interfaces;
using DeLight.Models;
using DeLight.Models.Files;
using DeLight.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace DeLight.ViewModels
{
    public enum ExpectedFileType
    {
        Image,
        Video,
        Gif,
        Audio,
        Blackout,
        Lights
    }
    public partial class CueFileViewModel : ObservableObject
    {
        [ObservableProperty]
        private string path;
        [ObservableProperty]
        private double volume;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DurationVisibility))]
        [NotifyPropertyChangedFor(nameof(VolumeVisibility))]
        [NotifyPropertyChangedFor(nameof(Header))]
        private ExpectedFileType type;
        [ObservableProperty]
        private EndAction endAction;
        [ObservableProperty]
        private double fadeInTime;
        [ObservableProperty]
        private double fadeOutTime;
        [ObservableProperty]
        private double duration;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Header))]
        private FileErrorState state;

        private int _index;
        public string HeaderStart => _index == 0 ? "Light Scene" : $"Projector {_index}";

        public string HeaderEnd => ": " + (Type == ExpectedFileType.Lights ? "Ready" : Type.ToString());

        public string ReasonString => State != FileErrorState.None ? $" ({State})" : "";

        public string FileName => Path != null ? $" ({System.IO.Path.GetFileNameWithoutExtension(Path)})" : "";

        public string Header => HeaderStart + HeaderEnd + (Type == ExpectedFileType.Blackout ? ReasonString : FileName);
        public bool DurationVisibility => Type == ExpectedFileType.Lights || Type == ExpectedFileType.Image;

        public bool VolumeVisibility => Type == ExpectedFileType.Video;


        public RelayCommand<Window> OpenFileDialog { get; }

        public CueFileViewModel(CueFile file, int index)
        {
            _index = index;
            path = file.FilePath;
            volume = file is VideoFile vf ? vf.Volume : 1;
            endAction = file.EndAction;
            fadeInTime = file.FadeInDuration;
            fadeOutTime = file.FadeOutDuration;
            duration = file is IDurationFile df ? df.Duration : 0;
            OpenFileDialog = new(OpenFileDialogAsync);

            OnPathChanged(Path);//I am only doing this because I cannot decide if I want to let the initial type be influenced by the incoming type or not
        }
        partial void OnPathChanged(string value)
        {
            string ext = System.IO.Path.GetExtension(value).ToLower();

            // Define file type groups
            var videoExtensions = new[] { ".mp4", ".avi", ".mov", ".mkv", ".flv", ".wmv" };
            var imageExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".tiff" };
            var audioExtensions = new[] { ".mp3", ".wav", ".ogg", ".flac", ".aac" };

            if (string.IsNullOrEmpty(value))
            {
                Type = ExpectedFileType.Blackout;
                State = FileErrorState.None;
            }
            else if (!File.Exists(value))
            {
                State = FileErrorState.InvalidPath;
                Type = ExpectedFileType.Blackout;
            }
            else if (ext == ".scex" && _index == 0)
            {
                Type = ExpectedFileType.Lights;
            }
            else if (ext == ".scex" && _index != 0)
            {
                State = FileErrorState.InvalidFileType;
                Type = ExpectedFileType.Blackout;
            }
            else if (videoExtensions.Contains(ext))
            {
                Type = ExpectedFileType.Video;
            }
            else if (ext == ".gif")  //GIF can also be treated as an image, it's up to you
            {
                Type = ExpectedFileType.Gif;
            }
            else if (imageExtensions.Contains(ext))
            {
                Type = ExpectedFileType.Image;
            }
            else if (audioExtensions.Contains(ext))
            {
                Type = ExpectedFileType.Audio;
            }
            else
            {
                State = FileErrorState.InvalidFileType;
                Type = ExpectedFileType.Blackout;
            }
        }

        public async void OpenFileDialogAsync(Window? window)
        {

            string startupFolder = System.IO.Path.GetDirectoryName(Path) ?? (_index == 0 ? GlobalSettings.Instance.LightShowDirectory : GlobalSettings.Instance.VideoDirectory);
            if (window is null)
                throw new Exception("Window is null");
            var st = await window.StorageProvider.TryGetFolderFromPathAsync(startupFolder);
            var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Source File",
                AllowMultiple = false,
                SuggestedStartLocation = st,
            });

            if (files.Count >= 1)
            {
                Path = Uri.UnescapeDataString(files[0].Path.AbsolutePath);
            }
        }


    }
    public partial class CueEditorViewModel : ObservableObject
    {
        public static List<string> FadeTypeStrings { get; } = new() {
            "ShowXPress",
            "Fade Over"
        };
        public static List<string> EndActionStrings { get; } = new() {
            "Loop" ,
            "Fade After End" ,
            "Fade Before End" ,
            "Freeze"
        };

        private Cue cue;
        private bool isNew;
        public bool IsSaved { get; private set; } = true;

        public int NumProjectors => GlobalSettings.Instance.NumProjectors;


        [ObservableProperty]
        private string number;
        [ObservableProperty]
        private string note;
        [ObservableProperty]
        private double fadeInTime;
        [ObservableProperty]
        private double fadeOutTime;
        [ObservableProperty]
        private double duration;
        [ObservableProperty]
        private double volume;
        [ObservableProperty]
        private FadeType fadeType;
        [ObservableProperty]
        private EndAction cueEndAction;
        [ObservableProperty]
        private List<CueFileViewModel?> files = new();
        [ObservableProperty]
        private bool isDefault;
        public CueEditorViewModel(Cue cue, bool isDefault = false)
        {
            this.cue = cue;
            isNew = cue.Number == "0";
            IsDefault = isDefault;
            number = cue.Number;
            note = cue.Note;
            fadeInTime = cue.FadeInTime;
            fadeOutTime = cue.FadeOutTime;
            duration = cue.Duration;
            volume = cue.Volume;
            fadeType = cue.FadeType;
            cueEndAction = cue.CueEndAction;
            files.Add(new(cue.LightScene, 0));
            foreach (var file in cue.ScreenFiles)
            {
                while (files.Count <= file.Key)
                    files.Add(null);
                files[file.Key] = new(file.Value, file.Key);
            }
        }

        public void Save()
        {
            IsSaved = true;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            IsSaved = false;
        }
    }
}
