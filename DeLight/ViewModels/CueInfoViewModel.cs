using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Models;
using DeLight.Models.Files;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeLight.ViewModels
{
    public partial class CueInfoViewModel : CueViewModel
    {
        public ObservableCollection<CueFile> CueFiles = new();

        public string Number => "#" + (Cue?.Number.ToString() ?? "") + ": ";

        public string Note => Cue?.Note ?? "";

        public string FadeIn => Cue?.FadeInTime.ToString() ?? "";

        public string FadeOut => Cue?.FadeOutTime.ToString() ?? "";

        public string Duration => Cue?.Duration.ToString() ?? "";

        public bool IsCueNull => Cue == null;

        public string CueNullReason { get; }

        public CueInfoViewModel()
        {
            Cue = new();
            CueFiles.Add(Cue.LightScene);
            foreach (CueFile file in Cue.ScreenFiles.Values.ToList())
            {
                CueFiles.Add(file);
            }
            CueNullReason = "No cue";
        }
        public CueInfoViewModel(Cue? cue, string type) : base(cue)
        {
            if (cue != null)
            {
                CueFiles.Add(cue.LightScene);
                foreach (CueFile file in cue.ScreenFiles.Values.ToList())
                {
                    CueFiles.Add(file);
                }
            }
            CueNullReason = type switch
            {
                "active" => "No cue active",
                "selected" => "No cue selected",
                _ => "Unknown error"
            };
        }
    }
}
