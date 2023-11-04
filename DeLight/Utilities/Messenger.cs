using DeLight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeLight.Utilities
{
    public static class Messenger
    {

        public static Cue? ActiveCue;
        public static event EventHandler<CueTickEventArgs>? CueTick;

        public static event Action<double, bool>? SeekTo;
        public static void SendCueTick(object sender, CueTickEventArgs e)
        {
            if (sender == ActiveCue)
                CueTick?.Invoke(sender, e);
        }

        public static void SendSeekTo(double time, bool play)
        {
            SeekTo?.Invoke(time, play);
        }
    }

    public class CueTickEventArgs : EventArgs
    {
        public double CurTime { get; set; }
        public double Duration { get; set; }
        public CueTickEventArgs(double time, double duration)
        {
            CurTime = time;
            Duration = duration;
        }
    }
}
