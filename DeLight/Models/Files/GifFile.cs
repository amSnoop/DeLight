using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Interfaces;

namespace DeLight.Models.Files
{
    public partial class GifFile : ScreenFile, IDurationFile
    {
        [ObservableProperty]
        private double duration;

        public GifFile()
        {
            Duration = 5;
        }
    }
}
