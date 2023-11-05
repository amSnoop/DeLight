using DeLight.Models;
using System;

namespace DeLight.Utilities
{

    public enum VolumeSource
    {
        Master,
        Cue,
        File
    }
    public static class Messenger
    {

        public static Cue? ActiveCue;
        public static event EventHandler<CueTickEventArgs>? CueTick;

        public static event Action<double, bool>? SeekTo;

        public static event Action<VolumeSource, double, Cue?>? VolumeChanged;
        public static void SendCueTick(object sender, CueTickEventArgs e)
        {
            if (sender == ActiveCue)
                CueTick?.Invoke(sender, e);
        }

        public static void SendSeekTo(double time, bool play)
        {
            SeekTo?.Invoke(time, play);
        }

        public static void SendVolumeChanged(VolumeSource source, double volume, Cue? sender = null)
        {
            volume = Math.Clamp(volume, 0, 1);
            VolumeChanged?.Invoke(source, volume, sender);
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
