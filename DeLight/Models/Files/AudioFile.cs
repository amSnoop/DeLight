using CommunityToolkit.Mvvm.ComponentModel;

namespace DeLight.Models.Files
{
    //TODO: Implement AudioFile
    public partial class AudioFile : CueFile
    {
        [ObservableProperty]
        private double volume;
        public bool HasValidFile;

        public AudioFile()
        {
            Volume = 1;
        }
    }
}
