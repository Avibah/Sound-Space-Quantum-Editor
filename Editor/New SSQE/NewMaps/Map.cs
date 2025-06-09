using New_SSQE.Audio;
using New_SSQE.EditHistory;
using New_SSQE.NewMaps.Parsing;
using New_SSQE.Objects;
using New_SSQE.Objects.Managers;
using New_SSQE.Objects.Other;
using New_SSQE.Preferences;

namespace New_SSQE.NewMaps
{
    internal class Map
    {
        public ObjectList<Note> Notes = [];
        public ObjectList<MapObject> VfxObjects = [];
        public ObjectList<MapObject> SpecialObjects = [];

        public List<int> BezierNodes = [];
        public List<Bookmark> Bookmarks = [];
        public List<TimingPoint> TimingPoints = [];

        public static TimingPoint? SelectedPoint;
        public static MapObject? SelectedObjDuration;

        public string? FileName;
        public string SoundID = "-1";
        public string FileID => Path.GetFileNameWithoutExtension(FileName) ?? SoundID;

        public bool IsSaved
        {
            get
            {
                if (FileName != null && File.Exists(FileName))
                {
                    Map old = Mapping.Current;
                    Mapping.Current = this;

                    bool saved = File.ReadAllText(FileName) == TXT.Copy();
                    Mapping.Current = old;
                    return saved;
                }

                return false;
            }
        }

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

        public void OpenSettings()
        {
            foreach (SettingBase setting in _baseSettings)
                setting.SetValue(settings[setting.Name]);

            Settings.tempo.Value.Value = _tempo;
            Settings.currentTime.Value.Value = currentTime;
            Settings.beatDivisor.Value.Value = beatDivisor;
            MusicPlayer.Tempo = Tempo;

            UndoRedoManager.ResetActions(urActions, urActionIndex);
        }

        public void CloseSettings()
        {
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
    }
}
