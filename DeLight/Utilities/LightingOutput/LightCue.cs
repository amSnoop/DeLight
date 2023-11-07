using DeLight.Models.Files;
using DeLight.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DeLight.Interfaces;

namespace DeLight.Utilities.LightingOutput
{
    //This file was created about 3 months after the rest of the IRunnableVisualCue implementations after not working on the project, so it's a bit different.


    public class LightCue
    {

        public double Duration { get; set; }
        public LightFile File { get; }
        public double? FadeOutStartTime { get; private set; }

        public bool IsFadingOut { private set; get; }
        public List<Step> Steps { get; private set; } = new();


        public int curTimeInHoS = 0;

        public LightCue(LightFile lf)
        {
            File = lf;
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


        #region Internal Methods

        #endregion

    }
}
