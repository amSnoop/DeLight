using DeLight.Models.Files;
using DeLight.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DeLight.Utilities.LightingOutput
{
    //This file was created about 3 months after the rest of the IRunnableVisualCue implementations after not working on the project, so it's a bit different.


    public class LightCue : IRunnableVisualCue
    {

        public double? Duration { get; set; }
        public LightFile File { get; }

        CueFile IRunnableVisualCue.File => File;
        public double Opacity { get; set; }//unused for lights but required for interface
        public double? FadeOutStartTime { get; private set; }

        public bool IsFadingOut { private set; get; }
        public List<Step> Steps { get; private set; } = new();

        public event EventHandler? FadedIn;
        public event EventHandler? FadedOut;
        public event EventHandler? PlaybackEnded;

        public int curTimeInHoS = 0;

        public LightCue(LightFile lf)
        {
            File = lf;
        }

        public void SendTimeUpdate(double timeInSeconds)
        {
            curTimeInHoS = (int)Math.Round(timeInSeconds * 100);
        }

        public void ClearCurrentAnimations() { }

        public void FadeIn(double startTime = 0)
        {
            LightingManager.Position = (int)Math.Round(startTime * 100);
        }

        public void FadeOut(double startTime)
        {
            if (!IsFadingOut)
            {
                IsFadingOut = true;
                LightingManager.FadeOut(startTime, File.FadeOutDuration);
            }
        }

        public Task LoadAsync()
        {
            Steps = SXPFileParser.ReadSXPSceneFile(File.FilePath);
            if (Steps.Count == 0)
            {
                Console.WriteLine("Error: No steps in scene file");
                Steps.Add(new());
                return Task.CompletedTask;
            }
            int i = 0;
            foreach (var step in Steps)
            {
                i += step.Duration;
            }
            Duration = i;


            if (File.EndAction == EndAction.FadeAfterEnd)
                FadeOutStartTime = Duration;
            else if (File.EndAction == EndAction.FadeBeforeEnd)
                FadeOutStartTime = Duration - File.FadeOutDuration;


            return Task.CompletedTask;
        }

        public void Pause()
        {
            LightingManager.Pause();
        }

        public void Play()
        {
            LightingManager.Play();
        }

        public void SeekTo(double time, bool play)
        {
            Pause();
            LightingManager.Position = (int)Math.Round(time * 100);
            if (play)
                Play();
        }

        public void Stop()
        {
            LightingManager.UpdateCue(null);
        }

        public void Restart()
        {
            LightingManager.UpdateCue(this);
        }


        #region Internal Methods

        #endregion

    }
}
