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

            if (keybind.Contains("gridKey") && CurrentMap.RenderMode == ObjectRenderMode.Notes)
            {
                string rep = keybind.Replace("gridKey", "");
                string[] xy = rep.Split('|');

                int x = 2 - int.Parse(xy[0]);
                int y = 2 - int.Parse(xy[1]);
                long ms = Timing.GetClosestBeat(Settings.currentTime.Value.Value);

                Note note = new(x, y, (long)(ms >= 0 ? ms : Settings.currentTime.Value.Value));
                NoteManager.Add("ADD NOTE", note);

                if (Settings.autoAdvance.Value)
                    Timing.Advance();

                return;
            }

            if (keybind.Contains("pattern"))
            {
                int index = int.Parse(keybind.Replace("pattern", ""));

                if (ShiftHeld)
                    Patterns.StorePattern(index);
                else if (CtrlHeld)
                    Patterns.ClearPattern(index);
                else
                    Patterns.RecallPattern(index);

                return;
            }

            if (keybind.Contains("extras") && CurrentMap.RenderMode == ObjectRenderMode.Special)
            {
                long ms = Timing.GetClosestBeat(Settings.currentTime.Value.Value);
                ms = ms > 0 ? ms : (long)Settings.currentTime.Value.Value;
                MapObject? obj = null;

                switch (keybind.Replace("extras", ""))
                {
                    case "Beat":
                        obj = new Beat(ms);
                        break;
                }

                if (obj != null)
                    SpecialObjectManager.Add($"ADD {obj.Name?.ToUpper()}", obj);

                return;
            }

            switch (keybind)
            {
                case "selectAll":
                    if (CurrentMap.RenderMode == ObjectRenderMode.VFX)
                        CurrentMap.VfxObjects.Selected = new(CurrentMap.VfxObjects);
                    else if (CurrentMap.RenderMode == ObjectRenderMode.Special)
                        CurrentMap.SpecialObjects.Selected = new(CurrentMap.SpecialObjects);
                    else
                    {
                        CurrentMap.Notes.Selected = new(CurrentMap.Notes);
                        CurrentMap.ClearSelection();
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
                        if (CurrentMap.Notes.Selected.Count > 0)
                        {
                            List<MapObject> copied = CurrentMap.Notes.Selected.Cast<MapObject>().ToList();
                            Clipboard.SetData(copied);

                            GuiWindowEditor.ShowToast("COPIED NOTES", Settings.color1.Value);
                        }
                        else if (CurrentMap.VfxObjects.Selected.Count > 0)
                        {
                            List<MapObject> copied = CurrentMap.VfxObjects.Selected.ToList();
                            Clipboard.SetData(copied);

                            GuiWindowEditor.ShowToast("COPIED OBJECTS", Settings.color1.Value);
                        }
                        else if (CurrentMap.SpecialObjects.Selected.Count > 0)
                        {
                            List<MapObject> copied = CurrentMap.SpecialObjects.Selected.ToList();
                            Clipboard.SetData(copied);

                            GuiWindowEditor.ShowToast("COPIED OBJECTS", Settings.color1.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Register("Failed to copy objects", LogSeverity.WARN, ex);
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

                            copied.ForEach(n => n.Ms = (long)MathHelper.Clamp(Settings.currentTime.Value.Value + n.Ms - offset, 0, Settings.currentTime.Value.Max));

                            if (isNote && CurrentMap.RenderMode == ObjectRenderMode.Notes)
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
                            else if (!isNote && CurrentMap.RenderMode != ObjectRenderMode.Notes)
                            {
                                (List<MapObject> vfxCopy, List<MapObject> specialCopy) = FormatUtils.SplitVFXSpecial(copied);

                                if (CurrentMap.RenderMode ==  ObjectRenderMode.VFX)
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
                        Logging.Register("Failed to paste objects", LogSeverity.WARN, ex);
                        GuiWindowEditor.ShowToast("FAILED TO PASTE", Settings.color1.Value);
                    }

                    break;

                case "cut":
                    try
                    {
                        if (CurrentMap.Notes.Selected.Count > 0)
                        {
                            List<Note> copied = CurrentMap.Notes.Selected.ToList();
                            Clipboard.SetData(copied.Cast<MapObject>().ToList());

                            NoteManager.Remove("CUT NOTE[S]", copied);
                            GuiWindowEditor.ShowToast("CUT NOTES", Settings.color1.Value);
                        }
                        else if (CurrentMap.Notes.Selected.Count > 0)
                        {
                            List<MapObject> copied = CurrentMap.VfxObjects.Selected.ToList();
                            Clipboard.SetData(copied);

                            VfxObjectManager.Remove("CUT OBJECT[S]", copied);
                            GuiWindowEditor.ShowToast("CUT OBJECTS", Settings.color1.Value);
                        }
                        else if (CurrentMap.SpecialObjects.Selected.Count > 0)
                        {
                            List<MapObject> copied = CurrentMap.SpecialObjects.Selected.ToList();
                            Clipboard.SetData(copied);

                            SpecialObjectManager.Remove("CUT OBJECT[S]", copied);
                            GuiWindowEditor.ShowToast("CUT OBJECTS", Settings.color1.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Register("Failed to cut objects", LogSeverity.WARN, ex);
                        GuiWindowEditor.ShowToast("FAILED TO CUT", Settings.color1.Value);
                    }

                    break;

                case "switchClickTool":
                    Settings.selectTool.Value ^= true;

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

                case "storeNodes":
                    if (CurrentMap.Notes.Selected.Count > 1)
                        CurrentMap.BezierNodes = CurrentMap.Notes.Selected.ToList();

                    break;

                case "drawBezier":
                    Patterns.RunBezier();

                    break;

                case "anchorNode":
                    if (CurrentMap.Notes.Selected.Count > 0)
                        NoteManager.Edit("ANCHOR NODE[S]", n => n.Anchored ^= true);

                    break;

                case "openDirectory":
                    Platform.OpenDirectory();

                    break;

                case "exportSSPM":
                    ExportSSPM.ShowWindow();

                    break;

                case "createBPM":
                    if (CurrentMap.Notes.Selected.Count == 2)
                    {
                        Note first = CurrentMap.Notes.Selected[0];
                        Note second = CurrentMap.Notes.Selected[1];

                        long minMs = Math.Min(first.Ms, second.Ms);
                        long maxMs = Math.Max(first.Ms, second.Ms);
                        float bpm = (float)Math.Round(60000f / (maxMs - minMs) * 4) / 4;

                        if (bpm > 0)
                            PointManager.Add("CREATE TIMING POINT", new(bpm, minMs));
                    }

                    break;
            }
        }
    }
}
