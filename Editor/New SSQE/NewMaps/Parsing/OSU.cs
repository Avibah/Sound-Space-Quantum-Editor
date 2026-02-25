using New_SSQE.Misc;
using System.Globalization;

namespace New_SSQE.NewMaps.Parsing
{
    internal class OSU : IFormatParser
    {
        public static bool Read(string path)
        {
            string data = File.ReadAllText(path);

            string[] split = data.Split("\n");

            bool timing = false;
            bool hitObj = false;

            string artist = "";
            string title = "";
            string audioPath = "";

            for (int i = 0; i < split.Length; i++)
            {
                string line = split[i].Trim();

                try
                {
                    string[] subsplit = line.Split(":");
                    string[] set = line.Split(",");

                    if (!timing && !hitObj)
                    {
                        switch (subsplit.FirstOrDefault())
                        {
                            case "Artist":
                                artist = subsplit[1].Trim();
                                break;
                            case "Title":
                                title = subsplit[1].Trim();
                                break;
                            case "AudioFilename":
                                audioPath = subsplit[1].Trim();
                                break;
                        }
                    }

                    if (timing && !string.IsNullOrWhiteSpace(line))
                    {
                        bool canParse = double.TryParse(set[0], NumberStyles.Any, Program.Culture, out double time);
                        canParse &= float.TryParse(set[1], NumberStyles.Any, Program.Culture, out float bpm);

                        if (canParse)
                        {
                            bool inhereted = set.Length > 6 ? set[6] == "1" : bpm > 0;

                            bpm = (float)Math.Abs(Math.Round(60000 / bpm, 3));

                            if (bpm > 0 && inhereted)
                                Mapping.Current.TimingPoints.Add(new(bpm, (long)time));
                        }
                    }

                    if (hitObj && !string.IsNullOrWhiteSpace(line))
                    {
                        bool canParse = float.TryParse(set[0], NumberStyles.Any, Program.Culture, out float x);
                        canParse &= float.TryParse(set[1], NumberStyles.Any, Program.Culture, out float y);
                        canParse &= double.TryParse(set[2], NumberStyles.Any, Program.Culture, out double time);
                        canParse &= int.TryParse(set[3], NumberStyles.Any, Program.Culture, out int type);

                        if (canParse)
                        {
                            x = 5 - x / 64;
                            y = 4 - y / 64;

                            if ((type & 1) != 0)
                                Mapping.Current.Notes.Add(new(x, y, (long)time));
                        }
                    }
                }
                catch { }

                timing = line != "[HitObjects]" && (timing || line == "[TimingPoints]");
                hitObj = line != "[TimingPoints]" && (hitObj || line == "[HitObjects]");
            }

            if (!string.IsNullOrWhiteSpace(audioPath))
            {
                string id = FormatUtils.FixID($"{artist} - {title}");
                Mapping.Current.SoundID = id;

                File.Copy(Path.Combine(Path.GetDirectoryName(path) ?? "", audioPath), Assets.CachedAt($"{id}.asset"), true);
            }

            return true;
        }

        public static bool Write(string path)
        {
            return false;
        }
    }
}
