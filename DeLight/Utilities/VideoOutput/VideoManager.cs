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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace DeLight.Utilities.VideoOutput
{
    public static class VideoManager

    {
        private static VideoWindow? VideoWindow = null;

        public static event Action<int>? VideoTimerTicked;

        private static List<BaseMediaElement> MediaElements => VideoWindow?.Container.Children.OfType<BaseMediaElement>().ToList() ?? new();

        private static IRunnableScreenCue? currentCue = null;

        private static IRunnableScreenCue? prevCue = null;

        private static Timer? timer = null;

        private static int elapsedTicks = 0;
        private static double elapsedSeconds => elapsedTicks / (GlobalSettings.TickRate * 1.0);

        private static double actionTime = double.MaxValue;

        private static BlackoutScreenCue curtain = new(new());

        private static bool fadingOut;

        public static async Task Startup()
        {
            if (!Design.IsDesignMode)//weird ass thing where each of these dont work in the other mode.
                VideoWindow = new VideoWindow();
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    VideoWindow = new VideoWindow();
                });
            }
            timer = new Timer(GlobalSettings.TickRate);
            timer.Elapsed += Timer_Elapsed;
            VideoWindow?.Container.Children.Add(curtain.GetUIElement());
            System.Windows.Controls.Panel.SetZIndex(curtain.GetUIElement(), 1);
        }

        private static void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            elapsedTicks++;
            VideoTimerTicked?.Invoke(elapsedTicks);
        }

        public static void Stop(bool hide = false)
        {
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
        public static async Task UpdateCue(Cue? c)
        {
            ShowVideoWindow();
            if (prevCue != null) RemoveCue(prevCue);
            prevCue = currentCue;
            if (prevCue != null)
                prevCue?.SendToBackground(c?.ScreenFile.FadeInDuration ?? prevCue.File.FadeOutDuration, VideoTimerTicked!);
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
            else if (c.ScreenFile is VideoFile vf)
            {
                rsc = new VideoMediaElement(vf);
            }
            else if (c.ScreenFile is ImageFile imgf)
            {
                rsc = new ImageMediaElement(imgf);
            }
            else
            {
                rsc = new BlackoutScreenCue(new());
            }
            currentCue = rsc;
            if(currentCue is not null)
                PrepareEndingListeners(c!);
            elapsedTicks = 0;
            fadingOut = false;
            await curtain.LoadAsync();
            if (currentCue is not null)
            {
                await currentCue.LoadAsync();
                VideoWindow?.Container.Children.Add(currentCue.GetUIElement());
            }
            curtain.Opacity = 0;
            timer!.Start();
            currentCue?.SeekTo(0, true);
            return;
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
                currentCue!.FadeBeforeEnd(VideoTimerTicked!);
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
            if (elapsedSeconds > actionTime)
            {
                if (!fadingOut)
                {
                    curtain.FadedIn += (s, e) => Pause();
                    curtain.FadeIn(elapsedSeconds - actionTime);
                    fadingOut = true;
                }
            }
        }
        private static void FreezeWatch()
        {
            currentCue?.Pause();
        }
        private static void LoopWatch(Cue c)
        {
            UpdateCue(c);
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
            elapsedTicks = (int)Math.Round(time * GlobalSettings.TickRate);
            FetchBGOpacity();
            if (curtain.Opacity == 1)
                Pause();
            prevCue?.SeekTo(time, play);
            currentCue?.SeekTo(time, play);
        }
        public static void ShowVideoWindow()
        {
            VideoWindow?.Show();
        }
        public static void HideVideoWindow()
        {
            VideoWindow?.Hide();
        }
        public static void SetVideoScreen(System.Windows.Forms.Screen screen)
        {
            ShowVideoWindow();
            VideoWindow?.SetScreen(screen);
        }
        public static void FetchBGOpacity()
        {
            if (elapsedSeconds < actionTime)
                curtain.Opacity = 0;
            else if (elapsedSeconds > actionTime && elapsedSeconds < actionTime + curtain.File.FadeInDuration)
                curtain.Opacity = (elapsedSeconds - actionTime) / curtain.File.FadeInDuration;
            else
                curtain.Opacity = 1;
        }
    }
}
