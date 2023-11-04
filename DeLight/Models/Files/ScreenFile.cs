using System.IO;

namespace DeLight.Models.Files
{
    public partial class ScreenFile : CueFile
    {
        //Eventually this will probably have information about what screen it should be displayed on, but that's a bit in the future...
        public static ScreenFile ConvertCueFile(ScreenFile file)
        {
            if (string.IsNullOrEmpty(file.FilePath))
            {
                return new BlackoutScreenFile();
            }

            ScreenFile newFile = file;
            newFile.ErrorState = FileErrorState.None;

            if (!Path.HasExtension(file.FilePath))
                newFile.ErrorState = FileErrorState.InvalidFileType;
            else
            {
                var extension = Path.GetExtension(file.FilePath).ToLower();
                if (extension == ".mp4" || extension == ".avi" || extension == ".wmv" || extension == ".mkv")
                {
                    if (file is not VideoFile)
                        newFile = new VideoFile()
                        {
                            FilePath = file.FilePath,
                            FadeInDuration = file.FadeInDuration,
                            FadeOutDuration = file.FadeOutDuration,
                            EndAction = file.EndAction,
                        };
                }
                else if (extension == ".gif")
                {
                    if (file is not GifFile)
                        newFile = new GifFile()
                        {
                            FilePath = file.FilePath,
                            FadeInDuration = file.FadeInDuration,
                            FadeOutDuration = file.FadeOutDuration,
                            EndAction = file.EndAction,
                        };
                }
                else if (extension == ".jpg" || extension == ".png" || extension == ".bmp")
                {
                    if (file is not ImageFile)
                        newFile = new ImageFile()
                        {
                            FilePath = file.FilePath,
                            FadeInDuration = file.FadeInDuration,
                            FadeOutDuration = file.FadeOutDuration,
                            EndAction = file.EndAction,
                        };
                }
                else
                {
                    newFile.ErrorState = FileErrorState.InvalidFileType;
                }
            }
            if (!Path.Exists(file.FilePath))
                newFile.ErrorState = FileErrorState.InvalidPath;

            return newFile;
        }
    }
}
