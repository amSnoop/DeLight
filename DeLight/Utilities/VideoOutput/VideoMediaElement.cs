using DeLight.Models.Files;
using System;
using DeLight.Models;
using System.Threading.Tasks;

namespace DeLight.Utilities.VideoOutput
{
    public class VideoMediaElement : BaseMediaElement
    {
        private readonly VideoFile file;
        public VideoMediaElement(VideoFile file) : base(file)
        {
            this.file = file;
            Volume = file.Volume;
            if (file.EndAction == EndAction.Loop)
                MediaEnded += (s, e) => Restart();
            else
                MediaEnded += (s, e) => TriggerPlaybackEnded();
        }
        //Loops the video without a fade
        public override void Restart()
        {
            Position = TimeSpan.Zero;
            Play();
        }
        public override async void SeekTo(double time, bool play)
        {
            if(!loaded)
            {
                bool loaded = await Task.WhenAny(tcs.Task, Task.Delay(5000)) == tcs.Task;
                if (!loaded)
                    throw new Exception("Video failed to load in time.");
            }
            Pause();
            if(Duration == -1)
                Duration = NaturalDuration.HasTimeSpan ? NaturalDuration.TimeSpan.TotalSeconds : -1;
            if (Duration == -1)
                throw new NullReferenceException("Attempted to seek to a time in a file with a null duration.");

            Position = TimeSpan.FromSeconds(
                                            File.EndAction == EndAction.Loop ?
                                                time % (double)Duration :
                                                Math.Min(time, (double)Duration));

            FetchOpacity(time);
            if(!IsInBackground || Opacity > 0)
                if (play)
                {
                    if (time < File.FadeInDuration)
                        FadeIn(time);
                    else if (time < intendedFadeOutStartTime || File.EndAction == EndAction.Loop || File.EndAction == EndAction.Freeze)
                        Play();
                    else
                        FadeOut(time);
                }
                else
                {
                    double vol = Volume;
                    Volume = 0;
                    Play();
                    System.Threading.Thread.Sleep(1);//This is a hack to get around a bug where the video will not seek to the correct position if it is not playing
                    Pause();
                    Volume = vol;
                }
        }

        public void SetVolume(VolumeSource src, double vol)
        {
            if (src == VolumeSource.Cue)
                file.Volume = vol;
            Volume = file.Volume * GlobalSettings.Instance.MasterVolume;
        }
    }
}
