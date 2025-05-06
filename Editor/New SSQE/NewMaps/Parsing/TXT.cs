using New_SSQE.Objects;
using New_SSQE.Objects.Managers;
using New_SSQE.Preferences;

namespace New_SSQE.NewMaps.Parsing
{
    internal class TXT : IFormatParser
    {
        public static string[][] ReadObjects(string data)
        {
            List<string[]> result = [];
            List<string> obj = [];
            List<char> chars = [];

            bool escaped = false;
            bool quoted = false;

            for (int i = 0; i < data.Length; i++)
            {
                if (quoted && !escaped)
                {
                    if (data[i] == '"')
                        quoted = false;
                    else
                        chars.Add(data[i]);
                    continue;
                }

                if (escaped)
                {
                    escaped = false;
                    chars.Add(data[i]);
                    continue;
                }

                switch (data[i])
                {
                    case '\\':
                        escaped = true;
                        break;
                    case '"':
                        quoted = true;
                        break;
                    case '|':
                        obj.Add(string.Join(null, chars));
                        chars = [];
                        break;
                    case ',':
                        obj.Add(string.Join(null, chars));
                        chars = [];
                        result.Add(obj.ToArray());
                        obj = [];
                        break;
                    default:
                        chars.Add(data[i]);
                        break;
                }
            }

            obj.Add(string.Join(null, chars));
            result.Add(obj.ToArray());
            return result.ToArray();
        }

        public static bool ReadData(string data)
        {
            string[] split = data.Split('`');

            if (split.Length == 1)
            {
                string[][] mapData = ReadObjects(split[0]);
                Mapping.Current.SoundID = mapData[0][0];

                for (int i = 1; i < mapData.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(string.Join('|', mapData[i])))
                    {
                        try
                        {
                            string[] subData = mapData[i];

                            float x = float.Parse(subData[0], Program.Culture);
                            float y = float.Parse(subData[1], Program.Culture);
                            long ms = long.Parse(subData[2]);

                            Mapping.Current.Notes.Add(new(x, y, ms));
                        }
                        catch (Exception ex)
                        {
                            throw new AggregateException($"Note failed to parse: {mapData[i]}", ex);
                        }
                    }
                }

                return true;
            }
            else
            {
                string fileSignature = split[0];

                switch (fileSignature)
                {
                    case "ssmapv2":
                        int diffID = int.Parse(split[2]);
                        string diffName = FormatUtils.Difficulties.Keys.ToArray()[diffID];
                        string customDiff = split[3];

                        Mapping.Current.SoundID = split[1];
                        Settings.difficulty.Value = diffName;
                        Settings.customDifficulty.Value = customDiff == diffName ? "" : customDiff;

                        string[] objSets = split[4].Split('/');

                        for (int i = 0; i < objSets.Length; i++)
                        {
                            if (string.IsNullOrWhiteSpace(objSets[i]))
                                continue;

                            string[][] objs = ReadObjects(objSets[i]);

                            FormatUtils.ResetDecode();

                            for (int j = 0; j < objs.Length; j++)
                            {
                                MapObject? obj = MOParser.Parse(i, objs[j]);

                                if (obj != null)
                                {
                                    if (obj is Note n)
                                        Mapping.Current.Notes.Add(n);
                                    else if (obj is TimingPoint p)
                                        Mapping.Current.TimingPoints.Add(p);
                                    else if (FormatUtils.VfxLookup[i])
                                        Mapping.Current.VfxObjects.Add(obj);
                                    else
                                        Mapping.Current.SpecialObjects.Add(obj);

                                    obj.Ms = FormatUtils.DecodeTimestamp(obj.Ms);
                                }
                            }
                        }

                        return true;

                    default:
                        throw new NotImplementedException($"Map format not supported - ${fileSignature}");
                }
            }
        }

        public static bool Read(string path)
        {
            string ini = Path.ChangeExtension(path, ".ini");

            return ReadData(File.ReadAllText(path)) && (!File.Exists(ini) || INI.Read(ini));
        }

        public static bool Write(string path)
        {
            string ini = Path.ChangeExtension(path, ".ini");
            File.WriteAllText(path, Copy());

            return INI.Write(ini);
        }

        private static MapHistory forcedHistory = new("save");
        private static MapHistory autosaveHistory = new("autosave");

        public static bool WriteForced(string path)
        {
            forcedHistory.Store(path);
            return Write(path);
        }

        public static bool WriteAutosave(string path)
        {
            autosaveHistory.Store(path);
            return Write(path);
        }

