using New_SSQE.Audio;
using New_SSQE.Misc;
using New_SSQE.NewGUI;
using New_SSQE.NewGUI.Dialogs;
using New_SSQE.NewGUI.Windows;
using New_SSQE.NewMaps.Parsing;
using New_SSQE.Preferences;
using New_SSQE.Services;
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
                Filter = $"Audio Files ({SoundEngine.SupportedExtensionsString})|{SoundEngine.SupportedExtensionsString}"
            }.Show(Settings.audioPath, out string fileName);

            if (result == DialogResult.OK)
            {
                if (string.IsNullOrWhiteSpace(id))
                    id = Path.GetFileNameWithoutExtension(fileName);
                id = FormatUtils.FixID(id);
                _current.SoundID = id;

                if (fileName != Assets.CachedAt($"{id}.asset"))
                    File.Copy(fileName, Assets.CachedAt($"{id}.asset"), true);

                return MusicPlayer.Load(Assets.CachedAt($"{id}.asset"));
            }

            return false;
        }



        private static readonly string cacheFile = Assets.TempAt("cache.txt");
        private static readonly string tempCacheTXT = Assets.TempAt("tempcache.txt");
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
            string id = _current.SoundID;
            if (id == "-1" || string.IsNullOrWhiteSpace(id))
                return false;

            static bool FinishLoad()
            {
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

            try
            {
                if (!File.Exists(Assets.CachedAt($"{id}.asset")))
                {
                    if (Settings.skipDownload.Value)
                    {
                        MessageDialog.Show($"No audio found for ID '{id}'.\n\nWould you like to import an audio file for this ID?", MBoxIcon.Warning, MBoxButtons.OK_Cancel, (result) =>
                        {
                            if (result == DialogResult.OK)
                            {
                                if (ImportAudio(id))
                                    FinishLoad();
                            }
                        });
                    }
                    else
                        Network.DownloadFile($"https://assetdelivery.roblox.com/v1/asset/?id={id}", Assets.CachedAt($"{id}.asset"), FileSource.Roblox);
                }
                else
                {
                    bool loaded = MusicPlayer.Load(Assets.CachedAt($"{id}.asset"));
                    return loaded && FinishLoad();
                }
            }
            catch (Exception e)
            {
                Logging.Log("Failed to load audio", LogSeverity.WARN, e);

                MessageDialog.Show($"Failed to load audio with ID '{id}':\n\n{e.Message}\n\nWould you like to import a new file?", MBoxIcon.Error, MBoxButtons.OK_Cancel, (result) =>
                {
                    if (result == DialogResult.OK)
                    {
                        if (ImportAudio(id))
                            FinishLoad();
                    }
                });
            }

            return true;
        }

        public static bool Close(Action? onClose = null)
        {
            _current.CloseSettings();
            bool saved = _current.IsSaved;

            if (!saved)
            {
                MessageDialog.Show($"{_current.FileID} ({_current.SoundID})\n\nWould you like to save before closing?", MBoxIcon.Warning, MBoxButtons.Yes_No_Cancel, (result) =>
                {
                    if (result == DialogResult.Yes)
                        Save();

                    if (result == DialogResult.Cancel)
                    {
                        Windowing.Open<GuiWindowMenu>();
                        return;
                    }

                    Cache.Remove(_current);
                    SaveCache();

                    Windowing.Open<GuiWindowMenu>();
                    onClose?.Invoke();
                });
            }
            else
            {
                Windowing.Open<GuiWindowMenu>();
                Cache.Remove(_current);
                SaveCache();
            }

            return saved;
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

                quit &= Close(MainWindow.Instance.Close);
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
            string directory = Assets.TempAt(extension);

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
                        ".qemz" => QEMZ.Read(data),
                        ".json" when PHZ.IsValid(data) => PHZ.Read(data),

                        _ => throw new NotSupportedException($"File extension not supported for loading: {extension}")
                    };
                }
                else
                {
                    try
                    {
                        while (true)
                            data = Network.DownloadString(data);
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
                    throw new FormatException("Failed to parse data");

                if (File.Exists(data) && Path.GetExtension(data) == ".txt")
                    _current.FileName = data;
                return Open();
            }
            catch (Exception ex)
            {
                Logging.Log("Failed to load map data", LogSeverity.WARN, ex);
                MessageDialog.Show($"Failed to load map data: {ex.Message}\n\nExit and check '*\\logs.txt' for more info", MBoxIcon.Warning, MBoxButtons.OK);
                return false;
            }
        }
    }
}
