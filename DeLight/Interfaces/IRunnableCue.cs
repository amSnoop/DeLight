using DeLight.Models.Files;
using System;
using System.Threading.Tasks;

namespace DeLight.Models
{
    public interface IRunnableVisualCue
    {
        CueFile File { get; }
        double Opacity { get; set; }

        bool IsFadingOut { get; }
        event EventHandler? FadedIn, FadedOut, PlaybackEnded;

        public double? Duration { get; }

        public void Play();
        public void Pause();
        public void Stop();
        public void SeekTo(double time);
        public void FadeIn(double duration = -1);
        public void FadeOut(double duration = -1);
        public void Restart();
        public void ClearCurrentAnimations();

        public Task LoadAsync();

    }
}
