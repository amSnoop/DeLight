using DeLight.Models;
using DeLight.Models.Files;
using DeLight.Utilities.VideoOutput;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace DeLight.Utilities
{
    public class ImageMediaElement : BaseMediaElement
    {
        public ImageMediaElement(ImageFile file) : base(file)
        {
            Stretch = System.Windows.Media.Stretch.Uniform;
            Duration = file.Duration;
        }
        public override void SeekTo(double time, bool play)
        {
            FetchOpacity(time);
            if (play)
                if (time < File.FadeInDuration)
                    FadeIn(time);
                else if (time < intendedFadeOutStartTime || File.EndAction == EndAction.Loop || File.EndAction == EndAction.Freeze)
                    Play();
                else
                    FadeOut(time);
        }

        public override void SendTimeUpdate(double time)
        {
            if (File.EndAction == EndAction.FadeAfterEnd || File.EndAction == EndAction.FadeBeforeEnd)
                if (!IsFadingOut)
                    if (time > intendedFadeOutStartTime)
                    {
                        FetchOpacity(time);
                        FadeOut(time);
                    }
        }
    }
}
