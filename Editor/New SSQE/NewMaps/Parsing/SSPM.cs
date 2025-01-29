using New_SSQE.ExternalUtils;
using New_SSQE.Misc.Dialogs;
using New_SSQE.Misc.Static;
using New_SSQE.Objects;
using New_SSQE.Preferences;
using System.Text;

namespace New_SSQE.NewMaps.Parsing
{
    internal class SSPM : IFormatParser
    {
        public static bool Read(string path)
        {
            using FileStream data = new(path, FileMode.Open, FileAccess.Read);
            data.Seek(0, SeekOrigin.Begin);
            using BinaryReader reader = new(data);

            string fileTypeSignature = Encoding.ASCII.GetString(reader.ReadBytes(4));

            if (fileTypeSignature != "SS+m")
            {
                MessageBox.Show("File type not recognized or supported\nCurrently supported: SSPM v1/v2", MBoxIcon.Warning, MBoxButtons.OK);
                return false;
            }

            ushort formatVersion = reader.ReadUInt16();

            if (formatVersion == 1)
            {
                string GetNextVariableString()
                {
                    List<byte> bytes = [];
                    byte cur = reader.ReadByte();

                    while (cur != 0x0a)
                    {
                        bytes.Add(cur);
                        cur = reader.ReadByte();
                    }

                    return Encoding.UTF8.GetString(bytes.ToArray());
                }


                // v1
                reader.ReadBytes(2);

                // metadata
                string mapID = FormatUtils.FixID(GetNextVariableString());
                string mapName = GetNextVariableString();
                string mappers = GetNextVariableString();

                CurrentMap.SoundID = mapID;
                Settings.songName.Value = mapName;
                Settings.mappers.Value = mappers;

                uint lastMs = reader.ReadUInt32();
                uint noteCount = reader.ReadUInt32();
                byte difficulty = reader.ReadByte();

                foreach (KeyValuePair<string, byte> key in FormatUtils.Difficulties)
                {
                    if (key.Value == difficulty)
                        Settings.difficulty.Value = key.Key;
                }

                // read cover
                byte containsCover = reader.ReadByte(); // sspm v1 has 0x02 as PNG so it cant be parsed as a bool

                Settings.useCover.Value = containsCover == 0x02;

                if (containsCover == 0x02)
                {
                    ulong coverLength = reader.ReadUInt64();
                    byte[] cover = reader.ReadBytes((int)coverLength);

                    File.WriteAllBytes(Path.Combine(Assets.CACHED, $"{mapID}.png"), cover);
                    Settings.cover.Value = Path.Combine(Assets.CACHED, $"{mapID}.png");
                }

                // read audio
                bool containsAudio = reader.ReadBoolean();

                if (containsAudio)
                {
                    ulong audioLength = reader.ReadUInt64();
                    byte[] audio = reader.ReadBytes((int)audioLength);

                    File.WriteAllBytes(Path.Combine(Assets.CACHED, $"{mapID}.asset"), audio);
                }

                // process notes
                for (uint i = 0; i < noteCount; i++)
                {
                    uint ms = reader.ReadUInt32();
                    bool isQuantum = reader.ReadBoolean();

                    float x;
                    float y;

                    if (isQuantum)
                    {
                        x = reader.ReadSingle();
                        y = reader.ReadSingle();
                    }
                    else
                    {
                        x = reader.ReadByte();
                        y = reader.ReadByte();
                    }

                    CurrentMap.Notes.Add(new(x, y, ms));
                }

                return true;
            }
            else if (formatVersion == 2)
            {
                string GetNextVariableString(bool fourBytes = false)
                {
                    uint length = fourBytes ? reader.ReadUInt32() : reader.ReadUInt16();
                    byte[] str = reader.ReadBytes((int)length);

                    return Encoding.UTF8.GetString(str);
                }


                // v2
                reader.ReadBytes(4);

                // metadata
                byte[] hash = reader.ReadBytes(20);
                uint lastMs = reader.ReadUInt32();
                uint noteCount = reader.ReadUInt32();
                uint markerCount = reader.ReadUInt32();

                byte difficulty = reader.ReadByte();
                ushort mapRating = reader.ReadUInt16();
                bool containsAudio = reader.ReadBoolean();
                bool containsCover = reader.ReadBoolean();
                bool requiresMod = reader.ReadBoolean();

                // pointers
                ulong customDataOffset = reader.ReadUInt64();
                ulong customDataLength = reader.ReadUInt64();
                ulong audioOffset = reader.ReadUInt64();
                ulong audioLength = reader.ReadUInt64();
                ulong coverOffset = reader.ReadUInt64();
                ulong coverLength = reader.ReadUInt64();
                ulong markerDefinitionsOffset = reader.ReadUInt64();
                ulong markerDefinitionsLength = reader.ReadUInt64();
                ulong markerOffset = reader.ReadUInt64();
                ulong markerLength = reader.ReadUInt64();

                foreach (KeyValuePair<string, byte> key in FormatUtils.Difficulties)
                {
                    if (key.Value == difficulty)
                        Settings.difficulty.Value = key.Key;
                }

                // get song name stuff and mappers
                string mapID = FormatUtils.FixID(GetNextVariableString());
                string mapName = GetNextVariableString();
                string songName = GetNextVariableString();

                CurrentMap.SoundID = mapID;
                Settings.songName.Value = mapName;

                ushort mapperCount = reader.ReadUInt16();
                string[] mappers = new string[mapperCount];

                for (ushort i = 0; i < mapperCount; i++)
                    mappers[i] = GetNextVariableString();

                Settings.mappers.Value = string.Join("\n", mappers);
                Settings.mapCreator.Value = string.Join(" | ", mappers);

                // read custom data block
                // may implement more fields in the future, but right now only 'difficulty_name' is used
                try
                {
                    void SetField(string field, dynamic value)
                    {
                        switch (field)
                        {
                            case "difficulty_name":
                                Settings.customDifficulty.Value = value;
                                break;
                        }
                    }

                    ushort customCount = reader.ReadUInt16();

                    for (ushort i = 0; i < customCount; i++)
                    {
                        string field = GetNextVariableString();
                        byte id = reader.ReadByte();

                        // discard all but 0x09 and 0x0b (string data)
                        switch (id)
                        {
                            case 0x00:
                                continue;
                            case 0x01:
                                reader.ReadBytes(1);
                                break;
                            case 0x02:
                                reader.ReadBytes(2);
                                break;
                            case 0x03:
                            case 0x05:
                                reader.ReadBytes(4);
                                break;
                            case 0x04:
                            case 0x06:
                                reader.ReadBytes(8);
                                break;
                            case 0x07:
                                bool type = reader.ReadBoolean();

                                if (!type)
                                    reader.ReadBytes(2);
                                else
                                    reader.ReadBytes(8);
                                break;
                            case 0x08:
                                GetNextVariableString();
                                break;
                            case 0x09:
                                SetField(field, GetNextVariableString());
                                break;
                            case 0x0a:
                                GetNextVariableString(true);
                                break;
                            case 0x0b:
                                SetField(field, GetNextVariableString(true));
                                break;
                            case 0x0c:
                                reader.ReadBytes(1);
                                uint valueLength = reader.ReadUInt32();
                                reader.ReadBytes((int)valueLength);
                                break;
                        }
                    }
                }
                catch { }

                // jump to beginning of audio block in case custom data reading was unsuccessful
                reader.BaseStream.Seek((long)audioOffset, SeekOrigin.Begin);

                // read and cache audio
                if (containsAudio)
                {
                    byte[] audio = reader.ReadBytes((int)audioLength);
                    File.WriteAllBytes(Path.Combine(Assets.CACHED, $"{mapID}.asset"), audio);
                }

                Settings.useCover.Value = containsCover;

                // read cover
                if (containsCover)
                {
                    byte[] cover = reader.ReadBytes((int)coverLength);
                    File.WriteAllBytes(Path.Combine(Assets.CACHED, $"{mapID}.png"), cover);
                    Settings.cover.Value = Path.Combine(Assets.CACHED, $"{mapID}.png");
                }

                // read marker definitions
                bool hasNotes = false;

                byte numDefinitions = reader.ReadByte();

                for (byte i = 0; i < numDefinitions; i++)
                {
                    string definition = GetNextVariableString();
                    hasNotes |= definition == "ssp_note" && i == 0;

                    byte numValues = reader.ReadByte();

                    byte definitionData = 0x01;
                    while (definitionData != 0x00)
                        definitionData = reader.ReadByte();
                }

                if (!hasNotes)
                    return true;

                // process notes
                for (uint i = 0; i < noteCount; i++)
                {
                    uint ms = reader.ReadUInt32();
                    byte markerType = reader.ReadByte();
                    bool isQuantum = reader.ReadBoolean();

                    float x;
                    float y;

                    if (isQuantum)
                    {
                        x = reader.ReadSingle();
                        y = reader.ReadSingle();
                    }
                    else
                    {
                        x = reader.ReadByte();
                        y = reader.ReadByte();
                    }

                    CurrentMap.Notes.Add(new(x, y, ms));
                }

                return true;
            }
            else
            {
                MessageBox.Show("File version not recognized or supported\nCurrently supported: SSPM v1/v2", MBoxIcon.Warning, MBoxButtons.OK);
                return false;
            }
        }

