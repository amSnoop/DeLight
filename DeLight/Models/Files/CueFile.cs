using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Interfaces;
using System.IO;

namespace DeLight.Models.Files
{


    /*
     * Structure:
     * CueFile
     *   - AudioFile
     *   - ScreenFile
     *      - VideoFile
     *      - GifFile
     *      - ImageFile: IDurationFile
     *      - BlackoutScreenFile
     *   - LightFile: ILightFile, IDurationFile
     *      - BlackoutLightFile
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
        partial void OnEndActionChanged(EndAction value)
        {
            
        }
    }

}
