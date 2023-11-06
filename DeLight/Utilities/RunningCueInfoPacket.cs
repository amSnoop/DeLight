using DeLight.Models;

namespace DeLight.Utilities
{
    public class RunningCueInfoPacket<T> where T : IRunnableVisualCue
    {
        public CueInfoPacket<T> StartInfo { get; set; }
        public T VisualCue { get; set; }

        public double SentToBGStartTime { get; set; } = 0;
        public double Opacity { get; set; } = 0;

        public RunningCueInfoPacket(CueInfoPacket<T> cip)
        {
            StartInfo = cip;
            VisualCue = cip.Cue;
        }
    }
}
