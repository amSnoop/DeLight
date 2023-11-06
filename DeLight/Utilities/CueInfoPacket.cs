using DeLight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeLight.Utilities
{
    public class CueInfoPacket<T> where T : IRunnableVisualCue
    {
        public double FadeOutStartTime { get; set; }

        public double RealDuration { get; set; }

        public EndAction EndAction { get; set; }

        public T Cue { get; set; }
        public CueInfoPacket(Cue c, T rc)
        {
            Cue = rc;
            EndAction = c.CueEndAction;
            if (c.Duration == 0)
            {
                RealDuration = 0;
                FadeOutStartTime = 0;
            }
            else
            {
                switch (EndAction)
                {
                    case EndAction.FadeAfterEnd:
                        FadeOutStartTime = c.Duration;
                        RealDuration = c.Duration + rc.File.FadeOutDuration;
                        break;
                    case EndAction.FadeBeforeEnd:
                        FadeOutStartTime = c.Duration - rc.File.FadeOutDuration;
                        RealDuration = c.Duration;
                        break;
                    default:
                        RealDuration = c.Duration;
                        break;
                }
            }
        }
    }
}
