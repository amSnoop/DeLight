using DeLight.Models.Files;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace DeLight.ViewModels
{
    public partial class ScreenFileViewModel : CueFileViewModel
    {
       

        private static readonly Dictionary<string[], Type> extensions = new()
        {
            { new[] { ".mp4", ".avi", ".mov", ".mkv", ".flv", ".wmv" }, typeof(VideoFile) },
            { new[] { ".gif" }, typeof(GifFile) },
            { new[] { ".png", ".jpg", ".jpeg", ".bmp", ".tiff" }, typeof(ImageFile) },
            { new[] { ".mp3", ".wav", ".ogg", ".flac", ".aac" }, typeof(AudioFile) },
        };


        private static readonly Dictionary<Type, string> friendlyName = new()
        {
            { typeof(VideoFile), "Video" },
            { typeof(ImageFile), "Image" },
            { typeof(BlackoutScreenFile), "Blackout" },
            { typeof(AudioFile), "Audio Only" },
            { typeof(GifFile), "Gif" }
        };
        public ScreenFileViewModel(ScreenFile sf) : base(sf)
        {
            file = sf;
            file.PropertyChanged += (s, e) => { if (s is ScreenFile f && e.PropertyName == nameof(ScreenFile.FilePath)) OnPathChanged(f.FilePath); };
        }
        public string HeaderStart => "Screen: ";
        public string HeaderEnd => File is BlackoutScreenFile ? "Blackout" : ReasonString;
        public string ReasonString => FileErrorState.None != File.ErrorState ? $"{File.ErrorState}" : friendlyName[File.GetType()];

        public new string Header => $"{HeaderStart}{HeaderEnd}";
        protected override void OnPathChanged(string s)
        {
            var file = (ScreenFile)File;
            if (string.IsNullOrEmpty(s))
            {
                File = new BlackoutScreenFile()
                {
                    FadeInDuration = file.FadeInDuration,
                    FadeOutDuration = file.FadeOutDuration,
                    EndAction = file.EndAction,
                };
            }
            var f = File;
            File = UpdateFileType();
            if (f != File)
            {
                file.PropertyChanged += (s, e) => { if (s is ScreenFile f) OnPathChanged(f.FilePath); };
                OnFileTypeChanged(this);
            }
            OnPropertyChanged(nameof(Header));
            base.OnPathChanged(s);
        }
        private ScreenFile UpdateFileType()
        {
            Type t = extensions.FirstOrDefault(x => x.Key.Contains(Path.GetExtension(File.FilePath).ToLower())).Value;
            if (t is null)
            {
                File.ErrorState = FileErrorState.InvalidFileType;
                return (ScreenFile)File;
            }
            if (!Path.Exists(File.FilePath))
            {
                File.ErrorState = FileErrorState.InvalidPath;
                return (ScreenFile)File;
            }
            if (File.GetType() != t)
            {
                var f = (ScreenFile)Activator.CreateInstance(t)!;
                f.FilePath = File.FilePath;
                f.FadeInDuration = File.FadeInDuration;
                f.FadeOutDuration = File.FadeOutDuration;
                f.EndAction = File.EndAction;

                return f;
            }
            File.ErrorState = FileErrorState.None;
            return (ScreenFile)File;
        }


    }
}
