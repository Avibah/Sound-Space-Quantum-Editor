using New_SSQE.EditHistory;
using New_SSQE.ExternalUtils;
using New_SSQE.NewGUI.Windows;
using New_SSQE.NewMaps;
using New_SSQE.NewMaps.Parsing;
using New_SSQE.Objects;
using New_SSQE.Objects.Managers;
using New_SSQE.Preferences;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace New_SSQE.NewGUI.Input
{
    internal class KeybindManager
    {
        public static bool CtrlHeld => MainWindow.Instance.CtrlHeld;
        public static bool AltHeld => MainWindow.Instance.AltHeld;
        public static bool ShiftHeld => MainWindow.Instance.ShiftHeld;

        public static void Run(Keys key)
        {
            List<string> keybinds = Settings.CompareKeybind(key, CtrlHeld, AltHeld, ShiftHeld);

            foreach (string keybind in keybinds)
                ParseKeybind(keybind);
        }

        public static void ParseKeybind(string keybind)
        {
            if (Windowing.Current is not GuiWindowEditor)
                return;
            if (Windowing.TextboxFocused())
                return;
            Windowing.KeybindUsed(keybind);

            if (keybind.Contains("pattern"))
            {
                int index = int.Parse(keybind.Replace("pattern", ""));

                if (ShiftHeld)
                    Patterns.StorePattern(index);
                else if (CtrlHeld)
                    Patterns.ClearPattern(index);
                else
                    Patterns.RecallPattern(index);
            }

            switch (keybind)
            {
                case "selectAll":
                    Mapping.ClearSelection();

                    if (Mapping.RenderMode == ObjectRenderMode.Notes || Mapping.ObjectMode == IndividualObjectMode.Note)
                        Mapping.Current.Notes.Selected = new(Mapping.Current.Notes);
                    else if (Mapping.ObjectMode != IndividualObjectMode.Disabled)
                    {
                        if (Mapping.RenderMode == ObjectRenderMode.VFX)
                            Mapping.Current.VfxObjects.Selected = new(Mapping.Current.VfxObjects.Where(n => n.ID == (int)Mapping.ObjectMode));
                        else
                            Mapping.Current.SpecialObjects.Selected = new(Mapping.Current.SpecialObjects.Where(n => n.ID == (int)Mapping.ObjectMode));
                    }
                    else
                    {
                        if (Mapping.RenderMode == ObjectRenderMode.VFX)
                            Mapping.Current.VfxObjects.Selected = new(Mapping.Current.VfxObjects);
                        else
                            Mapping.Current.SpecialObjects.Selected = new(Mapping.Current.SpecialObjects);
                    }

                    break;

                case "undo":
                    UndoRedoManager.Undo();

                    break;

                case "redo":
                    UndoRedoManager.Redo();

                    break;

                case "copy":
                    try
                    {
                        if (Mapping.Current.Notes.Selected.Count > 0)
                        {
                            List<MapObject> copied = Mapping.Current.Notes.Selected.Cast<MapObject>().ToList();
                            Clipboard.SetData(copied);

                            GuiWindowEditor.ShowToast("COPIED NOTES", Settings.color1.Value);
                        }
                        else if (Mapping.Current.VfxObjects.Selected.Count > 0)
                        {
                            List<MapObject> copied = Mapping.Current.VfxObjects.Selected.ToList();
                            Clipboard.SetData(copied);

                            GuiWindowEditor.ShowToast("COPIED OBJECTS", Settings.color1.Value);
                        }
                        else if (Mapping.Current.SpecialObjects.Selected.Count > 0)
                        {
                            List<MapObject> copied = Mapping.Current.SpecialObjects.Selected.ToList();
                            Clipboard.SetData(copied);

                            GuiWindowEditor.ShowToast("COPIED OBJECTS", Settings.color1.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Log("Failed to copy objects", LogSeverity.WARN, ex);
                        GuiWindowEditor.ShowToast("FAILED TO COPY", Settings.color1.Value);
                    }

                    break;

                case "paste":
                    try
                    {
                        List<MapObject> copied = Clipboard.GetData().ToList();

                        if (copied.Count > 0)
                        {
                            bool isNote = copied.FirstOrDefault() is Note;

                            long offset = copied.Min(n => n.Ms);
                            long max = copied.Max(n => n.Ms);

                            copied.ForEach(n => n.Ms = (long)Math.Clamp(Settings.currentTime.Value.Value + n.Ms - offset, 0, Settings.currentTime.Value.Max));

                            if (isNote && (Mapping.RenderMode == ObjectRenderMode.Notes || Mapping.ObjectMode == IndividualObjectMode.Note))
                            {
                                List<Note> copiedAsNotes = copied.Cast<Note>().ToList();

                                if (Settings.pasteReversed.Value)
                                {
                                    for (int i = 0; i < copiedAsNotes.Count / 2; i++)
                                    {
                                        Note cur = copiedAsNotes[i];
                                        Note opp = copiedAsNotes[copiedAsNotes.Count - 1 - i];

                                        Vector2 curPos = (cur.X, cur.Y);
                                        Vector2 oppPos = (opp.X, opp.Y);

                                        cur.X = oppPos.X;
                                        cur.Y = oppPos.Y;

                                        opp.X = curPos.X;
                                        opp.Y = curPos.Y;
                                    }
                                }

                                if (Settings.applyOnPaste.Value)
                                {
                                    if (float.TryParse(GuiWindowEditor.RotateBox.Text, out float deg) && float.TryParse(GuiWindowEditor.ScaleBox.Text, out float scale))
                                    {
                                        foreach (Note note in copiedAsNotes)
                                        {
                                            Patterns.Rotate(note, deg);
                                            Patterns.Scale(note, scale);
                                        }
                                    }
                                }

                                NoteManager.Add("PASTE NOTE[S]", copiedAsNotes);

                                if (Settings.jumpPaste.Value)
                                {
                                    Settings.currentTime.Value.Value += max - offset;
                                    if (Settings.autoAdvance.Value)
                                        Timing.Advance();
                                }
                            }
                            else if (!isNote && Mapping.RenderMode != ObjectRenderMode.Notes)
                            {
                                (List<MapObject> vfxCopy, List<MapObject> specialCopy) = FormatUtils.SplitVFXSpecial(copied);

                                if (Mapping.RenderMode ==  ObjectRenderMode.VFX)
                                    VfxObjectManager.Add("PASTE OBJECT[S]", vfxCopy);
                                else
                                    SpecialObjectManager.Add("PASTE OBJECT[S]", specialCopy);

                                if (Settings.jumpPaste.Value)
                                {
                                    Settings.currentTime.Value.Value += max - offset;
                                    if (Settings.autoAdvance.Value)
                                        Timing.Advance();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Log("Failed to paste objects", LogSeverity.WARN, ex);
                        GuiWindowEditor.ShowToast("FAILED TO PASTE", Settings.color1.Value);
                    }

                    break;

                case "cut":
                    try
                    {
                        if (Mapping.Current.Notes.Selected.Count > 0)
                        {
                            List<Note> copied = Mapping.Current.Notes.Selected.ToList();
                            Clipboard.SetData(copied.Cast<MapObject>().ToList());

                            NoteManager.Remove("CUT NOTE[S]", copied);
                            GuiWindowEditor.ShowToast("CUT NOTES", Settings.color1.Value);
                        }
                        else if (Mapping.Current.VfxObjects.Selected.Count > 0)
                        {
                            List<MapObject> copied = Mapping.Current.VfxObjects.Selected.ToList();
                            Clipboard.SetData(copied);

                            VfxObjectManager.Remove("CUT OBJECT[S]", copied);
                            GuiWindowEditor.ShowToast("CUT OBJECTS", Settings.color1.Value);
                        }
                        else if (Mapping.Current.SpecialObjects.Selected.Count > 0)
                        {
                            List<MapObject> copied = Mapping.Current.SpecialObjects.Selected.ToList();
                            Clipboard.SetData(copied);

                            SpecialObjectManager.Remove("CUT OBJECT[S]", copied);
                            GuiWindowEditor.ShowToast("CUT OBJECTS", Settings.color1.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Log("Failed to cut objects", LogSeverity.WARN, ex);
                        GuiWindowEditor.ShowToast("FAILED TO CUT", Settings.color1.Value);
                    }

                    break;

                case "quantum":
                    Settings.enableQuantum.Value ^= true;

                    break;

                case "openTimings":
                    TimingsWindow.ShowWindow();

                    break;

                case "openBookmarks":
                    BookmarksWindow.ShowWindow();

                    break;

                case "drawBezier":
                    Patterns.RunBezier();

                    break;

                case "anchorNode":
                    if (Mapping.Current.Notes.Selected.Count > 0)
                        NoteManager.Edit("ANCHOR NODE[S]", n => n.Anchored ^= true);

                    break;

                case "openDirectory":
                    PlatformUtils.OpenDirectory();

                    break;

                case "exportSSPM":
                    ExportSSPM.ShowWindow();

                    break;

                case "createBPM":
                    if (Mapping.Current.Notes.Selected.Count == 2)
                    {
                        Note first = Mapping.Current.Notes.Selected[0];
                        Note second = Mapping.Current.Notes.Selected[1];

                        long minMs = Math.Min(first.Ms, second.Ms);
                        long maxMs = Math.Max(first.Ms, second.Ms);
                        float bpm = (float)Math.Round(60000f / (maxMs - minMs) * 4) / 4;

                        if (bpm > 0)
                            PointManager.Add("CREATE TIMING POINT", new TimingPoint(bpm, minMs));
                    }

                    break;
            }
        }
    }
}
