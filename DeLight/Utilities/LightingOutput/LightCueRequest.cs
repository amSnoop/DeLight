using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeLight.Utilities.LightingOutput
{
    public class LightCueRequestCompletedEventArgs : EventArgs
    {
        public bool IsCancelled { get; set; }

        public LightCueRequestCompletedEventArgs(bool isCancelled)
        {
            IsCancelled = isCancelled;
        }
    }
    //This class is used to pass requests from the LightCue to the LightingManager.
    //The idea is to reuse this class in the LightCue changing the properties but never changing its place in the LightingManager's queue.
    public class LightCueRequest
    {
        public byte?[] StartValues { get; set; } = new byte?[512];
        public byte?[] EndValues { get; set; } = new byte?[512];
        public double Duration { get; set; }

        public double TimePassed { get; set; } = 0;

        public bool IsCancelled { get; private set; } = false;

        public LightCueRequest(byte?[] startValues, byte?[] endValues, double duration)
        {
            StartValues = startValues;
            EndValues = endValues;
            Duration = duration;
        }
        public void SendNext(byte?[] endValues, double duration)
        {
            StartValues = EndValues;
            EndValues = endValues;
            Duration = duration;
            TimePassed = 0;
        }

        public void Cancel()
        {
            IsCancelled = true;
        }
    }
}
