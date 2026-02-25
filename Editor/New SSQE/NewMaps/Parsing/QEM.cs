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
                    _ => throw new InvalidDataException($"Unknown field type {type}")
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
                        Settings.songArtist.Value = string.Join(" & ", artists);
                        break;
                    case "romanizedArtists" when entry.Value is object[] artists:
                        Settings.romanizedArtist.Value = string.Join(" & ", artists);
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
                    case "songLinks" when entry.Value is Dictionary<object, object> links:
                        foreach (KeyValuePair<object, object> link in links)
                        {
                            if (link.Key is not string key)
                                continue;
                            if (link.Value is not string value)
                                continue;

                            if (!Mapping.Current.SongLinks.TryAdd(key, value))
                                Mapping.Current.SongLinks[key] = value;
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

                try
                {
                    MapObject parsed = ParseObj(id);
                }
                catch (InvalidDataException)
                {
                    continue;
                }
                catch { }

                // [2]: Object name
                string objName = ReadString();

                // No known overrides for object name
                if (objName.Length > 0)
                    continue;

                // [1]: Number of types
                int numTypes = reader.ReadByte();
                
                byte[] types = new byte[numTypes];

                // For number of types:
                for (int j = 0; j < numTypes; j++)
                    // [1]: Type
                    types[j] = reader.ReadByte();

                // [4]: Number of objects
                long numObjects = reader.ReadInt64();

                // For number of objects:
                for (int j = 0; j < numObjects; j++)
                {
                    // [8]: Timestamp (double)
                    double ms = reader.ReadDouble();

                    // [x]: Type data (variable)
                    object[] objectData = new object[numTypes + 1];
                    objectData[0] = ms;

                    for (int k = 0; k < numTypes; k++)
                        objectData[k + 1] = ReadType(types[k]);

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
            throw new NotImplementedException();
        }
    }
}
