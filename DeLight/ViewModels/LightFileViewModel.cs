using DeLight.Models.Files;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace DeLight.ViewModels
{
    public partial class LightFileViewModel : CueFileViewModel
    {
       
        public LightFileViewModel(LightFile lf) : base(lf)
        {
            file = lf;
            file.PropertyChanged += (s, e) => { if (s is LightFile f && e.PropertyName == nameof(LightFile.FilePath)) OnPathChanged(f.FilePath); };
        }
        public string HeaderStart => "Lights: ";
        public string HeaderEnd => File is BlackoutLightFile ? "Blackout" : ReasonString;
        public string ReasonString => FileErrorState.None != File.ErrorState ? $"{File.ErrorState}" : "Ready";
        public new string Header => $"{HeaderStart}{HeaderEnd}";
        protected override void OnPathChanged(string path)
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
                File.PropertyChanged += (s, e) => { if (s is LightFile f) OnPathChanged(f.FilePath); };
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
                File.PropertyChanged += (s, e) => { if (s is LightFile f) OnPathChanged(f.FilePath); };
                OnFileTypeChanged(this);
            }
            else
            {
                File.FilePath = path;
                File.ErrorState = CheckErrorState(path);
            }
            OnPropertyChanged(nameof(Header));
            base.OnPathChanged(path);
        }

        private static FileErrorState CheckErrorState(string path)
        {
            if (!Path.Exists(path))
                return FileErrorState.InvalidPath;
            if (!Path.HasExtension(path) || Path.GetExtension(path).ToLower() != ".scex")
                return FileErrorState.InvalidFileType;
            return FileErrorState.None;
        }
    }
}
