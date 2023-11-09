using Avalonia.Controls;
using Avalonia.Threading;
using DeLight.Interfaces;
using DeLight.Models;
using DeLight.Models.Files;
using DeLight.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace DeLight.Utilities.VideoOutput
{
    public static class VideoManager

    {
        private static VideoWindow? VideoWindow;

        public static event Action<double>? VideoTimerTicked;

        private static List<BaseMediaElement> MediaElements => VideoWindow?.Container?.Children?.OfType<BaseMediaElement>()?.ToList() ?? new();

        private static IRunnableScreenCue? currentCue;

        private static IRunnableScreenCue? prevCue;

        private static IRunnableScreenCue? preppedCue;

        private static readonly Timer? timer;

        private static double ElapsedSeconds;

        private static DateTime lastTick;

        private static double actionTime = double.MaxValue;

        private static readonly BlackoutScreenCue curtain = new(new());

        private static bool fadingOut;
        private static TaskCompletionSource<bool> tcs = new();

        private static double duration;
        static VideoManager()
        {
            if (!Design.IsDesignMode)//weird ass thing where each of these dont work in the other mode.
                VideoWindow = new VideoWindow();
            else
            {
                Task.Run(() =>
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    VideoWindow = new VideoWindow();
                }));
            }
            timer = new(GlobalSettings.TickRate);
            timer.Elapsed += Timer_Elapsed;
            VideoWindow?.Container.Children.Add(curtain.GetUIElement());
            System.Windows.Controls.Panel.SetZIndex(curtain.GetUIElement(), 1);
        }

        private static void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            ElapsedSeconds += (DateTime.Now - lastTick).TotalSeconds;
            VideoTimerTicked?.Invoke(ElapsedSeconds);
            lastTick = DateTime.Now;
            Messenger.SendCueTick(null, new(ElapsedSeconds, duration));
        }

        public static void Stop(bool hide = false)
        {
            timer?.Stop();
            currentCue = null;
            prevCue = null;
            foreach (var element in MediaElements)
            {
                element?.Stop();
            }
            VideoWindow?.Stop();
            if (hide)
                VideoWindow?.Hide();
        }

        public static async Task PrepareCue(Cue? c)
        {
            tcs = new();
            if (preppedCue != null)
            {
                RemoveCue(preppedCue);
                preppedCue = null;
            }
            IRunnableScreenCue? rsc;
            if (c is null)
            {
                rsc = null;
            }
            else if (c.ScreenFile is BlackoutScreenFile bo)
            {
                rsc = new BlackoutScreenCue(bo);
            }
            else if (c.ScreenFile.ErrorState != FileErrorState.None)
            {
                rsc = new BlackoutScreenCue(new BlackoutScreenFile() { FadeInDuration = c.ScreenFile.FadeInDuration });
            }
#pragma warning disable IDE0045 // Convert to conditional expression
            else if (c.ScreenFile is VideoFile vf)
            {
                rsc = new VideoMediaElement(vf);
                ((VideoMediaElement)rsc).Volume = c.Volume;
                c.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(Cue.Volume))
                        ((VideoMediaElement)rsc).SetCueVolume(c.Volume);
                };
            }
            else if (c.ScreenFile is ImageFile imgf)
            {
                rsc = new ImageMediaElement(imgf);
            }
            else
            {
                rsc = new BlackoutScreenCue(new());
            }
            preppedCue = rsc;
