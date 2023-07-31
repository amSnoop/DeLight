using Avalonia;
using Avalonia.Threading;
using DeLight.Models;
using DeLight.Models.Files;
using DeLight.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace DeLight.Utilities
{

    public class Light : IRunnableVisualCue
    {
        public double? Duration =>/* Math.Round(new Random().NextDouble() * 10, 1)*/ 1;
        public LightFile File { get; }

        CueFile IRunnableVisualCue.File => File;

        public Light(LightFile lf)
        {
            File = lf;
        }
        public double Opacity { get; set; }

        public bool IsFadingOut => false;

        public event EventHandler? FadedIn;
        public event EventHandler? FadedOut;
        public event EventHandler? PlaybackEnded;

        public void ClearCurrentAnimations()
        {
            Console.WriteLine("ClearCurrentAnimations() called on Light");
        }

        public void FadeIn(double duration = -1)
        {
            Console.WriteLine("FadeIn() called on Light");
        }

        public void FadeOut(double duration = -1)
        {
           Console.WriteLine("FadeOut() called on Light");
        }

        public Task LoadAsync()
        {
            return Task.CompletedTask;
        }

        public void Pause()
        {
            Console.WriteLine("Pause() called on Light");
        }

        public void Play()
        {
            Console.WriteLine("Play() called on Light");
        }

        public void SeekTo(double time)
        {
           Console.WriteLine("SeekTo() called on Light");
        }

        public void Stop()
        {
            Console.WriteLine("Stop() called on Light");
        }

        public void Restart()
        {
            Console.WriteLine("Restart() called on Light");
        }
    }

    public class CueRunner
    {
        private readonly double OPACITY_FULL = 1;//chatGPT doesn't like magic numbers, and GPT is my code reviewer, so here we are
        private readonly double OPACITY_NONE = 0;

        private int fadeInCount = 0, fadeOutCount = 0; 

        public event EventHandler? FadedIn, FadedOut;
        public Cue Cue { get; set; }
        public VideoWindow VideoWindow { get; set; }

        //insert list of IRunnableVisualCues here
        public List<IRunnableVisualCue> VisualCues { get; set; } = new();
        public Timer Timer { get; set; }
        public int LoopCount { get; set; } = 0;
        public int ElapsedTicks { get; set; } = 0;

        public double RealDuration { get; set; } = 0;


        public CueRunner(Cue cue, VideoWindow videoWindow)
        {
            Cue = cue;
            VideoWindow = videoWindow;
            Timer = new System.Timers.Timer(GlobalSettings.TickRate);
            Timer.Elapsed += Timer_Tick;
            Light l = new(cue.LightScene);
            VisualCues.Add(l);
            DetermineFileEndingEvent(l);
            foreach (var sf in cue.ScreenFiles.Values)
            {
                //TODO: Add support for other types of cues
                CustomMediaElement cme;
                if (sf is VideoFile vf)
                    cme = new VideoMediaElement(vf);
                else if (sf is ImageFile imgf)
                    cme = new ImageMediaElement(imgf);
                else
                    throw new Exception("Unknown VisualCue type, dumbass. wtf u doin boi");
                DetermineFileEndingEvent(cme);
                VideoWindow.Container.Children.Add(cme);
                VisualCues.Add(cme);
            }
            FadedOut += OnFadedOut;
            Console.WriteLine(cue.CueEndAction);
        }

        private void DetermineFileEndingEvent(IRunnableVisualCue vc)
        {
            if (vc.File.EndAction == EndAction.FadeAfterEnd)
                vc.PlaybackEnded += (s, e) => vc.FadeOut();
            else if (vc.File.EndAction == EndAction.Loop)
                vc.PlaybackEnded += (s, e) => vc.Restart();
            else if (vc.File.EndAction == EndAction.FadeBeforeEnd)
                Timer.Elapsed += FileEndWatch;

        }
        private void FileEndWatch(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                foreach (var vc in VisualCues)
                    if (!vc.IsFadingOut && vc.Duration - (ElapsedTicks * GlobalSettings.TickRate / 1000.0) <= vc.File.FadeOutDuration)
                        vc.FadeOut();
            });
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
                Console.WriteLine(ElapsedTicks * GlobalSettings.TickRate / 1000.0);
                ElapsedTicks++;
                if (Cue.CueEndAction == EndAction.FadeBeforeEnd && ElapsedTicks * GlobalSettings.TickRate / 1000.0 >= (RealDuration - Cue.FadeOutTime))
                    End();
                else if (ElapsedTicks * GlobalSettings.TickRate / 1000.0 >= RealDuration)
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
        public void End()
        {
            ElapsedTicks = 0;
            Timer.Stop();
            foreach (var vc in VisualCues)
            {
                vc.FadedOut += VisualCueFadedOut;
                vc.FadeOut(Cue.FadeOutTime);
            }
        }

        public void OnFadedOut(object? sender, EventArgs e)
        {
            foreach (var vc in VisualCues)
            {
                vc.Stop();
                if (vc is CustomMediaElement cme)
                    VideoWindow.Container.Children.Remove(cme);
            }
        }

        public void VisualCueFadedIn(object? sender, EventArgs e)
        {
            fadeInCount++;

            if(fadeInCount >= VisualCues.Count)
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

        public void SeekTo(int tick, bool play = false)
        {
            ElapsedTicks = tick;
            double seconds = tick * GlobalSettings.TickRate / 1000.0;
            foreach (var vc in VisualCues)
            {
                vc.Pause();
                vc.ClearCurrentAnimations();
                if (seconds < Cue.FadeInTime)
                    SeekedToFadeIn(vc, seconds, play);
                else if (seconds > vc.Duration)
                    SeekedToAfterEnd(vc, seconds, play);
                else if (vc.File.EndAction == EndAction.FadeBeforeEnd && (vc.Duration - seconds) < Cue.FadeOutTime)
                    SeekedToFadeBeforeEnd(vc, seconds, play);
                else
                    SeekedToNormalPlayback(vc, seconds, play);
            }

        }

        private void SeekedToFadeIn(IRunnableVisualCue vc, double time, bool play)
        {
            if(LoopCount == 0)
                vc.Opacity = time / Cue.FadeInTime;
            vc.SeekTo(time);
            if (play)
                vc.FadeIn(Cue.FadeInTime - time);
        }

        private void SeekedToAfterEnd(IRunnableVisualCue vc, double time, bool play)
        {
            time = Math.Max(time, 0);
            if (vc.File.EndAction == EndAction.Freeze)
            {
                vc.Opacity = OPACITY_FULL;
                vc.SeekTo(time);
            }
            else if (vc.File.EndAction == EndAction.FadeBeforeEnd)
            {
                vc.Opacity = OPACITY_NONE;
                vc.SeekTo(time);
            }
            else if (vc.File.EndAction == EndAction.FadeAfterEnd)
            {
                vc.Opacity = (time - vc.Duration ?? 0) / Cue.FadeOutTime;
                vc.SeekTo(time);
                if (play)
                    vc.FadeOut(Cue.FadeOutTime - (time - vc.Duration ?? 0));
            }
            else if (vc.File.EndAction == EndAction.Loop)
            {
                vc.Opacity = OPACITY_FULL;
                while (time > vc.Duration)
                    time -= vc.Duration ?? 1;
                vc.SeekTo(time);
                if (play)
                    vc.Play();
            }
        }
        private void SeekedToFadeBeforeEnd(IRunnableVisualCue vc, double time, bool play)
        {
            vc.Opacity = (time - vc.Duration ?? 0) / Cue.FadeOutTime;
            vc.SeekTo(time);
            if (play)
                vc.FadeOut(Cue.FadeOutTime - (vc.Duration ?? 0 - time));
        }
        private void SeekedToNormalPlayback(IRunnableVisualCue vc, double time, bool play)
        {
            vc.Opacity = OPACITY_FULL;
            vc.SeekTo(time);
            if (play)
                vc.Play();
        }
    }
}
