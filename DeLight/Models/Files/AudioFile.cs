using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Interfaces;

namespace DeLight.Models.Files
{
    //TODO: Implement AudioFile
    public partial class AudioFile : CueFile, IAudioFile
    {
        [ObservableProperty]
        private double volume;

        public AudioFile()
        {
            Volume = 1;
        }
    }
}
