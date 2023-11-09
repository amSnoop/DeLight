
using DeLight.Models;
using System;
using System.Management;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace DeLight.Utilities.LightingOutput
{
    /*
     * 
     * The idea behind this class is to act as an intermediary between the LightCue and the LightingController.
     * 
     * It will take fade requests and if a request comes from a cue other than the active one, it will cancel the active one and start the new one.
     * each lightCue will use a single LightCueRequest and change its properties to request a new update.
     * 
     * There might be some funky threading issues - im not sure - because the LightCue will send info based on the CueRunner timer, while the LIghtingManager will send info based on the LightingController timer.
     */



    /*
     * 
     * Massive refactoring:
     * 
     * ---pass the cues themselves here, and this will handle the timing of movign from step to step, including FadeIns
     * ---THis will store a reference to the state when the cue was started for FadeIn and returning to that state for scrubbing
     * 
     * ---It will have a Position field that should work similar to the VideoMediaElement's Position field
     * 
     * ---It will have a FadeOut method that takes the time that fadeout was supposed to start, the time it is starting, and the duration of the fadeout
     * 
     * ---It will only FadeOut dimmer (and par_can) channels, which will need to be mapped from the cue file.
     * 
     * ---It will support channels that are set instantly.
     * 
     * ---This will have methods like Play() Pause() to control the cue
     * 
     * ---The idea is to have the LightingController call this every tick and get the current values, and use that timer to update the Position field.
     * 
     * It's supposed to resemble the MediaElement class from the outside.
     * 
     */


    /*
     * 
     * More massive refactoring: (this is the current plan)
     * THis needs to manage the cue and its duration checking to see what needs to happen to the lights at any given time. At the moment, everything is controlled by the Light file.
     * 
     */
    public static class LightingManager
    {

        private static LightCue? activeCue = null;
        public static int Position { get; set; } = 0;

        private static byte[] valuesWhenCueChanged = new byte[512];

        private static byte[] lastSentValues = new byte[512];

        private static bool isPaused = false;

        private static byte[] fadeOutStartValues = new byte[512];

        private static int fadeOutStartTime, fadeOutDuration;
        public static async Task UpdateCue(Cue? cue)
        {
            if(cue == null)
            {
                activeCue = null;
                return;
            }
            LightCue c = new(cue.LightFile);
            await c.LoadAsync();
            if(c == null)
            {
                activeCue = null;
                return;
            }
            valuesWhenCueChanged = (byte[])lastSentValues.Clone();
            activeCue = c;
            if (c.FadeOutStartTime != null)
            {
                fadeOutStartTime = (int)Math.Round((double)c.FadeOutStartTime * 100);//HoS
                fadeOutStartValues = DoLinearInterpolation(fadeOutStartTime);//HoS
            }
            else
            {
                fadeOutStartTime = int.MaxValue;
            }
            fadeOutDuration = (int)Math.Round(c.File.FadeOutDuration);
            Position = 0;
            isPaused = false;
            return;
        }

        public static byte[] RequestValues(int HoSPassed)//Hundtedths of a second that have passed since last request
        {
            if (activeCue == null)
            {
                return new byte[512];
            }
            if (Position < 0)
            {
                return valuesWhenCueChanged;
            }
            if (!isPaused)
            {
                Position += HoSPassed;
            }
            var values = CalculateFadeResults();
            lastSentValues = values;
            return values;
        }
        public static byte[] DoLinearInterpolation(int time)
        {
            var currentValues = new byte[512];
            Step? curStep = null;
            Step? prevStep = null;
            for (int i = activeCue!.Steps.Count - 1; i >= 0; i--)
            {
                Step tempStep = activeCue.Steps[i];
                if (time >= tempStep.StartTime && time < tempStep.Duration + tempStep.StartTime)
                {
                    curStep = tempStep;
                    if (i > 0)
                    {
                        prevStep = activeCue.Steps[i - 1];
                    }
                }
            }
            if (curStep == null)//position is outside of the cue
            {
                return new byte[512];
            }
            double percentComplete = (time - curStep.StartTime) / (double)curStep.Duration;
            //linear interpolation between start and end values for each value in the byte[]
            for (int i = 0; i < 512; i++)
            {
                double start, end;
                bool chanInStep = curStep.Chans.TryGetValue(i, out var chan);
                if (prevStep == null)
                {
                    start = valuesWhenCueChanged[i];
                }
                else
                {
                    start = chanInStep ? prevStep.Chans[i]!.Value : 0;
                }
                if (chanInStep)
                {
                    end = chan!.Value;
                }
                else
                {
                    end = 0;
                }
                double value = chan!.Fade ? start + (end - start) * percentComplete : end;
                currentValues[i] = (byte)double.Clamp(value, 0, 255);
            }
            return currentValues;

        }

        public static void FadeOut(double startTime, double duration)
        {
            if (activeCue == null)
            {
                return;
            }
            fadeOutStartTime = (int)Math.Round(startTime * 100);
            fadeOutStartValues = DoLinearInterpolation(fadeOutStartTime);//HoS
            fadeOutDuration = (int)Math.Round(duration * 100);
            Play();
        }

        private static byte[] CalculateFadeResults()
        {
            var valsToSend = new byte[512];

            if (Position < fadeOutStartTime)
            {
                return DoLinearInterpolation(Position);
            }
            else if (Position > fadeOutStartTime + fadeOutDuration)
            {
                return new byte[512];
            }
            else
            {
                for (int i = 0; i < 512; i++)
                {
                    double percentComplete = (Position - fadeOutStartTime) / fadeOutDuration;
                    if (!activeCue!.Steps[0].Chans.TryGetValue(i, out var chan) || chan.IsDimmer)
                    //if the channel is not in the cue or is a dimmer (all steps have the same channels)
                    {
                        double value = fadeOutStartValues[i] + (0 - fadeOutStartValues[i]) * percentComplete;//linear interp between start and 0
                        valsToSend[i] = (byte)double.Clamp(value, 0, 255);
                    }
                }
            }
            return valsToSend;
        }

        public static void Play()
        {
            isPaused = false;
        }
        public static void Pause()
        {
            isPaused = true;
        }


        public static void SeekTo(double time, bool play)
        {
            Pause();
            Position = (int)Math.Round(time * 100);
            if (play)
            {
                Play();
            }
        }

        public static async void Stop()
        {
            await UpdateCue(null);
        }
    }
}
