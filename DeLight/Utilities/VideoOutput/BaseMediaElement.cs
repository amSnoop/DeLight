using DeLight.Interfaces;
using DeLight.Models.Files;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace DeLight.Utilities.VideoOutput
{
    public class BaseMediaElement : MediaElement, IRunnableScreenCue
    {
        protected readonly TaskCompletionSource<bool> tcs = new();
        private readonly List<Storyboard> storyboards = new();
        public bool IsFadingOut { get; protected set; } = false; //Used to prevent the fade out from being called multiple times


        public ScreenFile File { get; set; }

        CueFile IRunnableVisualCue.File => File;
        public event EventHandler? PlaybackEnded;
        public double Duration { get; protected set; } = -1;
        public double RawCuePosition { get; protected set; } = 0;

        public double intendedFadeOutStartTime { get; protected set; } = double.MaxValue;

        public double? NextCueStartTime { get; set; } = null;

        public bool IsInBackground { get; set; } = false;

        public double NextCueFadeInTimeStamp { get; set; } = 0;
        public double NextCueFadeInDuration { get; set; } = 0;
        protected bool loaded = false;
        public BaseMediaElement(ScreenFile file)
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
            MediaEnded += (s, e) => PlaybackEnded?.Invoke(s, e);
        }

        public void FadeBeforeEnd()
        {
            intendedFadeOutStartTime = Duration - File.FadeOutDuration;
            VideoManager.VideoTimerTicked += SendTimeUpdate;
        }

        public void FadeAfterEnd()
        {
            intendedFadeOutStartTime = Duration;
            MediaEnded += (s, e) => FadeOut(Position.TotalSeconds);
        }

        public void SendToBackground(double end)
        {
            IsInBackground = true;
            NextCueFadeInTimeStamp = Position.TotalSeconds;
            NextCueFadeInDuration = end;
            VideoManager.VideoTimerTicked += SendTimeUpdate;
        }


        #region Interface Methods
        public void ClearCurrentAnimations()
        {
            foreach (Storyboard storyboard in storyboards)
                storyboard.Stop();
            storyboards.Clear();
        }

        public FrameworkElement GetUIElement() => this;

        public async Task LoadAsync()
        {
            var vol = Volume;
            Volume = 0;
            Play();
            Stop();

            await tcs.Task;
            if (Duration == -1)
                Duration = NaturalDuration.HasTimeSpan ? NaturalDuration.TimeSpan.TotalSeconds : -1;
            if (Duration == -1)
                throw new NullReferenceException("Attempted to load a file with a null duration.");
            Volume = vol;
            loaded = true;
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
            DoubleAnimation fadeOut = new(0, TimeSpan.FromSeconds(Math.Max(0, File.FadeOutDuration - (startTime - intendedFadeOutStartTime))));
            BeginAnimation(fadeOut);

        }

        public virtual void SendTimeUpdate(double time)
        {

            if(FetchOpacity(time) == 0)
            {
                Pause();
            }
            else if (!IsFadingOut && time > intendedFadeOutStartTime)
            {
                FadeOut(time);
            }
        }

        //Only plays hte video. not from start, no seeking, just plays.
        public new virtual void Play()
        {
            foreach (Storyboard storyboard in storyboards)
                storyboard.Resume();
            Dispatcher.Invoke(base.Play);
        }
        public new virtual void Pause()
        {
            foreach (Storyboard storyboard in storyboards)
                storyboard.Pause();
            Dispatcher.Invoke(base.Pause);
        }

        //does whatever the base stop does ig
        public new virtual void Stop()
        {
            foreach (Storyboard storyboard in storyboards)
                storyboard.Stop();
            storyboards.Clear();
            Dispatcher.Invoke(base.Stop);
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

        protected double FetchOpacity(double time)
        {
            if (Duration == -1)
                Duration = NaturalDuration.HasTimeSpan ? NaturalDuration.TimeSpan.TotalSeconds : -1;
            if (Duration == -1)
                throw new NullReferenceException("Attempted to fetch opacity on a file with null duration.");

            double opacity;


            if (IsInBackground && time + NextCueFadeInTimeStamp > NextCueFadeInTimeStamp + NextCueFadeInDuration)//if the video is in the background and the next video has finished fading in
            {
                opacity = 0;
            }
            else if (time < File.FadeInDuration)//if the video is still fading in
                opacity = time / File.FadeInDuration;
            else if (time > intendedFadeOutStartTime)//if the video is fading out
                opacity = 1 - (time - intendedFadeOutStartTime) / File.FadeOutDuration;
            else
                opacity = 1;//if the video is not fading in or out
            Dispatcher.Invoke(() => Opacity = Math.Clamp(opacity, 0, 1));
            return opacity;
        }

        protected void TriggerPlaybackEnded()
        {
            PlaybackEnded?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}