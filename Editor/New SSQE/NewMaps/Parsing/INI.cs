using New_SSQE.GUI;
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
                                CurrentMap.TimingPoints.Add(new(bpm, ms));
                        }

                        CurrentMap.SortTimings();

                        break;

                    case "Bookmarks":
                        string[] bookmarks = split[1].Split(',');

                        foreach (string bookmark in bookmarks)
                        {
                            string[] bookmarksplit = bookmark.Split('|');

                            if (bookmarksplit.Length == 2 && long.TryParse(bookmarksplit[1], out long ms))
                                CurrentMap.Bookmarks.Add(new(bookmarksplit[0], ms, ms));
                            else if (bookmarksplit.Length == 3 && long.TryParse(bookmarksplit[1], out long startMs) && long.TryParse(bookmarksplit[2], out long endMs))
                                CurrentMap.Bookmarks.Add(new(bookmarksplit[0], startMs, endMs));
                        }

                        CurrentMap.SortBookmarks();

                        break;

                    case "Offset":
                        if (oldVer) // back when timing points didnt exist and the offset meant bpm/note offset
                        {
                            if (CurrentMap.TimingPoints.Count > 0 && long.TryParse(split[1], out long bpmOffset))
                                CurrentMap.TimingPoints[0].Ms = bpmOffset;
                        }
                        else
                        {
                            foreach (Note note in CurrentMap.Notes)
                                note.Ms += (long)Settings.exportOffset.Value;

                            if (long.TryParse(split[1], out long offset))
                                Settings.exportOffset.Value = offset;

                            foreach (Note note in CurrentMap.Notes)
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

            if (MainWindow.Instance.CurrentWindow is GuiWindowEditor editor)
                editor.Timeline.GenerateOffsets();

            return true;
        }

        public static bool Read(string path)
        {
            string data = File.ReadAllText(path);

            CurrentMap.TimingPoints.Clear();
            CurrentMap.Bookmarks.Clear();

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
                                CurrentMap.TimingPoints.Add(new(values[0].GetSingle(), values[1].GetInt64()));
                            }

                            break;

                        case "bookmarks":
                            List<JsonElement> bookmarks = value.Deserialize<List<JsonElement>>() ?? [];

                            foreach (JsonElement bookmark in bookmarks)
                            {
                                JsonElement[] values = bookmark.Deserialize<JsonElement[]>() ?? [];
                                CurrentMap.Bookmarks.Add(new(values[0].GetString() ?? "", values[1].GetInt64(), values[^1].GetInt64()));
                            }

                            break;

                        case "vfxObjects":
                            List<string> vfx = value.Deserialize<List<string>>() ?? [];
                            List<MapObject> vfxObjects = [];

                            foreach (string obj in vfx)
                            {
                                MapObject? final = MOParser.Parse(null, obj.Split('|'));
                                if (final != null)
                                    vfxObjects.Add(final);
                            }

                            if (CurrentMap.VfxObjects.Count + vfxObjects.Count > 0)
                                VfxObjectManager.Replace("IMPORT VFX", CurrentMap.VfxObjects, vfxObjects);

                            break;

                        case "specialObjects":
                            List<string> special = value.Deserialize<List<string>>() ?? [];
                            List<MapObject> specialObjects = [];

                            foreach (string obj in special)
                            {
                                MapObject? final = MOParser.Parse(null, obj.Split('|'));
                                if (final != null)
                                    specialObjects.Add(final);
                            }

                            if (CurrentMap.SpecialObjects.Count + specialObjects.Count > 0)
                                SpecialObjectManager.Replace("IMPORT EXTRA", CurrentMap.SpecialObjects, specialObjects);

                            break;

                        case "currentTime":
                            Settings.currentTime.Value.Value = value.GetSingle();
                            break;

                        case "beatDivisor":
                            Settings.beatDivisor.Value.Value = value.GetSingle();
                            break;

                        case "exportOffset":
                            foreach (Note note in CurrentMap.Notes)
                                note.Ms += (long)Settings.exportOffset.Value;

                            Settings.exportOffset.Value = value.GetSingle();

                            foreach (Note note in CurrentMap.Notes)
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

                if (MainWindow.Instance.CurrentWindow is GuiWindowEditor editor)
                    editor.Timeline.GenerateOffsets();

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

            foreach (TimingPoint point in CurrentMap.TimingPoints)
                timingfinal.Add([point.BPM, point.Ms]);

            List<object[]> bookmarkfinal = [];

            foreach (Bookmark bookmark in CurrentMap.Bookmarks)
                bookmarkfinal.Add([bookmark.Text, bookmark.Ms, bookmark.EndMs]);

            List<string> vfxFinal = [];

            foreach (MapObject obj in CurrentMap.VfxObjects)
                vfxFinal.Add($"{obj.ID}|{obj.ToString()}");

            List<string> specialFinal = [];

            foreach (MapObject obj in CurrentMap.SpecialObjects)
                specialFinal.Add($"{obj.ID}|{obj.ToString()}");

            Dictionary<string, object> json = new()
            {
                {"timings", timingfinal },
                {"bookmarks", bookmarkfinal },
                {"vfxObjects", vfxFinal },
                {"specialObjects", specialFinal },

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
