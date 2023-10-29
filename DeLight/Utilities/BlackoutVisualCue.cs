using DeLight.Models.Files;
using DeLight.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows;
using System.Windows.Controls;

namespace DeLight.Utilities
{


    /*
     * 
     * Ah, yes, the BlackoutVisualCue. A classic.
     * 
     * It's a border that fades in. That's it. Doesn't even fade out.
     * 
     * There is some special SeekTo logic that handles partial fadeins, but otherwise most of this is just barebones implementation of the IRunnableScreenCue interface.
     * 
     */
    public class BlackoutVisualCue : Border, IRunnableScreenCue
    {
        public List<Storyboard> storyboards = new();
        public bool IsFadingOut { get; private set; } = false;

        public double? Duration => File.FadeInDuration + 1;

        public BlackoutScreenFile File { get; }

        CueFile IRunnableVisualCue.File => File;

        public event EventHandler? FadedIn;
        public event EventHandler? FadedOut;
        public event EventHandler? PlaybackEnded;

        public BlackoutVisualCue(BlackoutScreenFile file)
        {
            File = file;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
            Background = System.Windows.Media.Brushes.Black;
            Opacity = 0;
            FadedOut += OnFadedOut;
            FadedIn += OnFadedIn;
        }

        //I don't even remember why this method is needed
        public void ClearCurrentAnimations()
        {
            foreach (Storyboard storyboard in storyboards)
                storyboard.Stop();
            storyboards.Clear();
        }

        public void FadeIn(double startTime = 0)
        {
            FetchOpacity(startTime);
            DoubleAnimation fadeIn = new(1, TimeSpan.FromSeconds(File.FadeInDuration - startTime));
            fadeIn.Completed += (s, e) =>
            {
                FadedIn?.Invoke(this, EventArgs.Empty);
            };
            BeginAnimation(fadeIn);
        }

        public void FadeOut(double duration = -1)
        {
            FadedOut?.Invoke(this, EventArgs.Empty);
        }

        public Task LoadAsync()
        {
            return Task.CompletedTask;
        }

        public void Pause()
        {
            foreach (Storyboard storyboard in storyboards)
                storyboard.Pause();
        }

        public void Play()
        {
            foreach (Storyboard storyboard in storyboards)
                storyboard.Resume();
        }
        public void SendTimeUpdate(double time)
        {
            if (time > File.FadeInDuration)
            {
                Opacity = 1;
                PlaybackEnded?.Invoke(this, EventArgs.Empty);
                return;
            }
        }
        public void Restart() { }

        public void SeekTo(double time, bool play)
        {
            ClearCurrentAnimations();
            if (time < File.FadeInDuration)
            {
                if (play)
                    FadeIn(time);
                else
                    FetchOpacity(time);
            }
            else
            {
                Opacity = 1;
            }
        }

        public UIElement GetUIElement() => this;
        public void Stop()
        {
            ClearCurrentAnimations();
            Opacity = 1;
        }
        private void BeginAnimation(DoubleAnimation animation)
        {
            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));
            Storyboard storyboard = new();
            storyboard.Children.Add(animation);
            storyboard.Begin();
            storyboards.Add(storyboard);
        }

        private void OnFadedOut(object? s, EventArgs e)
        {
            IsFadingOut = false;
            storyboards.Clear();
        }

        private void OnFadedIn(object? s, EventArgs e)
        {
            storyboards.Clear();
        }

        private void FetchOpacity(double timeStamp)
        {
            if (File.FadeInDuration == 0)
            {
                Opacity = 1;
                return;
            }

            if (timeStamp < File.FadeInDuration)
            {
                Opacity = timeStamp / File.FadeInDuration;
            }
            else
            {
                Opacity = 1;
            }
        }
    }
}