#pragma warning restore IDE0045 // Convert to conditional expression
            await curtain.LoadAsync();
            if (preppedCue is not null)
            {
                await preppedCue.LoadAsync();
                VideoWindow?.Container.Children.Add(preppedCue.GetUIElement());
            }
            tcs.SetResult(true);
        }
        public static async Task UpdateCue(Cue? c)
        {
            await tcs.Task;
            ShowVideoWindow();
            if (prevCue != null) RemoveCue(prevCue);
            prevCue = currentCue;
            if (prevCue != null)
                prevCue?.SendToBackground(c?.ScreenFile.FadeInDuration ?? prevCue.File.FadeOutDuration);
            currentCue = preppedCue;
            if (currentCue is not null)
                PrepareEndingListeners(c!);
            ElapsedSeconds = 0;
            lastTick = DateTime.Now;
            fadingOut = false;
            curtain.Opacity = 0;
            timer!.Start();
            currentCue?.SeekTo(0, true);
            FetchDuration(c);
            preppedCue = null;
            return;
        }

        //TODO: this really needs to move tf out of here and into a class that is more generic to lights and video
        private static void FetchDuration(Cue? c)
        {
            if (c == null)
                duration = 0;
            else if (c.Duration == 0)
                duration = currentCue?.Duration ?? 0;
            else
                 duration = c.Duration;
        }
        //curentCue will never be null, put attribute here
        private static void PrepareEndingListeners(Cue c)
        {
            if (currentCue is null)
                throw new NullReferenceException("currentCue is null");
            if (timer is null)
                throw new NullReferenceException("timer is null");

            var ea = c.ScreenFile.EndAction;
            if (c.Duration == 0)
            {
                SetUpFileWatchers(ea);
            }
            else if (c.Duration < currentCue.Duration)
            {
                SetUpCueWatchers(c);
            }
            else
            {
                SetUpFileWatchers(ea);
                SetUpCueWatchers(c);
            }

        }
        private static void SetUpFileWatchers(EndAction ea)
        {
            if (ea == EndAction.Loop && currentCue is VideoMediaElement)
            {
                currentCue.PlaybackEnded += (s, e) => currentCue.Restart();
            }
            else if (ea == EndAction.FadeAfterEnd)
            {
                currentCue!.FadeAfterEnd();
            }
            else if (ea == EndAction.FadeBeforeEnd)
            {
                currentCue!.FadeBeforeEnd();
            }
        }
        private static void SetUpCueWatchers(Cue c)
        {
            if (c.CueEndAction == EndAction.Loop)
            {
                actionTime = c.Duration;
                VideoTimerTicked += (i) => LoopWatch(c);
            }
            else if (c.CueEndAction == EndAction.FadeAfterEnd)
            {
                actionTime = c.Duration;
                curtain.File.FadeInDuration = c.ScreenFile.FadeOutDuration;
                VideoTimerTicked += (i) => FadeEndWatch();
            }
            else if (c.CueEndAction == EndAction.FadeBeforeEnd)
            {
                actionTime = c.Duration - c.ScreenFile.FadeOutDuration;
                curtain.File.FadeInDuration = c.ScreenFile.FadeOutDuration;
                VideoTimerTicked += (i) => FadeEndWatch();
            }
            else
            {
                actionTime = c.Duration;
                VideoTimerTicked += (i) => FreezeWatch();
            }
        }
        private static void FadeEndWatch()
        {
            if (ElapsedSeconds > actionTime)
            {
                if (!fadingOut)
                {
                    curtain.FadedIn += (s, e) => Pause();
                    curtain.FadeIn(ElapsedSeconds - actionTime);
                    fadingOut = true;
                }
            }
        }
        private static void FreezeWatch()
        {
            currentCue?.Pause();
        }
        private static async void LoopWatch(Cue c)
        {
            await UpdateCue(c);
        }


        private static void RemoveCue(IRunnableScreenCue cip)
        {
            if (MediaElements.Contains(cip))
            {
                cip.Stop();
                VideoWindow?.RemoveMediaElement(cip.GetUIElement());
            }
        }

        public static void Play()
        {
            if (curtain.Opacity == 1)
            {
                return;
            }
            timer?.Start();
            ShowVideoWindow();
            prevCue?.Play();
            currentCue?.Play();
        }
        public static void Pause()
        {
            timer?.Stop();
            prevCue?.Pause();
            currentCue?.Pause();
        }
        public static void SeekTo(double time, bool play)
        {
            ShowVideoWindow();
            ElapsedSeconds = time;
            FetchBGOpacity();
            if (curtain.Opacity == 1)
                Pause();
            else
            {
                prevCue?.SeekTo(time, play);
                currentCue?.SeekTo(time, play);
            }
        }
        public static void ShowVideoWindow()
        {
            VideoWindow?.Show();
        }
        public static void HideVideoWindow()
        {
            Stop();
            VideoWindow?.Hide();
        }
        public static void SetVideoScreen(System.Windows.Forms.Screen screen)
        {
            ShowVideoWindow();
            VideoWindow?.SetScreen(screen);
        }
        public static void FetchBGOpacity()
        {
#pragma warning disable IDE0045 // Convert to conditional expression
            if (ElapsedSeconds < actionTime)
                curtain.Opacity = 0;
            else if (ElapsedSeconds > actionTime && ElapsedSeconds < actionTime + curtain.File.FadeInDuration)
                curtain.Opacity = (ElapsedSeconds - actionTime) / curtain.File.FadeInDuration;
            else
                curtain.Opacity = 1;
#pragma warning restore IDE0045 // Convert to conditional expression
        }
    }
}
