using DeLight.Models.Files;
using DeLight.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using DeLight.Utilities;

namespace DeLight.Utilities.LightingOutput
{
    //This file was created about 3 months after the rest of the IRunnableVisualCue implementations after not working on the project, so it's a bit different.

    /*
     * 
     * 
     * 
     */
    public class LightCue : IRunnableVisualCue
    {

        public double? Duration { get; set; }
        public LightFile File { get; }

        CueFile IRunnableVisualCue.File => File;
        public double Opacity { get; set; }


        private readonly FadeType FadeType;//Unused, probably will be removed
        public bool IsFadingOut { private set; get; }
        public bool IsFadingIn { private set; get; }
        public event EventHandler? FadedIn;
        public event EventHandler? FadedOut;
        public event EventHandler? PlaybackEnded;

        private List<Step> steps = new();

        private byte?[] roughValues = new byte?[512];

        private byte[] startingValues = new byte[512];
        public LightCue(LightFile lf, FadeType fadeType)
        {
            File = lf;
            FadeType = fadeType;
        }

        //private void Timer_Tick(object? sender, ElapsedEventArgs e)
        //{
        //    elapsedTicks++;
        //    var curTimeInHoS = elapsedTicks * GlobalSettings.TickRate / 10;
        //    //HoS = hundredths of a second. elapsedTicks * GlobalSettings.TickRate = milliseconds elapsed. / 10 = hundredths of a second
        //    //this is because the SXP file is in hundredths of a second. i.e. a 2.95 second step is 295
        //    roughValues = RawValue(curTimeInHoS);

        //}

        public void ClearCurrentAnimations()
        {
            Console.WriteLine("ClearCurrentAnimations() called on Light");
        }

        public void FadeIn(double duration = -1)
        {
            if(steps.Count == 0)
            {
                steps.Add(new Step(new byte?[512], 0));
            }
            if(duration != -1)
            {
                steps[0].Duration = (int)duration*100;
            }
        }

        public void FadeOut(double duration = -1)
        {
            Console.WriteLine("FadeOut() called on Light");
        }

        public Task LoadAsync()
        {
            steps = SXPFileParser.ReadSXPSceneFile(File.FilePath);
            int i = 0;
            foreach (var step in steps)
            {
                i += step.Duration;
            }
            Duration = i;
            return Task.CompletedTask;
        }

        public void Pause()
        {
            Console.WriteLine("Pause() called on Light");
        }

        public void Play()
        {
            timer.Start();
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


        #region Internal Methods

        private byte?[] RawValue(int time)
        {
            byte?[] values = new byte?[512];
            byte?[]? startingFrame = null;
            byte?[]? endingFrame = null;
            int startTime = 0;
            int endTime = 0;
            if(time < steps.First().Duration)//if the time is before the first frame, use the first frame and the values from the previous cue
            {
                startingFrame = new byte?[512];
                for(int i = 0; i < 512; i++)
                {
                    startingFrame[i] = startingValues[i];
                }
                endingFrame = steps.First().DmxValues;
                endTime = steps.First().Duration;
            }
            else//if the time is after the first frame, find the two frames that the time is between
            {
                var elapsedTime = 0;
                foreach(var step in steps)
                {
                    elapsedTime += step.Duration;
                    if (step.Duration > time)
                    {
                        startingFrame = steps[steps.IndexOf(step) - 1].DmxValues;
                        endingFrame = step.DmxValues;
                        startTime = elapsedTime - steps[steps.IndexOf(step) - 1].Duration;
                        endTime = elapsedTime;
                        break;
                    }
                }
            }
            if(startingFrame == null || endingFrame == null)
            {
                Console.WriteLine("Error: Couldn't find starting or ending frame for LightCue");
                var x = new byte?[512];
                for(int i = 0; i < 512; i++)
                {
                    x[i] = startingValues[i];
                }
                return x;
            }
            for(int i = 0; i < 512; i++)//interpolate between the two frames
            {
                double? y1 = startingFrame[i];
                double? y2 = endingFrame[i];
                if (y2 == null)//if the value is null, it means that the light is off, so we don't need to interpolate. If y2 is not null, y1 will also not be null. (not true the other way around).
                {
                    values[i] = null;
                    continue;
                }
                double fraction = (double)(time - startTime) / (endTime - startTime);

                double interpolatedValue = (double)y1! + ((double)y2 - (double)y1!) * fraction;
                values[i] = (byte)Math.Round(interpolatedValue);
            }
            return values;
        }

        #endregion

    }
}
