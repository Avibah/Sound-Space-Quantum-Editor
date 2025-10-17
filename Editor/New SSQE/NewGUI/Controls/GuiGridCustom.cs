using New_SSQE.Audio;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Font;
using New_SSQE.NewMaps;
using New_SSQE.Objects;
using New_SSQE.Objects.Other;
using New_SSQE.Preferences;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiGridCustom : InteractiveControl
    {
        private static readonly Vector2i CELL_DIMS = (4, 5);
        private static readonly Vector2i CELL_MAX = CELL_DIMS - (1, 1);
        private const string FONT = "main";

        private readonly int bezierPreviewLineVAO;
        private readonly int bezierPreviewLineVBO;

        private Vector2 CellSize => (rect.Width / CELL_DIMS.X, rect.Height / CELL_DIMS.Y);
        private Vector2 NoteSize => CellSize * 0.75f;
        private Vector2 PreviewSize => CellSize * 0.65f;
        private Vector2 CellGap => (CellSize - NoteSize) / 2f;
        private Vector2 PreviewGap => (CellSize - PreviewSize) / 2f;

        private readonly Instance autoplayCursor;

        private readonly Instance noteConstant;
        private readonly Instance noteApproach;
        private readonly Instance noteHover;
        private readonly Instance noteSelect;
        private readonly Instance notePreview;

        private readonly Instance beatConstant;
        private readonly Instance beatApproach;

        private Vector2? hoveringCell;
        private bool placing = false;
        private Vector2? lastPlacedPos;

        private XYMapObject? hoveringXY;
        public bool IsHoveringObject => hoveringXY != null;
        // The first element in this list represents the object that was initially dragged, with it being repeated later in the list to keep object ordering
        // Ex.: With notes 1, 2, 3, the corresponding drag list (if note 2 is clicked) is {2, 1, 2, 3}
        private List<XYMapObject> draggingXY = [];
        private Vector2 dragCellStart;

        private Vector4[] keybindVerts = [];
        private Vector4[] gridNumData = [];
        private float[] gridNumAlphaData = [];
        private List<Vector2> bezierPositions = [];

        public Vector2 CellBoundsX => Settings.enableQuantum.Value ? (-0.85f, CELL_MAX.X + 0.85f) : (0, CELL_MAX.X);
        public Vector2 CellBoundsY => Settings.enableQuantum.Value ? (-0.85f, CELL_MAX.Y - 1 + 0.85f) : (0, CELL_MAX.Y);

        private readonly bool respectObjectMode;

        public GuiGridCustom(float x, float y, float w, float h) : base(x, y, w, h)
        {
            (bezierPreviewLineVAO, bezierPreviewLineVBO) = GLState.NewVAO_VBO(2, 4);

            autoplayCursor = Instancing.Generate("grid_autoplayCursor", Shader.InstancedMain);

            noteConstant = Instancing.Generate("grid_noteConstant", Shader.InstancedObject);
            noteApproach = Instancing.Generate("grid_noteApproach", Shader.InstancedObject);
            noteHover = Instancing.Generate("grid_noteHover", Shader.InstancedMain);
            noteSelect = Instancing.Generate("grid_noteSelect", Shader.InstancedMain);
            notePreview = Instancing.Generate("grid_notePreview", Shader.InstancedMain);

            beatConstant = Instancing.Generate("grid_beatConstant", Shader.InstancedMain);
            beatApproach = Instancing.Generate("grid_beatApproach", Shader.InstancedMain);

            Style = ControlStyle.Grid_Colored;
        }

        private Vector2 MouseToGridSpaceUnclamped(float mousex, float mousey)
        {
            bool quantum = Settings.enableQuantum.Value;

            float increment = quantum ? (Settings.quantumSnapping.Value.Value + 3) / 3 : 1;
            float x = (mousex - rect.X - rect.Width / 2) / rect.Width * CELL_DIMS.X + 1 / increment;
            float y = (mousey - rect.Y - rect.Height / 2) / rect.Height * CELL_DIMS.Y + 1 / increment;

            if (Settings.quantumGridSnap.Value || !quantum)
            {
                x = (float)Math.Floor((x + 1 / increment / 2) * increment) / increment;
                y = (float)Math.Floor((y + 1 / increment / 2) * increment) / increment;
            }

            x = x - 1 / increment + CELL_DIMS.X / 2;
            y = y - 1 / increment + CELL_DIMS.Y / 2;

            return (CELL_DIMS.X - x, CELL_DIMS.Y - y);
        }

        private Vector2 MouseToGridSpace(float mousex, float mousey)
        {
            Vector2 pos = MouseToGridSpaceUnclamped(mousex, mousey);

            float x = Math.Clamp(pos.X, CellBoundsX.X, CellBoundsX.Y);
            float y = Math.Clamp(pos.Y, CellBoundsY.X, CellBoundsY.Y);

            return (x, y);
        }

        private void UpdateInstanceData(float mousex, float mousey)
        {
            float currentTime = Settings.currentTime.Value.Value;
            float approachRate = (Settings.approachRate.Value.Value + 1) / 10;
            float maxMs = currentTime + 1000 / approachRate;

            int colorCount = Settings.noteColors.Value.Count;

            List<(Vector3, string)> gridNumStrings = [];
            int gridNumLength = 0;

            ObjectList<Note> notes = Mapping.Current.Notes;
            Note last = notes.FirstOrDefault() ?? new(1, 1, 0);
            Note? next = null;

            (int low, int high) = notes.SearchRange(currentTime, maxMs);
            int range = high - low;

            Vector4[] noteConstants = new Vector4[range];
            Vector4[] noteApproaches = new Vector4[range];
            Vector4 noteHovers = Vector4.Zero;
            Vector4[] noteSelects = new Vector4[notes.Selected.Count];

            int selectIndex = 0;

            if (notes.Count > 0)
            {
                if (notes[low].Ms <= currentTime)
                    last = notes[low];
                else if (low > 0)
                    last = notes[low - 1];
                next = notes[low];
            }

            bool gridNumbers = Settings.gridNumbers.Value;
            int gridNumberSize = (int)(28 * TextScale);
            hoveringXY = null;

            for (int i = low; i < high; i++)
            {
                Note note = notes[i];
                int c = i % colorCount;

                float x = rect.X + (CELL_MAX.X - note.X) * CellSize.X + CellGap.X;
                float y = rect.Y + (CELL_MAX.Y - note.Y) * CellSize.Y + CellGap.Y;

                float progress = (float)Math.Min(1, Math.Pow(1 - Math.Min(1, (note.Ms - currentTime) * approachRate / 750), 2));
                Vector2 approachSize = (4, 4) + NoteSize + NoteSize * (1 - progress) * 2 + (0.5f, 0.5f);
                float approachScale = approachSize.X;
                approachSize = approachSize / 2 - NoteSize / 2;

                noteConstants[i - low] = (x, y, 1, 2 * c + progress);
                noteApproaches[i - low] = (approachSize.X, approachSize.Y, approachScale, 2 * c + progress);

                if (Settings.ClickMode.HasFlag(ClickMode.Select))
                {
                    if (Math.Abs(mousex - NoteSize.X / 2 - x) <= NoteSize.X / 2 && Math.Abs(mousey - NoteSize.Y / 2 - y) <= NoteSize.Y / 2)
                        hoveringXY ??= note;
                    if (hoveringXY == note)
                        noteHovers = (x, y, 1, 2 * 5 + 1);
                }

                if (note.Selected)
                    noteSelects[selectIndex++] = (x, y, 1, 2 * 6 + progress);

                if (gridNumbers)
                {
                    string numText = $"{i + 1:##,###}";
                    int width = FontRenderer.GetWidth(numText, gridNumberSize, FONT);
                    int height = FontRenderer.GetHeight(gridNumberSize, FONT);

                    gridNumStrings.Add(((x + NoteSize.X / 2 - width / 2, y + NoteSize.Y / 2 - height / 2, 1 - progress), numText));
                    gridNumLength += numText.Length;
                }
            }

            noteConstant.UploadData(noteConstants);
            noteApproach.UploadData(noteApproaches);
            noteHover.UploadData([noteHovers]);
            noteSelect.UploadData(noteSelects);

            gridNumData = new Vector4[gridNumLength];
            gridNumAlphaData = new float[gridNumLength];

            int offset = 0;

            for (int i = 0; i < gridNumStrings.Count; i++)
            {
                (Vector3, string) data = gridNumStrings[i];

                FontRenderer.PrintInto(gridNumData, offset, data.Item1.X, data.Item1.Y, data.Item2, gridNumberSize, FONT);

                for (int j = 0; j < data.Item2.Length; j++)
                    gridNumAlphaData[offset++] = data.Item1.Z;
            }

            if (Settings.autoplay.Value)
            {
                next ??= last;
                long msDiff = next.Ms - last.Ms;
                float timePos = currentTime - last.Ms;

                float progress = msDiff == 0 ? 1 : timePos / msDiff;
                progress = (float)Math.Sin(progress * MathHelper.PiOver2);

                float width = (float)Math.Sin(progress * MathHelper.Pi) * 8 + 16;

                float lastX = rect.X + (CELL_MAX.X - last.X) * CellSize.X;
                float lastY = rect.Y + (CELL_MAX.Y - last.Y) * CellSize.Y;

                float nextX = rect.X + (CELL_MAX.X - next.X) * CellSize.X;
                float nextY = rect.Y + (CELL_MAX.Y - next.Y) * CellSize.Y;

                float x = CellSize.X / 2 + lastX + (nextX - lastX) * progress;
                float y = CellSize.Y / 2 + lastY + (nextY - lastY) * progress;

                autoplayCursor.UploadData([(x - width / 2, y - width / 2, width, 2 * 4 + 1)]);
            }

            if (Mapping.Current.RenderMode == ObjectRenderMode.Special)
            {
                bool shouldCheckID = respectObjectMode && Mapping.Current.ObjectMode != IndividualObjectMode.Disabled;

                List<Vector4> beatConstants = [];
                List<Vector4> beatApproaches = [];

                ObjectList<MapObject> objects = Mapping.Current.SpecialObjects;
                (low, high) = objects.SearchRange(currentTime, maxMs);

                for (int i = low; i < high; i++)
                {
                    MapObject obj = objects[i];
                    float progress = (float)Math.Min(1, (float)Math.Pow(1 - Math.Min(1, (obj.Ms - currentTime) * approachRate / 750), 2));

                    if (shouldCheckID && obj.ID != (int)Mapping.Current.ObjectMode)
                        continue;

                    switch (obj.ID)
                    {
                        case 12:
                            float approachSize = 4 + rect.Width + rect.Width * (1 - progress) + 0.5f;

                            beatConstants.Add((rect.X, rect.Y, 1, 2 * 4 + progress));
                            beatApproaches.Add((rect.X + rect.Width / 2 - approachSize / 2, rect.Y + rect.Height / 2 - approachSize / 2, approachSize, 2 * 4 + progress));
                            break;
                    }
                }

                beatConstant.UploadData(beatConstants.ToArray());
                beatApproach.UploadData(beatApproaches.ToArray());
            }

            List<Vector4> notePreviews = [];
            bezierPositions = [];

            if (hoveringCell != null && hoveringXY == null && Settings.ClickMode.HasFlag(ClickMode.Place))
            {
                Vector2 hover = hoveringCell ?? Vector2.One;
                float x = rect.X + (CELL_MAX.X - hover.X) * CellSize.X + PreviewGap.X;
                float y = rect.Y + (CELL_MAX.Y - hover.Y) * CellSize.Y + PreviewGap.Y;

                notePreviews.Add((x, y, 1, 2 * 9 + 1));
            }

            List<int> bezierNodes = Mapping.Current.BezierNodes;

            if (bezierNodes.Count > 0)
            {
                List<Note> result = [];
                List<int> anchored = [0];

                for (int i = 1; i < bezierNodes.Count; i++)
                {
                    Note note = notes[bezierNodes[i]];

                    if (note.Anchored)
                        anchored.Add(i);
                }

                if (!anchored.Contains(bezierNodes.Count - 1))
                    anchored.Add(bezierNodes.Count - 1);

                for (int i = 1; i < anchored.Count; i++)
                {
                    List<Note> toDraw = [];

                    for (int j = anchored[i - 1]; j <= anchored[i]; j++)
                    {
                        Note note = notes[bezierNodes[j]];
                        toDraw.Add(note);
                    }

                    result.AddRange(Patterns.DrawBezier(toDraw, (int)(Settings.bezierDivisor.Value + 0.5f)));
                }

                for (int i = 0; i < result.Count; i++)
                {
                    Note note = result[i];

                    float x = rect.X + (CELL_MAX.X - note.X) * CellSize.X + PreviewGap.X;
                    float y = rect.Y + (CELL_MAX.Y - note.Y) * CellSize.Y + PreviewGap.Y;

                    notePreviews.Add((x, y, 1, 2 * 2 + 1));
                    bezierPositions.Add((x, y));
                }
            }

            notePreview.UploadData(notePreviews.ToArray());
        }

        public override float[] Draw()
        {
            List<float> autoplayVerts = [];
            autoplayVerts.AddRange(GLVerts.Outline(0, 0, 1, 1, 0.075f, Style.Quaternary));
            autoplayVerts.AddRange(GLVerts.Rect(0, 0, 1, 1, Style.Quaternary, 0.25f));
            autoplayCursor.UploadStaticData(autoplayVerts.ToArray());

            List<float> noteVerts = [];
            noteVerts.AddRange(GLVerts.Outline(0, 0, NoteSize.X, NoteSize.Y, 2, 1f, 1f, 1f, 1f));
            noteVerts.AddRange(GLVerts.Rect(0, 0, NoteSize.X, NoteSize.Y, 1f, 1f, 1f, 0.15f));
            noteConstant.UploadStaticData(noteVerts.ToArray());

            noteApproach.UploadStaticData(GLVerts.Outline(0, 0, 1, 1, 0.0125f, 1f, 1f, 1f, 1f));
            noteHover.UploadStaticData(GLVerts.Outline(-4, -4, NoteSize.X + 8, NoteSize.Y + 8, 2, 1f, 1f, 1f, 0.25f));
            noteSelect.UploadStaticData(GLVerts.Outline(-4, -4, NoteSize.X + 8, NoteSize.Y + 8, 2, 1f, 1f, 1f, 1f));

            List<float> previewVerts = [];
            previewVerts.AddRange(GLVerts.Outline(0, 0, PreviewSize.X, PreviewSize.Y, 2, 1f, 1f, 1f, 0.09375f));
            previewVerts.AddRange(GLVerts.Rect(0, 0, PreviewSize.X, PreviewSize.Y, 1f, 1f, 1f, 0.125f));
            notePreview.UploadStaticData(previewVerts.ToArray());

            beatConstant.UploadStaticData(GLVerts.Outline(-5, -5, rect.Width + 10, rect.Width + 10, 3, Settings.color5.Value));
            beatApproach.UploadStaticData(GLVerts.Outline(0, 0, 1, 1, 0.0075f, Settings.color5.Value));

            List<float> verts = [];
            verts.AddRange(GLVerts.Rect(rect, Style.Primary, Settings.gridOpacity.Value / 255f));
            verts.AddRange(GLVerts.Outline(rect, 1, Style.Secondary));

            for (int i = 1; i < CELL_DIMS.X; i++)
            {
                float x = rect.X + CellSize.X * i;
                verts.AddRange(GLVerts.Line(x, rect.Y, x, rect.Bottom, 1, Style.Tertiary));
            }

            for (int i = 1; i < CELL_DIMS.Y; i++)
            {
                float y = rect.Y + CellSize.Y * i;
                verts.AddRange(GLVerts.Line(rect.X, y, rect.Right, y, 1, Style.Tertiary));
            }

            float divisor = Settings.quantumGridLines.Value ? Settings.quantumSnapping.Value.Value + 3 : 3;
            float offset = Math.Round(divisor) % 2 == 0 ? 0.5f : 1f;

            for (int i = (int)(2 * offset); i <= divisor; i++)
            {
                float x = rect.X + rect.Width / divisor * (i - offset);
                float y = rect.Y + rect.Height / divisor * (i - offset);

                verts.AddRange(GLVerts.Line(x, rect.Y, x, rect.Bottom, 1, Style.Secondary));
                verts.AddRange(GLVerts.Line(rect.X, y, rect.Right, y, 1, Style.Secondary));
            }

            List<Vector4> keyVerts = [];
            int gridLetterSize = (int)(38 * TextScale);

            if (Settings.gridLetters.Value)
            {
                Dictionary<Keys, Tuple<int, int>> keybinds = new(MainWindow.Instance.KeyMapping);

                foreach (KeyValuePair<Keys, Tuple<int, int>> key in keybinds)
                {
                    string letter = key.Key.ToString().Replace("KeyPad", "");

                    float x = rect.X + key.Value.Item1 * CellSize.X + CellSize.X / 2;
                    float y = rect.Y + key.Value.Item2 * CellSize.Y + CellSize.Y / 2;

                    int width = FontRenderer.GetWidth(letter, gridLetterSize, FONT);
                    int height = FontRenderer.GetHeight(gridLetterSize, FONT);

                    keyVerts.AddRange(FontRenderer.Print(x - width / 2f, y - height / 2f, letter, gridLetterSize, FONT));
                }
            }

            keybindVerts = keyVerts.ToArray();

            return verts.ToArray();
        }

        public override void PreRender(float mousex, float mousey, float frametime)
        {
            base.PreRender(mousex, mousey, frametime);
            Update();

            if (placing && MusicPlayer.IsPlaying)
            {
                float currentTIme = Settings.currentTime.Value.Value;
                long ms = Timing.GetClosestBeat(currentTIme);

                if (ms > 0 && ms <= currentTIme && Mapping.Current.Notes.FirstOrDefault(n => Math.Abs(n.Ms - ms) < 2) == null)
                {
                    Vector2 pos = MouseToGridSpace(mousex, mousey);
                    Mapping.Current.Notes.Modify_Add("ADD NOTE", new Note(pos.X, pos.Y, ms));
                }
            }
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            base.Render(mousex, mousey, frametime);
            UpdateInstanceData(mousex, mousey);

            if (Settings.gridLetters.Value)
            {
                FontRenderer.SetActive(FONT);
                FontRenderer.SetColor(Style.Secondary);
                FontRenderer.RenderData(FONT, keybindVerts);
            }

            noteConstant.Render();
            if (Settings.approachSquares.Value)
                noteApproach.Render();
            noteSelect.Render();
            noteHover.Render();
            if (Settings.autoplay.Value)
                autoplayCursor.Render();
            notePreview.Render();

            if (Mapping.Current.RenderMode == ObjectRenderMode.Special)
            {
                beatConstant.Render();
                if (Settings.approachSquares.Value)
                    beatApproach.Render();
            }
        }

        public override void PostRender(float mousex, float mousey, float frametime)
        {
            base.PostRender(mousex, mousey, frametime);

            if (Settings.gridNumbers.Value)
            {
                FontRenderer.SetActive(FONT);
                FontRenderer.SetColor(Style.Quaternary);
                FontRenderer.RenderData(FONT, gridNumData, gridNumAlphaData);
            }

            if (bezierPositions.Count > 0)
            {
                List<float> verts = [];

                for (int i = 1; i < bezierPositions.Count; i++)
                {
                    Vector2 prev = bezierPositions[i - 1];
                    Vector2 cur = bezierPositions[i];

                    verts.AddRange(GLVerts.Line(prev.X, prev.Y, cur.X, cur.Y, 1, Style.Quaternary));
                }

                shader.Enable();
                GLState.BufferData(bezierPreviewLineVBO, verts.ToArray());
                GLState.DrawTriangles(bezierPreviewLineVAO, 0, verts.Count / 6);
            }
        }

        public override void MouseMove(float x, float y)
        {
            base.MouseMove(x, y);

            Vector2 hover = MouseToGridSpace(x, y);
            Vector2 unclamped = MouseToGridSpaceUnclamped(x, y);

            if (Math.Abs(hover.X - unclamped.X) <= 1 && Math.Abs(hover.Y - unclamped.Y) <= 1)
                hoveringCell = hover;
            else
                hoveringCell = null;

            if (placing && !MusicPlayer.IsPlaying)
            {
                if (hover != lastPlacedPos)
                {
                    long ms = Timing.GetClosestBeat(Settings.currentTime.Value.Value);

                    Note note = new(hover.X, hover.Y, ms >= 0 ? ms : (long)Settings.currentTime.Value.Value);
                    Mapping.Current.Notes.Modify_Add("ADD NOTE", note);

                    if (Settings.autoAdvance.Value)
                        Timing.Advance();

                    lastPlacedPos = hover;
                }
            }
            else if (draggingXY.Count > 0)
            {
                XYMapObject drag = draggingXY[0];

                float xOffset = hover.X - drag.X;
                float yOffset = hover.Y - drag.Y;

                if (xOffset == 0 && yOffset == 0)
                    return;

                float minX = drag.X;
                float maxX = drag.X;
                float minY = drag.Y;
                float maxY = drag.Y;

                foreach (XYMapObject obj in draggingXY)
                {
                    minX = Math.Min(minX, obj.X);
                    maxX = Math.Max(maxX, obj.X);
                    minY = Math.Min(minY, obj.Y);
                    maxY = Math.Max(maxY, obj.Y);
                }

                xOffset = Math.Max(CellBoundsX.X, minX + xOffset) - minX;
                xOffset = Math.Min(CellBoundsX.Y, maxX + xOffset) - maxX;
                yOffset = Math.Max(CellBoundsY.X, minY + yOffset) - minY;
                yOffset = Math.Min(CellBoundsY.Y, maxY + yOffset) - maxY;

                for (int i = 1; i < draggingXY.Count; i++)
                {
                    draggingXY[i].X += xOffset;
                    draggingXY[i].Y += yOffset;
                }
            }
        }

        public override void MouseClickLeft(float x, float y)
        {
            base.MouseClickLeft(x, y);

            if ((hoveringXY == null && Settings.ClickMode == ClickMode.Both) || Settings.ClickMode == ClickMode.Place)
            {
                if (Mapping.Current.RenderMode != ObjectRenderMode.Notes || hoveringCell == null)
                    return;
                if (Windowing.HoveringInteractive(this))
                    return;

                long ms = Timing.GetClosestBeat(Settings.currentTime.Value.Value);
                Vector2 toPlace = hoveringCell.Value;

                Note note = new(toPlace.X, toPlace.Y, ms >= 0 ? ms : (long)Settings.currentTime.Value.Value);
                Mapping.Current.Notes.Modify_Add("ADD NOTE", note);

                if (Settings.autoAdvance.Value && !MusicPlayer.IsPlaying)
                    Timing.Advance();

                lastPlacedPos = toPlace;
                placing = true;
            }
            else if (hoveringXY != null)
            {
                ObjectList<XYMapObject> objects = hoveringXY is Note ? new(Mapping.Current.Notes.Cast<XYMapObject>()) : new(Mapping.Current.SpecialObjects.Where(n => n is XYMapObject).Cast<XYMapObject>());
                List<XYMapObject> selected = new(Mapping.Current.SelectedObjects.Where(n => n is XYMapObject).Cast<XYMapObject>());

                if (MainWindow.Instance.ShiftHeld)
                {
                    XYMapObject first = selected.FirstOrDefault() ?? hoveringXY;
                    XYMapObject last = hoveringXY;

                    long min = Math.Min(first.Ms, last.Ms);
                    long max = Math.Max(first.Ms, last.Ms);
                    (int low, int high) = objects.SearchRange(min, max);

                    selected = objects.Take(new Range(low, high)).ToList();
                    selected.Remove(first);
                    selected.Insert(0, first);
                }
                else if (MainWindow.Instance.CtrlHeld)
                {
                    if (hoveringXY.Selected)
                        selected.Remove(hoveringXY);
                    else
                        selected.Add(hoveringXY);
                }
                else if (selected.Count == 0)
                    selected = [hoveringXY];

                Mapping.Current.SelectedObjects = new(selected);

                if (hoveringXY.Selected)
                {
                    draggingXY = [hoveringXY, .. selected];
                    dragCellStart = (hoveringXY.X, hoveringXY.Y);
                }

                if (!Hovering)
                    InvokeLeftClick(new(x, y));
            }
        }

        public override void MouseUpLeft(float x, float y)
        {
            base.MouseUpLeft(x, y);

            placing = false;
            lastPlacedPos = null;

            if (draggingXY.Count > 0)
            {
                Vector2 cellDiff = (draggingXY[0].X, draggingXY[0].Y) - dragCellStart;

                if (cellDiff.LengthSquared > 0)
                {
                    for (int i = 1; i < draggingXY.Count; i++)
                    {
                        draggingXY[i].X -= cellDiff.X;
                        draggingXY[i].Y -= cellDiff.Y;
                    }

                    Mapping.Current.Notes.Modify_Edit($"MOVE {(draggingXY[0] is Note ? "NOTE" : "OBJECT")}[S]", n =>
                    {
                        n.X += cellDiff.X;
                        n.Y += cellDiff.Y;
                    });
                }

                draggingXY = [];
            }
        }
    }
}
