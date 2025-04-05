using New_SSQE.Audio;
using New_SSQE.ExternalUtils;
using New_SSQE.Misc.Dialogs;
using New_SSQE.Misc.Static;
using New_SSQE.NewGUI;
using New_SSQE.NewGUI.Windows;
using New_SSQE.NewMaps.Parsing;
using New_SSQE.Objects;
using New_SSQE.Objects.Managers;
using New_SSQE.Objects.Other;
using New_SSQE.Preferences;
using OpenTK.Mathematics;
using System.Drawing;
using System.IO.Compression;

namespace New_SSQE.NewMaps
{
    internal enum ObjectRenderMode
    {
        Notes,
        Special,
        VFX
    }

    internal enum IndividualObjectMode
    {
        Disabled,

        Beat = 12,
        Glide = 13,
        Mine = 14
    }

    internal enum ClickMode
    {
        Select = 1,
        Place = 2,
        Both = 3
    }

    internal class Mapping
    {
        public static List<Map> Cache = [];
        public static Map Current { get; private set; } = new();

        public static ObjectRenderMode RenderMode = ObjectRenderMode.Notes;
        public static IndividualObjectMode ObjectMode = IndividualObjectMode.Disabled;

        public static ClickMode ClickMode => Settings.separateClickTools.Value ? (Settings.selectTool.Value ? ClickMode.Select : ClickMode.Place) : ClickMode.Both;

        public static TimingPoint? SelectedPoint = null;

        public static List<MapObject> GetSelected()
        {
            return RenderMode switch
            {
                ObjectRenderMode.Notes => Current.Notes.Selected.Cast<MapObject>().ToList(),
                ObjectRenderMode.VFX => Current.VfxObjects.Selected,
                ObjectRenderMode.Special => Current.SpecialObjects.Selected,
                _ => []
            };
        }

        public static void SetSelected(List<MapObject> selected)
        {
            switch (RenderMode)
            {
                case ObjectRenderMode.Notes:
                    Current.Notes.Selected = new(selected.Cast<Note>());
                    break;
                case ObjectRenderMode.VFX:
                    Current.VfxObjects.Selected = new(selected);
                    break;
                case ObjectRenderMode.Special:
                    Current.SpecialObjects.Selected = new(selected);
                    break;
            }
        }

        public static void SetSelected(TimingPoint? selected = null)
        {
            SelectedPoint = selected;
        }

        public static void ClearSelection()
        {
            Current.Notes.ClearSelection();
            Current.VfxObjects.ClearSelection();
            Current.SpecialObjects.ClearSelection();
            SelectedPoint = null;
        }

        public static void SortAll(bool updateLists = true)
        {
            Current.Notes.Sort();
            Current.VfxObjects.Sort();
            Current.SpecialObjects.Sort();
            SortTimings(updateLists);
            SortBookmarks(updateLists);
        }

        public static void SortObjects()
        {
            switch (RenderMode)
            {
                case ObjectRenderMode.Notes:
                    Current.Notes.Sort();
                    break;
                case ObjectRenderMode.VFX:
                    Current.VfxObjects.Sort();
                    break;
                case ObjectRenderMode.Special:
                    Current.SpecialObjects.Sort();
                    break;
            }
        }

        public static void SortTimings(bool updateList = true)
        {
            Current.TimingPoints = new(Current.TimingPoints.OrderBy(n => n.Ms));

            if (updateList)
                TimingsWindow.Instance?.ResetList();
            GuiWindowEditor.Timeline.Update();
        }

        public static void SortBookmarks(bool updateList = true)
        {
            Current.Bookmarks = new(Current.Bookmarks.OrderBy(n => n.Ms));

            if (updateList)
                BookmarksWindow.Instance?.ResetList();
            GuiWindowEditor.Timeline.Update();
        }

        public static List<MapObject> GetObjectsInRange(float start, float end)
        {
            int low, high;

            switch (RenderMode)
            {
                case ObjectRenderMode.Notes:
                    (low, high) = Current.Notes.SearchRange(start, end);
                    return Current.Notes.Take(new Range(low, high)).Cast<MapObject>().ToList();

                case ObjectRenderMode.VFX:
                    (low, high) = Current.VfxObjects.SearchRange(start, end);
                    return Current.VfxObjects.Take(new Range(low, high)).ToList();

                case ObjectRenderMode.Special:
                    (low, high) = Current.SpecialObjects.SearchRange(start, end);
                    return Current.SpecialObjects.Take(new Range(low, high)).ToList();
            }

            return [];
        }


