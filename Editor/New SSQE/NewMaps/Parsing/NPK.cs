using New_SSQE.Audio;
using New_SSQE.ExternalUtils;
using New_SSQE.Misc.Dialogs;
using New_SSQE.Misc.Static;
using New_SSQE.Objects;
using New_SSQE.Preferences;
using System.IO.Compression;
using System.Text.Json;
using Un4seen.Bass;

namespace New_SSQE.NewMaps.Parsing
{
    internal class NPK : IFormatParser
    {
        private const int CURRENT_VERSION = 2;

        private static readonly Predicate<string>?[] readers = [Read_Version1, Read_Version2];

        public static bool Read(string path)
        {
            string directory = Path.GetDirectoryName(path) ?? "";
            string metadataPath = Path.Combine(directory, "metadata.json");

            Dictionary<string, JsonElement> metadata = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(metadataPath)) ?? [];
            int formatVersion = metadata.TryGetValue("formatVersion", out JsonElement format) ? format.GetInt32() : 1;

            if (formatVersion <= 0 || formatVersion > readers.Length || readers[formatVersion - 1] == null)
            {
                MessageBox.Show($"Unsupported NPK version: {formatVersion}", MBoxIcon.Warning, MBoxButtons.OK);
                return false;
            }

            return readers[formatVersion - 1]!(path);
        }

        public static void WriteNCH(string path)
        {
            List<Dictionary<string, object>> notes = new(Mapping.Current.Notes.Count);
            List<Dictionary<string, object>> beats = [];
            List<Dictionary<string, object>> glides = [];
            List<Dictionary<string, object>> events = [];

            for (int i = 0; i < Mapping.Current.Notes.Count; i++)
            {
                Note note = Mapping.Current.Notes[i];

                Dictionary<string, object> noteData = new()
                {
                    {"x", 1 - note.X },
                    {"y", note.Y - 1 },
                    {"t", note.Ms }
                };

                if (note.EnableEasing)
                {
                    noteData.Add("e", new Dictionary<string, object>()
                    {
                        {"s", (int)note.Style },
                        {"d", (int)note.Direction }
                    });
                }

                notes.Add(noteData);
            }

            Mapping.Current.Notes.Sort();
            Mapping.Current.SpecialObjects.Sort();

            MapObject[] validObjects = [..Mapping.Current.SpecialObjects.Where(n => n is Beat || n is Glide || n is Mine)];

            long lastMs = Mapping.Current.Notes.LastOrDefault()?.Ms ?? 0;
            lastMs = Math.Max(lastMs, validObjects.LastOrDefault()?.Ms ?? 0);

            long? prevFeverMs = null;

            foreach (MapObject obj in Mapping.Current.SpecialObjects)
            {
                switch (obj.ID)
                {
                    case 12 when obj is Beat beat:
                        beats.Add(new()
                        {
                            {"t", beat.Ms }
                        });
                        break;

                    case 13 when obj is Glide glide:
                        glides.Add(new()
                        {
                            {"t", glide.Ms },
                            {"d", glide.Direction switch
                            {
                                GlideDirection.Up => "up",
                                GlideDirection.Right => "right",
                                GlideDirection.Down => "down",
                                GlideDirection.Left => "left",
                                _ => "none"
                            } }
                        });
                        break;

                    case 14 when obj is Mine mine:
                        notes.Add(new()
                        {
                            {"x", 1 - mine.X },
                            {"y", mine.Y - 1 },
                            {"t", mine.Ms },
                            {"m", true }
                        });
                        break;

                    case 16 when obj is Fever fever:
                        if (fever.Ms >= lastMs)
                            break;

                        long curMs = Math.Max(prevFeverMs ?? 0, fever.Ms);
                        prevFeverMs = fever.Ms + fever.Duration;

                        events.Add(new()
                        {
                            {"type", "fever" },
                            {"t", curMs },
                            {"active", true }
                        });

                        events.Add(new()
                        {
                            {"type", "fever" },
                            {"t", Math.Min(fever.Ms + fever.Duration, lastMs) },
                            {"active", false }
                        });
                        break;
                }
            }

            foreach (TimingPoint point in Mapping.Current.TimingPoints)
            {
                events.Add(new()
                {
                    {"type", "tempo" },
                    {"t", point.Ms },
                    {"bpm", point.BPM }
                });
            }

            notes = [..notes.OrderBy(n => n["t"])];
            beats = [..beats.OrderBy(n => n["t"])];
            glides = [..glides.OrderBy(n => n["t"])];
            events = [..events.OrderBy(n => n["t"])];

            long exportOffset = (long)Settings.exportOffset.Value;
            long songOffset = long.TryParse(Metadata["songOffset"], out long result) ? result : 0;

            Dictionary<string, object> chart = new()
            {
                {"songOffset", songOffset - exportOffset },
                {"notes", notes },
                {"beats", beats },
                {"glides", glides },
                {"events", events }
            };

            File.WriteAllText(path, JsonSerializer.Serialize(chart, Program.JsonOptions));
        }

