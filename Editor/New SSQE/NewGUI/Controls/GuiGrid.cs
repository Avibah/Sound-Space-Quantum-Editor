using New_SSQE.Audio;
using New_SSQE.GUI.Font;
using New_SSQE.GUI.Shaders;
using New_SSQE.Maps;
using New_SSQE.Objects;
using New_SSQE.Objects.Managers;
using New_SSQE.Preferences;
using OpenTK.Graphics;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing;

namespace New_SSQE.NewGUI.Controls
{
    internal enum GridRenderMode
    {
        Notes,
        Special
    }

    internal class GuiGrid : InteractiveControl
    {
        private const string FONT = "main";

        private readonly VertexArrayHandle bezierPreviewLineVAO;
        private readonly BufferHandle bezierPreviewLineVBO;

        private float CellSize => rect.Width / 3f;
        private float NoteSize => CellSize * 0.75f;
        private float PreviewSize => CellSize * 0.65f;
        private float CellGap => (CellSize - NoteSize) / 2f;

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

        private Note? hoveringNote;
        private List<Note> draggingNotes = [];
        private Vector2 dragCellStart;

        private Vector4[] keybindVerts = [];
        private Vector4[] gridNumData = [];
        private float[] gridNumAlphaData = [];
        private List<Vector2> bezierPositions = [];

        public GridRenderMode RenderMode;
        public Vector2 CellBounds => Settings.enableQuantum.Value ? (-0.85f, 2.85f) : (0, 2);

        public GuiGrid(float x, float y, float w, float h, GridRenderMode gridRenderMode = GridRenderMode.Notes) : base(x, y, w, h)
        {
            RenderMode = gridRenderMode;
            (bezierPreviewLineVAO, bezierPreviewLineVBO) = GLState.NewVAO_VBO(2, 4);

            autoplayCursor = Instancing.Generate("grid_autoplayCursor", Shader.ScalingProgram);

            noteConstant = Instancing.Generate("grid_noteConstant", Shader.ScalingProgram);
            noteApproach = Instancing.Generate("grid_noteApproach", Shader.ScalingProgram);
            noteHover = Instancing.Generate("grid_noteHover", Shader.TimelineProgram);
            noteSelect = Instancing.Generate("grid_noteSelect", Shader.TimelineProgram);
            notePreview = Instancing.Generate("grid_notePreview", Shader.TimelineProgram);

            beatConstant = Instancing.Generate("grid_beatConstant", Shader.ColoredProgram);
            beatApproach = Instancing.Generate("grid_beatApproach", Shader.ColoredProgram);
        }

        private Vector2 MouseToGridSpace(float mousex, float mousey)
        {
            bool quantum = Settings.enableQuantum.Value;

            float increment = quantum ? (Settings.quantumSnapping.Value.Value + 3) / 3 : 1;
            float x = (mousex - rect.X - rect.Width / 2) / rect.Width * 3 + 1 / increment;
            float y = (mousey - rect.Y - rect.Height / 2) / rect.Height * 3 + 1 / increment;

            if (Settings.quantumGridSnap.Value || !quantum)
            {
                x = (float)Math.Floor((x + 1 / increment / 2) * increment) / increment;
                y = (float)Math.Floor((y + 1 / increment / 2) * increment) / increment;
            }

            x = MathHelper.Clamp(x - 1 / increment + 1, CellBounds.X, CellBounds.Y);
            y = MathHelper.Clamp(y - 1 / increment + 1, CellBounds.X, CellBounds.Y);

            return (2 - x, 2 - y);
        }