        public static void IncrementZoom(float increment)
        {
            float zoom = Current.Zoom;
            float step = zoom < 0.1f || (zoom == 0.1f && increment < 0) ? 0.01f : 0.1f;

            zoom = (float)Math.Round(zoom + increment * step, 2);
            if (zoom > 0.1f)
                zoom = (float)Math.Round(zoom * 10) / 10;

            Current.Zoom = MathHelper.Clamp(zoom, 0.01f, 10f);
        }

        public static void CopyBookmarks()
        {
            string[] data = new string[Current.Bookmarks.Count];

            for (int i = 0; i < Current.Bookmarks.Count; i++)
            {
                Bookmark bookmark = Current.Bookmarks[i];

                if (bookmark.Ms != bookmark.EndMs)
                    data[i] = $"{bookmark.Ms}-{bookmark.EndMs} ~ {bookmark.Text.Replace(" ~", "_~")}";
                else
                    data[i] = $"{bookmark.Ms} ~ {bookmark.Text.Replace(" ~", "_~")}";
            }

            if (data.Length == 0)
                return;

            Clipboard.SetText(string.Join("\n", data));
            GuiWindowEditor.ShowToast("COPIED TO CLIPBOARD", Color.FromArgb(0, 255, 200));
        }

        public static void PasteBookmarks()
        {
            string data = Clipboard.GetText();
            string[] bookmarks = data.Split('\n');

            List<Bookmark> tempBookmarks = [];

            for (int i = 0; i < bookmarks.Length; i++)
            {
                bookmarks[i] = bookmarks[i].Trim();

                string[] split = bookmarks[i].Split(" ~");
                if (split.Length != 2)
                    continue;

                string[] subsplit = split[0].Split("-");

                if (subsplit.Length == 1 && long.TryParse(subsplit[0], out long ms))
                    tempBookmarks.Add(new Bookmark(split[1].Trim().Replace("_~", " ~"), ms, ms));
                else if (subsplit.Length == 2 && long.TryParse(subsplit[0], out long startMs) && long.TryParse(subsplit[1], out long endMs))
                    tempBookmarks.Add(new Bookmark(split[1].Trim().Replace("_~", " ~"), startMs, endMs));
            }

            if (tempBookmarks.Count > 0)
                BookmarkManager.Replace("PASTE BOOKMARK[S]", Current.Bookmarks, tempBookmarks);
        }



        private static long currentAutosave;

        public static void Autosave()
        {
            if (Current.Notes.Count + Current.Bookmarks.Count + Current.TimingPoints.Count > 0)
            {
                if (Current.IsSaved)
                    return;

                if (Current.FileName == null)
                {
                    string autosave = ToCache();
                    if (string.IsNullOrWhiteSpace(autosave))
                        return;

                    string[] data = autosave.Split('\n');
                    Settings.autosavedFile.Value = data[0];
                    Settings.autosavedProperties.Value = data[1];
                    Settings.Save(false);

                    GuiWindowEditor.ShowToast("AUTOSAVED", Settings.color1.Value);
                }
                else if (Save(false))
                    GuiWindowEditor.ShowToast("AUTOSAVED", Settings.color1.Value);
            }
        }

        public static void StartAutosaving()
        {
            long time = DateTime.Now.Ticks;

            if (Settings.enableAutosave.Value)
            {
                currentAutosave = time;

                Task.Run(() =>
                {
                    while (currentAutosave == time)
                    {
                        Thread.Sleep((int)(Settings.autosaveInterval.Value * 60000f));
                        if (currentAutosave == time)
                            Autosave();
                    }
                });
            }
        }



        public static bool ImportAudio(string id)
        {
            DialogResult result = new OpenFileDialog()
            {
                Title = "Select Audio File",
                Filter = $"Audio Files ({MusicPlayer.SupportedExtensionsString})|{MusicPlayer.SupportedExtensionsString}"
            }.RunWithSetting(Settings.audioPath, out string fileName);

            if (result == DialogResult.OK)
            {
                if (string.IsNullOrWhiteSpace(id))
                    id = Path.GetFileNameWithoutExtension(fileName);
                id = FormatUtils.FixID(id);

                if (fileName != Path.Combine(Assets.CACHED, $"{id}.asset"))
                    File.Copy(fileName, Path.Combine(Assets.CACHED, $"{id}.asset"), true);

                return MusicPlayer.Load(Path.Combine(Assets.CACHED, $"{id}.asset"));
            }

            return false;
        }