        public static string CopyLegacy(bool correctNotes = false)
        {
            List<Note> notes = Mapping.Current.Notes;

            long offset = (long)Settings.exportOffset.Value;
            string[] final = new string[notes.Count + 1];
            final[0] = Mapping.Current.SoundID.Replace(",", "");

            for (int i = 0; i < notes.Count; i++)
            {
                Note clone = notes[i].Clone();
                clone.Ms += offset;

                if (correctNotes)
                {
                    clone.X = Math.Clamp(clone.X, -0.85f, 2.85f);
                    clone.Y = Math.Clamp(clone.Y, -0.85f, 2.85f);

                    clone.Ms = (long)Math.Clamp(clone.Ms, 0, Settings.currentTime.Value.Max);
                }

                double x = Math.Round(clone.X, 2);
                double y = Math.Round(clone.Y, 2);

                final[i + 1] = $"{x.ToString(Program.Culture)}|{y.ToString(Program.Culture)}|{clone.Ms}";
            }

            return string.Join(',', final);
        }

        public static string Copy(bool correctNotes = false)
        {
            return CopyLegacy(correctNotes);

            ObjectList<Note> notes = Mapping.Current.Notes;
            ObjectList<MapObject> vfxObjects = new(Mapping.Current.VfxObjects.OrderBy(n => n.ID).ThenBy(n => n.Ms));
            ObjectList<MapObject> specObjects = new(Mapping.Current.SpecialObjects.OrderBy(n => n.ID).ThenBy(n => n.Ms));
            List<TimingPoint> timingPoints = Mapping.Current.TimingPoints;

            long staticOffset = (long)Settings.exportOffset.Value;

            List<string> final =
            [
                "ssmapv2", Mapping.Current.SoundID,
                FormatUtils.Difficulties[Settings.difficulty.Value].ToString(),
                string.IsNullOrWhiteSpace(Settings.customDifficulty.Value) ? Settings.difficulty.Value : Settings.customDifficulty.Value
            ];

            FormatUtils.ResetEncode();

            string SaveObject(MapObject obj)
            {
                MapObject clone = obj.Clone();
                clone.Ms += staticOffset;

                if (correctNotes && clone is Note n)
                {
                    n.X = Math.Clamp(n.X, -0.85f, 2.85f);
                    n.Y = Math.Clamp(n.Y, -0.85f, 2.85f);

                    n.Ms = (long)Math.Clamp(n.Ms, 0, Settings.currentTime.Value.Max);
                }

                clone.Ms = FormatUtils.EncodeTimestamp(clone.Ms);

                return clone.ToString();
            }

            List<string> objStrs = [];

            string[] noteSet = new string[notes.Count];
            for (int i = 0; i < notes.Count; i++)
                noteSet[i] = SaveObject(notes[i]);
            objStrs.Add(string.Join(',', noteSet));

            FormatUtils.ResetEncode();

            string[] timingSet = new string[timingPoints.Count];
            for (int i = 0; i < timingPoints.Count; i++)
                timingSet[i] = SaveObject(timingPoints[i]);
            objStrs.Add(string.Join(',', timingSet));

            int id = 1;
            int index = 0;

            List<string> tempSet = [];

            while (index < vfxObjects.Count)
            {
                MapObject obj = vfxObjects[index];

                if (obj.ID != id)
                {
                    for (int i = id; i < obj.ID; i++)
                    {
                        if (i > 1)
                            objStrs.Add(string.Join(',', tempSet));
                        tempSet = [];
                    }

                    id = obj.ID;

                    FormatUtils.ResetEncode();
                    continue;
                }

                tempSet.Add(SaveObject(obj));

                index++;
            }

            if (tempSet.Count > 0)
                objStrs.Add(string.Join(',', tempSet));
            tempSet = [];

            int count = objStrs.Count - 1;
            int maxId = count;
            id = 1;

            FormatUtils.ResetEncode();
            index = 0;

            while (index < specObjects.Count)
            {
                MapObject obj = specObjects[index];

                if (obj.ID > maxId)
                {
                    for (int i = maxId; i < obj.ID; i++)
                    {
                        if (i > count)
                            objStrs.Add(string.Join(',', tempSet));
                        tempSet = [];
                    }

                    maxId = obj.ID;
                    id = obj.ID;

                    FormatUtils.ResetEncode();
                    continue;
                }
                else if (obj.ID != id)
                {
                    objStrs[obj.ID] = string.Join(',', tempSet);
                    tempSet = [];

                    id = obj.ID;

                    FormatUtils.ResetEncode();
                }

                tempSet.Add(SaveObject(obj));

                index++;
            }

            if (tempSet.Count > 0)
                objStrs.Add(string.Join(',', tempSet));
            final.Add(string.Join('/', objStrs));

            return string.Join("`", final);
        }
    }
}
