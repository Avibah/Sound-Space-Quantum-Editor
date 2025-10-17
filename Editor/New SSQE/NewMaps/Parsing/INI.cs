using New_SSQE.Misc.Static;
using New_SSQE.NewGUI.Windows;
using New_SSQE.Objects;
using New_SSQE.Objects.Managers;
using New_SSQE.Objects.Other;
using New_SSQE.Preferences;
using System.Text.Json;

namespace New_SSQE.NewMaps.Parsing
{
    internal class INI : IFormatParser
    {
        public static bool ReadLegacy(string path)
        {
            string data = File.ReadAllText(path);

            string[] lines = data.Split('\n');
            bool oldVer = false; // pre 1.7

            foreach (string line in lines)
            {
                string[] split = line.Split('=');

                switch (split[0])
                {
                    case "BPM":
                        string[] points = split[1].Split(',');

                        foreach (string point in points)
                        {
                            string[] pointsplit = point.Split('|');

                            if (pointsplit.Length == 1)
                            {
                                pointsplit = [pointsplit[0], "0"];
                                oldVer = true;
                            }

                            if (pointsplit.Length == 2 && float.TryParse(pointsplit[0], out float bpm) && long.TryParse(pointsplit[1], out long ms))
                                Mapping.Current.TimingPoints.Add(new(bpm, ms));
                        }

                        Mapping.Current.SortTimings();

                        break;

                    case "Bookmarks":
                        string[] bookmarks = split[1].Split(',');

                        foreach (string bookmark in bookmarks)
                        {
                            string[] bookmarksplit = bookmark.Split('|');

                            if (bookmarksplit.Length == 2 && long.TryParse(bookmarksplit[1], out long ms))
                                Mapping.Current.Bookmarks.Add(new(bookmarksplit[0], ms, ms));
                            else if (bookmarksplit.Length == 3 && long.TryParse(bookmarksplit[1], out long startMs) && long.TryParse(bookmarksplit[2], out long endMs))
                                Mapping.Current.Bookmarks.Add(new(bookmarksplit[0], startMs, endMs));
                        }

                        Mapping.Current.SortBookmarks();

                        break;

                    case "Offset":
                        if (oldVer) // back when timing points didnt exist and the offset meant bpm/note offset
                        {
                            if (Mapping.Current.TimingPoints.Count > 0 && long.TryParse(split[1], out long bpmOffset))
                                Mapping.Current.TimingPoints[0].Ms = bpmOffset;
                        }
                        else
                        {
                            foreach (Note note in Mapping.Current.Notes)
                                note.Ms += (long)Settings.exportOffset.Value;

                            if (long.TryParse(split[1], out long offset))
                                Settings.exportOffset.Value = offset;

                            foreach (Note note in Mapping.Current.Notes)
                                note.Ms -= (long)Settings.exportOffset.Value;
                        }

                        break;

                    case "Time":
                        if (long.TryParse(split[1], out long time))
                            Settings.currentTime.Value.Value = time;

                        break;

                    case "Divisor":
                        if (float.TryParse(split[1], out float divisor))
                            Settings.beatDivisor.Value.Value = divisor - 1f;

                        break;
                }
            }

            return true;
        }