        public static void WriteNLR(string path)
        {
            Lyric[] lyrics = [..Mapping.Current.SpecialObjects.Where(n => n is Lyric).Cast<Lyric>()];

            if (lyrics.Length > 0)
            {
                List<string> set = [];

                for (int i = 0; i < lyrics.Length; i++)
                {
                    Lyric lyric = lyrics[i];
                    string text = lyric.Text;
                    string properties = "";

                    if (string.IsNullOrWhiteSpace(text) && !lyric.FadeOut)
                        properties += 'r';

                    if (text.StartsWith('-'))
                    {
                        properties += 'r';
                        text = text[1..];
                    }

                    if (text.EndsWith('-'))
                    {
                        properties += 'n';
                        text = text[..^1];
                    }

                    if (lyric.FadeIn)
                        properties += 'i';
                    if (lyric.FadeOut)
                        properties += 'o';

                    if (properties.Length == 0)
                        properties = "x";

                    set.Add($"{lyric.Ms}|{properties}|{text}");
                }

                File.WriteAllText(path, string.Join('\n', set));
            }
        }

        public static bool Write(string path)
        {
            string id = Mapping.Current.SoundID;

            string temp = Path.Combine(Assets.TEMP, "nova");
            Directory.CreateDirectory(temp);
            foreach (string file in Directory.GetFiles(temp))
                File.Delete(file);

            string extension = MusicPlayer.ctype switch
            {
                BASSChannelType.BASS_CTYPE_STREAM_MP3 => ".mp3",
                BASSChannelType.BASS_CTYPE_STREAM_OGG => ".ogg",
                _ => throw new FormatException($"AUDIO - not MP3/OGG ({MusicPlayer.ctype})"),
            };

            bool hasCover = !string.IsNullOrWhiteSpace(Metadata["coverPath"]);
            bool hasIcon = !string.IsNullOrWhiteSpace(Metadata["iconPath"]);

            if (hasCover)
                File.Copy(Metadata["coverPath"], Path.Combine(temp, $"{id}{Path.GetExtension(Metadata["coverPath"])}"), true);
            if (hasIcon)
                File.Copy(Metadata["iconPath"], Path.Combine(temp, $"profile{Path.GetExtension(Metadata["iconPath"])}"), true);
            
            File.Copy(Path.Combine(Assets.CACHED, $"{id}.asset"), Path.Combine(temp, $"{id}{extension}"), true);

            WriteNCH(Path.Combine(temp, "chart.nch"));

            Dictionary<string, object> metadata = new()
            {
                {"songTitle", Metadata["songTitle"] },
                {"songArtist", Metadata["songArtist"] },
                {"mapCreator", Metadata["mapCreator"] },
                {"mapCreatorPersonalLink", Metadata["mapCreatorPersonalLink"] },
                {"previewStartTime", long.Parse(Metadata["previewStartTime"]) / 1000f },
                {"previewDuration", long.Parse(Metadata["previewDuration"]) / 1000f },
                {"formatVersion", CURRENT_VERSION },
                {"ssqeVersion", Program.Version }
            };

            File.WriteAllText(Path.Combine(temp, "metadata.json"), JsonSerializer.Serialize(metadata, Program.JsonOptions));

            WriteNLR(Path.Combine(temp, "lyrics.nlr"));

            if (File.Exists(path))
                File.Delete(path);
            ZipFile.CreateFromDirectory(temp, path);
            foreach (string file in Directory.GetFiles(temp))
                File.Delete(file);

            return true;
        }