        private void UpdateInstanceData(float mousex, float mousey)
        {
            float currentTime = Settings.currentTime.Value.Value;
            float approachRate = (Settings.approachRate.Value.Value + 1) / 10;
            float maxMs = currentTime + 1000 / approachRate;

            int colorCount = Settings.noteColors.Value.Count;

            List<(Vector3, string)> gridNumStrings = [];
            int gridNumLength = 0;

            ObjectList<Note> notes = CurrentMap.Notes;
            Note last = notes.FirstOrDefault() ?? new(1, 1, 0);
            Note? next = null;

            (int low, int high) = notes.SearchRange(currentTime, maxMs);
            int range = high - low;

            Vector4[] noteConstants = new Vector4[range];
            Vector4[] noteApproaches = new Vector4[range];
            Vector4 noteHovers = Vector4.Zero;
            Vector4[] noteSelects = new Vector4[notes.Selected.Count];

            int selectIndex = 0;

            if (range > 0)
            {
                if (notes[low].Ms <= currentTime)
                    last = notes[low];
                else if (low > 0)
                    last = notes[low - 1];
                next = notes[low];
            }

            bool gridNumbers = Settings.gridNumbers.Value;
            hoveringNote = null;

            for (int i = low; i < high; i++)
            {
                Note note = notes[i];
                int c = i % colorCount;

                float x = rect.X + (2 - note.X) * CellSize + CellGap;
                float y = rect.Y + (2 - note.Y) * CellSize + CellGap;

                float progress = (float)Math.Min(1, Math.Pow(1 - Math.Min(1, (note.Ms - currentTime) * approachRate / 750), 2));
                float approachSize = 4 + NoteSize + NoteSize * (1 - progress) * 2 + 0.5f;

                noteConstants[i - low] = (x, y, 2 + progress, 5);
                noteApproaches[i - low] = (x - approachSize / 2 + NoteSize / 2, y - approachSize / 2 + NoteSize / 2, 2 * (int)approachSize + progress, c);

                if (hoveringCell == (note.X, note.Y) && Math.Abs(x - (mousex + CellSize / 2)) <= NoteSize / 2 && Math.Abs(y - (mousey + CellSize / 2)) <= NoteSize / 2)
                    hoveringNote ??= note;
                if (hoveringNote == note)
                    noteHovers = (x, y, 1, 5);
                if (note.Selected)
                    noteSelects[selectIndex++] = (x, y, progress, 6);

                if (gridNumbers)
                {
                    string numText = $"{i + 1:##,###}";
                    int width = FontRenderer.GetWidth(numText, 28, FONT);
                    int height = FontRenderer.GetHeight(28, FONT);

                    gridNumStrings.Add(((x, y, 1 - progress), numText));
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

                FontRenderer.PrintInto(gridNumData, offset, data.Item1.X, data.Item1.Y, data.Item2, 28, FONT);

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

                float lastX = rect.X + (2 - last.X) * CellSize;
                float lastY = rect.Y + (2 - last.Y) * CellSize;

                float nextX = rect.X + (2 - next.X) * CellSize;
                float nextY = rect.Y + (2 - next.Y) * CellSize;

                float x = CellSize / 2 + lastX + (nextX - lastX) * progress;
                float y = CellSize / 2 + lastY + (nextY - lastY) * progress;

                autoplayCursor.UploadData([(x - width / 2, y - width / 2, 2 * (int)width + 1, 4)]);
            }

            if (RenderMode == GridRenderMode.Special)
            {
                List<Vector4> beatConstants = [];
                List<Vector4> beatApproaches = [];

                ObjectList<MapObject> objects = CurrentMap.SpecialObjects;
                (low, high) = objects.SearchRange(currentTime, maxMs);

                for (int i = low; i < high; i++)
                {
                    MapObject obj = objects[i];
                    float progress = (float)Math.Min(1, (float)Math.Pow(1 - Math.Min(1, (obj.Ms - currentTime) * approachRate / 750), 2));

                    switch (obj.ID)
                    {
                        case 12 when obj is Beat beat:
                            float approachSize = 4 + rect.Width + rect.Width * (1 - progress) + 0.5f;
                            
                            beatConstants.Add((rect.X, rect.Y, 1, progress));
                            beatApproaches.Add((rect.X + rect.Width / 2 - approachSize / 2, rect.Y + rect.Height / 2 - approachSize / 2, approachSize, progress));
                            break;
                    }
                }

                beatConstant.UploadData(beatConstants.ToArray());
                beatApproach.UploadData(beatApproaches.ToArray());
            }

            List<Vector4> notePreviews = [];
            bezierPositions = [];

            if (hoveringCell != null)
            {
                Vector2 hover = hoveringCell ?? Vector2.One;
                float x = rect.X + (2 - hover.X) * CellSize + CellGap;
                float y = rect.Y + (2 - hover.Y) * CellSize + CellGap;

                notePreviews.Add((x, y, 1, 9));
            }

            List<Note> bezierNodes = CurrentMap.BezierNodes;

            if (bezierNodes.Count > 0)
            {
                List<Note> result = [];
                List<int> anchored = [0];

                for (int i = 1; i < bezierNodes.Count; i++)
                    if (bezierNodes[i].Anchored)
                        anchored.Add(i);

                if (!anchored.Contains(bezierNodes.Count - 1))
                    anchored.Add(bezierNodes.Count - 1);

                for (int i = 1; i < anchored.Count; i++)
                {
                    List<Note> toDraw = [];

                    for (int j = anchored[i - 1]; j <= anchored[i]; j++)
                        toDraw.Add(bezierNodes[j]);

                    result.AddRange(Patterns.DrawBezier(toDraw, (int)(Settings.bezierDivisor.Value + 0.5f)));
                }

                for (int i = 0; i < result.Count; i++)
                {
                    Note note = result[i];

                    float x = rect.X + (2 - note.X) * CellSize + CellGap;
                    float y = rect.Y + (2 - note.Y) * CellSize + CellGap;

                    notePreviews.Add((x, y, 1, 2));
                    bezierPositions.Add((x, y));
                }
            }

            notePreview.UploadData(notePreviews.ToArray());
        }

        public override float[] Draw()
        {
            List<float> autoplayVerts = [];
            autoplayVerts.AddRange(GLVerts.Outline(0, 0, 1, 1, 0.075f, 1f, 1f, 1f, 1f));
            autoplayVerts.AddRange(GLVerts.Rect(0, 0, 1, 1, 1f, 1f, 1f, 0.25f));
            autoplayCursor.UploadStaticData(autoplayVerts.ToArray());

            List<float> noteVerts = [];
            noteVerts.AddRange(GLVerts.Outline(0, 0, NoteSize, NoteSize, 2, 1f, 1f, 1f, 1f));
            noteVerts.AddRange(GLVerts.Rect(0, 0, NoteSize, NoteSize, 1f, 1f, 1f, 0.15f));
            noteConstant.UploadStaticData(noteVerts.ToArray());

            noteApproach.UploadStaticData(GLVerts.Outline(0, 0, 1, 1, 0.0125f, 1f, 1f, 1f, 1f));
            noteHover.UploadStaticData(GLVerts.Outline(-4, -4, NoteSize + 8, NoteSize + 8, 2, 1f, 1f, 1f, 0.25f));
            noteSelect.UploadStaticData(GLVerts.Outline(-4, -4, NoteSize + 8, NoteSize + 8, 2, 1f, 1f, 1f, 1f));

            List<float> previewVerts = [];
            previewVerts.AddRange(GLVerts.Outline(0, 0, PreviewSize, PreviewSize, 2, 1f, 1f, 1f, 0.09375f));
            previewVerts.AddRange(GLVerts.Rect(0, 0, PreviewSize, PreviewSize, 1f, 1f, 1f, 0.125f));
            notePreview.UploadStaticData(previewVerts.ToArray());

            beatConstant.UploadStaticData(GLVerts.Outline(-5, -5, rect.Width + 10, rect.Width + 10, 3, Settings.color5.Value));
            beatApproach.UploadStaticData(GLVerts.Outline(0, 0, 1, 1, 0.0075f, Settings.color5.Value));

            List<float> verts = [];
            verts.AddRange(GLVerts.Rect(rect, 0.15f, 0.15f, 0.15f, Settings.gridOpacity.Value / 255f));
            verts.AddRange(GLVerts.Outline(rect, 1, 0.2f, 0.2f, 0.2f));

            for (int i = 0; i < 3; i++)
            {
                float x = rect.X + rect.Width / 3 * i;
                float y = rect.Y + rect.Height / 3 * i;

                verts.AddRange(GLVerts.Line(x, rect.Y, x, rect.Bottom, 1, 0.05f, 0.05f, 0.05f));
                verts.AddRange(GLVerts.Line(rect.X, y, rect.Right, y, 1, 0.05f, 0.05f, 0.05f));
            }

            float divisor = Settings.quantumGridLines.Value ? Settings.quantumSnapping.Value.Value + 3 : 3;
            float offset = Math.Round(divisor) % 2 == 0 ? 0.5f : 1f;

            for (int i = (int)(2 * offset); i <= divisor; i++)
            {
                float x = rect.X + rect.Width / divisor * (i - offset);
                float y = rect.Y + rect.Height / divisor * (i - offset);

                verts.AddRange(GLVerts.Line(x, rect.Y, x, rect.Bottom, 1, 0.2f, 0.2f, 0.2f));
                verts.AddRange(GLVerts.Line(rect.X, y, rect.Right, y, 1, 0.2f, 0.2f, 0.2f));
            }

            List<Vector4> keyVerts = [];

            if (Settings.gridLetters.Value)
            {
                Dictionary<Keys, Tuple<int, int>> keybinds = new(MainWindow.Instance.KeyMapping);

                foreach (KeyValuePair<Keys, Tuple<int, int>> key in keybinds)
                {
                    string letter = key.Key.ToString().Replace("KeyPad", "");
                    Tuple<int, int> pos = key.Value;

                    float x = rect.X + key.Value.Item1 * CellSize + CellSize / 2;
                    float y = rect.Y + key.Value.Item2 * CellSize + CellSize / 2;

                    int width = FontRenderer.GetWidth(letter, 38, FONT);
                    int height = FontRenderer.GetHeight(38, FONT);

                    keyVerts.AddRange(FontRenderer.Print(x - width / 2f, y - height / 2f, letter, 38, FONT));
                }
            }

            keybindVerts = keyVerts.ToArray();

            return verts.ToArray();
        }

        public override void PreRender(float mousex, float mousey, float frametime)
        {
            base.PreRender(mousex, mousey, frametime);

            if (placing && MusicPlayer.IsPlaying)
            {
                float currentTIme = Settings.currentTime.Value.Value;
                long ms = Timing.GetClosestBeat(currentTIme);

                if (ms > 0 && ms <= currentTIme && CurrentMap.Notes.FirstOrDefault(n => Math.Abs(n.Ms - ms) < 2) == null)
                {
                    Vector2 pos = MouseToGridSpace(mousex, mousey);
                    NoteManager.Add("ADD NOTE", new Note(pos.X, pos.Y, ms));
                }
            }
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            base.Render(mousex, mousey, frametime);
            UpdateInstanceData(mousex, mousey);

            FontRenderer.SetActive(FONT);
            FontRenderer.SetColor(51, 51, 51);
            FontRenderer.RenderData(FONT, keybindVerts);

            noteConstant.Render();
            if (Settings.approachSquares.Value)
                noteApproach.Render();
            noteHover.Render();
            noteSelect.Render();
            if (Settings.autoplay.Value)
                autoplayCursor.Render();
            notePreview.Render();

            if (RenderMode == GridRenderMode.Special)
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
                FontRenderer.SetColor(Color.White);
                FontRenderer.RenderData(FONT, gridNumData, gridNumAlphaData);
            }

            if (bezierPositions.Count > 0)
            {
                List<float> verts = [];

                for (int i = 1; i < bezierPositions.Count; i++)
                {
                    Vector2 prev = bezierPositions[i - 1];
                    Vector2 cur = bezierPositions[i];

                    verts.AddRange(GLVerts.Line(prev.X, prev.Y, cur.X, cur.Y, 1, 1f, 1f, 1f, 1f));
                }

                GLState.EnableProgram(Shader.Program);
                GLState.BufferData(bezierPreviewLineVBO, verts.ToArray());
                GLState.DrawTriangles(bezierPreviewLineVAO, 0, verts.Count / 6);
            }
        }

