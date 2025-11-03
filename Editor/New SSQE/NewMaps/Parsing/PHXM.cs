using New_SSQE.Preferences;
using System.IO.Compression;
using System.Text.Json;
using System.Text;
using New_SSQE.Misc.Static;
using New_SSQE.Objects;

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

            foreach (string key in metadata.Keys)
            {
                JsonElement value = metadata[key];

                switch (key)
                {
                    case "Artist":
                        Settings.songArtist.Value = value.GetString() ?? "";
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
                        Settings.useCover.Value = value.GetBoolean();
                        break;
                    case "HasVideo":
                        Settings.useVideo.Value = value.GetBoolean();
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
                        break;
                }

                Settings.songName.Value = $"{Settings.songArtist.Value} - {Settings.songTitle.Value}";
            }

            Mapping.Current.SoundID = audioId;

            if (hasAudio)
                File.Copy(Path.Combine(path, $"audio.{audioExt}"), Path.Combine(Assets.CACHED, $"{audioId}.asset"), true);

            if (Settings.useCover.Value)
            {
                string cover = Path.Combine(Assets.CACHED, $"{audioId}-cover.png");
                File.Copy(Path.Combine(path, "cover.png"), cover, true);

                Settings.cover.Value = cover;
                Settings.novaCover.Value = cover;
            }

            if (Settings.useVideo.Value)
            {
                string video = Path.Combine(Assets.CACHED, $"{audioId}-video.mp4");
                File.Copy(Path.Combine(path, "video.mp4"), video, true);

                Settings.video.Value = video;
            }

            using FileStream data = new(Path.Combine(path, "objects.phxmo"), FileMode.Open, FileAccess.Read);
            data.Seek(0, SeekOrigin.Begin);
            using BinaryReader reader = new(data);

            int types = reader.ReadInt32();
            int noteCount = reader.ReadInt32();

            FormatUtils.ResetEncode();

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

                Mapping.Current.Notes.Add(new(x, y, FormatUtils.EncodeTimestamp(ms)));
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

        public static bool Write(string path)
        {
            return false;
        }
    }
}
