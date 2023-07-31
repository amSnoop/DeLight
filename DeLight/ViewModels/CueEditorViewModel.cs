using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Interfaces;
using DeLight.Models;
using DeLight.Models.Files;
using DeLight.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeLight.ViewModels
{
    public enum ExpectedFileType
    {
        Image,
        Video,
        Gif,
        Audio,
        Blackout,
        Lights
    }
    public partial class CueFileViewModel : ObservableObject
    {
        [ObservableProperty]
        private string path;
        [ObservableProperty]
        private double volume;
        [ObservableProperty]
        private ExpectedFileType type;
        [ObservableProperty]
        private EndAction endAction;
        [ObservableProperty]
        private double fadeInDuration;
        [ObservableProperty]
        private double fadeOutDuration;
        [ObservableProperty]
        private double duration;
        [ObservableProperty]
        private BlackoutReason reason;

        private int _index;
        public string Header => _index == 0 ? "Light Scene" : $"Projector {_index}";

        public CueFileViewModel(CueFile file, int index)
        {
            _index = index;
            path = file.FilePath;
            volume = file is VideoFile vf ? vf.Volume : 1;
            endAction = file.EndAction;
            fadeInDuration = file.FadeInDuration;
            fadeOutDuration = file.FadeOutDuration;
            duration = file is IDurationFile df ? df.Duration : 0;


            if (file is AudioFile)
                type = ExpectedFileType.Audio;
            else if (file is ScreenFile screenFile)
            {
                if (screenFile is VideoFile)
                    type = ExpectedFileType.Video;
                else if (screenFile is GifFile)
                    type = ExpectedFileType.Gif;
                else if (screenFile is ImageFile)
                    type = ExpectedFileType.Image;
                else if (screenFile is BlackoutScreenFile bsf)
                {
                    type = ExpectedFileType.Blackout;
                    reason = bsf.Reason;
                }
                else
                    throw new Exception("Invalid ScreenFile type");
            }
            else if (file is LightFile)
            {
                if (file is BlackoutLightFile bsf)
                {
                    type = ExpectedFileType.Blackout;
                    reason = bsf.Reason;
                }
                else
                    type = ExpectedFileType.Lights;
            }
            else
                throw new Exception("Invalid CueFile type");
        }
    }
    public partial class CueEditorViewModel : ObservableObject
    {
        private Cue cue;
        private bool isNew;
        public bool IsSaved { get; private set; } = true;

        public int NumProjectors => GlobalSettings.Instance.NumProjectors;


        [ObservableProperty]
        private string number;
        [ObservableProperty]
        private string note;
        [ObservableProperty]
        private double fadeInTime;
        [ObservableProperty]
        private double fadeOutTime;
        [ObservableProperty]
        private double duration;
        [ObservableProperty]
        private double volume;
        [ObservableProperty]
        private FadeType fadeType;
        [ObservableProperty]
        private EndAction cueEndAction;
        [ObservableProperty]
        private List<CueFileViewModel?> files = new();
        [ObservableProperty]
        private bool isDefault;
        public CueEditorViewModel(Cue cue, bool isDefault = false)
        {
            this.cue = cue;
            isNew = cue.Number == "0";
            IsDefault = isDefault;
            number = cue.Number;
            note = cue.Note;
            fadeInTime = cue.FadeInTime;
            fadeOutTime = cue.FadeOutTime;
            duration = cue.Duration;
            volume = cue.Volume;
            fadeType = cue.FadeType;
            cueEndAction = cue.CueEndAction;
            files.Add(new(cue.LightScene, 0));
            foreach (var file in cue.ScreenFiles)
            {
                while (files.Count <= file.Key)
                    files.Add(null);
                files[file.Key] = new(file.Value, file.Key);
            }
        }

        public void Save()
        {
            IsSaved = true;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            IsSaved = false;
        }
    }
}