        public static bool Read(string path)
        {
            string data = File.ReadAllText(path);

            Mapping.Current.TimingPoints.Clear();
            Mapping.Current.Bookmarks.Clear();

            try
            {
                Dictionary<string, JsonElement> result = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(data) ?? [];

                foreach (string key in result.Keys)
                {
                    JsonElement value = result[key];

                    switch (key)
                    {
                        case "timings":
                            List<JsonElement> timings = value.Deserialize<List<JsonElement>>() ?? [];

                            foreach (JsonElement timing in timings)
                            {
                                JsonElement[] values = timing.Deserialize<JsonElement[]>() ?? [];
                                Mapping.Current.TimingPoints.Add(new(values[0].GetSingle(), values[1].GetInt64()));
                            }

                            break;

                        case "bookmarks":
                            List<JsonElement> bookmarks = value.Deserialize<List<JsonElement>>() ?? [];

                            foreach (JsonElement bookmark in bookmarks)
                            {
                                JsonElement[] values = bookmark.Deserialize<JsonElement[]>() ?? [];
                                Mapping.Current.Bookmarks.Add(new(values[0].GetString() ?? "", values[1].GetInt64(), values[^1].GetInt64()));
                            }

                            break;

                        case "vfxObjects":
                            List<string> vfx = value.Deserialize<List<string>>() ?? [];
                            List<MapObject> vfxObjects = [];

                            foreach (string obj in vfx)
                            {
                                string[] temp = TXT.ReadObjects(obj)[0];
                                MapObject? final = MOParser.Parse(null, temp);
                                if (final != null)
                                    vfxObjects.Add(final);
                            }

                            if (Mapping.Current.VfxObjects.Count + vfxObjects.Count > 0)
                                Mapping.Current.VfxObjects.Modify_Replace("IMPORT VFX", Mapping.Current.VfxObjects, vfxObjects);

                            break;

                        case "specialObjects":
                            List<string> special = value.Deserialize<List<string>>() ?? [];
                            List<MapObject> specialObjects = [];

                            foreach (string obj in special)
                            {
                                string[] temp = TXT.ReadObjects(obj)[0];
                                MapObject? final = MOParser.Parse(null, temp);
                                if (final != null)
                                    specialObjects.Add(final);
                            }

                            if (Mapping.Current.SpecialObjects.Count + specialObjects.Count > 0)
                                Mapping.Current.SpecialObjects.Modify_Replace("IMPORT EXTRA", Mapping.Current.SpecialObjects, specialObjects);

                            break;

                        case "noteData":
                            List<JsonElement> noteData = value.Deserialize<List<JsonElement>>() ?? [];
                            List<Note> notes = Mapping.Current.Notes;

                            foreach (JsonElement nData in noteData)
                            {
                                JsonElement[] values = nData.Deserialize<JsonElement[]>() ?? [];
                                int index = values[0].GetInt32();

                                if (index >= 0 && index < notes.Count)
                                {
                                    Note note = notes[index];

                                    note.EnableEasing = true;
                                    note.Style = (EasingStyle)values[1].GetInt32();
                                    note.Direction = (EasingDirection)values[2].GetInt32();
                                }
                            }

                            break;

                        case "currentTime":
                            Settings.currentTime.Value.Value = value.GetSingle();
                            break;

                        case "beatDivisor":
                            Settings.beatDivisor.Value.Value = value.GetSingle();
                            break;

                        case "exportOffset":
                            foreach (Note note in Mapping.Current.Notes)
                                note.Ms += (long)Settings.exportOffset.Value;

                            Settings.exportOffset.Value = value.GetSingle();
                            GuiWindowEditor.ExportOffset.Text = $"{Math.Floor(Settings.exportOffset.Value)}";

                            foreach (Note note in Mapping.Current.Notes)
                                note.Ms -= (long)Settings.exportOffset.Value;

                            break;

                        case "mappers":
                            Settings.mappers.Value = value.GetString() ?? "";
                            break;
                        case "songName":
                            Settings.songName.Value = value.GetString() ?? "";
                            break;
                        case "difficulty":
                            Settings.difficulty.Value = value.GetString() ?? "";
                            break;
                        case "useCover":
                            Settings.useCover.Value = value.GetBoolean();
                            break;
                        case "cover":
                            Settings.cover.Value = value.GetString() ?? "";
                            break;
                        case "customDifficulty":
                            Settings.customDifficulty.Value = value.GetString() ?? "";
                            break;

                        case "songOffset":
                            Settings.songOffset.Value = value.GetString() ?? "";
                            break;
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
                            Settings.previewStartTime.Value = value.GetString() ?? "";
                            break;
                        case "previewDuration":
                            Settings.previewDuration.Value = value.GetString() ?? "";
                            break;
                        case "novaCover":
                            Settings.novaCover.Value = value.GetString() ?? "";
                            break;
                        case "novaIcon":
                            Settings.novaIcon.Value = value.GetString() ?? "";
                            break;

                        case "rating":
                            Settings.rating.Value = value.GetSingle();
                            break;
                        case "useVideo":
                            Settings.useVideo.Value = value.GetBoolean();
                            break;
                        case "video":
                            Settings.video.Value = value.GetString() ?? "";
                            break;
                    }
                }

                Mapping.Current.ClearSelected();
                return true;
            }
            catch
            {
                try
                {
                    return ReadLegacy(path);
                }
                catch { }
            }

            return false;
        }

        public static bool Write(string path)
        {
            List<object[]> timingfinal = [];

            foreach (TimingPoint point in Mapping.Current.TimingPoints)
                timingfinal.Add([point.BPM, point.Ms]);

            List<object[]> bookmarkfinal = [];

            foreach (Bookmark bookmark in Mapping.Current.Bookmarks)
                bookmarkfinal.Add([bookmark.Text, bookmark.Ms, bookmark.EndMs]);

            List<string> vfxFinal = [];

            foreach (MapObject obj in Mapping.Current.VfxObjects)
                vfxFinal.Add($"{obj.ID}|{obj.ToString()}");

            List<string> specialFinal = [];

            foreach (MapObject obj in Mapping.Current.SpecialObjects)
                specialFinal.Add($"{obj.ID}|{obj.ToString()}");

            List<object[]> noteData = [];

            for (int i = 0; i < Mapping.Current.Notes.Count; i++)
            {
                Note note = Mapping.Current.Notes[i];

                if (note.EnableEasing)
                    noteData.Add([i, (int)note.Style, (int)note.Direction]);
            }

            Dictionary<string, object> json = new()
            {
                {"timings", timingfinal },
                {"bookmarks", bookmarkfinal },
                {"vfxObjects", vfxFinal },
                {"specialObjects", specialFinal },
                {"noteData", noteData },

                {"currentTime", Settings.currentTime.Value.Value },
                {"beatDivisor", Settings.beatDivisor.Value.Value },
                {"exportOffset", Settings.exportOffset.Value },

                {"mappers", Settings.mappers.Value },
                {"songName", Settings.songName.Value },
                {"difficulty", Settings.difficulty.Value },
                {"useCover", Settings.useCover.Value },
                {"cover", Settings.cover.Value },
                {"customDifficulty", Settings.customDifficulty.Value },

                {"songOffset", Settings.songOffset.Value },
                {"songTitle", Settings.songTitle.Value },
                {"songArtist", Settings.songArtist.Value },
                {"mapCreator", Settings.mapCreator.Value },
                {"mapCreatorPersonalLink", Settings.mapCreatorPersonalLink.Value },
                {"previewStartTime", Settings.previewStartTime.Value },
                {"previewDuration", Settings.previewDuration.Value },
                {"novaCover", Settings.novaCover.Value },
                {"novaIcon", Settings.novaIcon.Value },

                {"rating", Settings.rating.Value },
                {"useVideo", Settings.useVideo.Value },
                {"video", Settings.video.Value }
            };

            File.WriteAllText(path, JsonSerializer.Serialize(json));
            return true;
        }
    }
}
