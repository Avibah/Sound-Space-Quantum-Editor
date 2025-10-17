using New_SSQE.Misc.Static;
using New_SSQE.Objects;
using New_SSQE.Objects.Other;
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

            // [2]: Number of fields
            int numFields = reader.ReadInt16();
            // [1]: Number of object blocks
            int numIDs = reader.ReadByte();

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
                    _ => throw new InvalidDataException($"Unknown field type {type}")
                };
            }
            
            // UInt8 length type array
            object[] ReadTypeArray()
            {
                int length = reader.ReadByte();
                byte type = reader.ReadByte();

                object[] typeData = new object[length];

                for (int i = 0; i < length; i++)
                    typeData[i] = ReadType(type);

                return typeData;
            }

            // UInt16 length type array
            object[] ReadTypeArrayLong()
            {
                int length = reader.ReadInt16();
                byte type = reader.ReadByte();

                object[] typeData = new object[length];

                for (int i = 0; i < length; i++)
                    typeData[i] = ReadType(type);

                return typeData;
            }

            MapObject ParseObj(int id, params object[] data)
            {
                return id switch
                {
                    0 => new Note((float)data[0], (float)data[1], (long)data[2], (bool)data[3], (EasingStyle)data[4], (EasingDirection)data[5]),
                    1 => new TimingPoint((float)data[0], (long)data[1], ((float)data[2], (float)data[3])),
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
                    _ => throw new InvalidDataException($"Invalid object ID {id}")
                };
            }

            Dictionary<string, object> metadata = [];

            // Field block:
            for (int i = 0; i < numFields; i++)
            {
                // [2]: Field name
                string fieldName = ReadString();
                // [1]: Field type
                byte type = reader.ReadByte();

                // Field value
                object value = ReadType(type);

                metadata.Add(fieldName, value);
            }

            foreach (string field in metadata.Keys)
            {
                switch (field)
                {

                }
            }

            ObjectList<Note> notes = Mapping.Current.Notes;
            ObjectList<MapObject> vfxObjects = Mapping.Current.VfxObjects;
            ObjectList<MapObject> specialObjects = Mapping.Current.SpecialObjects;

            // Object block:
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
                int numObjects = reader.ReadInt32();

                // For number of objects:
                for (int j = 0; j < numObjects; j++)
                {
                    // [8]: Timestamp (double)
                    double ms = reader.ReadDouble();

                    // [x]: Type data (variable)
                    object[] objectData = new object[numTypes];

                    for (int k = 0; k < numTypes; k++)
                        objectData[k] = ReadType(types[k]);

                    MapObject obj = ParseObj(id, objectData); // make parser for objs with default data for each id

                    if (id == 0)
                        notes.Add((Note)obj);
                    else if (FormatUtils.VfxLookup[id])
                        vfxObjects.Add(obj);
                    else
                        specialObjects.Add(obj);
                }
            }

            throw new NotImplementedException();
        }

        public static bool Write(string path)
        {
            throw new NotImplementedException();
        }
    }
}
