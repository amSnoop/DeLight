using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Interfaces;

namespace DeLight.Models.Files
{
    public partial class VideoFile : ScreenFile, IAudioFile
    {
        [ObservableProperty]
        private double volume;

        public VideoFile()
        {
            Volume = 1;
        }
    }
}
