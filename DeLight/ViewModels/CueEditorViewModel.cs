using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeLight.Interfaces;
using DeLight.Models;
using DeLight.Models.Files;
using DeLight.Utilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

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
        [NotifyPropertyChangedFor(nameof(DurationVisibility))]
        [NotifyPropertyChangedFor(nameof(VolumeVisibility))]
        [NotifyPropertyChangedFor(nameof(Header))]
        private string path;
        [ObservableProperty]
        private double volume;
        [ObservableProperty]
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

        public int Index;
        public string HeaderStart => Index == 0 ? "Light Scene" : $"Projector {Index}";

        public string HeaderEnd => ": " + (Type == ExpectedFileType.Lights ? "Ready" : Type.ToString());

        public string ReasonString => State != FileErrorState.None ? $" ({State})" : "";

        public string FileName => Path != null ? $" ({System.IO.Path.GetFileNameWithoutExtension(Path)})" : "";

        public string Header => HeaderStart + HeaderEnd + (Type == ExpectedFileType.Blackout ? ReasonString : FileName);
        public bool DurationVisibility => Type == ExpectedFileType.Lights || Type == ExpectedFileType.Image;

        public bool VolumeVisibility => Type == ExpectedFileType.Video;


        public RelayCommand<Avalonia.Controls.Window> OpenFileDialog { get; }

        public CueFileViewModel(CueFile file, int index)
        {
            Index = index;
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
            else if (ext == ".scex" && Index == 0)
            {
                Type = ExpectedFileType.Lights;
            }
            else if (ext != ".scex" && Index == 0)
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

        public async void OpenFileDialogAsync(Avalonia.Controls.Window? window)
        {

            string startupFolder = System.IO.Path.GetDirectoryName(Path) ?? (Index == 0 ? GlobalSettings.Instance.LightShowDirectory : GlobalSettings.Instance.VideoDirectory);
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
        private bool useLetters = false;
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
        private ObservableCollection<CueFileViewModel?> files = new();
        [ObservableProperty]
        private bool isDefault;

        private Action<Cue?, bool> closingAction;

        public event EventHandler? Saved;

        public CueEditorViewModel(Cue cue, Action<Cue?, bool> action, bool isNew = false)//Why do I use an action instead of an event? Because the DataContext is not set until after the constructor is called, so I cannot subscribe to an event in the constructor of the view. I could use a routed event, but that would be more complicated than this. (Mostly because I don't know how to use routed events) I could also have set up a DataContextChanged event, but that's probably the same level of complexity as this. Idk.
        {
            closingAction = action;
            this.cue = cue;
            this.isNew = isNew;
            IsDefault = isDefault;
            IsSaved = !isNew;
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
            foreach (var file in files)
            {
                if (file is not null)
                    file.PropertyChanged += (s, e) => OnPropertyChanged(e);
            }
        }
        [RelayCommand]
        public void Save()
        {
            Cue c = new()
            {
                Number = Number,
                Note = Note,
                FadeInTime = FadeInTime,
                FadeOutTime = FadeOutTime,
                Duration = Duration,
                Volume = Volume,
                FadeType = FadeType,
                CueEndAction = CueEndAction,
            };
            CreateCueFiles(c);
            Saved?.Invoke(c, EventArgs.Empty);
            IsSaved = true;
            closingAction.Invoke(c, UseLetters);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            IsSaved = false;
        }
        [RelayCommand]
        public void Cancel()
        {
            if (!IsSaved)
            {
                var result = MessageBox.Show("You have unsaved changes. Are you sure you want to cancel?", "Unsaved Changes", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                    return;
            }
            closingAction.Invoke(null, false);
        }

        public void CreateCueFiles(Cue c)
        {
            c.ScreenFiles.Clear();
            foreach (var file in Files)
            {
                if (file != null && file.Index == 0) {
                    if (file.Type == ExpectedFileType.Lights && file.State == FileErrorState.None)
                    {
                        c.LightScene = new()
                        {
                            FilePath = file.Path,
                            FadeInDuration = file.FadeInTime,
                            FadeOutDuration = file.FadeOutTime,
                            Duration = file.Duration,
                            EndAction = file.EndAction,
                        };
                    }
                    else
                    {
                        c.LightScene = new BlackoutLightFile()
                        {
                            FadeInDuration = file.FadeInTime,
                            Duration = file.FadeInTime + 1
                        };
                    }
                }
                else if(file != null && file.State == FileErrorState.None && file.Type != ExpectedFileType.Blackout)
                {
                    if(file.Type == ExpectedFileType.Video)
                    {
                        c.ScreenFiles.Add(file.Index, new VideoFile()
                        {
                            FilePath = file.Path,
                            FadeInDuration = file.FadeInTime,
                            FadeOutDuration = file.FadeOutTime,
                            EndAction = file.EndAction,
                            Volume = file.Volume,
                        });
                    }
                    else if(file.Type == ExpectedFileType.Image)
                    {
                        c.ScreenFiles.Add(file.Index, new ImageFile()
                        {
                            FilePath = file.Path,
                            FadeInDuration = file.FadeInTime,
                            FadeOutDuration = file.FadeOutTime,
                            EndAction = file.EndAction,
                            Duration = file.Duration,
                        });
                    }
                    else
                    {
                        c.ScreenFiles.Add(file.Index, new BlackoutScreenFile()
                        {
                            FadeInDuration = file.FadeInTime,
                        });
                    }
                }
            }
        }
    }
}

