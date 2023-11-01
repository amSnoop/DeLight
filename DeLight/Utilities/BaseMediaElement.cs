using Avalonia.Animation;
using DeLight.Models;
using DeLight.Models.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace DeLight.Utilities
{
    public class BaseMediaElement : MediaElement, IRunnableScreenCue
    {
        private readonly TaskCompletionSource<bool> tcs = new();
        private readonly List<Storyboard> storyboards = new();

        protected double? fadeOutStartTime;


        public event EventHandler? FadedIn, FadedOut, PlaybackEnded;

        public ScreenFile File { get; private set; }

        CueFile IRunnableVisualCue.File => File;

        public bool IsFadingOut { get; protected set; } = false; //Used to prevent the fade out from being called multiple times

        protected bool IsFadedOut { get => Opacity == 0; }

        public double? Duration { get; protected set; } = null;

        public BaseMediaElement(ScreenFile file) : base()
        {
            LoadedBehavior = MediaState.Manual;
            UnloadedBehavior = MediaState.Manual;
            File = file;
            IsMuted = false;
            try
            {
                Source = new Uri(file.FilePath);
            }
            catch (Exception e)
            {
                file.ErrorState = FileErrorState.InvalidPath;
                Console.WriteLine(e);
            }
            Opacity = 0;
            FadedOut += OnFadedOut;
            MediaOpened += (s, e) => tcs.SetResult(true);
        }


        #region Interface Methods

        public void ClearCurrentAnimations()
        {
            foreach (Storyboard storyboard in storyboards)
                storyboard.Stop();
            storyboards.Clear();
        }

        public UIElement GetUIElement() => this;

        public async Task LoadAsync()
        {
            Play();
            Stop();

            await tcs.Task;
            Duration ??= NaturalDuration.HasTimeSpan ? NaturalDuration.TimeSpan.TotalSeconds : null;
            if (Duration != null)
            {
                if (File.EndAction == EndAction.FadeBeforeEnd)
                {
                    fadeOutStartTime = (double)Duration - File.FadeOutDuration;
                }
                else if (File.EndAction == EndAction.FadeAfterEnd)
                {
                    fadeOutStartTime = (double)Duration;
                }

            }
        }

        public virtual void Restart() { }
        public virtual void SeekTo(double time, bool p) { }

        //Will fade in the video with the remaining duration of the fade in. Also plays the video.
        public void FadeIn(double startTime)
        {
            ClearCurrentAnimations();
            Play();
            DoubleAnimation fadeIn = new(1, TimeSpan.FromSeconds(File.FadeInDuration - startTime));
            fadeIn.Completed += (s, e) => TriggerFadedIn();
            BeginAnimation(fadeIn);
        }

        //Will fade out the video with the remaining duration of the fade out. Also plays the video.
        public void FadeOut(double startTime)
        {
            ClearCurrentAnimations();
            Play();
            IsFadingOut = true;
            DoubleAnimation fadeOut = new(0, TimeSpan.FromSeconds(File.FadeOutDuration - (startTime - (double)(fadeOutStartTime ?? startTime))));
            fadeOut.Completed += (s, e) => TriggerFadedOut();
            BeginAnimation(fadeOut);

        }

        public virtual void SendTimeUpdate(double time) { }

        //Only plays hte video. not from start, no seeking, just plays.
        public new virtual void Play()
        {
            foreach (Storyboard storyboard in storyboards)
                storyboard.Resume();
            base.Play();
        }
        public new virtual void Pause()
        {
            foreach (Storyboard storyboard in storyboards)
                storyboard.Pause();
            base.Pause();
        }

        //does whatever the base stop does ig
        public new virtual void Stop()
        {
            foreach (Storyboard storyboard in storyboards)
                storyboard.Stop();
            storyboards.Clear();
            base.Stop();
        }

        #endregion

        #region Event Handlers for Video End Actions
        public void OnFadedOut(object? s, EventArgs e)
        {
            Stop();
            IsFadingOut = false;
            storyboards.Clear();
        }
        #endregion

        #region Protected Methods
        protected void BeginAnimation(DoubleAnimation animation)
        {
            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));
            Storyboard storyboard = new();
            storyboard.Children.Add(animation);
            storyboard.Begin();
            storyboards.Add(storyboard);
        }

        protected void FetchOpacity(double time)
        {
            Duration ??= NaturalDuration.HasTimeSpan ? NaturalDuration.TimeSpan.TotalSeconds : null;
            if (Duration == null)
                throw new NullReferenceException("Attempted to fetch opacity on a file with null duration.");

            double opacity;

            if (time < File.FadeInDuration)
                opacity = time / File.FadeInDuration;
            else if (File.EndAction == EndAction.FadeAfterEnd)
            {
                if (time < Duration)
                    opacity = 1;
                else
                {
                    if (File.FadeOutDuration == 0)
                        opacity = 0;
                    else
                        opacity = 1 - ((time - (double)Duration) / File.FadeOutDuration);
                }
            }
            else if (File.EndAction == EndAction.FadeBeforeEnd)
            {
                if (time >= Duration - File.FadeOutDuration)
                {
                    if (File.FadeOutDuration == 0)
                        opacity = 0;
                    else
                        opacity = 1 - (((double)Duration - time) / File.FadeOutDuration);
                }
                else
                    opacity = 1;
            }
            else
                opacity = 1;
            Opacity = Math.Clamp(opacity, 0, 1);
        }

        protected void TriggerFadedIn()
        {
            FadedIn?.Invoke(this, EventArgs.Empty);
        }
        protected void TriggerFadedOut()
        {
            IsFadingOut = false;
            FadedOut?.Invoke(this, EventArgs.Empty);
        }
        protected void TriggerPlaybackEnded()
        {
            PlaybackEnded?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}