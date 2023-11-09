using System;

namespace DeLight.Utilities
{
    public static class Messenger
    {
        public static event EventHandler<CueTickEventArgs>? CueTick;

        public static event Action<double, bool>? SeekTo;

        public static void SendCueTick(object? sender, CueTickEventArgs e)
        {
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
