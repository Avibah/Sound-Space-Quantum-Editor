using New_SSQE.Audio;
using New_SSQE.EditHistory;
using New_SSQE.NewGUI;
using New_SSQE.NewGUI.Windows;
using New_SSQE.NewMaps.Parsing;
using New_SSQE.Objects;
using New_SSQE.Objects.Managers;
using New_SSQE.Objects.Other;
using New_SSQE.Preferences;
using New_SSQE.Services;

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
        Note,

        Beat = 12,
        Glide = 13,
        Mine = 14,
        Lyric = 15,
        Fever = 16
    }

    internal class Map
    {
        public ObjectList<Note> Notes = [];
        public ObjectList<MapObject> VfxObjects = [];
        public ObjectList<MapObject> SpecialObjects = [];

        public List<int> BezierNodes => Notes.BezierNodes;
        public List<Bookmark> Bookmarks = [];
        public List<TimingPoint> TimingPoints = [];

        public TimingPoint? SelectedPoint;
        public MapObject? SelectedObjDuration;

        public string? FileName;
        public string SoundID = "-1";
        public string FileID => Path.GetFileNameWithoutExtension(FileName) ?? SoundID;

        public bool IsSaved
        {
            get
            {
                if (FileName != null && File.Exists(FileName))
                {
                    bool isOpen = Mapping.Current == this;
                    Map? old = null;

                    if (!isOpen)
                    {
                        old = Mapping.Current;
                        Mapping.Current = this;
                    }

                    bool saved = File.ReadAllText(FileName) == TXT.Copy();

                    if (!isOpen && old != null)
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
        private int urActionIndex = -1;

        private readonly SettingBase[] _baseSettings =
        [
            Settings.exportOffset,
            Settings.mappers, Settings.songName, Settings.difficulty, Settings.useCover, Settings.cover,
            Settings.customDifficulty, Settings.rating, Settings.useVideo, Settings.video, Settings.songOffset,
            Settings.songTitle, Settings.songArtist, Settings.mapCreator, Settings.mapCreatorPersonalLink,
            Settings.previewStartTime, Settings.previewDuration, Settings.novaCover, Settings.novaIcon,
            Settings.romanizedArtist, Settings.romanizedTitle, Settings.colorsetPath
        ];

        private readonly Dictionary<string, object> settings = [];

        public List<MapObject> SelectedObjects
        {
            get
            {
                return RenderMode switch
                {
                    ObjectRenderMode.Notes => [.. Notes.Selected.Cast<MapObject>()],
                    ObjectRenderMode.VFX => VfxObjects.Selected,
                    ObjectRenderMode.Special when ObjectMode == IndividualObjectMode.Note => [.. Notes.Selected.Cast<MapObject>()],
                    ObjectRenderMode.Special => SpecialObjects.Selected,
                    _ => []
                };
            }

            set
            {
                switch (RenderMode)
                {
                    case ObjectRenderMode.Notes:
                        Notes.Selected = [.. value.Cast<Note>()];
                        break;
                    case ObjectRenderMode.VFX:
                        VfxObjects.Selected = [.. value];
                        break;
                    case ObjectRenderMode.Special when ObjectMode == IndividualObjectMode.Note:
                        Notes.Selected = [.. value.Cast<Note>()];

                        if (value.Count > 0 && value[0] is Note note)
                        {
                            ListSetting style = Settings.modchartStyle.Value;
                            ListSetting direction = Settings.modchartDirection.Value;

                            style.Current = style.Possible[(int)note.Style];
                            direction.Current = direction.Possible[(int)note.Direction];

                            GuiWindowEditor.NoteEnableEasing.Toggle = note.EnableEasing;
                            GuiWindowEditor.NoteEasingStyle.Update();
                            GuiWindowEditor.NoteEasingDirection.Update();
                        }
                        break;
                    case ObjectRenderMode.Special:
                        SpecialObjects.Selected = [.. value];

                        if (value.Count == 1 && value[0] is Lyric lyric)
                        {
                            GuiWindowEditor.LyricBox.Text = lyric.Text;
                            GuiWindowEditor.LyricFadeIn.Toggle = lyric.FadeIn;
                            GuiWindowEditor.LyricFadeOut.Toggle = lyric.FadeOut;
                        }
                        break;
                }
            }
        }

        public ObjectRenderMode RenderMode = ObjectRenderMode.Notes;
        public IndividualObjectMode ObjectMode = IndividualObjectMode.Disabled;

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

            urActions = [.. UndoRedoManager.actions];
            urActionIndex = UndoRedoManager._index;

            UndoRedoManager.Clear();
            MusicPlayer.Stop();
        }

        public void ClearSelected()
        {
            Notes.ClearSelected();
            VfxObjects.ClearSelected();
            SpecialObjects.ClearSelected();
            SelectedPoint = null;
        }

        public void SortAll(bool updateLists = true)
        {
            Notes.Sort();
            VfxObjects.Sort();
            SpecialObjects.Sort();
            SortTimings(updateLists);
            SortBookmarks(updateLists);
        }

        public void SortObjects()
        {
            switch (RenderMode)
            {
                case ObjectRenderMode.Notes:
                    Notes.Sort();
                    break;
                case ObjectRenderMode.VFX:
                    VfxObjects.Sort();
                    break;
                case ObjectRenderMode.Special when ObjectMode == IndividualObjectMode.Note:
                    Notes.Sort();
                    break;
                case ObjectRenderMode.Special:
                    SpecialObjects.Sort();
                    break;
            }
        }

        public void SortTimings(bool updateList = true)
        {
            TimingPoints = [.. TimingPoints.OrderBy(n => n.Ms)];

            if (updateList)
                TimingsWindow.Instance?.ResetList();
            GuiWindowEditor.Timeline.RefreshInstances();
        }

        public void SortBookmarks(bool updateList = true)
        {
            Bookmarks = [.. Bookmarks.OrderBy(n => n.Ms)];

            if (updateList)
                BookmarksWindow.Instance?.ResetList();
            GuiWindowEditor.Timeline.RefreshInstances();
        }

        public List<MapObject> GetObjectsInRange(float start, float end)
        {
            int low, high;

            switch (RenderMode)
            {
                case ObjectRenderMode.Notes:
                    (low, high) = Notes.SearchRange(start, end);
                    return [.. Notes.Take(new Range(low, high)).Cast<MapObject>()];

                case ObjectRenderMode.VFX:
                    (low, high) = VfxObjects.SearchRange(start, end);
                    if (ObjectMode == IndividualObjectMode.Disabled)
                        return [.. VfxObjects.Take(new Range(low, high))];
                    else
                        return [.. VfxObjects.Take(new Range(low, high)).Where(n => n.ID == (int)ObjectMode)];

                case ObjectRenderMode.Special when ObjectMode == IndividualObjectMode.Note:
                    (low, high) = Notes.SearchRange(start, end);
                    return [.. Notes.Take(new Range(low, high)).Cast<MapObject>()];

                case ObjectRenderMode.Special:
                    (low, high) = SpecialObjects.SearchRange(start, end);
                    if (ObjectMode == IndividualObjectMode.Disabled)
                        return [.. SpecialObjects.Take(new Range(low, high))];
                    else
                        return [.. SpecialObjects.Take(new Range(low, high)).Where(n => n.ID == (int)ObjectMode)];
            }

            return [];
        }


        public void IncrementZoom(float increment)
        {
            float zoom = Zoom;
            float step = zoom < 0.1f || (zoom == 0.1f && increment < 0) ? 0.01f : 0.1f;

            zoom = (float)Math.Round(zoom + increment * step, 2);
            if (zoom > 0.1f)
                zoom = (float)Math.Round(zoom * 10) / 10;

            Zoom = Math.Clamp(zoom, 0.01f, 10f);
        }

        public void CopyBookmarks()
        {
            string[] data = new string[Bookmarks.Count];

            for (int i = 0; i < Bookmarks.Count; i++)
            {
                Bookmark bookmark = Bookmarks[i];

                if (bookmark.Ms != bookmark.EndMs)
                    data[i] = $"{bookmark.Ms}-{bookmark.EndMs} ~ {bookmark.Text.Replace(" ~", "_~")}";
                else
                    data[i] = $"{bookmark.Ms} ~ {bookmark.Text.Replace(" ~", "_~")}";
            }

            if (data.Length == 0)
                return;

            Clipboard.SetText(string.Join("\n", data));
            GuiWindowEditor.ShowOther("COPIED TO CLIPBOARD");
        }

        public void PasteBookmarks()
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
                BookmarkManager.Replace("PASTE BOOKMARK[S]", Bookmarks, tempBookmarks);
        }
    }
}
