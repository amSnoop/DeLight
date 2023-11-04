
using System.Collections.Generic;
using System.Windows.Documents;

namespace DeLight.Models
{
    public class Step
    {
        public Dictionary<int, Channel> Chans = new();
        public int Duration { get; set; } = 300;

        public int StartTime { get; set; } = 0;
        public Step(List<Channel> chans, int duration, int startTime)
        {
            chans.ForEach(c => Chans.TryAdd(c.Index, c));//avoid refactoring the SXP parser
            Duration = duration;
            StartTime = startTime;
        }
        public Step() { }
    }
}
