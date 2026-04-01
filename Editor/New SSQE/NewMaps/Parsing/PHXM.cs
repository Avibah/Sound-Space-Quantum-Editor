using New_SSQE.Audio;
using New_SSQE.Misc;
using New_SSQE.NewGUI.Dialogs;
using New_SSQE.Objects;
using New_SSQE.Preferences;
using New_SSQE.Services;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace New_SSQE.NewMaps.Parsing
{
    internal class PHXM : IFormatParser
    {
        public static bool Read(string path)
        {
            bool hasAudio = false;
            string audioId = "";
            string audioExt = "";

            Dictionary<string, JsonElement> metadata = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(Path.Combine(path, "metadata.json"))) ?? [];
            string artistLink = "";

            foreach (string key in metadata.Keys)
            {
                JsonElement value = metadata[key];

                switch (key)
                {
                    case "Artist":
                        Settings.songArtist.Value = value.GetString() ?? "";
                        Settings.romanizedArtist.Value = FormatUtils.FixASCII(Settings.songArtist.Value);
                        break;
                    case "AudioExt":
                        audioExt = value.GetString() ?? "";
                        break;
                    case "Difficulty":
                        Settings.difficulty.Value = FormatUtils.Difficulties.Keys.ToArray()[value.GetInt32()];
                        break;
                    case "DifficultyName":
                        Settings.customDifficulty.Value = value.GetString() ?? "";
                        break;
                    case "HasAudio":
                        hasAudio = value.GetBoolean();
                        break;
                    case "HasCover":
                        Settings.useCover.Value = value.GetBoolean() && File.Exists(Path.Combine(path, "cover.png"));
                        break;
                    case "HasVideo":
                        Settings.useVideo.Value = value.GetBoolean() && File.Exists(Path.Combine(path, "video.mp4"));
                        break;
                    case "ID":
                        audioId = value.GetString() ?? "";
                        break;
                    case "Mappers":
                        string[] mappers = value.Deserialize<string[]>() ?? [];
                        Settings.mappers.Value = string.Join('\n', mappers);
                        break;
                    case "Rating":
                        Settings.rating.Value = value.GetSingle();
                        break;
                    case "Title":
                        Settings.songTitle.Value = value.GetString() ?? "";
                        Settings.romanizedTitle.Value = FormatUtils.FixASCII(Settings.songTitle.Value);
                        break;
                    case "ArtistLink":
                        artistLink = value.GetString() ?? "";
                        break;
                    case "ArtistPlatform":
                        Settings.phxmPlatform.Value = value.GetString() ?? "";
                        break;
                }

                Settings.songName.Value = $"{Settings.songArtist.Value} - {Settings.songTitle.Value}";
            }

            Mapping.Current.SoundID = audioId;
            Mapping.Current.ArtistLinks = new()
            {
                {Settings.songArtist.Value, artistLink }
            };

            if (hasAudio)
                File.Copy(Path.Combine(path, $"audio.{audioExt}"), Assets.CachedAt($"{audioId}.asset"), true);

            if (Settings.useCover.Value)
            {
                string cover = Assets.CachedAt($"{audioId}-cover.png");
                File.Copy(Path.Combine(path, "cover.png"), cover, true);

                Settings.cover.Value = cover;
                Settings.novaCover.Value = cover;
            }

            if (Settings.useVideo.Value)
            {
                string video = Assets.CachedAt($"{audioId}-video.mp4");
                File.Copy(Path.Combine(path, "video.mp4"), video, true);

                Settings.video.Value = video;
            }

            using FileStream data = new(Path.Combine(path, "objects.phxmo"), FileMode.Open, FileAccess.Read);
            data.Seek(0, SeekOrigin.Begin);
            using BinaryReader reader = new(data);

            int types = reader.ReadInt32();
            int noteCount = reader.ReadInt32();

            for (int i = 0; i < noteCount; i++)
            {
                uint ms = reader.ReadUInt32();
                bool quantum = reader.ReadBoolean();

                float x;
                float y;

                if (quantum)
                {
                    x = 1 - reader.ReadSingle();
                    y = reader.ReadSingle() + 1;
                }
                else
                {
                    x = 2 - reader.ReadByte();
                    y = reader.ReadByte();
                }

                Mapping.Current.Notes.Add(new(x, y, ms));
            }

            string ReadString()
            {
                ushort length = reader.ReadUInt16();
                byte[] str = reader.ReadBytes(length);

                return Encoding.UTF8.GetString(str);
            }

            object[] ParseObject(int id)
            {
                return id switch
                {
                    2 => // brightness
                    [
                        reader.ReadUInt32(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadSingle().ToString(Program.Culture)
                    ],
                    3 => // contrast
                    [
                        reader.ReadUInt32(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadSingle().ToString(Program.Culture)
                    ],
                    4 => // saturation
                    [
                        reader.ReadUInt32(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadSingle().ToString(Program.Culture)
                    ],
                    5 => // blur
                    [
                        reader.ReadUInt32(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadSingle().ToString(Program.Culture)
                    ],
                    6 => // fov
                    [
                        reader.ReadUInt32(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadSingle().ToString(Program.Culture)
                    ],
                    7 => // tint
                    [
                        reader.ReadUInt32(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte()
                    ],
                    8 => // position
                    [
                        reader.ReadUInt32(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadSingle().ToString(Program.Culture),
                        reader.ReadSingle().ToString(Program.Culture),
                        reader.ReadSingle().ToString(Program.Culture)
                    ],
                    9 => // rotation
                    [
                        reader.ReadUInt32(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadSingle().ToString(Program.Culture)
                    ],
                    10 => // ar factor
                    [
                        reader.ReadSingle().ToString(Program.Culture)
                    ],
                    11 => // text
                    [
                        reader.ReadUInt32(),
                        ReadString(),
                        reader.ReadByte()
                    ],
                    _ => [],
                };
            }

            for (int i = 2; i < types + 1; i++)
            {
                FormatUtils.ResetEncode();

                int count = reader.ReadInt32();
                string[] set = new string[count];

                for (int j = 0; j < count; j++)
                {
                    string obj = string.Join('|', FormatUtils.EncodeTimestamp(reader.ReadUInt32()), ParseObject(i));
                    MapObject? mapObject = MOParser.Parse(i, obj.Split('|'));

                    if (mapObject != null)
                        Mapping.Current.VfxObjects.Add(mapObject);
                }
            }

            return true;
        }

        public static Dictionary<string, string> Metadata = new()
        {
            {"songTitle", "" },
            {"songArtist", "" },
            {"mappers", "" },
            {"coverPath", "" },
            {"videoPath", "" },
            {"rating", "" },
            {"artistLink", "" },
            {"artistPlatform", "" },
            {"difficultyName", "" },
            {"difficulty", "" }
        };

        public static bool Export()
        {
            string id = FormatUtils.FixID($"{Metadata["mappers"]} - {Metadata["songTitle"]} - {Metadata["songArtist"]}");

            DialogResult result = new SaveFileDialog()
            {
                Title = "Export PHXM",
                Filter = "Rhythia Maps (*.phxm)|*.phxm",
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
                    MessageDialog.Show($"Failed to export PHXM:\n\n{ex.Message}", MBoxIcon.Warning, MBoxButtons.OK);
                }
            }

            return false;
        }

        public static bool Write(string path)
        {
            string temp = Assets.TempAt("phxm");
            Directory.CreateDirectory(temp);
            foreach (string file in Directory.GetFiles(temp))
                File.Delete(file);

            string extension = MusicPlayer.IsMP3 ? "mp3" : MusicPlayer.IsOGG ? "ogg" : "asset";
            File.Copy(Assets.CachedAt($"{Mapping.Current.SoundID}.asset"), Path.Combine(temp, $"audio.{extension}"), true);

            if (Settings.useCover.Value)
                File.Copy(Metadata["coverPath"], Path.Combine(temp, "cover.png"), true);
            if (Settings.useVideo.Value)
                File.Copy(Metadata["videoPath"], Path.Combine(temp, "video.mp4"), true);

            Dictionary<string, object> metadata = new()
            {
                {"Artist", Settings.songArtist.Value },
                {"AudioExt", extension },
                {"Difficulty", FormatUtils.Difficulties[Settings.difficulty.Value] },
                {"DifficultyName", string.IsNullOrWhiteSpace(Settings.customDifficulty.Value) ? Settings.difficulty.Value : Settings.customDifficulty.Value },
                {"HasAudio", true },
                {"HasCover", Settings.useCover.Value },
                {"HasVideo", Settings.useVideo.Value },
                {"ID", FormatUtils.FixID($"{Settings.mappers.Value} - {Settings.songArtist.Value} - {Settings.songTitle.Value}") },
                {"Mappers", Settings.mappers.Value.Split('\n') },
                {"Rating", Settings.rating.Value },
                {"Title", Settings.songTitle.Value },
                {"ArtistLink", Mapping.Current.ArtistLinks.Values.FirstOrDefault() ?? "" },
                {"ArtistPlatform", Settings.phxmPlatform.Value },
                {"Length", (long)MusicPlayer.TotalTime.TotalMilliseconds }
            };

            File.WriteAllText(Path.Combine(temp, "metadata.json"), JsonSerializer.Serialize(metadata, Program.JsonOptions));

            List<Note> notes = Mapping.Current.Notes;

            using FileStream data = new(Path.Combine(temp, "objects.phxmo"), FileMode.Create, FileAccess.Write);
            data.Seek(0, SeekOrigin.Begin);
            using BinaryWriter writer = new(data);

            writer.Write(12);
            writer.Write(notes.Count);

            for (int i = 0; i < notes.Count; i++)
            {
                Note note = notes[i];
                bool quantum = note.X % 1 != 0 || note.Y % 1 != 0;
                
                writer.Write((uint)note.Ms);
                writer.Write(quantum);

                if (quantum)
                {
                    writer.Write(1 - note.X);
                    writer.Write(note.Y - 1);
                }
                else
                {
                    writer.Write((byte)(2 - (int)note.X));
                    writer.Write((byte)note.Y);
                }
            }

            for (int i = 0; i < 11; i++)
                writer.Write(0);
            writer.Close();

            if (File.Exists(path))
                File.Delete(path);
            ZipFile.CreateFromDirectory(temp, path);
            foreach (string file in Directory.GetFiles(temp))
                File.Delete(file);

            return true;
        }
    }
}
