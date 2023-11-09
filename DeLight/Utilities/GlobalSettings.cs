using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using DeLight.Models;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace DeLight.Utilities
{
    public partial class GlobalSettings : ObservableObject
    {
        private static GlobalSettings? instance;

        public static GlobalSettings Instance => instance ??= new();
        public static readonly int TickRate = 1000 / 20;// in milliseconds. this is 20 ticks per second

        public Cue DefaultCue { get; set; } = new();

        [ObservableProperty]
        private string lastShowPath = "";
        [ObservableProperty]
        private string videoDirectory = "";
        [ObservableProperty]
        private string lightShowDirectory = "";
        public double LastVideoScreenTop { get; set; }
        public double LastVideoScreenLeft { get; set; }
        public int LastScreenTop { get; set; }
        public int LastScreenLeft { get; set; }

        [ObservableProperty]
        private double masterVolume = 1;

        public WindowState WindowState { get; set; } = WindowState.Normal;

        [JsonIgnore]
        public Screen? Screen { get; set; }


        public GlobalSettings()
        {

        }

        public static void Load()
        {
            if (File.Exists("settings.json"))
            {
                using StreamReader r = new("settings.json");
                string json = r.ReadToEnd();
                Deserialize(json);
            }
            else
            {
                Console.WriteLine("settings.json file not found. Creating new settings file.");
                CreateNewInstance();
                Save();
            }
        }

        public static void Save()
        {
            Instance.LastVideoScreenTop = Instance.Screen?.Bounds.Top ?? 0;
            Instance.LastVideoScreenLeft = Instance.Screen?.Bounds.Left ?? 0;
            string json = JsonSerializer.Serialize(Instance);
            File.WriteAllText("settings.json", json);
        }

        private static void Deserialize(string json)
        {
            try
            {
                instance = JsonSerializer.Deserialize<GlobalSettings>(json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            instance ??= new();
            instance.Screen = Screen.AllScreens.Where(s => s.Bounds.Top == instance.LastVideoScreenTop && s.Bounds.Left == instance.LastVideoScreenLeft).FirstOrDefault() ?? Screen.PrimaryScreen;

            var oldScreen = Screen.AllScreens.FirstOrDefault(s => s.Bounds.Contains(instance.LastScreenLeft, instance.LastScreenTop));

            if (oldScreen == null)
            {
                if (Screen.PrimaryScreen != null)
                {
                    instance.LastScreenTop = Screen.PrimaryScreen.Bounds.Top + (Screen.PrimaryScreen.Bounds.Height - 720) / 2;
                    instance.LastScreenLeft = Screen.PrimaryScreen.Bounds.Left + (Screen.PrimaryScreen.Bounds.Width - 1080) / 2;
                }
                else
                {
                    instance.LastScreenTop = 0;
                    instance.LastScreenLeft = 0;
                }
            }
        }

        private static void CreateNewInstance()
        {
            instance = new();
            Console.WriteLine("Created new settings");
        }
    }

}
