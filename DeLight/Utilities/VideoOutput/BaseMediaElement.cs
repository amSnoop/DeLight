using Avalonia.Animation;
using DeLight.Models;
using DeLight.Models.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace DeLight.Utilities.VideoOutput
{
    public class BaseMediaElement : MediaElement, IRunnableScreenCue
    {
        private readonly TaskCompletionSource<bool> tcs = new();
        private readonly List<Storyboard> storyboards = new();
        public bool IsFadingOut { get; protected set; } = false; //Used to prevent the fade out from being called multiple times


        public ScreenFile File { get; set; }

        public double FileDuration { get; protected set; } = -1;
        public double CueDuration { get; protected set; } = 0;
        public double RawCuePosition { get; protected set; } = 0;

        public double intendedFadeOutStartTime { get; protected set; } = 0;

        public double? NextCueStartTime { get; set; } = null;
        private bool loop;

        private EndAction cueEndAction;

        private bool useCueTimeForFadeOut = false;


        public BaseMediaElement(ScreenFile file, double cueDuration, EndAction cueEndAction)
        {
            LoadedBehavior = MediaState.Manual;
            UnloadedBehavior = MediaState.Manual;
            IsMuted = false;
            File = file;
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
            MediaOpened += (s, e) => tcs.SetResult(true);

            loop = file.EndAction == EndAction.Loop;
            CueDuration = cueDuration;
            this.cueEndAction = cueEndAction;
        }

        private double GetIntendedFadeoutStartTime()
        {
            intendedFadeOutStartTime = double.MaxValue;
            useCueTimeForFadeOut = false;
            if(CueDuration == 0)
            {
                if(File.EndAction == EndAction.FadeAfterEnd)
                {
                    intendedFadeOutStartTime = FileDuration;
                }
                else if(File.EndAction == EndAction.FadeBeforeEnd)
                {
                    intendedFadeOutStartTime = FileDuration - File.FadeOutDuration;
                }
            }
            else
            {
                if()
            }
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
            if(FileDuration == -1)
                FileDuration = NaturalDuration.HasTimeSpan ? NaturalDuration.TimeSpan.TotalSeconds : -1;
            if (FileDuration == -1)
                throw new NullReferenceException("Attempted to load a file with a null duration.");

            intendedFadeOutStartTime = GetIntendedFadeoutStartTime();
        }

        public virtual void Restart() { }
        public virtual void SeekTo(double time, bool p) { }

        //Will fade in the video with the remaining duration of the fade in. Also plays the video.
        public void FadeIn(double startTime)
        {
            ClearCurrentAnimations();
            Play();
            DoubleAnimation fadeIn = new(1, TimeSpan.FromSeconds(File.FadeInDuration - startTime));
            BeginAnimation(fadeIn);
        }

        //Will fade out the video with the remaining duration of the fade out. Also plays the video.
        public void FadeOut(double startTime)
        {
            ClearCurrentAnimations();
            Play();
            IsFadingOut = true;
            DoubleAnimation fadeOut = new(0, TimeSpan.FromSeconds(File.FadeOutDuration - (startTime - (double)(fadeOutStartTime ?? startTime))));
            BeginAnimation(fadeOut);

        }

        public virtual void SendTimeUpdate(double rawCueTime)
        {
            
        }

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
            Pause();
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
            if (IsInBackground)
                time = NextCueFadeInTimeStamp + time;
            if (IsInBackground && time > NextCueFadeInTimeStamp + NextCueFadeInDuration)
            {
                opacity = 0;
            }
            else if (time < File.FadeInDuration)
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
                        opacity = 1 - (time - (double)Duration) / File.FadeOutDuration;
                }
            }
            else if (File.EndAction == EndAction.FadeBeforeEnd)
            {
                if (time >= Duration - File.FadeOutDuration)
                {
                    if (File.FadeOutDuration == 0)
                        opacity = 0;
                    else
                        opacity = 1 - ((double)Duration - time) / File.FadeOutDuration;
                }
                else
                    opacity = 1;
            }
            else
                opacity = 1;
            Opacity = Math.Clamp(opacity, 0, 1);
        }

        #endregion
    }
}