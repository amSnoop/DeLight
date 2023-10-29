
namespace DeLight.Models
{
    public class Step
    {
        public byte?[] DmxValues { get; set; } = new byte?[512];
        public int Duration { get; set; } = 300;
        public Step(byte?[] dmxValues, int duration)
        {
            DmxValues = dmxValues;
            Duration = duration;
        }
        public Step() { }
    }
}