        public override void MouseMove(float x, float y)
        {
            base.MouseMove(x, y);

            hoveringCell = MouseToGridSpace(x, y);
            Vector2 hover = hoveringCell ?? Vector2.One;

            if (placing && !MusicPlayer.IsPlaying)
            {
                if (hover != lastPlacedPos)
                {
                    long ms = Timing.GetClosestBeat(Settings.currentTime.Value.Value);

                    Note note = new(hover.X, hover.Y, ms >= 0 ? ms : (long)Settings.currentTime.Value.Value);
                    NoteManager.Add("ADD NOTE", note);

                    if (Settings.autoAdvance.Value)
                        Timing.Advance();

                    lastPlacedPos = hover;
                }
            }
            else if (draggingNotes.Count > 0)
            {
                Note drag = draggingNotes[0];
                
                float xOffset = hover.X - drag.X;
                float yOffset = hover.Y - drag.Y;

                if (xOffset == 0 && yOffset == 0)
                    return;

                float minX = drag.X;
                float maxX = drag.X;
                float minY = drag.Y;
                float maxY = drag.Y;

                foreach (Note note in draggingNotes)
                {
                    minX = Math.Min(minX, note.X);
                    maxX = Math.Max(maxX, note.X);
                    minY = Math.Min(minY, note.Y);
                    maxY = Math.Max(maxY, note.Y);
                }

                xOffset = Math.Max(CellBounds.X, minX + xOffset) - minX;
                xOffset = Math.Min(CellBounds.X, maxX + xOffset) - maxX;
                yOffset = Math.Max(CellBounds.Y, minY + yOffset) - minY;
                yOffset = Math.Min(CellBounds.Y, maxY + yOffset) - maxY;

                foreach (Note note in draggingNotes)
                {
                    note.X += xOffset;
                    note.Y += yOffset;
                }
            }
        }