        public static Dictionary<string, string> Metadata = new()
        {
            {"songOffset", "" },
            {"songTitle", "" },
            {"songArtist", "" },
            {"mapCreator", "" },
            {"mapCreatorPersonalLink", "" },
            {"previewStartTime", "" },
            {"previewDuration", "" },
            {"coverPath", "" },
            {"iconPath", "" }
        };

        public static bool Export()
        {
            string mapper = Metadata["mapCreator"].ToLower().Replace(" ", "_");
            string title = Metadata["songTitle"].ToLower().Replace(" ", "_");
            string artist = Metadata["songArtist"].ToLower().Replace(" ", "_");
            string id = FormatUtils.FixID($"{mapper}_-_{artist}_-_{title}");

            DialogResult result = new SaveFileDialog()
            {
                Title = "Export NOVA",
                Filter = "Novastra Charts (*.npk)|*.npk",
                InitialFileName = id
            }.Show(Settings.exportPath, out string fileName);

            if (result == DialogResult.OK)
            {
                try
                {
                    Write(fileName);
                    return true;
                }
                catch (Exception ex)
                {
                    Logging.Log("Failed to export", LogSeverity.WARN, ex);
                    MessageBox.Show($"Failed to export NPK:\n\n{ex.Message}", MBoxIcon.Warning, MBoxButtons.OK);
                }
            }

            return false;
        }

