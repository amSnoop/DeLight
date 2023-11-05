using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Interfaces;
using System.IO;

namespace DeLight.Models.Files
{
    public partial class LightFile : CueFile, IDurationFile
    {
        [ObservableProperty]
        private double duration;
        public LightFile()
        {
            Duration = 0;
        }

        public static LightFile CheckLightFile(LightFile file)
        {
            var filePath = file.FilePath;

            if (string.IsNullOrEmpty(filePath))
            {
                return new BlackoutLightFile() {
                    FadeInDuration = file.FadeInDuration,
                    FadeOutDuration = file.FadeOutDuration,
                    EndAction = file.EndAction
                };

            }

            if(!Path.Exists(filePath))
                file.ErrorState = FileErrorState.InvalidPath;
            else if (Path.GetExtension(filePath) != ".scex")
                file.ErrorState = FileErrorState.InvalidFileType;
            else
                file.ErrorState = FileErrorState.None;
            return file;
        }
    }
}