        public override void MouseClickLeft(float x, float y)
        {
            base.MouseClickLeft(x, y);

            if (hoveringNote == null || (Settings.separateClickTools.Value && !Settings.selectTool.Value))
            {
                long ms = Timing.GetClosestBeat(Settings.currentTime.Value.Value);
                Vector2 toPlace = hoveringCell ?? Vector2.One;

                Note note = new(toPlace.X, toPlace.Y, ms >= 0 ? ms : (long)Settings.currentTime.Value.Value);
                NoteManager.Add("ADD NOTE", note);

                if (Settings.autoAdvance.Value && !MusicPlayer.IsPlaying)
                    Timing.Advance();

                lastPlacedPos = toPlace;
                placing = true;
            }
            else if (hoveringNote != null)
            {
                List<Note> selected = CurrentMap.Notes.Selected.ToList();

                if (MainWindow.Instance.ShiftHeld)
                {
                    Note first = selected.FirstOrDefault() ?? hoveringNote;
                    Note last = hoveringNote;

                    long min = Math.Min(first.Ms, last.Ms);
                    long max = Math.Max(first.Ms, last.Ms);
                    (int low, int high) = CurrentMap.Notes.SearchRange(min, max);

                    selected = CurrentMap.Notes.Take(new Range(low, high)).ToList();
                    selected.Insert(0, first);
                }
                else if (MainWindow.Instance.CtrlHeld)
                {
                    if (hoveringNote.Selected)
                        selected.Remove(hoveringNote);
                    else
                        selected.Add(hoveringNote);
                }
                else if (selected.Count == 0)
                    selected = [hoveringNote];

                CurrentMap.Notes.Selected = new(selected);

                if (hoveringNote.Selected)
                {
                    draggingNotes = [hoveringNote, ..selected];
                    dragCellStart = selected.Count > 0 ? (selected[0].X, selected[0].Y) : Vector2.One;
                }
            }
        }

        public override void MouseUpLeft(float x, float y)
        {
            base.MouseUpLeft(x, y);

            placing = false;
            lastPlacedPos = null;

            if (draggingNotes.Count > 0)
            {
                Vector2 cellDiff = (draggingNotes[0].X, draggingNotes[0].Y) - dragCellStart;
                
                if (cellDiff.LengthSquared > 0)
                {
                    foreach (Note note in draggingNotes)
                    {
                        note.X -= cellDiff.X;
                        note.Y -= cellDiff.Y;
                    }

                    NoteManager.Edit("MOVE NOTE[S]", n =>
                    {
                        n.X += cellDiff.X;
                        n.Y += cellDiff.Y;
                    });
                }

                draggingNotes = [];
            }
        }
    }
}