        private static bool Read_Version1(string path)
        {
            string directory = Path.GetDirectoryName(path) ?? "";

            string id = "";
            string profile = "";
            string icon = "";

            Settings.songTitle.Value = "";
            Settings.songArtist.Value = "";
            Settings.mapCreator.Value = "";
            Settings.mapCreatorPersonalLink.Value = "";
            Settings.previewStartTime.Value = "";
            Settings.previewDuration.Value = "";

            foreach (string file in Directory.GetFiles(directory))
            {
                string filename = Path.GetFileNameWithoutExtension(file);

                switch (Path.GetExtension(file))
                {
                    case ".json" when filename == "metadata":
                        Dictionary<string, JsonElement> metadata = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(file)) ?? [];

                        foreach (string key in metadata.Keys)
                        {
                            try
                            {
                                JsonElement value = metadata[key];

                                switch (key)
                                {
                                    case "songTitle":
                                        Settings.songTitle.Value = value.GetString() ?? "";
                                        break;
                                    case "songArtist":
                                        Settings.songArtist.Value = value.GetString() ?? "";
                                        break;
                                    case "mapCreator":
                                        Settings.mapCreator.Value = value.GetString() ?? "";
                                        break;
                                    case "mapCreatorPersonalLink":
                                        Settings.mapCreatorPersonalLink.Value = value.GetString() ?? "";
                                        break;
                                    case "previewStartTime":
                                        Settings.previewStartTime.Value = ((long)(value.GetDouble() * 1000d)).ToString();
                                        break;
                                    case "previewDuration":
                                        Settings.previewDuration.Value = ((long)(value.GetDouble() * 1000d)).ToString();
                                        break;
                                }
                            }
                            catch { }
                        }

                        break;

                    case ".nlr":
                        string[] lines = File.ReadAllLines(file);
                        Lyric? prev = null;

                        for (int i = 0; i < lines.Length; i++)
                        {
                            string line = lines[i];
                            if (string.IsNullOrWhiteSpace(line))
                                continue;

                            string[] split = line.Split(' ');
                            string timestamp = split[0];
                            string text = string.Join(' ', split[1..]);
                            if (string.IsNullOrWhiteSpace(text))
                                continue;

                            timestamp = timestamp[1..^1];
                            string[] components = timestamp.Split('.');
                            string[] minSec = components[0].Split(':');

                            long minutes = long.Parse(minSec[0]);
                            long seconds = long.Parse(minSec[1]);
                            long ms = long.Parse(components[1]);
                            ms += 60000 * minutes + 1000 * seconds;

                            if (text == "...>" && prev != null)
                            {
                                prev.FadeOut = true;
                                if (prev.Ms != ms)
                                    prev = new(ms, "", false, false);
                            }
                            else if (text == "..." && prev != null && prev.Ms != ms)
                                prev = new(ms, "", false, false);
                            else if (text.StartsWith("->"))
                                prev = new(ms, '-' + text[2..], true, false);
                            else
                                prev = new(ms, text, false, false);

                            Mapping.Current.SpecialObjects.Add(prev);
                        }

                        break;

                    case ".png" when filename == "profile":
                    case ".jpg" when filename == "profile":
                        profile = file;
                        break;

                    case ".png" when filename != "profile":
                    case ".jpg" when filename != "profile":
                        icon = file;
                        break;

                    case ".mp3":
                    case ".ogg":
                    case ".wav":
                        id = filename;
                        File.Copy(file, Path.Combine(Assets.CACHED, $"{filename}.asset"), true);
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(profile))
            {
                string temp = Path.Combine(Assets.CACHED, $"{id}-profile{Path.GetExtension(profile)}");
                File.Copy(profile, temp, true);
                Settings.novaIcon.Value = temp;
            }
            else
                Settings.novaIcon.Value = "";

            if (!string.IsNullOrWhiteSpace(icon))
            {
                string temp = Path.Combine(Assets.CACHED, $"{id}-icon{Path.GetExtension(icon)}");
                File.Copy(icon, temp, true);
                Settings.novaCover.Value = temp;
                Settings.cover.Value = temp;
                Settings.useCover.Value = true;
            }
            else
            {
                Settings.novaCover.Value = "";
                Settings.cover.Value = "";
                Settings.useCover.Value = false;
            }

            Mapping.Current.SoundID = id;
            Settings.songName.Value = $"{Settings.songArtist.Value} - {Settings.songTitle.Value}";
            Settings.mappers.Value = Settings.mapCreator.Value;

            Dictionary<string, JsonElement> result = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(path)) ?? [];

            foreach (string key in result.Keys)
            {
                JsonElement value = result[key];

                switch (key)
                {
                    case "songOffset":
                        Settings.songOffset.Value = value.GetDouble().ToString(Program.Culture);
                        break;

                    case "notes":
                        Dictionary<string, JsonElement>[] noteSet = value.Deserialize<Dictionary<string, JsonElement>[]>() ?? [];

                        for (int i = 0; i < noteSet.Length; i++)
                        {
                            Dictionary<string, JsonElement> note = noteSet[i];
                            float x = 1 - note["x"].GetSingle();
                            float y = note["y"].GetSingle() + 1;
                            long t = note["t"].GetInt64();

                            bool m = note.TryGetValue("m", out JsonElement tempM) && tempM.GetBoolean();

                            if (m)
                                Mapping.Current.SpecialObjects.Add(new Mine(x, y, t));
                            else
                                Mapping.Current.Notes.Add(new(x, y, t));
                        }

                        break;

                    case "beats":
                        Dictionary<string, JsonElement>[] beatSet = value.Deserialize<Dictionary<string, JsonElement>[]>() ?? [];

                        for (int i = 0; i < beatSet.Length; i++)
                        {
                            Dictionary<string, JsonElement> beat = beatSet[i];
                            long t = beat["t"].GetInt64();

                            Mapping.Current.SpecialObjects.Add(new Beat(t));
                        }

                        break;

                    case "glides":
                        Dictionary<string, JsonElement>[] glideSet = value.Deserialize<Dictionary<string, JsonElement>[]>() ?? [];

                        for (int i = 0; i < glideSet.Length; i++)
                        {
                            Dictionary<string, JsonElement> glide = glideSet[i];
                            long t = glide["t"].GetInt64();
                            string d = glide["d"].GetString() ?? "none";

                            Mapping.Current.SpecialObjects.Add(new Glide(t, d switch
                            {
                                "up" => GlideDirection.Up,
                                "right" => GlideDirection.Right,
                                "down" => GlideDirection.Down,
                                "left" => GlideDirection.Left,
                                _ => GlideDirection.None
                            }));
                        }

                        break;
                }
            }

