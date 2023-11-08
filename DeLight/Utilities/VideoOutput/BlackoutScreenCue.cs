using DeLight.Models.Files;
using DeLight.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows;
using System.Windows.Controls;
using DeLight.Interfaces;
using System.Windows.Data;

namespace DeLight.Utilities.VideoOutput
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
    public class BlackoutScreenCue : Border, IRunnableScreenCue
    {
        public List<Storyboard> storyboards = new();
        public bool IsFadingOut { get; private set; } = false;

        public double Duration => File.FadeInDuration + 1;

        public BlackoutScreenFile File { get; }

        CueFile IRunnableVisualCue.File => File;

        public event EventHandler? FadedIn;
        public event EventHandler? FadedOut;
        public event EventHandler? PlaybackEnded;

        public BlackoutScreenCue(BlackoutScreenFile file)
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
            Dispatcher.Invoke(() =>
            {
                foreach (Storyboard storyboard in storyboards)
                    storyboard.Stop();
                storyboards.Clear();
            });
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
            Dispatcher.Invoke(() =>
            {
                foreach (Storyboard storyboard in storyboards)
                    storyboard.Pause();
            });
        }

        public void Play()
        {
            Dispatcher.Invoke(() =>
            {
                foreach (Storyboard storyboard in storyboards)
                    storyboard.Resume();
            });
        }
        public void SendTimeUpdate(double time)
        {
            if (time > File.FadeInDuration)
            {
                Dispatcher.Invoke(() => Opacity = 1);
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

        public FrameworkElement GetUIElement() => this;

        public void SendToBackground(double newCueFadeInTime) { }

        public void Stop()
        {
            ClearCurrentAnimations();
            Dispatcher.Invoke(() =>
            {
                Opacity = 1;
            });
        }
        private void BeginAnimation(DoubleAnimation animation)
        {
            Dispatcher.Invoke(() =>
            {
                Storyboard.SetTarget(animation, this);
                Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));
                Storyboard storyboard = new();
                storyboard.Children.Add(animation);
                storyboard.Begin();
                storyboards.Add(storyboard);
            });
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
            double opacity = 1;
            if (timeStamp < File.FadeInDuration)
            {
                opacity = timeStamp / File.FadeInDuration;
            }
            Dispatcher.Invoke(() => Opacity = Math.Clamp(opacity, 0, 1));
        }

        public void FadeBeforeEnd() { }

        public void FadeAfterEnd() { }
    }
}
