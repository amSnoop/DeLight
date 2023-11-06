using Avalonia.Threading;
using DeLight.Models;
using DeLight.Models.Files;
using DeLight.Utilities.LightingOutput;
using DeLight.Utilities.VideoOutput;
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
    //public class CueRunner
    //{

    //    private int fadeInCount = 0, fadeOutCount = 0;

    //    public event EventHandler? FadedIn, FadedOut;
    //    public Cue Cue { get; set; }

    //    public LightCue LightCue { get; set; }

    //    public IRunnableScreenCue ScreenFile { get; set; }
    //    public Timer Timer { get; set; }
    //    public int LoopCount { get; set; } = 0;
    //    public int ElapsedTicks { get; set; } = 0;
    //    public double ElapsedTime { get => ElapsedTicks * GlobalSettings.TickRate / 1000.0; }
    //    public double RealDuration { get; set; } = 0;

    //    private double nextCueFadeInTimeStamp = 0;
    //    private double nextCueFadeInDuration = 0;
    //    private bool isTakenOver = false;
    //    public CueRunner(Cue cue)
    //    {
    //        Cue = cue;
    //        Timer = new Timer(GlobalSettings.TickRate);
    //        Timer.Elapsed += Timer_Tick;
    //        LightCue = new(cue.LightFile);
    //        var sf = cue.ScreenFile;
    //        IRunnableScreenCue cme;
    //        if (sf.ErrorState != FileErrorState.None)
    //            cme = new BlackoutScreenCue(new() { FadeInDuration = sf.FadeInDuration });
    //        else if (sf is BlackoutScreenFile bof)
    //            cme = new BlackoutScreenCue(bof); //extends Border
    //        else if (sf is VideoFile vf)
    //        {
    //            vf.Volume = cue.Volume;//only one screen file per cue means just make it the cue volume to manage it easier.
    //            cme = new VideoMediaElement(vf);
    //        }
    //        else if (sf is ImageFile imgf)
    //            cme = new ImageMediaElement(imgf); //extends CustomMediaElement
    //        else
    //            throw new Exception("Unknown VisualCue type: " + sf.GetType());
    //        ScreenFile = cme;
    //        cme.FadedIn += VisualCueFadedIn;
    //        FadedOut += OnFadedOut;
    //        Messenger.VolumeChanged += (source, volume, cue) =>
    //        {
    //            if (cue != null && cue == Cue)
    //            {
    //                if (cme is VideoMediaElement vme)
    //                    vme.SetVolume(source, volume);
    //            }
    //            else if (source == VolumeSource.Master && cme is VideoMediaElement vm)
    //                vm.SetVolume(source, volume);
    //        };

    //    }


    //    public void FindRealCueDuration()
    //    {
    //        if (Cue.Duration == 0)
    //        {
    //            if(LightCue.Duration > RealDuration)
    //                RealDuration = LightCue.Duration ?? 0;
    //            if(ScreenFile.Duration > RealDuration)
    //                RealDuration = ScreenFile.Duration ?? 0;
    //        }
    //        else
    //            RealDuration = Cue.Duration;
    //        if(Cue.CueEndAction == EndAction.FadeAfterEnd)
    //            RealDuration += Cue.FadeOutTime;
    //    }

    //    public void Timer_Tick(object? s, EventArgs e)
    //    {
    //        Dispatcher.UIThread.InvokeAsync(() =>
    //        {
    //            ElapsedTicks++;
    //            if ((Cue.CueEndAction == EndAction.FadeBeforeEnd || Cue.CueEndAction == EndAction.FadeAfterEnd) && ElapsedTime >= (RealDuration - Cue.FadeOutTime))
    //                End();
    //            else if (ElapsedTime >= RealDuration)
    //            {
    //                if (Cue.CueEndAction == EndAction.Loop)
    //                {
    //                    Console.WriteLine("Looping");
    //                    LoopCount++;
    //                    Timer.Stop();
    //                    Restart();
    //                }
    //                else if (Cue.CueEndAction == EndAction.FadeAfterEnd)
    //                    End();
    //            }
    //        });
    //        Messenger.SendCueTick(Cue, new(ElapsedTime, RealDuration));
    //    }
    //    public void End()
    //    {
    //        ElapsedTicks = 0;
    //        Timer.Stop();
    //        LightCue.FadeOut(ElapsedTime);
    //        ScreenFile.FadeOut(ElapsedTime);
    //        ScreenFile.FadedOut += VisualCueFadedOut;
    //    }

    //    public void OnFadedOut(object? sender, EventArgs e)
    //    {
    //        Cue.IsActive = false;
    //    }

    //    public void VisualCueFadedIn(object? sender, EventArgs e)
    //    {
    //        FadedIn?.Invoke(this, EventArgs.Empty);
    //    }
    //    public void VisualCueFadedOut(object? sender, EventArgs e)
    //    {
    //        FadedOut?.Invoke(this, EventArgs.Empty);

    //    }

    //    public void Restart()
    //    {
    //        SeekTo(0, true);
    //        Timer.Start();
    //    }

    //    //Plays from the beginning of the cue
    //    public async void Play()
    //    {

    //        //wait for media to open before attempting to find its duration
    //        List<Task> loadTasks = new () { ScreenFile.LoadAsync(), LightCue.LoadAsync() };
    //        await Task.WhenAll(loadTasks);


    //        //if HasTimeSpan returns false, then some real shenanigans are afoot... Should probably throw an exception or something but whatever (TODO) :P
    //        //That comment brought to you by GitHub Copilot - Surprisingly funny lmao
    //        FindRealCueDuration();
    //        SeekTo(0, true);
    //        Timer.Start();
    //    }

    //    public void Pause()
    //    {
    //        Timer.Stop();
    //        SeekTo(ElapsedTicks, false);
    //    }
    //    public void Unpause()
    //    {
    //        Timer.Start();
    //        SeekTo(ElapsedTicks, true);
    //    }
    //    public void Stop()
    //    {
    //        Timer.Stop();
    //        ElapsedTicks = 0;
    //        OnFadedOut(this, EventArgs.Empty);
    //        ScreenFile.Stop();
    //    }

    //    public void SeekTo(double time, bool play = false)
    //    {
    //        if(isTakenOver)
    //            time = nextCueFadeInTimeStamp + time;
    //        int tick = (int)(time * 1000 / GlobalSettings.TickRate);
    //        ElapsedTicks = tick;
    //        LightCue.SeekTo(time, play);
    //        ScreenFile.SeekTo(time, play);

    //    }
    //    public void PrepareForTakeover(Cue cue)
    //    {
    //        nextCueFadeInTimeStamp = ElapsedTime;
    //        nextCueFadeInDuration = cue.ScreenFile.FadeInDuration;
    //    }
    //    private bool IsBehindOtherCue()
    //    {
    //        return isTakenOver && ElapsedTime >= nextCueFadeInTimeStamp + nextCueFadeInDuration;
    //    }
    //}
}
