using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Models.Files;
using System;
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
        private bool disabled;


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
                { 1, new BlackoutScreenFile() }
            };
            LightScene = new BlackoutLightFile();
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
                ScreenFiles[curScreen] = new BlackoutScreenFile();
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

        public static int CompareCues(string cue1, string cue2)
        {
            // Splitting numeric and alphabetical portions of the strings
            var match1 = CueNumberRegex().Match(cue1);
            var match2 = CueNumberRegex().Match(cue2);

            if (match1.Success && match2.Success)
            {
                int number1 = int.Parse(match1.Groups[1].Value);
                int number2 = int.Parse(match2.Groups[1].Value);

                int comparison = number1.CompareTo(number2);
                if (comparison == 0) // If numbers are equal, compare the letters
                {
                    return string.Compare(match1.Groups[2].Value, match2.Groups[2].Value, StringComparison.OrdinalIgnoreCase);
                }
                return comparison;
            }
            throw new FormatException("The cues are not in the expected format.");
        }
        public int FetchNum()
        {
            return int.Parse(CueNumberRegex().Match(Number).Groups[1].Value);
        }
        public char FetchAlpha()
        {
            return CueNumberRegex().Match(Number).Groups[2].Value[0];
        }
        // Overloading the < operator
        public static bool operator <(Cue c1, Cue c2)
        {
            return CompareCues(c1.Number, c2.Number) < 0;
        }

        // Overloading the <= operator
        public static bool operator <=(Cue c1, Cue c2)
        {
            return CompareCues(c1.Number, c2.Number) <= 0;
        }
        public static bool operator >(Cue c1, Cue c2)
        {
            return CompareCues(c1.Number, c2.Number) > 0;
        }
        public static bool operator >=(Cue c1, Cue c2)
        {
            return CompareCues(c1.Number, c2.Number) >= 0;
        }

        [System.Text.RegularExpressions.GeneratedRegex("(\\d+)([a-z]*)")]
        private static partial System.Text.RegularExpressions.Regex CueNumberRegex();
    }
}
