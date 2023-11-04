using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Models.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DeLight.Models
{
    public enum CueType
    {
        Blackout,
        VidOnly,
        ImgOnly,
        VidLight,
        ImgLight,
        LightOnly,
        WARNING
    }
    public enum EndAction
    {
        Loop,
        FadeAfterEnd,
        FadeBeforeEnd,
        Freeze,
    }
    public enum FadeType
    {
        ShowXPress,
        FadeOver,
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
        private bool isActive;//Used for changing the color in the CueList because I didn't want to make a whole new view model for it

        [ObservableProperty]
        private double fadeInTime;
        [ObservableProperty]
        private double fadeOutTime;
        [ObservableProperty]//TODO: Implement Volume
        private double volume;//0 to 1 
        [ObservableProperty]
        private double duration;
        [ObservableProperty]
        private FadeType fadeType;//TODO: Implement FadeType
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
            fadeType = FadeType.FadeOver;
            screenFile = new BlackoutScreenFile();
            lightFile = new BlackoutLightFile();
        }

        public int CompareNum(Cue? c2)
        {
            if (c2 == null || Number > c2.Number || (Number == c2.Number && string.Compare(Letter, c2.Letter) > 0))
                return 1;
            if (Number == c2.Number && Letter == c2.Letter)
                return 0;
            return -1;
        }
    }
}