            return true;
        }

        private static bool Read_Version2(string path)
        {
            string directory = Path.GetDirectoryName(path) ?? "";

            string id = "";
            string profile = "";
            string icon = "";

            foreach (string file in Directory.GetFiles(directory))
            {
                string filename = Path.GetFileNameWithoutExtension(file);

                switch (Path.GetExtension(file))
                {
                    case ".json" when filename == "metadata":
                        Dictionary<string, JsonElement> metadata = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(file)) ?? [];
                        JsonElement temp;

                        if (metadata.TryGetValue("songTitle", out temp))
                            Settings.songTitle.Value = temp.GetString() ?? "";
                        if (metadata.TryGetValue("songArtist", out temp))
                            Settings.songArtist.Value = temp.GetString() ?? "";
                        if (metadata.TryGetValue("mapCreator", out temp))
                            Settings.mapCreator.Value = temp.GetString() ?? "";
                        if (metadata.TryGetValue("mapCreatorPersonalLink", out temp))
                            Settings.mapCreatorPersonalLink.Value = temp.GetString() ?? "";
                        if (metadata.TryGetValue("previewStartTime", out temp))
                            Settings.previewStartTime.Value = ((long)(temp.GetDouble() * 1000d)).ToString();
                        if (metadata.TryGetValue("previewDuration", out temp))
                            Settings.previewDuration.Value = ((long)(temp.GetDouble() * 1000d)).ToString();

                        break;

                    case ".nlr":
                        string[] lines = File.ReadAllLines(file);

                        for (int i = 0; i < lines.Length; i++)
                        {
                            string[] split = lines[i].Split('|');
                            if (split.Length < 3)
                                continue;

                            string timestamp = split[0];
                            string properties = split[1];
                            string text = string.Join('|', split[2..]);

                            string[] components = timestamp.Split('.');
                            long ms = long.Parse(components[0]);

                            if (properties.Contains('r') && !string.IsNullOrWhiteSpace(text))
                                text = $"-{text}";
                            if (properties.Contains('n'))
                                text = $"{text}-";

                            bool fadeIn = properties.Contains('i');
                            bool fadeOut = properties.Contains('o');

                            Mapping.Current.SpecialObjects.Add(new Lyric(ms, text, fadeIn, fadeOut));
                        }

                        break;

                    case ".png" when filename == "profile":
                    case ".jpg" when filename == "profile":
                        profile = file;
                        break;

                    case ".png" when filename != "profile":
                    case ".jpg" when filename != "profile":
                        icon = file;
                        break;

                    case ".mp3":
                    case ".ogg":
                    case ".wav":
                        id = filename;
                        File.Copy(file, Path.Combine(Assets.CACHED, $"{filename}.asset"), true);
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(profile))
            {
                string temp = Path.Combine(Assets.CACHED, $"{id}-profile{Path.GetExtension(profile)}");
                File.Copy(profile, temp, true);
                Settings.novaIcon.Value = temp;
            }

            if (!string.IsNullOrWhiteSpace(icon))
            {
                string temp = Path.Combine(Assets.CACHED, $"{id}-icon{Path.GetExtension(icon)}");
                File.Copy(icon, temp, true);
                Settings.novaCover.Value = temp;
                Settings.cover.Value = temp;
                Settings.useCover.Value = true;
            }

            Mapping.Current.SoundID = id;
            Settings.songName.Value = $"{Settings.songArtist.Value} - {Settings.songTitle.Value}";
            Settings.mappers.Value = Settings.mapCreator.Value;

            Dictionary<string, JsonElement> result = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(path)) ?? [];

            long lastMs = 0;
            Fever? lastFever = null;

            if (result.TryGetValue("songOffset", out JsonElement songOffset))
                Settings.songOffset.Value = songOffset.GetDouble().ToString(Program.Culture);

            if (result.TryGetValue("notes", out JsonElement notes))
            {
                Dictionary<string, JsonElement>[] noteSet = notes.Deserialize<Dictionary<string, JsonElement>[]>() ?? [];

                for (int i = 0; i < noteSet.Length; i++)
                {
                    Dictionary<string, JsonElement> note = noteSet[i];
                    float x = 1 - note["x"].GetSingle();
                    float y = note["y"].GetSingle() + 1;
                    long t = note["t"].GetInt64();

                    bool m = note.TryGetValue("m", out JsonElement tempM) && tempM.GetBoolean();
                    bool e = note.TryGetValue("e", out JsonElement tempE);

                    EasingStyle s = EasingStyle.Linear;
                    EasingDirection d = EasingDirection.In;

                    if (e)
                    {
                        Dictionary<string, JsonElement> easing = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(tempE) ?? [];

                        s = (EasingStyle)easing["s"].GetInt32();
                        d = (EasingDirection)easing["d"].GetInt32();
                    }

                    if (m)
                        Mapping.Current.SpecialObjects.Add(new Mine(x, y, t));
                    else
                        Mapping.Current.Notes.Add(new(x, y, t, e, s, d));

                    lastMs = Math.Max(lastMs, t);
                }
            }

            if (result.TryGetValue("beats", out JsonElement beats))
            {
                Dictionary<string, JsonElement>[] beatSet = beats.Deserialize<Dictionary<string, JsonElement>[]>() ?? [];

                for (int i = 0; i < beatSet.Length; i++)
                {
                    Dictionary<string, JsonElement> beat = beatSet[i];
                    long t = beat["t"].GetInt64();

                    Mapping.Current.SpecialObjects.Add(new Beat(t));
                    lastMs = Math.Max(lastMs, t);
                }
            }

            if (result.TryGetValue("glides", out JsonElement glides))
            {
                Dictionary<string, JsonElement>[] glideSet = glides.Deserialize<Dictionary<string, JsonElement>[]>() ?? [];

                for (int i = 0; i < glideSet.Length; i++)
                {
                    Dictionary<string, JsonElement> glide = glideSet[i];
                    long t = glide["t"].GetInt64();
                    string d = glide["d"].GetString() ?? "none";

                    Mapping.Current.SpecialObjects.Add(new Glide(t, d switch
                    {
                        "up" => GlideDirection.Up,
                        "right" => GlideDirection.Right,
                        "down" => GlideDirection.Down,
                        "left" => GlideDirection.Left,
                        _ => GlideDirection.None
                    }));

                    lastMs = Math.Max(lastMs, t);
                }
            }

            if (result.TryGetValue("events", out JsonElement events))
            {
                Dictionary<string, JsonElement>[] eventSet = events.Deserialize<Dictionary<string, JsonElement>[]>() ?? [];

                for (int i = 0; i < eventSet.Length; i++)
                {
                    Dictionary<string, JsonElement> tEvent = eventSet[i];
                    string eventType = tEvent["type"].GetString() ?? "";
                    long t = tEvent["t"].GetInt64();

                    switch (eventType)
                    {
                        case "fever":
                            bool active = tEvent["active"].GetBoolean();

                            if (active)
                                lastFever = new(t, 0);
                            else if (lastFever != null)
                            {
                                lastFever.Duration = t - lastFever.Ms;
                                Mapping.Current.SpecialObjects.Add(lastFever);

                                lastFever = null;
                            }

                            break;

                        case "tempo":
                            float bpm = (float)tEvent["bpm"].GetDouble();
                            Mapping.Current.TimingPoints.Add(new(bpm, t));
                            break;
                    }
                }
            }

            if (lastFever != null)
            {
                lastFever.Duration = lastMs - lastFever.Ms;
                Mapping.Current.SpecialObjects.Add(lastFever);
            }

            return true;
        }
    }
}
