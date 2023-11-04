using Avalonia.Threading;
using DeLight.Models;
using DeLight.Models.Files;
using DeLight.Utilities.LightingOutput;
using DeLight.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace DeLight.Utilities
{
    /*
     * This class is responsible for running a cue. It handles the timing, fading, and playback of the cue.
     * It is created by the ShowRunner when a new cue is started.
     * It handles loading the cue's files, and creating the IRunnableVisualCue objects that are used to play the cue.
     * 
     * Multiple refactorings need to be done:
     * 
     * Pass off fade handling to the IRunnableVisualCue objects
     *      Ideally, the CueRunner would just be a wrapper for the IRunnableVisualCue objects, and would just handle timing and looping. The IRunnableVisualCue objects would handle fading and playback.
     * 
     */
    public class CueRunner
    {

        private int fadeInCount = 0, fadeOutCount = 0;

        public event EventHandler? FadedIn, FadedOut;
        public Cue Cue { get; set; }
        public VideoWindow? VideoWindow { get; set; }

        //insert list of IRunnableVisualCues here
        public List<IRunnableVisualCue> VisualCues { get; set; } = new();
        public Timer Timer { get; set; }
        public int LoopCount { get; set; } = 0;
        public int ElapsedTicks { get; set; } = 0;
        public double ElapsedTime { get => ElapsedTicks * GlobalSettings.TickRate / 1000.0; }
        public double RealDuration { get; set; } = 0;


        public CueRunner(Cue cue, VideoWindow? videoWindow)
        {
            Cue = cue;
            VideoWindow = videoWindow;
            Timer = new System.Timers.Timer(GlobalSettings.TickRate);
            Timer.Elapsed += Timer_Tick;
            LightCue l = new(cue.LightFile);
            VisualCues.Add(l);
            var sf = cue.ScreenFile;
            IRunnableScreenCue cme;
            if (sf.ErrorState != FileErrorState.None)
                cme = new BlackoutVisualCue(new() { FadeInDuration = sf.FadeInDuration });
            else if (sf is BlackoutScreenFile bof)
                cme = new BlackoutVisualCue(bof); //extends Border
            else if (sf is VideoFile vf)
                cme = new VideoMediaElement(vf);
            else if (sf is ImageFile imgf)
                cme = new ImageMediaElement(imgf); //extends CustomMediaElement
            else
                throw new Exception("Unknown VisualCue type: " + sf.GetType());
            VideoWindow?.Container.Children.Add(cme.GetUIElement());//can't just add an IRunnableScreenCue to the layout
            VisualCues.Add(cme);
            cme.FadedIn += VisualCueFadedIn;
            FadedOut += OnFadedOut;
            Console.WriteLine(cue.CueEndAction);
        }


        public void FindRealCueDuration()
        {
            if (Cue.Duration == 0)
            {
                foreach (var vc in VisualCues)
                    if ((vc.Duration ?? 0) > RealDuration)
                        RealDuration = vc.Duration ?? 0;
            }
            else
                RealDuration = Cue.Duration;
        }

        public void Timer_Tick(object? s, EventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                ElapsedTicks++;
                if (Cue.CueEndAction == EndAction.FadeBeforeEnd && ElapsedTime >= (RealDuration - Cue.FadeOutTime))
                    End();
                else if (ElapsedTime >= RealDuration)
                {
                    if (Cue.CueEndAction == EndAction.Loop)
                    {
                        Console.WriteLine("Looping");
                        LoopCount++;
                        Timer.Stop();
                        Restart();
                    }
                    else if (Cue.CueEndAction == EndAction.FadeAfterEnd)
                        End();
                }
            });
            Messenger.SendCueTick(Cue, new(ElapsedTime, RealDuration));
        }
        public void End()
        {
            ElapsedTicks = 0;
            Timer.Stop();
            foreach (var vc in VisualCues)
            {
                vc.FadedOut += VisualCueFadedOut;
                vc.FadeOut(ElapsedTime);
            }
        }

        public void OnFadedOut(object? sender, EventArgs e)
        {
            foreach (var vc in VisualCues)
            {
                vc.Stop();
                if (vc is IRunnableScreenCue cme)
                    VideoWindow?.Container.Children.Remove(cme.GetUIElement());
            }
            Cue.IsActive = false;
        }

        public void VisualCueFadedIn(object? sender, EventArgs e)
        {
            fadeInCount++;

            if (fadeInCount >= VisualCues.Where(vc => vc is IRunnableScreenCue).Count())
            {
                FadedIn?.Invoke(this, EventArgs.Empty);
                fadeInCount = 0;
                foreach (var vc in VisualCues)
                    vc.FadedIn -= VisualCueFadedIn;
            }
        }
        public void VisualCueFadedOut(object? sender, EventArgs e)
        {
            fadeOutCount++;
            if (fadeOutCount >= VisualCues.Count)
            {
                FadedOut?.Invoke(this, EventArgs.Empty);
                fadeOutCount = 0;
                foreach (var vc in VisualCues)
                    vc.FadedOut -= VisualCueFadedOut;
            }

        }

        public void Restart()
        {
            SeekTo(0, true);
            Timer.Start();
        }

        //Plays from the beginning of the cue
        public async void Play()
        {

            //wait for media to open before attempting to find its duration
            var loadTasks = VisualCues.Select(vc => vc.LoadAsync()).ToList();
            await Task.WhenAll(loadTasks);


            //if HasTimeSpan returns false, then some real shenanigans are afoot... Should probably throw an exception or something but whatever (TODO) :P
            //That comment brought to you by GitHub Copilot - Surprisingly funny lmao
            FindRealCueDuration();
            SeekTo(0, true);
            Timer.Start();
        }

        public void Pause()
        {
            Timer.Stop();
            SeekTo(ElapsedTicks, false);
        }
        public void Unpause()
        {
            Timer.Start();
            SeekTo(ElapsedTicks, true);
        }
        public void Stop()
        {
            Timer.Stop();
            ElapsedTicks = 0;
            OnFadedOut(this, EventArgs.Empty);
        }

        public void SeekTo(double time, bool play = false)
        {
            int tick = (int)(time * 1000 / GlobalSettings.TickRate);
            ElapsedTicks = tick;
            foreach (var vc in VisualCues)
            {
                vc.Pause();
                vc.ClearCurrentAnimations();
                vc.SeekTo(ElapsedTime, play);
            }

        }
    }
}
