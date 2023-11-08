using DeLight.Interfaces;
using DeLight.Models.Files;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace DeLight.Interfaces
{
    public interface IRunnableVisualCue
    {
        public CueFile File { get; }
        public double Opacity { get; }

        public bool IsFadingOut { get; }
        public event EventHandler? PlaybackEnded;

        public double Duration { get; }

        public void Play();
        public void Pause();
        public void Stop();
        public void SeekTo(double time, bool play);
        public void FadeIn(double startTime);
        public void FadeOut(double startTime);
        public void Restart();
        public void ClearCurrentAnimations();

        public Task LoadAsync();

        //Every tick, the CueRunner will call this method on every IRunnableVisualCue. Used only for FadeOut animations. FadeIn is handled by SeekTo.
        public void SendTimeUpdate(double time);

    }
    //Exists only because IRunnableVisualCue is not a UIElement
    public interface IRunnableScreenCue : IRunnableVisualCue {
        public FrameworkElement GetUIElement();

        public void SendToBackground(double newCueFadeInDuration);

        public void FadeBeforeEnd();

        public void FadeAfterEnd();
    }
}
