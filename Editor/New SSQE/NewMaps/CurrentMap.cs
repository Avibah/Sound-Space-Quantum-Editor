using New_SSQE.ExternalUtils;
using New_SSQE.NewGUI.Windows;
using New_SSQE.Objects;
using New_SSQE.Objects.Managers;
using New_SSQE.Objects.Other;
using New_SSQE.Preferences;
using OpenTK.Mathematics;
using System.Drawing;

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

    internal static class CurrentMap
    {
        public static ObjectRenderMode RenderMode = ObjectRenderMode.Notes;
        public static IndividualObjectMode ObjectMode = IndividualObjectMode.Disabled;

        public static ClickMode ClickMode => Settings.separateClickTools.Value ? (Settings.selectTool.Value ? ClickMode.Select : ClickMode.Place) : ClickMode.Both;

        public static Map? Map;

        public static ObjectList<Note> Notes
        {
            get => Map?.Notes ?? [];
            set
            {
                if (Map != null)
                    Map.Notes = value;
            }
        }

        public static ObjectList<MapObject> VfxObjects
        {
            get => Map?.VfxObjects ?? [];
            set
            {
                if (Map != null)
                    Map.VfxObjects = value;
            }
        }

        public static ObjectList<MapObject> SpecialObjects
        {
            get => Map?.SpecialObjects ?? [];
            set
            {
                if (Map != null)
                    Map.SpecialObjects = value;
            }
        }

        public static List<Note> BezierNodes
        {
            get => Map?.BezierNodes ?? [];
            set
            {
                if (Map != null)
                    Map.BezierNodes = value;
            }
        }

        public static List<Bookmark> Bookmarks
        {
            get => Map?.Bookmarks ?? [];
            set
            {
                if (Map != null)
                    Map.Bookmarks = value;
            }
        }

        public static List<TimingPoint> TimingPoints
        {
            get => Map?.TimingPoints ?? [];
            set
            {
                if (Map != null)
                    Map.TimingPoints = value;
            }
        }

        public static float Tempo
        {
            get => Map?.Tempo ?? 1f;
            set
            {
                if (Map != null)
                    Map.Tempo = value;
            }
        }

        public static float Zoom
        {
            get => Map?.Zoom ?? 1f;
            set
            {
                if (Map != null)
                    Map.Zoom = value;
            }
        }

        public static string? FileName
        {
            get => Map?.FileName;
            set
            {
                if (Map != null)
                    Map.FileName = value;
            }
        }

        public static string SoundID
        {
            get => Map?.SoundID ?? "";
            set
            {
                if (Map != null)
                    Map.SoundID = value;
            }
        }
        public static string FileID => Map?.FileID ?? "";
        public static bool IsSaved => Map?.IsSaved ?? true;

        public static bool Save(string path) => Map?.Save(path) ?? false;
        public static bool Save() => Map?.Save() ?? false;
        public static bool SaveAs() => Map?.SaveAs() ?? false;



        public static List<MapObject> GetSelected()
        {
            return RenderMode switch
            {
                ObjectRenderMode.Notes => Notes.Selected.Cast<MapObject>().ToList(),
                ObjectRenderMode.VFX => VfxObjects.Selected,
                ObjectRenderMode.Special => SpecialObjects.Selected,
                _ => []
            };
        }

        public static void SetSelected(List<MapObject> selected)
        {
            switch (RenderMode)
            {
                case ObjectRenderMode.Notes:
                    Notes.Selected = new(selected.Cast<Note>());
                    break;
                case ObjectRenderMode.VFX:
                    VfxObjects.Selected = new(selected);
                    break;
                case ObjectRenderMode.Special:
                    SpecialObjects.Selected = new(selected);
                    break;
            }
        }



        private static long currentAutosave;

        public static void Autosave()
        {
            if (Notes.Count + Bookmarks.Count + TimingPoints.Count > 0)
            {
                if (IsSaved)
                    return;

                if (FileName == null)
                {
                    string autosave = Map?.ToCache() ?? "";
                    if (string.IsNullOrWhiteSpace(autosave))
                        return;

                    string[] data = autosave.Split('\n');
                    Settings.autosavedFile.Value = data[0];
                    Settings.autosavedProperties.Value = data[1];
                    Settings.Save(false);

                    GuiWindowEditor.ShowToast("AUTOSAVED", Settings.color1.Value);
                }
                else if (Save())
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

        public static void SortTimings(bool updateList = true) => Map?.SortTimings(updateList);
        public static void SortBookmarks(bool updateList = true) => Map?.SortBookmarks(updateList);

        public static void CopyBookmarks()
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
                BookmarkManager.Replace("PASTE BOOKMARK[S]", Bookmarks, tempBookmarks);
        }


        public static void IncrementZoom(float increment)
        {
            if (Map == null)
                return;

            float step = Zoom < 0.1f || (Zoom == 0.1f && increment < 0) ? 0.01f : 0.1f;

            Map.Zoom = (float)Math.Round(Zoom + increment * step, 2);
            if (Zoom > 0.1f)
                Map.Zoom = (float)Math.Round(Zoom * 10) / 10;

            Map.Zoom = MathHelper.Clamp(Zoom, 0.01f, 10f);
        }

        public static void ClearSelection()
        {
            Notes.ClearSelection();
            VfxObjects.ClearSelection();
            SpecialObjects.ClearSelection();
        }
    }
}
