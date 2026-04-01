using New_SSQE.Misc;
using New_SSQE.Preferences;
using System.Text.Json;

namespace New_SSQE.NewMaps.Parsing
{
    internal class VMAP : IFormatParser
    {
        public static bool IsValid(string data)
        {
            Dictionary<string, JsonElement> result = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(data)) ?? [];
            bool diffs = result.TryGetValue("_difficulties", out _);
            bool song = result.TryGetValue("_music", out _);
            bool ver = result.TryGetValue("_version", out _);

            return diffs && song && ver;
        }

        public static bool Read(string path)
        {
            if (!File.Exists(path))
                return false;

            Dictionary<string, JsonElement> result = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(path)) ?? [];
            string[] difficulties = [];
            string artist = "";
            string title = "";

            string music = "";

            foreach (string key in result.Keys)
            {
                JsonElement value = result[key];

                switch (key)
                {
                    case "_artist":
                        artist = value.ToString() ?? "";
                        Settings.songArtist.Value = artist;
                        Settings.romanizedArtist.Value = artist;
                        break;
                    case "_difficulties":
                        difficulties = JsonSerializer.Deserialize<string[]>(value) ?? [];
                        break;
                    case "_mappers":
                        string[] mappers = JsonSerializer.Deserialize<string[]>(value) ?? [];
                        Settings.mappers.Value = string.Join('\n', mappers);
                        break;
                    case "_music":
                        music = value.ToString() ?? "";
                        break;
                    case "_title":
                        title = value.ToString() ?? "";
                        Settings.songTitle.Value = title;
                        Settings.romanizedTitle.Value = title;
                        break;
                    case "_version":
                        int ver = value.GetInt32();
                        if (ver != 1)
                            throw new NotSupportedException($"Invalid VMAP version (Got: {ver} | Expected: 1)");
                        break;
                }
            }

            Settings.songName.Value = $"{artist} - {title}";

            music = Path.Combine(Path.GetDirectoryName(path) ?? "", music);
            if (!File.Exists(music))
                return false;

            for (int i = 0; i < difficulties.Length; i++)
            {
                string file = Path.Combine(Path.GetDirectoryName(path) ?? "", difficulties[i]);
                if (!File.Exists(file))
                    continue;

                string id = FormatUtils.FixID($"{artist} - {title} - {Path.GetFileNameWithoutExtension(file)}");
                Mapping.Current.SoundID = id;
                File.Copy(music, Assets.CachedAt($"{id}.asset"), true);

                Dictionary<string, JsonElement> map = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(file)) ?? [];

                foreach (string key in map.Keys)
                {
                    JsonElement value = map[key];

                    switch (key)
                    {
                        case "_name":
                            string diff = value.ToString() ?? "";
                            if (FormatUtils.Difficulties.ContainsKey(diff))
                                Settings.difficulty.Value = diff;
                            else
                                Settings.customDifficulty.Value = diff;
                            break;
                        case "_notes":
                            Dictionary<string, JsonElement>[] notes = JsonSerializer.Deserialize<Dictionary<string, JsonElement>[]>(value) ?? [];

                            foreach (Dictionary<string, JsonElement> note in notes)
                            {
                                if (!note.TryGetValue("_time", out JsonElement time))
                                    continue;
                                if (!note.TryGetValue("_x", out JsonElement x))
                                    continue;
                                if (!note.TryGetValue("_y", out JsonElement y))
                                    continue;

                                Mapping.Current.Notes.Add(new(x.GetSingle() + 1, y.GetSingle() + 1, (long)(time.GetDouble() * 1000)));
                            }

                            break;
                    }
                }

                if (i + 1 < difficulties.Length)
                {
                    Mapping.CacheCurrent();
                    Mapping.Current = new();
                }
            }

            return true;
        }

        public static bool Write(string path)
        {
            return false;
        }
    }
}
