using CommunityToolkit.Mvvm.ComponentModel;

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

    public enum BlackoutReason
    {
        EmptyPath,
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


        public CueFile()
        {
            FilePath = "";
            EndAction = EndAction.Freeze;
            FadeInDuration = 3;
            FadeOutDuration = 3;
        }
    }
}
