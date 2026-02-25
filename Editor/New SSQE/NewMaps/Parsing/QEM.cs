using New_SSQE.Misc;
using New_SSQE.Misc.Static;
using New_SSQE.Objects;
using New_SSQE.Objects.Other;
using New_SSQE.Preferences;
using System.Drawing;
using System.Text;

namespace New_SSQE.NewMaps.Parsing
{
    internal class QEM : IFormatParser
    {
        private const int TOTAL_OBJ_BLOCKS = 18;

        public static bool Read(string path)
        {
            using FileStream data = new(path, FileMode.Open, FileAccess.Read);
            data.Seek(0, SeekOrigin.Begin);
            using BinaryReader reader = new(data);

            // [4]: File signature ([I]nternal [Q]uantum [E]ditor [M]ap)
            byte[] signature = reader.ReadBytes(4);
            if (Encoding.ASCII.GetString(signature) != "IQEM")
                throw new InvalidDataException("Incorrect file signature for QEM");
            int version = reader.ReadInt16();
            if (version != 0)
                throw new InvalidDataException("Unrecognized QEM version");

            // Int16 length byte array
            byte[] ReadBytes()
            {
                int length = reader.ReadInt16();
                return reader.ReadBytes(length);
            }

            // Int32 length byte array
            byte[] ReadBytesLong()
            {
                int length = reader.ReadInt32();
                return reader.ReadBytes(length);
            }

            // Int16 length string
            string ReadString()
            {
                byte[] bytes = ReadBytes();
                return Encoding.ASCII.GetString(bytes);
            }

            // Int32 length string
            string ReadStringLong()
            {
                byte[] bytes = ReadBytesLong();
                return Encoding.ASCII.GetString(bytes);
            }

            // Int16 length type array
            object[] ReadTypeArray()
            {
                int length = reader.ReadInt16();
                byte type = reader.ReadByte();

                object[] typeData = new object[length];

                for (int i = 0; i < length; i++)
                    typeData[i] = ReadType(type);

                return typeData;
            }

            // Int32 length type array
            object[] ReadTypeArrayLong()
            {
                int length = reader.ReadInt32();
                byte type = reader.ReadByte();

                object[] typeData = new object[length];

                for (int i = 0; i < length; i++)
                    typeData[i] = ReadType(type);

                return typeData;
            }

            // Int16 length type dict
            Dictionary<object, object> ReadTypeDict()
            {
                int length = reader.ReadInt16();
                byte keyType = reader.ReadByte();
                byte valueType = reader.ReadByte();

                Dictionary<object, object> typeData = [];

                for (int i = 0; i < length; i++)
                {
                    object key = ReadType(keyType);
                    object value = ReadType(valueType);

                    if (!typeData.TryAdd(key, value))
                        typeData[key] = value;
                }

                return typeData;
            }

            // Int32 length type dict
            Dictionary<object, object> ReadTypeDictLong()
            {
                int length = reader.ReadInt32();
                byte keyType = reader.ReadByte();
                byte valueType = reader.ReadByte();

                Dictionary<object, object> typeData = [];

                for (int i = 0; i < length; i++)
                {
                    object key = ReadType(keyType);
                    object value = ReadType(valueType);

                    if (!typeData.TryAdd(key, value))
                        typeData[key] = value;
                }

                return typeData;
            }

            // Int8 length tuple
            object[] ReadTuple()
            {
                int length = reader.ReadByte();
                object[] data = new object[length];

                for (int i = 0; i < length; i++)
                {
                    byte type = reader.ReadByte();
                    data[i] = ReadType(type);
                }

                return data;
            }

            // Generic field type
            object ReadType(byte type)
            {
                return type switch
                {
                    0 => reader.ReadByte(),
                    1 => reader.ReadInt16(),
                    2 => reader.ReadUInt16(),
                    3 => reader.ReadInt32(),
                    4 => reader.ReadUInt32(),
                    5 => reader.ReadInt64(),
                    6 => reader.ReadUInt64(),
                    7 => reader.ReadSingle(),
                    8 => reader.ReadDouble(),
                    9 => reader.ReadBoolean(),
                    10 => ReadString(),
                    11 => ReadStringLong(),
                    12 => ReadBytes(),
                    13 => ReadBytesLong(),
                    14 => ReadTypeArray(),
                    15 => ReadTypeArrayLong(),
                    16 => ReadTypeDict(),
                    17 => ReadTypeDictLong(),
                    18 => ReadTuple(),
                    _ => throw new InvalidDataException($"Invalid field type {type}")
                };
            }

            object[] GetDefaultData(int id)
            {
                return id switch
                {
                    0 => [0, 1, 1, false, 0, 0],
                    1 => [0, 0, 4, 4],
                    2 => [0, 0, 0, 0, 1],
                    3 => [0, 0, 0, 0, 1],
                    4 => [0, 0, 0, 0, 1],
                    5 => [0, 0, 0, 0, 1],
                    6 => [0, 0, 0, 0, 1],
                    7 => [0, 0, 0, 0, 0, 0, 0, 0],
                    8 => [0, 0, 0, 0, 0, 0, 0],
                    9 => [0, 0, 0, 0, 0],
                    10 => [0, 0, 0, 0, 1],
                    11 => [0, 0, "", 0],
                    12 => [0],
                    13 => [0, 0],
                    14 => [0, 0, 0],
                    15 => [0, "", false, false],
                    16 => [0, 0],
                    17 => [0, 0, ""],
                    _ => throw new InvalidDataException($"Invalid object ID {id}")
                };
            }

            MapObject ParseObj(int id, params object[] data)
            {
                return id switch
                {
                    0 => new Note((float)data[1], (float)data[2], (long)data[0], (bool)data[3], (EasingStyle)data[4], (EasingDirection)data[5]),
                    1 => new TimingPoint((float)data[1], (long)data[0], ((float)data[2], (float)data[3])),
                    2 => new Brightness((long)data[0], (long)data[1], (EasingStyle)data[2], (EasingDirection)data[3], (float)data[4]),
                    3 => new Contrast((long)data[0], (long)data[1], (EasingStyle)data[2], (EasingDirection)data[3], (float)data[4]),
                    4 => new Saturation((long)data[0], (long)data[1], (EasingStyle)data[2], (EasingDirection)data[3], (float)data[4]),
                    5 => new Blur((long)data[0], (long)data[1], (EasingStyle)data[2], (EasingDirection)data[3], (float)data[4]),
                    6 => new FOV((long)data[0], (long)data[1], (EasingStyle)data[2], (EasingDirection)data[3], (float)data[4]),
                    7 => new Tint((long)data[0], (long)data[1], (EasingStyle)data[2], (EasingDirection)data[3], Color.FromArgb((byte)data[4], (byte)data[5], (byte)data[6], (byte)data[7])),
                    8 => new Position((long)data[0], (long)data[1], (EasingStyle)data[2], (EasingDirection)data[3], ((float)data[4], (float)data[5], (float)data[6])),
                    9 => new Rotation((long)data[0], (long)data[1], (EasingStyle)data[2], (EasingDirection)data[3], (float)data[4]),
                    10 => new ARFactor((long)data[0], (long)data[1], (EasingStyle)data[2], (EasingDirection)data[3], (float)data[4]),
                    11 => new Text((long)data[0], (long)data[1], (string)data[2], (int)data[3]),
                    12 => new Beat((long)data[0]),
                    13 => new Glide((long)data[0], (GlideDirection)data[1]),
                    14 => new Mine((float)data[0], (float)data[1], (long)data[2]),
                    15 => new Lyric((long)data[0], (string)data[1], (bool)data[2], (bool)data[3]),
                    16 => new Fever((long)data[0], (long)data[1]),
                    17 => new Bookmark((string)data[2], (long)data[0], (long)data[1] + (long)data[0]),
                    _ => throw new InvalidDataException($"Invalid object ID {id}")
                };
            }

            // Path field block
            Dictionary<string, string> paths = [];

            // [2]: Number of paths
            int numPaths = reader.ReadInt16();

            for (int i = 0; i < numPaths; i++)
            {
                // [2]: Field name
                string fieldName = ReadString();
                // [2]: Field value
                string value = ReadString();

                paths.Add(fieldName, value);
            }

            string audioSource = "";

            foreach (KeyValuePair<string, string> entry in paths)
            {
                if (!File.Exists(entry.Value))
                    continue;

                string source = Path.IsPathFullyQualified(entry.Value) ? entry.Value : Path.GetFullPath(entry.Value, Path.GetDirectoryName(path) ?? "");
                string dest = Assets.CachedAt(Path.GetFileName(source));
                if (dest == source)
                    continue;

                switch (entry.Key)
                {
                    case "audio":
                        audioSource = source;
                        continue;
                    case "cover":
                        Settings.useCover.Value = true;
                        Settings.cover.Value = dest;
                        break;
                    case "video":
                        Settings.useVideo.Value = true;
                        Settings.video.Value = dest;
                        break;
                    default:
                        continue;
                }

                File.Copy(source, dest, true);
            }

            // Metadata field block
            Dictionary<string, object> metadata = [];

            // [2]: Number of fields
            int numFields = reader.ReadInt16();

            for (int i = 0; i < numFields; i++)
            {
                // [2]: Field name
                string fieldName = ReadString();
                // [1]: Field type
                byte type = reader.ReadByte();

                // Field value
                object value = ReadType(type);

                if (!metadata.TryAdd(fieldName, value))
                    metadata[fieldName] = value;
            }

            foreach (KeyValuePair<string, object> entry in metadata)
            {
                switch (entry.Key)
                {
                    case "title" when entry.Value is string title:
                        Settings.songTitle.Value = title;
                        break;
                    case "romanizedTitle" when entry.Value is string title:
                        Settings.romanizedTitle.Value = title;
                        break;
                    case "artists" when entry.Value is object[] artists:
                        Settings.songArtist.Value = string.Join('\n', artists);
                        break;
                    case "romanizedArtists" when entry.Value is object[] artists:
                        Settings.romanizedArtist.Value = string.Join('\n', artists);
                        break;
                    case "mappers" when entry.Value is object[] mappers:
                        Settings.mappers.Value = string.Join('\n', mappers);
                        break;
                    case "difficulty" when entry.Value is byte diff:
                        string difficulty = "N/A";
                        
                        foreach (KeyValuePair<string, byte> diffEntry in FormatUtils.Difficulties)
                        {
                            if (diffEntry.Value == diff)
                                difficulty = diffEntry.Key;
                        }

                        Settings.difficulty.Value = difficulty;
                        break;
                    case "difficultyStars" when entry.Value is float diff:
                        Settings.rating.Value = diff;
                        break;
                    case "difficultyName" when entry.Value is string diff:
                        Settings.customDifficulty.Value = diff;
                        break;
                    case "songLinks" when entry.Value is object[] links:
                        foreach (object link in links)
                        {
                            if (link is string str)
                                Mapping.Current.SongLinks.Add(str);
                        }

                        break;
                    case "artistLinks" when entry.Value is Dictionary<object, object> links:
                        foreach (KeyValuePair<object, object> link in links)
                        {
                            if (link.Key is not string key)
                                continue;
                            if (link.Value is not string value)
                                continue;

                            if (!Mapping.Current.ArtistLinks.TryAdd(key, value))
                                Mapping.Current.ArtistLinks[key] = value;
                        }

                        break;
                    case "mapperIds" when entry.Value is Dictionary<object, object> ids:
                        foreach (KeyValuePair<object, object> id in ids)
                        {
                            if (id.Key is not string key)
                                continue;
                            if (id.Value is not long value)
                                continue;

                            if (!Mapping.Current.MapperIds.TryAdd(key, value))
                                Mapping.Current.MapperIds[key] = value;
                        }

                        break;
                    case "mapId" when entry.Value is string id:
                        Mapping.Current.SoundID = id;
                        if (!string.IsNullOrWhiteSpace(audioSource))
                            File.Copy(audioSource, Assets.CachedAt($"{id}.asset"), true);
                        break;
                }
            }

            // Object block
            ObjectList<Note> notes = Mapping.Current.Notes;
            ObjectList<MapObject> vfxObjects = Mapping.Current.VfxObjects;
            ObjectList<MapObject> specialObjects = Mapping.Current.SpecialObjects;
            List<TimingPoint> timingPoints = Mapping.Current.TimingPoints;
            List<Bookmark> bookmarks = Mapping.Current.Bookmarks;

            // [1]: Number of object blocks
            int numIDs = reader.ReadByte();

            for (int i = 0; i < numIDs; i++)
            {
                // [1]: Object ID
                int id = reader.ReadByte();

                // [2]: Object name
                string objName = ReadString();

                // [1]: Number of types
                int numTypes = reader.ReadByte();
                
                byte[] types = new byte[numTypes];

                // For number of types:
                for (int j = 0; j < numTypes; j++)
                    // [1]: Type
                    types[j] = reader.ReadByte();

                // [4]: Number of objects
                long numObjects = reader.ReadInt32();

                // For number of objects:
                for (int j = 0; j < numObjects; j++)
                {
                    // [8]: Timestamp (double)
                    double ms = reader.ReadDouble();

                    // [x]: Type data (variable)
                    List<object> objectData = [.. GetDefaultData(id)];
                    objectData[0] = ms;

                    while (objectData.Count < numTypes + 1)
                        objectData.Add(0);

                    for (int k = 0; k < numTypes; k++)
                        objectData[k + 1] = ReadType(types[k]);

                    // No known overrides for object name
                    if (!string.IsNullOrWhiteSpace(objName))
                        continue;

                    MapObject obj = ParseObj(id, objectData);

                    if (id == 0)
                        notes.Add((Note)obj);
                    else if (id == 1)
                        timingPoints.Add((TimingPoint)obj);
                    else if (id == 17)
                        bookmarks.Add((Bookmark)obj);
                    else if (FormatUtils.VfxLookup[id])
                        vfxObjects.Add(obj);
                    else
                        specialObjects.Add(obj);
                }
            }

            return true;
        }

