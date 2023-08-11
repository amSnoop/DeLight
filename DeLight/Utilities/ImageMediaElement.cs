using DeLight.Models;
using DeLight.Models.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace DeLight.Utilities
{
    public class ImageMediaElement : CustomMediaElement
    {
        Timer timer = new(GlobalSettings.TickRate);
        int elapsedTicks = 0;
        public override double? Duration => ((ImageFile)File).Duration;
        public ImageMediaElement(ImageFile file) : base(file)
        {
            Stretch = System.Windows.Media.Stretch.Uniform;
            timer.Elapsed += OnTimerElapsed;
        }

        public void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            elapsedTicks++;
            if (elapsedTicks >= Duration)
            {
                timer.Stop();
                OnMediaEnded(this, EventArgs.Empty);
            }
        }

        public override void Restart()
        {
            elapsedTicks = 0;
            timer.Start();
        }
        public override void SeekTo(double time)
        {
            elapsedTicks = (int)time * 1000 / GlobalSettings.TickRate;
        }

        public override void Play()
        {
            timer.Start();
            base.Play();
        }
        public override void Pause()
        {
            timer.Stop();
            base.Pause();
        }
        public override void Stop()
        {
            timer.Stop();
            elapsedTicks = 0;
            base.Stop();
        }
    }
    public class BlackoutVisualCue : Border, IRunnableScreenCue
    {
        public List<Storyboard> storyboards = new();
        public bool IsFadingOut { get; private set; } = false;

        public double? Duration => 0;

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
        }
        public void ClearCurrentAnimations()
        {
            foreach (Storyboard storyboard in storyboards)
                storyboard.Stop();
            storyboards.Clear();
        }

        public void FadeIn(double duration = -1)
        {
            DoubleAnimation fadeIn = new(1, TimeSpan.FromSeconds(duration == -1 ? File.FadeInDuration : duration));
            fadeIn.Completed += (s, e) => FadedIn?.Invoke(this, EventArgs.Empty);
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

        public void Pause() { }

        public void Play() { }

        public void Restart() { }

        public void SeekTo(double time) { }

        public UIElement GetUIElement() => this;
        public void Stop() { }
        private void BeginAnimation(DoubleAnimation animation)
        {
            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));
            Storyboard storyboard = new();
            storyboard.Children.Add(animation);
            storyboard.Begin();
            storyboards.Add(storyboard);
        }
    }
    public class VideoMediaElement : CustomMediaElement
    {
        public VideoMediaElement(VideoFile file) : base(file)
        {
            Volume = file.Volume;
            MediaEnded += OnMediaEnded;
        }
        public override void Restart()
        {
            Stop();
            Position = TimeSpan.Zero;
            Play();
        }
        public override void SeekTo(double time)
        {
            Position = TimeSpan.FromSeconds(time);
        }
    }
}