        public static bool Write(string path)
        {
            List<Note> notes = CurrentMap.Notes;

            using FileStream file = new(path, FileMode.Create, FileAccess.Write);
            using BinaryWriter writer = new(file);

            writer.Write(new byte[]
            {
                0x53, 0x53, 0x2b, 0x6d, // file type signature
                0x02, 0x00, // SSPM format version - 2
                0x00, 0x00, 0x00, 0x00 // reserved space
            });

            bool hasCover = Metadata["coverPath"] != ""; // whether cover should be present

            void WriteString(string str)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(str); // get bytes encoded as utf-8
                writer.Write((ushort)bytes.Length); // length of bytes - 2 byte uint
                writer.Write(bytes); // the utf-8 string itself
            }

            // metadata
            writer.Write(new byte[20]); // a 20 byte hash i am not going to care about because rhythia doesnt seem to check it

            writer.Write((uint)notes.Last().Ms); // last note ms - 4 byte uint
            writer.Write((uint)notes.Count); // note count - 4 byte uint
            writer.Write((uint)notes.Count); // marker count, repeated from last since no other markers

            writer.Write(FormatUtils.Difficulties[Metadata["difficulty"]]); // difficulty - 1 byte uint
            writer.Write(new byte[2]); // rating? whatever that means
            writer.Write(true); // contains audio
            writer.Write(hasCover); // may contain cover
            writer.Write(false); // does not require a mod