        public static bool Write(string path)
        {
            using FileStream file = new(path, FileMode.Create, FileAccess.Write);
            using BinaryWriter writer = new(file);

            void WriteString(string str)
            {
                writer.Write((short)str.Length);
                writer.Write(Encoding.UTF8.GetBytes(str));
            }

            writer.Write(
            [
                0x49, 0x51, 0x45, 0x4d, // file type signature
                0x00, 0x00, // QEM format version - 0
            ]);

            // path block
            bool hasCover = Settings.useCover.Value && File.Exists(Settings.cover.Value);
            bool hasVideo = Settings.useVideo.Value && File.Exists(Settings.video.Value);

            int numPaths = 1 + (hasCover ? 1 : 0) + (hasVideo ? 1 : 0);
            writer.Write((short)numPaths); // number of path block entries

            // audio path
            WriteString("audio");
            WriteString(Assets.CachedAt($"{Mapping.Current.SoundID}.asset"));

            // cover path
            if (hasCover)
            {
                WriteString("cover");
                WriteString(Settings.cover.Value);
            }

            // video path
            if (hasVideo)
            {
                WriteString("video");
                WriteString(Settings.video.Value);
            }

            // metadata block
            writer.Write((short)12); // 12 metadata fields

            // 1 - title
            WriteString("title");
            writer.Write((byte)0x0a); // data type 0x0a - string
            WriteString(Settings.songTitle.Value);

            // 2 - romanized title
            WriteString("romanizedTitle");
            writer.Write((byte)0x0a); // data type 0x0a - string
            WriteString(Settings.romanizedTitle.Value);

            // 3 - artists
            string[] artists = Settings.songArtist.Value.Split('\n');

            WriteString("artists");
            writer.Write((byte)0x0e); // data type 0x0e - array
            writer.Write((short)artists.Length);
            writer.Write((byte)0x0a); // data type 0x0e[0x0a] - string array

            foreach (string artist in artists)
                WriteString(artist);

            // 4 - romanized artists
            string[] romanizedArtists = Settings.romanizedArtist.Value.Split('\n');

            WriteString("romanizedArtists");
            writer.Write((byte)0x0e); // data type 0x0e - array
            writer.Write((short)romanizedArtists.Length);
            writer.Write((byte)0x0a); // data type 0x0e[0x0a] - string array

            foreach (string artist in romanizedArtists)
                WriteString(artist);

            // 5 - mappers
            string[] mappers = Settings.mappers.Value.Split('\n');

            WriteString("mappers");
            writer.Write((byte)0x0e); // data type 0x0e - array
            writer.Write((short)mappers.Length);
            writer.Write((byte)0x0a); // data type 0x0e[0x0a] - string array

            foreach (string mapper in mappers)
                WriteString(mapper);

            // 6 - difficulty
            WriteString("difficulty");
            writer.Write((byte)0x00); // data type 0x00 - byte
            writer.Write(FormatUtils.Difficulties[Settings.difficulty.Value]);

            // 7 - difficulty stars
            WriteString("difficultyStars");
            writer.Write((byte)0x07); // data type 0x00 - float
            writer.Write(Settings.rating.Value);

            // 8 - difficulty name
            if (!string.IsNullOrWhiteSpace(Settings.customDifficulty.Value))
            {
                WriteString("difficultyName");
                writer.Write((byte)0x0a); // data type 0x0a - string
                WriteString(Settings.customDifficulty.Value);
            }

            // 9 - song links
            WriteString("songLinks");
            writer.Write((byte)0x0e); // data type 0x0e - array
            writer.Write((short)Mapping.Current.SongLinks.Count);
            writer.Write((byte)0x0a); // data type 0x0e[0x0a] - string array

            foreach (string link in Mapping.Current.SongLinks)
                WriteString(link);

            // 10 - artist links
            WriteString("artistLinks");
            writer.Write((byte)0x10); // data type 0x10 - dictionary
            writer.Write((short)Mapping.Current.ArtistLinks.Count);
            writer.Write((byte)0x0a); // data type 0x10[0x0a] - (string) dictionary
            writer.Write((byte)0x0a); // data type 0x10[0x0a,0x0a] - (string, string) dictionary

            foreach (KeyValuePair<string, string> link in Mapping.Current.ArtistLinks)
            {
                if (!artists.Contains(link.Key))
                    continue;

                WriteString(link.Key);
                WriteString(link.Value);
            }

            // 11 - mapper ids
            WriteString("mapperIds");
            writer.Write((byte)0x10); // data type 0x10 - dictionary
            writer.Write((short)Mapping.Current.MapperIds.Count);
            writer.Write((byte)0x0a); // data type 0x10[0x0a] - (string) dictionary
            writer.Write((byte)0x05); // data type 0x10[0x0a,0x05] - (string, long) dictionary

            foreach (KeyValuePair<string, long> id in Mapping.Current.MapperIds)
            {
                if (!mappers.Contains(id.Key))
                    continue;

                WriteString(id.Key);
                writer.Write(id.Value);
            }

            // 12 - map id
            string firstMapper = mappers.Length > 0 ? mappers[0] : "none";
            string firstArtist = artists.Length > 0 ? artists[0] : "unknown";
            string mapId = $"{firstMapper} - {firstArtist} - {Settings.songTitle.Value}";

            WriteString("mapId");
            WriteString(FormatUtils.FixID(mapId));

            // object blocks
            List<List<MapObject>> separated = [];

            separated.Add([.. Mapping.Current.Notes]);
            separated.Add([.. Mapping.Current.TimingPoints]);

            foreach (MapObject obj in Mapping.Current.VfxObjects)
            {
                if (obj.ID < 0)
                    continue;
                while (separated.Count <= obj.ID)
                    separated.Add([]);
                separated[obj.ID].Add(obj);
            }

            foreach (MapObject obj in Mapping.Current.SpecialObjects)
            {
                if (obj.ID < 0)
                    continue;
                while (separated.Count <= obj.ID)
                    separated.Add([]);
                separated[obj.ID].Add(obj);
            }

            foreach (MapObject obj in Mapping.Current.Bookmarks)
            {
                if (obj.ID < 0)
                    continue;
                while (separated.Count <= obj.ID)
                    separated.Add([]);
                separated[obj.ID].Add(obj);
            }

            while (separated.Count < TOTAL_OBJ_BLOCKS)
                separated.Add([]);
            writer.Write((byte)TOTAL_OBJ_BLOCKS); // 18 object blocks

            // note block
            writer.Write((byte)0); // ID 0
            WriteString("");
            writer.Write((byte)5); // 5 types
            writer.Write((byte)0x07); // data type 0x07 - float (x)
            writer.Write((byte)0x07); // data type 0x07 - float (y)
            writer.Write((byte)0x09); // data type 0x09 - boolean (tweened AR)
            writer.Write((byte)0x00); // data type 0x00 - byte (easing style)
            writer.Write((byte)0x00); // data type 0x00 - byte (easing direction)
            writer.Write(separated[0].Count);

            foreach (MapObject obj in separated[0])
            {
                Note note = (obj as Note)!;

                writer.Write((double)note.Ms);
                writer.Write(note.X);
                writer.Write(note.Y);
                writer.Write(note.EnableEasing);
                writer.Write((byte)note.Style);
                writer.Write((byte)note.Direction);
            }

            // timing point block
            writer.Write((byte)1); // ID 1
            WriteString("");
            writer.Write((byte)3); // 3 types
            writer.Write((byte)0x07); // data type 0x07 - float (bpm)
            writer.Write((byte)0x00); // data type 0x00 - byte (time signature top)
            writer.Write((byte)0x00); // data type 0x00 - byte (time signature bottom)
            writer.Write(separated[1].Count);

            foreach (MapObject obj in separated[1])
            {
                TimingPoint timingPoint = (obj as TimingPoint)!;

                writer.Write((double)timingPoint.Ms);
                writer.Write(timingPoint.BPM);
                writer.Write((byte)timingPoint.TimeSignature.X);
                writer.Write((byte)timingPoint.TimeSignature.Y);
            }

            // brightness block
            /* Unreleased */

            // contrast block
            /* Unreleased */

            // saturation block
            /* Unreleased */

            // blur block
            /* Unreleased */

            // fov block
            /* Unreleased */

            // tint block
            /* Unreleased */

            // position block
            /* Unreleased */

            // rotation block
            /* Unreleased */

            // ar factor block
            /* Unreleased */

            // text block
            /* Unreleased */

            // beat block
            writer.Write((byte)12); // ID 12
            WriteString("");
            writer.Write((byte)0); // 0 types
            writer.Write(separated[12].Count);

            foreach (MapObject obj in separated[12])
            {
                Beat beat = (obj as Beat)!;

                writer.Write((double)beat.Ms);
            }

            // glide block
            writer.Write((byte)13); // ID 13
            WriteString("");
            writer.Write((byte)1); // 1 type
            writer.Write((byte)0x00); // data type 0x00 - byte (glide direction)
            writer.Write(separated[13].Count);

            foreach (MapObject obj in separated[13])
            {
                Glide glide = (obj as Glide)!;

                writer.Write((double)glide.Ms);
                writer.Write((byte)glide.Direction);
            }

            // mine block
            writer.Write((byte)14); // ID 14
            WriteString("");
            writer.Write((byte)2); // 2 types
            writer.Write((byte)0x07); // data type 0x07 - float (x)
            writer.Write((byte)0x07); // data type 0x07 - float (y)
            writer.Write(separated[14].Count);

            foreach (MapObject obj in separated[14])
            {
                Mine mine = (obj as Mine)!;

                writer.Write((double)mine.Ms);
                writer.Write(mine.X);
                writer.Write(mine.Y);
            }

            // lyric block
            writer.Write((byte)15); // ID 15
            WriteString("");
            writer.Write((byte)3); // 3 types
            writer.Write((byte)0x0a); // data type 0x0a - string (text)
            writer.Write((byte)0x09); // data type 0x09 - boolean (fades in)
            writer.Write((byte)0x09); // data type 0x09 - boolean (fades out)
            writer.Write(separated[15].Count);

            foreach (MapObject obj in separated[15])
            {
                Lyric lyric = (obj as Lyric)!;

                writer.Write((double)lyric.Ms);
                WriteString(lyric.Text);
                writer.Write(lyric.FadeIn);
                writer.Write(lyric.FadeOut);
            }

            // fever block
            writer.Write((byte)16); // ID 16
            WriteString("");
            writer.Write((byte)1); // 1 type
            writer.Write((byte)0x08); // data type 0x08 - double (duration)
            writer.Write(separated[16].Count);

            foreach (MapObject obj in separated[16])
            {
                Fever fever = (obj as Fever)!;

                writer.Write((double)fever.Ms);
                writer.Write((double)fever.Duration);
            }

            // bookmark block
            writer.Write((byte)17); // ID 17
            WriteString("");
            writer.Write((byte)2); // 2 types
            writer.Write((byte)0x08); // data type 0x08 - double (duration)
            writer.Write((byte)0x0a); // data type 0x0a - string (text)
            writer.Write(separated[17].Count);

            foreach (MapObject obj in separated[17])
            {
                Bookmark bookmark = (obj as Bookmark)!;

                writer.Write((double)bookmark.Ms);
                writer.Write((double)(bookmark.EndMs - bookmark.Ms));
                WriteString(bookmark.Text);
            }

            return true;
        }
    }
}
