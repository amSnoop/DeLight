using DeLight.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace DeLight.Utilities
{
    public static class SXPFileParser
    {


        private static readonly Dictionary<int, int> fixIDToChanMapping = LoadFixtures();

        public static Dictionary<int, int> LoadFixtures()
        {
            var dict = new Dictionary<int, int>();
            var lines = File.ReadAllLines(Path.Combine(GlobalSettings.Instance.LightShowDirectory, "fixtures.ini"));

            int? currentId = null;
            int? currentAddress = null;

            foreach (var line in lines)
            {
                if (line.StartsWith("id = "))
                {
                    currentId = int.Parse(line.Split('=')[1].Trim());
                }
                else if (line.StartsWith("address = "))
                {
                    currentAddress = int.Parse(line.Split('=')[1].Trim());
                }

                if (currentId.HasValue && currentAddress.HasValue)
                {
                    dict[currentId.Value] = currentAddress.Value;
                    currentId = null;
                    currentAddress = null;
                }
            }
            return dict;
        }

        public static List<Step> ReadSXPSceneFile(string filePath)
        {
            if(!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File {filePath} not found.");
                return new();
            }
            var frames = new List<Step>();
            int elapsed = 0;
            XDocument xDoc = XDocument.Load(filePath);

            foreach (var step in xDoc.Descendants("Step"))
            {
                List<Channel> channels = new();
                if (!int.TryParse(step.Attribute("length")?.Value, out int stepLength))
                {
                    Console.WriteLine($"Error: Couldn't parse 'length' attribute in Step. Skipping this Step.");
                    continue;
                }
                elapsed += stepLength;
                foreach (var fixture in step.Descendants("Fixture"))
                {
                    if (!int.TryParse(fixture.Attribute("id")?.Value, out int fixtureId))
                    {
                        Console.WriteLine($"Error: Couldn't parse 'id' attribute in Fixture. Skipping this Fixture.");
                        continue;
                    }

                    if (!fixIDToChanMapping.TryGetValue(fixtureId, out int fixtureAddress))
                    {
                        Console.WriteLine($"Error: Fixture ID {fixtureId} not found in mapping. Skipping this Fixture.");
                        continue;
                    }

                    foreach (var channel in fixture.Descendants("Channel"))
                    {
                        if (!int.TryParse(channel.Attribute("index")?.Value, out int channelIndex) ||
                            !byte.TryParse(channel.Attribute("value")?.Value, out byte value) ||
                            !byte.TryParse(channel.Attribute("fade")?.Value, out byte fade) ||
                            channel.Attribute("name")?.Value is not string name)
                        {
                            Console.WriteLine($"Error: Couldn't parse 'index', 'value', or 'fade' attribute in Channel. Skipping this Channel.");
                            continue;
                        }

                        if ((fixtureAddress + channelIndex - 1) < 512) // Checking to avoid index out of range, also - 1 because DMX channels start at 1
                        {
                            channels.Add(new()
                            {
                                Value = value,
                                Index = fixtureAddress + channelIndex - 1,
                                Fade = fade == 1,
                                IsDimmer = name.ToLower().Contains("dimmer") || name.ToLower().Contains("par_can")
                            });
                        }
                    }
                }

                frames.Add(new(channels, stepLength, elapsed));
            }

            return frames;
        }


    }
}
