using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Models.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DeLight.Models
{
    public partial class Show : ObservableObject
    {
        [ObservableProperty]
        private string name = "";
        [ObservableProperty]
        private string path = "";
        [ObservableProperty]
        private List<Cue> cues = new();

        public Cue? this[int i]
        {
            get => Cues[i];
            set
            {
                if (Cues.Count > i)
                    Cues[i] = value ?? throw new ArgumentNullException(nameof(value));
                else if (Cues.Count == i)
                    Cues.Add(value ?? throw new ArgumentNullException(nameof(value)));
                else
                    throw new Exception("Tried to set value at index " + i + "/" + Cues.Count);
            }
        }

        public Show(string name, string path, List<Cue> cues)
        {
            Name = name;
            Path = path;
            Cues = cues;
        }
        public Show()
        {
            Name = "";
            Path = "";
            Cues = new();
        }
        public static Show Load(string filepath)
        {
            Show? show = null;
            if (!File.Exists(filepath) || !filepath.EndsWith("json"))
                Console.WriteLine("Could not find show at " + filepath);
            else
                show = JsonSerializer.Deserialize<Show>(filepath);

            if (show == null)
                Console.WriteLine("Could not load show from " + filepath);
            return show ?? LoadTestShow();
        }
        public static void Save(Show show, string? filepath = null)
        {
            string json = JsonSerializer.Serialize(show);
            File.WriteAllText(filepath ?? show.Path, json);
        }

        public static Show LoadTestShow()
        {
            Show show = new("Test Show", "testshow.json", new List<Cue>());
            show.Cues.Add(new Cue()
            {
                Number = "1",
                Note = "This was a triumph",
                ScreenFiles = new() {
                    {1, new VideoFile() {
                        FilePath = "C:\\Users\\Snoopy\\Videos\\Halo  The Master Chief Collection\\cutscene example.mp4"
                    } }
                },
                CueEndAction = EndAction.FadeBeforeEnd
                
            });
            show.Cues.Add(new Cue()
            {
                Number = "2",
                Note = "I'm leaving a note here:",
                ScreenFiles = new() {
                    {1,  new VideoFile() {
                        FilePath = "C:\\Users\\Snoopy\\Videos\\Halo  The Master Chief Collection\\elite dont give a fuck.mp4",
                    } }
                },
                CueEndAction = EndAction.Loop
            });
            show.Cues.Add(new Cue()
            {
                Number = "3",
                Note = "Huge success!",
                ScreenFiles = new() { {1, new VideoFile() { FilePath = "No" } } },
            });
            show.Cues.Add(new Cue()
            {
                Number = "4",
                Note = "It's hard to overstate my satisfaction.",
                ScreenFiles = new() { { 1, new ImageFile() { FilePath = "C:\\Users\\Snoopy\\Pictures\\4k-space-wallpaper-1.jpg" } } },
            });
            show.Cues.Add(new Cue()
            {
                Number = "5",
                Note = "Aperture Science",
                ScreenFiles = new() { { 1, new ImageFile() { FilePath = "\"C:\\Users\\Snoopy\\Pictures\\4k-space-wallpaper-1.jpg\"" } } },
            });
            show.Cues.Add(new Cue()
            {
                Number = "6",
                Note = "We do what we must, because, we can.",
            });
            show.Cues.Add(new Cue()
            {
                Number = "7",
                Note = "For the good of all of us,",
                Disabled = true
            });
            show.Cues.Add(new Cue()
            {
                Number = "8",
                Note = "Except the ones who are dead.",
            });
            return show;
        }
    }
}