            long pointerOffset = writer.BaseStream.Position;
            writer.Write(new byte[80]); // placeholder for pointer block

            WriteString(Metadata["songId"]); // song ID from form
            WriteString(Metadata["mapName"]); // map name from form
            WriteString(Metadata["mapName"]); // again because map and song names are different apparently

            string[] mappers = Metadata["mappers"].Split('\n'); // list of provided valid mappers or "None" if empty
            writer.Write((ushort)mappers.Length); // number of provided valid mappers

            foreach (string mapper in mappers)
                WriteString(mapper); // mapper name

            // custom data
            long customDataOffset = 0;
            long customDataLength = 0;

            if (!string.IsNullOrWhiteSpace(Metadata["customDifficulty"]))
            {
                // custom difficulty
                customDataOffset = writer.BaseStream.Position;

                writer.Write(new byte[] { 0x01, 0x00 }); // one field
                WriteString("difficulty_name"); // "difficulty_name" field indicator
                writer.Write((byte)0x09); // data type 09 - string w/ 2 byte uint length
                WriteString(Metadata["customDifficulty"]); // custom difficulty from form

                customDataLength = writer.BaseStream.Position - customDataOffset;
            }
            else
                writer.Write(new byte[2]); // no custom data

            // audio
            long audioOffset = writer.BaseStream.Position;
            writer.Write(File.ReadAllBytes(Path.Combine(Assets.CACHED, $"{CurrentMap.SoundID}.asset"))); // audio in bytes
            long audioLength = writer.BaseStream.Position - audioOffset;

