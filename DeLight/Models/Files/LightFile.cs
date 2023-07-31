using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Interfaces;

namespace DeLight.Models.Files
{
    public partial class LightFile : CueFile, IDurationFile
    {
        [ObservableProperty]
        private double duration;
        public LightFile()
        {
            Duration = 0;
        }
    }
}
