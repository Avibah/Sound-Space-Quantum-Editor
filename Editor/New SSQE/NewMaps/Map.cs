using New_SSQE.Audio;
using New_SSQE.EditHistory;
using New_SSQE.ExternalUtils;
using New_SSQE.GUI;
using New_SSQE.Misc.Dialogs;
using New_SSQE.Misc.Static;
using New_SSQE.NewMaps.Parsing;
using New_SSQE.Objects;
using New_SSQE.Objects.Managers;
using New_SSQE.Objects.Other;
using New_SSQE.Preferences;
using System.IO.Compression;

namespace New_SSQE.NewMaps
{
    internal class Map
    {
        public ObjectList<Note> Notes = [];
        public ObjectList<MapObject> VfxObjects = [];
        public ObjectList<MapObject> SpecialObjects = [];

        public List<Note> BezierNodes = [];
        public List<Bookmark> Bookmarks = [];
        public List<TimingPoint> TimingPoints = [];

        public static TimingPoint? SelectedPoint;
        public static MapObject? SelectedObjDuration;

        public string? FileName;
        public string SoundID = "-1";
        public string FileID => Path.GetFileNameWithoutExtension(FileName) ?? SoundID;
        public bool IsSaved => FileName != null && File.Exists(FileName) && File.ReadAllText(FileName) == ToString();

        private float _tempo = Settings.tempo.Value.Default;
        public float Tempo
        {
            get => Math.Min(_tempo, 0.9f) + Math.Max(_tempo - 0.9f, 0) * 2 + 0.1f;
            set
            {
                _tempo = value;
                MusicPlayer.Tempo = Tempo;
            }
        }
        public float Zoom = 1f;

        private float currentTime;
        private float beatDivisor;

        private URAction[] urActions = [];
        private int urActionIndex;

        private readonly SettingBase[] _baseSettings =
        [
            Settings.exportOffset,
            Settings.mappers, Settings.songName, Settings.difficulty, Settings.useCover, Settings.cover,
            Settings.customDifficulty, Settings.rating, Settings.useVideo, Settings.video, Settings.songOffset,
            Settings.songTitle, Settings.songArtist, Settings.mapCreator, Settings.mapCreatorPersonalLink,
            Settings.previewStartTime, Settings.previewDuration, Settings.novaCover, Settings.novaIcon
        ];

        private readonly Dictionary<string, object> settings = [];

        public Map()
        {
            foreach (SettingBase setting in _baseSettings)
                settings.Add(setting.Name, setting.GetDefault());
        }

        public bool Open(bool loadAudio = true)
        {
            CurrentMap.Map?.Close();
            CurrentMap.Map = this;

            foreach (SettingBase setting in _baseSettings)
                setting.SetValue(settings[setting.Name]);

            if (loadAudio)
            {
                if (!MapManager.LoadAudio(SoundID))
                    return false;        
                MusicPlayer.Volume = Settings.masterVolume.Value.Value;
                Settings.currentTime.Value.Max = (float)MusicPlayer.TotalTime.TotalMilliseconds;
                Settings.currentTime.Value.Step = (float)MusicPlayer.TotalTime.TotalMilliseconds / 2000f;
            }

            Settings.tempo.Value.Value = _tempo;
            Settings.currentTime.Value.Value = currentTime;
            Settings.beatDivisor.Value.Value = beatDivisor;

            UndoRedoManager.ResetActions(urActions, urActionIndex);

            return true;
        }

        public void Close()
        {
            SortTimings();
            SortBookmarks();

            foreach (SettingBase setting in _baseSettings)
                settings[setting.Name] = setting.GetValue();

            _tempo = Settings.tempo.Value.Value;
            currentTime = Settings.currentTime.Value.Value;
            beatDivisor = Settings.beatDivisor.Value.Value;

            urActions = UndoRedoManager.actions.ToArray();
            urActionIndex = UndoRedoManager._index;

            UndoRedoManager.Clear();
            MusicPlayer.Reset();
        }

        public bool Save(string path)
        {
            Open(false);

            if (string.IsNullOrWhiteSpace(path))
            {
                DialogResult result = new SaveFileDialog()
                {
                    Title = "Save Map As",
                    Filter = "Text Documents(*.txt)|*.txt"
                }.RunWithSetting(Settings.defaultPath, out string newPath);

                if (result == DialogResult.OK)
                    FileName = newPath;
                else
                    return false;
            }

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

        public bool Save() => Save(FileName ?? "");

        public bool SaveAs()
        {
            string? old = FileName;
            FileName = null;

            bool result = Save();

            if (!result)
                FileName = old;

            return result;
        }

        public bool Load(string data)
        {
            Open(false);

            try
            {
                if (File.Exists(data))
                {
                    Settings.lastFile.Value = data;
                    string extension = Path.GetExtension(data);

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



        public void SortTimings(bool updateList = true)
        {
            TimingPoints = new(TimingPoints.OrderBy(n => n.Ms));

            if (updateList)
                TimingsWindow.Instance?.ResetList();

            if (MainWindow.Instance.CurrentWindow is GuiWindowEditor editor)
                editor.Timeline.GenerateOffsets();
        }

        public void SortBookmarks(bool updateList = true)
        {
            Bookmarks = new(Bookmarks.OrderBy(n => n.Ms));

            if (updateList)
                BookmarksWindow.Instance?.ResetList();

            if (MainWindow.Instance.CurrentWindow is GuiWindowEditor editor)
                editor.Timeline.GenerateOffsets();
        }



        public string ToCache()
        {
            Open(false);

            string txt = Path.Combine(Assets.TEMP, "tempcache.txt");
            string ini = Path.ChangeExtension(txt, ".ini");

            if (TXT.Write(txt))
                return $"{File.ReadAllText(txt)}\n{File.ReadAllText(ini)}";

            return "";
        }

        public bool FromCache(string cache)
        {
            Open(false);

            string txt = Path.Combine(Assets.TEMP, "tempcache.txt");
            string ini = Path.ChangeExtension(txt, ".ini");
            string[] data = cache.Split('\n');

            File.WriteAllText(txt, data[0]);
            File.WriteAllText(ini, data[1]);

            try
            {
                return TXT.Read(txt);
            }
            catch (Exception ex)
            {
                Logging.Register("Failed to load map from cache", LogSeverity.WARN, ex);
            }

            return false;
        }
    }
}
