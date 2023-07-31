using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Interfaces;

namespace DeLight.Models.Files
{
    public partial class ImageFile : ScreenFile, IDurationFile
    {
        [ObservableProperty]
        private double duration;

        public ImageFile()
        {
            Duration = 5;
        }
    }
}