        public static bool LoadAudio(string id)
        {
            try
            {
                if (!File.Exists(Path.Combine(Assets.CACHED, $"{id}.asset")))
                {
                    if (Settings.skipDownload.Value)
                    {
                        DialogResult message = MessageBox.Show($"No asset with id '{id}' is present in cache.\n\nWould you like to import a file with this id?", MBoxIcon.Warning, MBoxButtons.OK_Cancel);

                        return message == DialogResult.OK && ImportAudio(id);
                    }
                    else
                        WebClient.DownloadFile($"https://assetdelivery.roblox.com/v1/asset/?id={id}", Path.Combine(Assets.CACHED, $"{id}.asset"), FileSource.Roblox);
                }

                return MusicPlayer.Load(Path.Combine(Assets.CACHED, $"{id}.asset"));
            }
            catch (Exception e)
            {
                Logging.Register("Failed to load audio", LogSeverity.WARN, e);
                DialogResult message = MessageBox.Show($"Failed to download asset with id '{id}':\n\n{e.Message}\n\nWould you like to import a file with this id instead?", MBoxIcon.Error, MBoxButtons.OK_Cancel);

                if (message == DialogResult.OK)
                    return ImportAudio(id);
            }

            return false;
        }



        private static readonly string cacheFile = Path.Combine(Assets.TEMP, "cache.txt");
        private static readonly string tempCacheTXT = Path.Combine(Assets.TEMP, "tempcache.txt");
        private static readonly string tempCacheINI = Path.ChangeExtension(tempCacheTXT, ".ini");

        public static void LoadCache()
        {
            if (File.Exists(cacheFile))
            {
                Cache.Clear();
                string[] data = File.ReadAllText(cacheFile).Split("\n\n");

                for (int i = 0; i < data.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(data[i]))
                        continue;
                    Map? map = FromCache(data[i]);

                    if (map != null)
                        Cache.Add(map);
                }
            }
        }

        public static Map? FromCache(string cache)
        {
            Current.CloseSettings();
            Current = new();

            string[] data = cache.Split('\n');

            File.WriteAllText(tempCacheTXT, data[0]);
            File.WriteAllText(tempCacheINI, data[1]);

            try
            {
                if (TXT.Read(tempCacheTXT))
                    return Current;
            }
            catch (Exception ex)
            {
                Logging.Register("Failed to load map from cache", LogSeverity.WARN, ex);
            }

            return null;
        }

        public static void SaveCache()
        {
            List<string> data = [];
            foreach (Map map in Cache)
            {
                Open(map, false);
                data.Add(ToCache());
            }

            File.WriteAllText(cacheFile, string.Join("\n\n", data));
        }

        public static string ToCache()
        {
            if (TXT.Write(tempCacheTXT))
                return $"{File.ReadAllText(tempCacheTXT)}\n{File.ReadAllText(tempCacheINI)}";

            return "";
        }



        public static bool Open(Map map, bool loadAudio = true)
        {
            Current.CloseSettings();
            Current = map;
            map.OpenSettings();

            SortAll();

            if (loadAudio)
            {
                if (!LoadAudio(map.SoundID))
                    return false;

                MusicPlayer.Volume = Settings.masterVolume.Value.Value;
                Settings.currentTime.Value.Max = (float)MusicPlayer.TotalTime.TotalMilliseconds;
                Settings.currentTime.Value.Step = (float)MusicPlayer.TotalTime.TotalMilliseconds / 2000f;
            }

            if (!Cache.Contains(map))
            {
                Cache.Add(map);
                SaveCache();
            }

            Windowing.SwitchWindow(new GuiWindowEditor());
            return Windowing.Current is GuiWindowEditor && Current.SoundID != "-1";
        }

        public static bool Close()
        {
            Current.CloseSettings();
            
            if (!Current.IsSaved)
            {
                DialogResult result = MessageBox.Show($"{Current.FileID} ({Current.SoundID})\n\nWould you like to save before closing?", MBoxIcon.Warning, MBoxButtons.Yes_No_Cancel);

                if (result == DialogResult.Cancel)
                    return false;
                if (result == DialogResult.Yes)
                    Save();
            }

            Cache.Remove(Current);
            SaveCache();
            Windowing.SwitchWindow(new GuiWindowMenu());

            return true;
        }

