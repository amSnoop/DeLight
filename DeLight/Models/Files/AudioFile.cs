using CommunityToolkit.Mvvm.ComponentModel;

namespace DeLight.Models.Files
{
    //TODO: Implement
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
