using New_SSQE.Audio;
using New_SSQE.ExternalUtils;
using New_SSQE.Misc.Dialogs;
using New_SSQE.Misc.Static;
using New_SSQE.NewGUI;
using New_SSQE.NewGUI.Windows;
using New_SSQE.NewMaps.Parsing;
using New_SSQE.Preferences;
using System.IO.Compression;

namespace New_SSQE.NewMaps
{
    internal class Mapping
    {
        public static List<Map> Cache = [];
        private static Map _current = new();
        public static Map Current
        {
            get => _current;
            set
            {
                _current.CloseSettings();
                _current = value;
                value.OpenSettings();
                value.SortAll();
            }
        }

        private static long currentAutosave;

        public static void Autosave()
        {
            if (_current.Notes.Count + _current.Bookmarks.Count + _current.TimingPoints.Count > 0)
            {
                if (_current.IsSaved)
                    return;

                if (_current.FileName == null)
                {
                    string autosave = ToCache();
                    if (string.IsNullOrWhiteSpace(autosave))
                        return;

                    string[] data = autosave.Split('\n');
                    Settings.autosavedFile.Value = data[0];
                    Settings.autosavedProperties.Value = data[1];
                    Settings.Save(false);

                    GuiWindowEditor.ShowInfo("AUTOSAVED");
                }
                else if (Save(false, true))
                    GuiWindowEditor.ShowInfo("AUTOSAVED");
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
            }.Show(Settings.audioPath, out string fileName);

            if (result == DialogResult.OK)
            {
                if (string.IsNullOrWhiteSpace(id))
                    id = Path.GetFileNameWithoutExtension(fileName);
                id = FormatUtils.FixID(id);
                _current.SoundID = id;

                if (fileName != Path.Combine(Assets.CACHED, $"{id}.asset"))
                    File.Copy(fileName, Path.Combine(Assets.CACHED, $"{id}.asset"), true);

                return MusicPlayer.Load(Path.Combine(Assets.CACHED, $"{id}.asset"));
            }

            return false;
        }

        public static bool LoadAudio(string id)
        {
            if (id == "-1")
                return false;

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
                        Networking.DownloadFile($"https://assetdelivery.roblox.com/v1/asset/?id={id}", Path.Combine(Assets.CACHED, $"{id}.asset"), FileSource.Roblox);
                }

                return MusicPlayer.Load(Path.Combine(Assets.CACHED, $"{id}.asset"));
            }
            catch (Exception e)
            {
                Logging.Log("Failed to load audio", LogSeverity.WARN, e);
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
            Current = new();

            try
            {
                string[] data = cache.Split('\n');

                File.WriteAllText(tempCacheTXT, data[0]);
                File.WriteAllText(tempCacheINI, data[1]);
                string fileName = data[2][1..^1];

                if (TXT.Read(tempCacheTXT))
                {
                    if (!string.IsNullOrWhiteSpace(fileName))
                        _current.FileName = fileName;
                    return _current;
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Failed to load map from cache", LogSeverity.WARN, ex);
            }

            return null;
        }

        public static void SaveCache()
        {
            Map old = _current;

            List<string> data = [];
            foreach (Map map in Cache)
            {
                Current = map;
                data.Add(ToCache());
            }

            File.WriteAllText(cacheFile, string.Join("\n\n", data));
            Current = old;
        }

        public static string ToCache()
        {
            if (TXT.Write(tempCacheTXT))
                return $"{File.ReadAllText(tempCacheTXT)}\n{File.ReadAllText(tempCacheINI)}\n[{_current.FileName}]";

            return "";
        }

        public static void CacheCurrent()
        {
            if (!Cache.Contains(_current))
                Cache.Add(_current);
        }



        public static bool Open()
        {
            if (!LoadAudio(_current.SoundID))
                return false;

            MusicPlayer.Volume = Settings.masterVolume.Value.Value;
            Settings.currentTime.Value.Max = (float)MusicPlayer.TotalTime.TotalMilliseconds;
            Settings.currentTime.Value.Step = (float)MusicPlayer.TotalTime.TotalMilliseconds / 2000f;

            CacheCurrent();
            Map old = _current;
            SaveCache();
            old.OpenSettings();

            Windowing.Open<GuiWindowEditor>();
            return Windowing.Current is GuiWindowEditor && _current.SoundID != "-1";
        }

        public static bool Close()
        {
            _current.CloseSettings();
            
            if (!_current.IsSaved)
            {
                DialogResult result = MessageBox.Show($"{_current.FileID} ({_current.SoundID})\n\nWould you like to save before closing?", MBoxIcon.Warning, MBoxButtons.Yes_No_Cancel);

                if (result == DialogResult.Cancel)
                    return false;
                if (result == DialogResult.Yes)
                    Save();
            }

            Cache.Remove(_current);
            SaveCache();
            Windowing.Open<GuiWindowMenu>();

            return true;
        }

        public static bool Close(Map map)
        {
            Current = map;
            return Close();
        }

        public static bool Quit()
        {
            bool quit = true;

            foreach (Map map in Cache.ToArray())
            {
                Current = map;
                Windowing.Open<GuiWindowEditor>();
                MainWindow.Instance.ForceRender();

                quit &= Close();

                if (!quit)
                    break;
            }

            return quit;
        }

        public static bool Save(bool forced = true, bool fromAutosave = false)
        {
            if (forced && (!File.Exists(_current.FileName) || string.IsNullOrWhiteSpace(_current.FileName)))
                return SaveAs();

            try
            {
                if (string.IsNullOrWhiteSpace(_current.FileName))
                    return false;
                return fromAutosave ? TXT.WriteAutosave(_current.FileName) : TXT.WriteForced(_current.FileName);
            }
            catch (Exception ex)
            {
                Logging.Log($"Failed to save map - {_current.FileName}", LogSeverity.WARN, ex);
                return false;
            }
        }

        public static bool SaveAs()
        {
            DialogResult result = new SaveFileDialog()
            {
                Title = "Save Map As",
                Filter = "Text Documents(*.txt)|*.txt"
            }.Show(Settings.defaultPath, out string path);

            if (result == DialogResult.OK)
            {
                _current.FileName = path;
                return Save(false);
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
                    ".txt" => TXT.WriteForced(path),
                    ".sspm" => SSPM.Write(path),
                    ".npk" => NPK.Write(path),

                    _ => throw new NotSupportedException($"File extension not supported for saving: {extension}")
                };
            }
            catch (Exception ex)
            {
                Logging.Log($"Failed to save map - {path}", LogSeverity.WARN, ex);
                return false;
            }
        }

