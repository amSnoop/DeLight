namespace DeLight.Models
{
    //Represnts a single channel in a step of a cue
    public class Channel
    {
        public bool Fade { get; set; }
        public byte Value { get; set; }
        public bool IsDimmer { get; set; }
        public int Index { get; set; }
    }
}
