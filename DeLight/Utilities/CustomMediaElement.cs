using DeLight.Models;
using DeLight.Models.Files;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace DeLight.Utilities
{
    public abstract class CustomMediaElement : MediaElement, IRunnableScreenCue
    {

        private readonly TaskCompletionSource<bool> tcs = new();
        private readonly List<Storyboard> storyboards = new();

        public event EventHandler? FadedIn, FadedOut, PlaybackEnded;


        public ScreenFile File { get; set; }
        public virtual double? Duration { get => NaturalDuration.HasTimeSpan ? NaturalDuration.TimeSpan.TotalSeconds : null; }
        public bool IsFadingOut { get; private set; } = false; //Used to prevent the fade out from being called multiple times

        private bool IsFadedOut { get => Opacity == 0; }
        CueFile IRunnableVisualCue.File { get => File; }

        public CustomMediaElement(ScreenFile file) : base()
        {
            LoadedBehavior = MediaState.Manual;
            UnloadedBehavior = MediaState.Manual;
            File = file;
            IsMuted = false;
            try {
                Source = new Uri(file.FilePath);
            } catch (Exception e) {
                Console.WriteLine(e);
            }
            Opacity = 0;
            FadedOut += OnFadedOut;
            MediaOpened += (s, e) => tcs.SetResult(true);
        }


        #region Event Handlers for Video End Actions

        public void OnMediaEnded(object? sender, EventArgs e)
        {
            PlaybackEnded?.Invoke(this, EventArgs.Empty);
        }
        public void OnFadedOut(object? s, EventArgs e)
        {
            Stop();
            IsFadingOut = false;
            storyboards.Clear();
        }
        #endregion


        #region Public Methods
        public virtual void Restart() { }
        public virtual void SeekTo(double time, bool p) { }
        public virtual void SendTimeUpdate(double time) { }
        public new virtual void Play() { base.Play(); }
        public new virtual void Pause() { base.Pause(); }
        public new virtual void Stop() { base.Stop(); }

        public UIElement GetUIElement() => this;
        //Loads the cue so that it has a NaturalDuration TimeSpan TODO: Disallow playing until loaded.
        public async Task LoadAsync()
        {
            Play();
            Stop();
            await tcs.Task;
        }
        public void FadeIn(double duration = -1)
        {
            Play();
            DoubleAnimation fadeIn = new(1, TimeSpan.FromSeconds(duration == -1 ? File.FadeInDuration : duration));
            fadeIn.Completed += (s, e) => FadedIn?.Invoke(this, EventArgs.Empty);
            BeginAnimation(fadeIn);
        }

        public void FadeOut(double duration = -1)
        {
            if(!IsFadedOut && !IsFadingOut)
                Play();

            IsFadingOut = true;
            DoubleAnimation fadeOut = new(0, TimeSpan.FromSeconds(duration == -1 ? File.FadeOutDuration : duration));
            fadeOut.Completed += (s, e) => FadedOut?.Invoke(this, EventArgs.Empty);
            BeginAnimation(fadeOut);
        }

        

        public void ClearCurrentAnimations()
        {
            foreach (Storyboard storyboard in storyboards)
                storyboard.Stop();
            storyboards.Clear();
        }
        #endregion


        #region Helper Methods
        private void BeginAnimation(DoubleAnimation animation)
        {
            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));
            Storyboard storyboard = new();
            storyboard.Children.Add(animation);
            storyboard.Begin();
            storyboards.Add(storyboard);
        }
        #endregion
    }


}