            // cover
            long coverOffset = 0;
            long coverLength = 0;

            if (hasCover)
            {
                coverOffset = writer.BaseStream.Position;

                string coverPath = Metadata["coverPath"] == "Default" || !File.Exists(Metadata["coverPath"]) ? Path.Combine(Assets.TEXTURES, "Cover.png") : Metadata["coverPath"];
                writer.Write(File.ReadAllBytes(coverPath)); // cover in bytes

                coverLength = writer.BaseStream.Position - coverOffset;
            }

            // marker definitions
            long markerDefinitionsOffset = writer.BaseStream.Position;
            writer.Write((byte)0x01); // one definition
            WriteString("ssp_note"); // "ssp_note" marker indicator
            writer.Write(new byte[] { 0x01, /* one value */ 0x07, /* data type 07 - note */ 0x00 /* end of definition */ });
            long markerDefinitionsLength = writer.BaseStream.Position - markerDefinitionsOffset;

            // markers
            long markerOffset = writer.BaseStream.Position;
            long exportOffset = (long)Settings.exportOffset.Value;

            foreach (Note note in notes)
            {
                // [Ms | Type | Quantum | X | Y] repeated
                writer.Write((uint)(note.Ms + exportOffset)); // timestamp in bytes - 4 byte uint
                writer.Write((byte)0x00); // marker type 0 - first definition, ssp_note

                bool quantum = Math.Round(note.X) != Math.Round(note.X, 2) || Math.Round(note.Y) != Math.Round(note.Y, 2); // whether the note is quantum
                writer.Write(quantum); // quantum identifier - 1 byte boolean

                if (quantum)
                {
                    writer.Write(2 - note.X); // note x - 4 byte float
                    writer.Write(2 - note.Y); // note y - 4 byte float
                }
                else
                {
                    writer.Write((byte)(2 - note.X)); // note x - 1 byte uint
                    writer.Write((byte)(2 - note.Y)); // note y - 1 byte uint
                }
            }

            long markerLength = writer.BaseStream.Position - markerOffset;

            // pointers, going back to the 80 byte placeholder from earlier
            writer.BaseStream.Seek(pointerOffset, SeekOrigin.Begin);
            // all values are 8 byte uints (technically)
            writer.Write(customDataOffset); // offset/length will be 0 if no custom data
            writer.Write(customDataLength);
            writer.Write(audioOffset);
            writer.Write(audioLength);
            writer.Write(coverOffset); // offset/length will be 0 if no cover
            writer.Write(coverLength);
            writer.Write(markerDefinitionsOffset);
            writer.Write(markerDefinitionsLength);
            writer.Write(markerOffset);
            writer.Write(markerLength);

            // in case something goes wrong, the export can be traced back to the version it was made on
            writer.BaseStream.Seek(0, SeekOrigin.End);
            WriteString($"\nSSQE Export - {Program.Version}");

            // the documentation for this stuff really isnt great, some examples would be nice
            // https://github.com/basils-garden/types/blob/main/sspm/v2.md

            return true;
        }

        public static Dictionary<string, string> Metadata = new()
        {
            {"songId", "" },
            {"mapName", "" },
            {"mappers", "" },
            {"coverPath", "" },
            {"difficulty", "" },
            {"customDifficulty", "" }
        };

        public static bool Export()
        {
            DialogResult result = new SaveFileDialog()
            {
                Title = "Export SSPM",
                Filter = "Rhythia Maps (*.sspm)|*.sspm",
                InitialFileName = $"{Metadata["songId"]}.sspm"
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
                    MessageBox.Show($"Failed to export SSPM:\n\n{ex.Message}", MBoxIcon.Warning, MBoxButtons.OK);
                }
            }

            return false;
        }
    }
}
