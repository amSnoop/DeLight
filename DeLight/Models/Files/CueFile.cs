using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;

namespace DeLight.Models.Files
{


    /*
     * Structure:
     * CueFile
     *   - AudioFile
     *   - ScreenFile, IVisualFile
     *      - VideoFile
     *      - GifFile
     *      - ImageFile
     *   - LightFile, ILightFile
     * 
     * 
     * Gif and Image file are different because MediaElement is needed for gifs, but not for images, but they can't set a gif duration. Therefore special behaviors are needed.
     * 
     */

    public enum FileErrorState
    {
        None,
        InvalidPath,
        InvalidFileType,
        Other
    }

    public abstract partial class CueFile : ObservableObject
    {
        [ObservableProperty]
        private string filePath;
        [ObservableProperty]
        private EndAction endAction;
        [ObservableProperty]
        private double fadeInDuration;
        [ObservableProperty]
        private double fadeOutDuration;
        [ObservableProperty]
        private FileErrorState errorState;

        public CueFile()
        {
            FilePath = "";
            EndAction = EndAction.Freeze;
            FadeInDuration = 3;
            FadeOutDuration = 3;
            ErrorState = FileErrorState.None;
        }

        public static CueFile ConvertCueFile(CueFile file)//should only be used for ScreenFiles. AudioFiles and LightFiles should be checked separately
        {
            if (string.IsNullOrEmpty(file.FilePath))
            {
                return new BlackoutScreenFile();//will break if attempted with AudioFile or LightFile
            }

            CueFile newFile = file;
            newFile.ErrorState = FileErrorState.None;

            if (!Path.HasExtension(file.FilePath))
                newFile.ErrorState = FileErrorState.InvalidFileType;
            else
            {
                var extension = Path.GetExtension(file.FilePath).ToLower();
                if (extension == ".mp4" || extension == ".avi" || extension == ".wmv" || extension == ".mkv")
                {
                    if (file is not VideoFile)
                        newFile = new VideoFile();
                }
                else if (extension == ".gif")
                {
                    if (file is not GifFile)
                        newFile = new GifFile();
                }
                else if (extension == ".jpg" || extension == ".png" || extension == ".bmp")
                {
                    if (file is not ImageFile)
                        newFile = new ImageFile();
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
