using DeLight.Models.Files;
using System.IO;

namespace DeLight.ViewModels
{
    public partial class LightFileViewModel : CueFileViewModel
    {
        public LightFileViewModel(LightFile lf) : base(lf)
        {
            file = lf;
            file.PropertyChanged += (s, e) => { if (s is LightFile f) OnPathChanged(f.FilePath); };
        }
        public string HeaderStart => "Lights: ";
        public string HeaderEnd => File is BlackoutLightFile ? "Blackout" : ReasonString;
        public string ReasonString => FileErrorState.None != File.ErrorState ? $"{File.ErrorState}" : "Ready";
        public string Header => $"{HeaderStart}{HeaderEnd}";
        private void OnPathChanged(string path)
        {
            var file = (LightFile)File;
            if (string.IsNullOrEmpty(path))
            {
                File = new BlackoutLightFile()
                {
                    FadeInDuration = file.FadeInDuration,
                    Duration = file.Duration,
                    FadeOutDuration = file.FadeOutDuration,
                    EndAction = file.EndAction,
                };
                OnFileTypeChanged(this);
            }
            else if (file is BlackoutLightFile)
            {
                File = new LightFile()
                {
                    FilePath = path,
                    FadeInDuration = file.FadeInDuration,
                    Duration = file.Duration,
                    FadeOutDuration = file.FadeOutDuration,
                    EndAction = file.EndAction,
                };
                OnFileTypeChanged(this);
            }
            else
            {
                File.FilePath = path;
                File.ErrorState = CheckErrorState(path);
            }
            OnPropertyChanged(nameof(Header));
        }

        private static FileErrorState CheckErrorState(string path)
        {
            if (!Path.Exists(path))
                return FileErrorState.InvalidPath;
            if (!Path.HasExtension(path) || path.ToLower() != ".scex")
                return FileErrorState.InvalidFileType;
            return FileErrorState.None;
        }
    }
}