        private static string Unzip(string path)
        {
            string extension = Path.GetExtension(path);
            string directory = Path.Combine(Assets.TEMP, extension);

            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
            Directory.CreateDirectory(directory);
            
            ZipFile.ExtractToDirectory(path, directory);
            return directory;
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
                        string npk = Unzip(data);
                        data = Path.Combine(npk, "chart.nch");
                    }

                    if (extension == ".phz")
                    {
                        string phz = Unzip(data);
                        data = Directory.GetFiles(phz).FirstOrDefault() ?? "";
                        extension = Path.GetExtension(data);
                    }

                    if (extension == ".rhym")
                        data = Unzip(data);
                    if (extension == ".phxm")
                        data = Unzip(data);

                    return extension switch
                    {
                        ".txt" => TXT.Read(data),
                        ".sspm" => SSPM.Read(data),
                        ".phxm" => PHXM.Read(data),
                        ".npk" or ".nch" => NPK.Read(data),
                        ".osu" => OSU.Read(data),
                        ".qem" => QEM.Read(data),
                        ".json" when PHZ.IsValid(data) => PHZ.Read(data),

                        _ => throw new NotSupportedException($"File extension not supported for loading: {extension}")
                    };
                }
                else
                {
                    try
                    {
                        while (true)
                            data = Networking.DownloadString(data);
                    }
                    catch { }

                    return TXT.ReadData(data);
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Failed to load map", LogSeverity.WARN, ex);
                return false;
            }
        }

        public static bool Load(string data)
        {
            foreach (Map map in Cache)
            {
                if (map.FileName == data)
                {
                    Current = map;
                    return Open();
                }
            }

            try
            {
                Current = new();

                if (!Parse(data))
                {
                    MessageBox.Show("Failed to load map data\n\nExit and check '*\\logs.txt' for more info", MBoxIcon.Warning, MBoxButtons.OK);
                    return false;
                }

                if (File.Exists(data) && Path.GetExtension(data) == ".txt")
                    _current.FileName = data;
                return Open();
            }
            catch (Exception ex)
            {
                Logging.Log("Failed to load map data", LogSeverity.WARN, ex);
                MessageBox.Show($"Failed to load map data: {ex.Message}\n\nExit and check '*\\logs.txt' for more info", MBoxIcon.Warning, MBoxButtons.OK);
                return false;
            }
        }
    }
}
