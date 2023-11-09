using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Models.Files;

namespace DeLight.Models
{
    public enum EndAction
    {
        Loop = 0,
        FadeAfterEnd = 1,
        FadeBeforeEnd = 2,
        Freeze = 3,
    }


    public partial class Cue : ObservableObject
    {
        [ObservableProperty]
        private int number;
        [ObservableProperty]
        private string letter;
        [ObservableProperty]
        private string note;

        [ObservableProperty]
        private double fadeInTime;
        [ObservableProperty]
        private double fadeOutTime;
        [ObservableProperty]//TODO: Implement Volume
        private double volume;//0 to 1 
        [ObservableProperty]
        private double duration;
        [ObservableProperty]
        private EndAction cueEndAction;
        [ObservableProperty]
        private ScreenFile screenFile;
        [ObservableProperty]
        private LightFile lightFile;
        [ObservableProperty]
        private bool disabled;


        public Cue()
        {
            number = 0;
            letter = "";
            fadeInTime = 3;
            fadeOutTime = 3;
            volume = .2;
            note = "New Cue";
            duration = 0;
            cueEndAction = EndAction.FadeAfterEnd;
            ScreenFile = new BlackoutScreenFile();
            LightFile = new BlackoutLightFile();
        }
        public Cue(string s) : this()
        {
            ScreenFile = new()
            {
                FilePath = s
            };
        }
        public int CompareNum(Cue? c2)
        {
            if (c2 == null || Number > c2.Number || (Number == c2.Number && string.Compare(Letter, c2.Letter) > 0))
                return 1;
            if (Number == c2.Number && Letter == c2.Letter)
                return 0;
            return -1;
        }
        public void SetScreenFile(ScreenFile file)
        {
            ScreenFile = file;
            file.PropertyChanged += (s, e) => OnFilePropertyChanged();
            OnPropertyChanged(nameof(ScreenFile));
        }

        public void SetLightFile(LightFile file)
        {
            LightFile = file;
            file.PropertyChanged += (s, e) => OnFilePropertyChanged();
            OnPropertyChanged(nameof(LightFile));
        }

        private void OnFilePropertyChanged()
        {
            OnPropertyChanged(nameof(ScreenFile));
            OnPropertyChanged(nameof(LightFile));
        }
    }
}
