using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Models.Files;
using System.Collections.Generic;
using System.Linq;

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
        private string number = "";
        [ObservableProperty]
        private string note = "";

        [ObservableProperty]
        private bool isActive = false;//Used for changing the color in the CueList because I didn't want to make a whole new view model for it

        [ObservableProperty]
        private double fadeInTime;
        [ObservableProperty]
        private double fadeOutTime;
        [ObservableProperty]
        private double volume;//0 to 1 TODO: Implement this
        [ObservableProperty]
        private double duration;
        [ObservableProperty]
        private FadeType fadeType;//TODO: Implement this
        [ObservableProperty]
        private EndAction cueEndAction;
        [ObservableProperty]
        private Dictionary<int, ScreenFile> screenFiles;
        [ObservableProperty]
        private LightFile lightScene;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(BGColor))]
        private bool ready;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(BGColor))]
        private bool disabled;//TODO: Implement this


        public SolidColorBrush BGColor => new(!Ready ? Color.Parse("#56211d") : Colors.Transparent);//TODO: Make a ViewModel for this

        public Cue()
        {
            Number = "0";
            FadeInTime = 3;
            FadeOutTime = 3;
            Volume = .2;
            Note = "New Cue";
            Duration = 0;
            CueEndAction = EndAction.FadeAfterEnd;
            FadeType = FadeType.FadeOver;
            ScreenFiles = new()
            {
                { 1, new BlackoutScreenFile(BlackoutReason.EmptyPath) }
            };
            LightScene = new BlackoutLightFile(BlackoutReason.EmptyPath);
        }

        public bool SetScreenFile(int screenNumber, ScreenFile file)
        {
            if(screenNumber > 0)
            {
                ScreenFiles[screenNumber] = file;
                return true;
            }
            return false;
        }

        public bool ChangeFileScreen(int screenNumber, ScreenFile file)
        {
            int curScreen = ScreenFiles.FirstOrDefault(x => x.Value == file).Key;
            if (curScreen != 0 && screenNumber > 0)
            {
                ScreenFiles[curScreen] = new BlackoutScreenFile(BlackoutReason.EmptyPath);
                ScreenFiles[screenNumber] = file;
                return true;
            }
            return false;
        }

        public bool SwapFileScreen(int screenNumber, ScreenFile file)
        {
            int curScreen = ScreenFiles.FirstOrDefault(x => x.Value == file).Key;
            if (curScreen != 0 && screenNumber > 0)
            {
                ScreenFile temp = ScreenFiles[screenNumber];
                ScreenFiles[screenNumber] = file;
                ScreenFiles[curScreen] = temp;
                return true;
            }
            return false;
        }
        public void RemoveScreen(int screenNumber)
        {
            if (screenNumber > 0)
            {
                ScreenFiles.Remove(screenNumber);
            }
        }

        //TODO: Make a class that handles editing a cue using a temp cue and temp media file types
    }
}