        public static bool Close(Map map)
        {
            Open(map, false);
            return Close();
        }

        public static bool Quit()
        {
            bool quit = true;

            foreach (Map map in Cache.ToArray())
            {
                quit &= Close(map);

                if (!quit)
                    break;
            }

            return quit;
        }

        public static bool Save(bool forced = true)
        {
            if (forced && (!File.Exists(Current.FileName) || string.IsNullOrWhiteSpace(Current.FileName)))
                return SaveAs();

            try
            {
                return TXT.Write(Current.FileName ?? "");
            }
            catch (Exception ex)
            {
                Logging.Register($"Failed to save map - {Current.FileName}", LogSeverity.WARN, ex);
                return false;
            }
        }

        public static bool SaveAs()
        {
            DialogResult result = new SaveFileDialog()
            {
                Title = "Save Map As",
                Filter = "Text Documents(*.txt)|*.txt"
            }.RunWithSetting(Settings.defaultPath, out string path);

            if (result == DialogResult.OK)
            {
                Current.FileName = path;
                return Save();
            }

            return false;
        }

        public static bool Export(string path)
        {
            try
            {
                string extension = Path.GetExtension(path);

                return extension switch
                {
                    ".txt" => TXT.Write(path),
                    ".sspm" => SSPM.Write(path),
                    ".rhym" => RHYM.Write(path),
                    ".npk" => NPK.Write(path),

                    _ => throw new NotSupportedException($"File extension not supported for saving: {extension}")
                };
            }
            catch (Exception ex)
            {
                Logging.Register($"Failed to save map - {path}", LogSeverity.WARN, ex);
                return false;
            }
        }

        public static bool Parse(string data)
        {
            try
            {
                if (File.Exists(data))
                {
                    string extension = Path.GetExtension(data);
                    if (extension == ".txt")
                        Settings.lastFile.Value = data;

                    if (extension == ".npk")
                    {
                        string npk = Path.Combine(Assets.TEMP, "nova");
                        Directory.CreateDirectory(npk);
                        foreach (string temp in Directory.GetFiles(npk))
                            File.Delete(temp);
                        ZipFile.ExtractToDirectory(data, npk);

                        data = Path.Combine(npk, "chart.nch");
                    }

                    if (extension == ".phz")
                    {
                        string phz = Path.Combine(Assets.TEMP, "pulsus");
                        Directory.CreateDirectory(phz);
                        foreach (string temp in Directory.GetFiles(phz))
                            File.Delete(temp);
                        ZipFile.ExtractToDirectory(data, phz);

                        data = Directory.GetFiles(phz).FirstOrDefault() ?? "";
                    }

                    extension = Path.GetExtension(data);

                    return extension switch
                    {
                        ".txt" => TXT.Read(data),
                        ".sspm" => SSPM.Read(data),
                        ".phxm" => PHXM.Read(data),
                        ".nch" => NPK.Read(data),
                        ".osu" => OSU.Read(data),
                        ".json" when PHZ.IsValid(data) => PHZ.Read(data),

                        _ => throw new NotSupportedException($"File extension not supported for loading: {extension}")
                    };
                }
                else
                {
                    try
                    {
                        while (true)
                            data = WebClient.DownloadString(data);
                    }
                    catch { }

                    return TXT.ReadData(data);
                }
            }
            catch (Exception ex)
            {
                Logging.Register("Failed to load map", LogSeverity.WARN, ex);
                return false;
            }
        }

        public static bool Load(string data)
        {
            foreach (Map map in Cache)
            {
                if (map.FileName == data)
                    return Open(map);
            }

            try
            {
                Current.CloseSettings();
                Current = new();

                if (!Parse(data))
                {
                    MessageBox.Show("Failed to load map data\n\nExit and check '*\\logs.txt' for more info", MBoxIcon.Warning, MBoxButtons.OK);
                    return false;
                }

                return Open(Current);
            }
            catch (Exception ex)
            {
                Logging.Register("Failed to load map data", LogSeverity.WARN, ex);
                MessageBox.Show($"Failed to load map data: {ex.Message}\n\nExit and check '*\\logs.txt' for more info", MBoxIcon.Warning, MBoxButtons.OK);
                return false;
            }
        }
    }
}
