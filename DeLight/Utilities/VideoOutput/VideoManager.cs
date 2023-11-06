using Avalonia.Controls;
using Avalonia.Threading;
using DeLight.Models;
using DeLight.Utilities.LightingOutput;
using DeLight.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace DeLight.Utilities.VideoOutput
{
    public static class VideoManager

    {
        private static VideoWindow? VideoWindow = null;

        private static List<BaseMediaElement> MediaElements => VideoWindow?.Container.Children.OfType<BaseMediaElement>().ToList() ?? new();

        private static IRunnableScreenCue? currentCue = null;

        private static IRunnableScreenCue? prevCue = null;

        private static Timer? timer = null;

        private static int elapsedTicks = 0;

        private static double elapsedSeconds => elapsedTicks / (GlobalSettings.TickRate * 1.0);
        public static void Startup()
        {
            if (!Design.IsDesignMode)//weird ass thing where each of these dont work in the other mode.
                VideoWindow = new VideoWindow();
            else
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    VideoWindow = new VideoWindow();
                });
            }
            timer = new Timer(GlobalSettings.TickRate);
            timer.Elapsed += Timer_Elapsed;
        }

        private static void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            throw new NotImplementedException();
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
            if(hide)
                VideoWindow?.Hide();
        }
        public static void UpdateCue(CueInfoPacket<IRunnableScreenCue> cip)
        {
            ShowVideoWindow();
            if (prevCue != null) RemoveCue(prevCue);
            prevCue = currentCue;
            if(prevCue != null)
                prevCue?.SendToBackground(cip.Cue.File.FadeInDuration);
            currentCue = cip.Cue;
            Play();
        }
        private static void RemoveCue(IRunnableScreenCue cue)
        {
            if (MediaElements.Contains(cue))
            {
                cue.Stop();
                VideoWindow?.RemoveMediaElement(cue.GetUIElement());
            }
        }

        public static void Play()
        {
            ShowVideoWindow();
            prevCue?.Play();
            currentCue?.Play();
        }
        public static void Pause()
        {
            prevCue?.Pause();
            currentCue?.Pause();
        }
        public static void SeekTo(double time, bool play)
        {
            ShowVideoWindow();
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

    }
}
