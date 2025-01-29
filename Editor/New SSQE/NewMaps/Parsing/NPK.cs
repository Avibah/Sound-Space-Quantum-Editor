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
        public static bool Read(string path)
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

            CurrentMap.SoundID = id;
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

                        long prevDiffN = 0;
                        long prevMsN = 0;

                        for (int i = 0; i < noteSet.Length; i++)
                        {
                            Dictionary<string, JsonElement> note = noteSet[i];
                            float x = 1 - note["x"].GetSingle();
                            float y = note["y"].GetSingle() + 1;
                            long t = note["t"].GetInt64();

                            long diff = t - prevMsN;
                            long offset = diff - prevDiffN;

                            prevDiffN = diff;
                            prevMsN = t;

                            CurrentMap.Notes.Add(new(x, y, offset));
                        }

                        break;

                    case "beats":
                        Dictionary<string, JsonElement>[] beatSet = value.Deserialize<Dictionary<string, JsonElement>[]>() ?? [];

                        long prevDiffB = 0;
                        long prevMsB = 0;

                        for (int i = 0; i < beatSet.Length; i++)
                        {
                            Dictionary<string, JsonElement> beat = beatSet[i];
                            long t = beat["t"].GetInt64();

                            long diff = t - prevMsB;
                            long offset = diff - prevDiffB;

                            prevDiffB = diff;
                            prevMsB = t;

                            CurrentMap.SpecialObjects.Add(new Beat(offset));
                        }

                        break;
                }
            }

            return true;
        }

        public static bool Write(string path)
        {
            string id = CurrentMap.SoundID;

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

            File.Copy(Metadata["coverPath"], Path.Combine(temp, $"{id}{Path.GetExtension(Metadata["coverPath"])}"), true);
            File.Copy(Metadata["iconPath"], Path.Combine(temp, $"profile{Path.GetExtension(Metadata["iconPath"])}"), true);
            File.Copy(Path.Combine(Assets.CACHED, $"{id}.asset"), Path.Combine(temp, $"{id}{extension}"), true);

            Dictionary<string, object>[] notes = new Dictionary<string, object>[CurrentMap.Notes.Count];
            for (int i = 0; i < CurrentMap.Notes.Count; i++)
            {
                Note note = CurrentMap.Notes[i];

                notes[i] = new()
                {
                    {"x", 1 - note.X },
                    {"y", note.Y - 1 },
                    {"t", note.Ms }
                };
            }

            List<Dictionary<string, object>> beats = [];
            foreach (MapObject obj in CurrentMap.SpecialObjects)
            {
                if (obj is Beat beat)
                {
                    beats.Add(new()
                    {
                        {"t", beat.Ms }
                    });
                }
            }

            Dictionary<string, object> chart = new()
            {
                {"songOffset", long.Parse(Metadata["songOffset"]) },
                {"notes", notes },
                {"beats", beats }
            };

            File.WriteAllText(Path.Combine(temp, "chart.nch"), JsonSerializer.Serialize(chart, Program.JsonOptions));

            Dictionary<string, object> metadata = new()
            {
                {"songTitle", Metadata["songTitle"] },
                {"songArtist", Metadata["songArtist"] },
                {"mapCreator", Metadata["mapCreator"] },
                {"mapCreatorPersonalLink", Metadata["mapCreatorPersonalLink"] },
                {"previewStartTime", long.Parse(Metadata["previewStartTime"]) / 1000f },
                {"previewDuration", long.Parse(Metadata["previewDuration"]) / 1000f }
            };

            File.WriteAllText(Path.Combine(temp, "metadata.json"), JsonSerializer.Serialize(metadata, Program.JsonOptions));

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

            DialogResult result = new SaveFileDialog()
            {
                Title = "Export NOVA",
                Filter = "Nova Maps (*.npk)|*.npk",
                InitialFileName = $"{mapper}_-_{artist}_-_{title}"
            }.RunWithSetting(Settings.exportPath, out string fileName);

            if (result == DialogResult.OK)
            {
                try
                {
                    Write(fileName);
                    return true;
                }
                catch (Exception ex)
                {
                    Logging.Register("Failed to export", LogSeverity.WARN, ex);
                    MessageBox.Show($"Failed to export NPK:\n\n{ex.Message}", MBoxIcon.Warning, MBoxButtons.OK);
                }
            }

            return false;
        }
    }
}
