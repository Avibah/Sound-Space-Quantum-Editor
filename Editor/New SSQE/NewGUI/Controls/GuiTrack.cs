using New_SSQE.Audio;
using New_SSQE.NewGUI.Font;
using New_SSQE.NewGUI.Shaders;
using New_SSQE.NewMaps;
using New_SSQE.Objects;
using New_SSQE.Objects.Managers;
using New_SSQE.Preferences;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Drawing;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiTrack : InteractiveControl
    {
        private static float MS_TO_PX => CurrentMap.Zoom * 0.5f;
        private float NoteSize => (rect.Height - Settings.trackHeight.Value.Value) * 0.65f;
        private float CellGap => (rect.Height - Settings.trackHeight.Value.Value - NoteSize) / 2f;
        private float CellPos(float x) => x * NoteSize * 0.2f + NoteSize * 0.1f;

        private bool selecting = false;
        private float selectMsStart;
        private float selectMsEnd;

        private MapObject? hoveringObject;
        private MapObject? hoveringDuration;
        private List<MapObject> draggingObjects = [];
        private MapObject? draggingDuration;
        private float dragMsStart;
        private float dragXStart;
        private bool shouldReplay;

        private TimingPoint? selectedPoint;
        private MapObject? lastPlayedObject;
        private float? lastPlayedTick;

        private readonly Instance selectBox;
        private readonly Instance staticLine;

        private readonly Instance noteConstant;
        private readonly Instance noteLocation;
        private readonly Instance noteHover;
        private readonly Instance noteSelect;

        private readonly Instance objConstant;
        private readonly Instance objIcon;
        private readonly Instance objHover;
        private readonly Instance objSelect;

        private readonly Instance objDurationMarker;
        private readonly Instance objDurationBar;
        private readonly Instance objDurationHover;
        private readonly Instance objDurationSelect;

        private readonly Instance textLine;
        private readonly Instance dragLine;

        private readonly Instance bpmStart;
        private readonly Instance bpmBeat;
        private readonly Instance bpmHalfBeat;
        private readonly Instance bpmSubBeat;

        private readonly Instance bpmHover;
        private readonly Instance bpmSelect;

        private readonly Texture spriteSheet;

        private Vector4[] color1Data = [];
        private Vector4[] color2Data = [];

        public float StartPositionRelative;
        public float EndPositionRelative;

        public GuiTrack(float x, float y, float w, float h) : base(x, y, w, h)
        {
            selectBox = Instancing.Generate("track_selectBox", Shader.InstancedMainExtra, true);
            staticLine = Instancing.Generate("track_staticLine", Shader.InstancedMain);

            noteConstant = Instancing.Generate("track_noteConstant", Shader.InstancedObject);
            noteLocation = Instancing.Generate("track_noteLocation", Shader.InstancedObject);
            noteHover = Instancing.Generate("track_noteHover", Shader.InstancedMain);
            noteSelect = Instancing.Generate("track_noteSelect", Shader.InstancedMain);

            objConstant = Instancing.Generate("track_objConstant", Shader.InstancedObject);
            objIcon = Instancing.Generate("track_objIcon", Shader.Sprite);
            objHover = Instancing.Generate("track_objHover", Shader.InstancedMain);
            objSelect = Instancing.Generate("track_objSelect", Shader.InstancedMain);

            objDurationMarker = Instancing.Generate("track_objDurationMarker", Shader.InstancedObject);
            objDurationBar = Instancing.Generate("track_objDurationBar", Shader.InstancedObjectExtra, true);
            objDurationHover = Instancing.Generate("track_objDurationhover", Shader.InstancedMain);
            objDurationSelect = Instancing.Generate("track_objDurationSelect", Shader.InstancedMain);

            textLine = Instancing.Generate("track_textLine", Shader.InstancedMain);
            dragLine = Instancing.Generate("track_dragLine", Shader.InstancedMain);

            bpmStart = Instancing.Generate("track_bpmStart", Shader.InstancedMain);
            bpmBeat = Instancing.Generate("track_bpmBeat", Shader.InstancedMain);
            bpmHalfBeat = Instancing.Generate("track_bpmHalfBeat", Shader.InstancedMain);
            bpmSubBeat = Instancing.Generate("track_bpmSubBeat", Shader.InstancedMain);

            bpmHover = Instancing.Generate("track_bpmHover", Shader.InstancedMainExtra, true);
            bpmSelect = Instancing.Generate("track_bpmSelect", Shader.InstancedMainExtra, true);

            spriteSheet = new("sprites", null, true, TextureUnit.Texture2);
        }

        private void RenderText(List<(float, string)> text, Vector4[] data, int objCount)
        {
            int offset = 0;

            for (int i = 0; i < text.Count; i++)
            {
                string str = text[i].Item2;

                float x = text[i].Item1;
                float y = i >= objCount ? rect.Height - 2 : rect.Height + 26;

                FontRenderer.PrintInto(data, offset, x, y, str, 20, "main");
                offset += str.Length;
            }
        }

        private void UpdateInstanceData(float mousex, float mousey)
        {
            float currentTime = Settings.currentTime.Value.Value;
            float totalTime = Settings.currentTime.Value.Max;
            float sfxOffset = Settings.sfxOffset.Value;

            float currentPos = currentTime * MS_TO_PX;
            float cursorPos = rect.Width * Settings.cursorPos.Value.Value / 100f;
            float endPos = cursorPos - currentPos + totalTime * MS_TO_PX + 1;

            float minMs = (-cursorPos - NoteSize) / MS_TO_PX + currentTime;
            float maxMs = (rect.Width - cursorPos) / MS_TO_PX + currentTime;

            int colorCount = Settings.noteColors.Value.Count;
            float? lastRenderedText = null;
            hoveringObject = null;

            MapObject? toPlay = null;
            int objCount = 0;

            List<(float, string)> color1Strings = [];
            List<(float, string)> color2Strings = [];

            int color1Length = 0;
            int color2Length = 0;

            if (CurrentMap.RenderMode == ObjectRenderMode.Notes)
            {
                ObjectList<Note> notes = CurrentMap.Notes;
                (int low, int high) = notes.SearchRange(minMs, maxMs);
                int range = high - low;

                Vector4[] noteConstants = new Vector4[range];
                Vector4[] noteLocations = new Vector4[range];
                Vector4 noteHovers = Vector4.Zero;
                Vector4[] noteSelects = new Vector4[notes.Selected.Count];
                int selectIndex = 0;

                Vector4[] textLines = new Vector4[range];
                Vector4[] dragLines = new Vector4[noteSelects.Length];

                for (int i = low; i < high; i++)
                {
                    Note note = notes[i];
                    float x = cursorPos - currentPos + note.Ms * MS_TO_PX;
                    float a = note.Ms < currentTime - 1 ? 0.35f : 1f;

                    float gridX = x + CellPos(2 - note.X);
                    float gridY = CellGap + CellPos(2 - note.Y);

                    noteConstants[i - low] = (x, 0, 1, 2 * (i % colorCount) + a);
                    noteLocations[i - low] = (gridX, gridY, 1, 2 * (i % colorCount) + a);
                    textLines[i - low] = (x, 0, 1, 2 * 4 + a);

                    if (mousex > x && mousex < x + NoteSize && mousey > CellGap && mousey < CellGap + NoteSize)
                        hoveringObject ??= note;

                    if (hoveringObject == note)
                        noteHovers = (x, 0, 1, 2 * 5 + 1);
                    else if (note.Selected)
                    {
                        if (Dragging)
                            dragLines[selectIndex++] = (cursorPos - currentPos + note.DragStartMs * MS_TO_PX, 0, 1, 2 * 7 + 1);
                        else
                            noteSelects[selectIndex++] = (x, 0, 1, 2 * 6 + 1);
                    }

                    if (note.Ms <= currentTime - sfxOffset)
                        toPlay = note;

                    if (x - 8 > (lastRenderedText ?? float.MinValue))
                    {
                        string noteStr = $"Note {i + 1:##,###}";
                        string msStr = $"{note.Ms:##,##0}ms";

                        color1Strings.Add((x + 3, noteStr));
                        color1Length += noteStr.Length;

                        color2Strings.Add((x + 3, msStr));
                        color2Length += msStr.Length;

                        lastRenderedText = x;
                        objCount++;
                    }
                }

                noteConstant.UploadData(noteConstants);
                noteLocation.UploadData(noteLocations);
                noteHover.UploadData([noteHovers]);
                noteSelect.UploadData(noteSelects);

                textLine.UploadData(textLines);
                dragLine.UploadData(dragLines);
            }
            else
            {
                ObjectList<MapObject> objects = CurrentMap.RenderMode == ObjectRenderMode.VFX ? CurrentMap.VfxObjects : CurrentMap.SpecialObjects;
                List<int> indices = [];

                Vector4[] objConstants = new Vector4[objects.Count];
                Vector4[] objIcons = new Vector4[objects.Count];
                Vector3[] objIconsSecondary = new Vector3[objects.Count];
                Vector4 objHovers = Vector4.Zero;
                Vector4[] objSelects = new Vector4[objects.Selected.Count];
                int selectIndex = 0;

                Vector4[] objDurationMarkers = new Vector4[objects.Count];
                Vector4[] objDurationBars = new Vector4[objects.Count];
                Vector3[] objDurationBarsSecondary = new Vector3[objects.Count];
                Vector4 objDurationHovers = Vector4.Zero;
                Vector4 objDurationSelects = Vector4.Zero;

                Vector4[] textLines = new Vector4[objects.Count];
                Vector4[] dragLines = new Vector4[objSelects.Length];

                for (int i = 0; i < objects.Count; i++)
                {
                    MapObject obj = objects[i];
                    for (int j = indices.Count; j <= obj.ID; j++)
                        indices.Add(0);
                    indices[obj.ID]++;

                    if (obj.Duration + obj.Ms < minMs)
                        continue;
                    if (obj.Ms > maxMs)
                        break;

                    float x = cursorPos - currentPos + obj.Ms * MS_TO_PX;
                    float a = obj.Ms < currentTime - 1 ? 0.35f : 1f;

                    int c = i % colorCount;

                    objConstants[i] = (x, 0, 1, 2 * c + a);
                    objIcons[i] = (x, 0, 1, a);
                    objIconsSecondary[i] = (obj.ID, c, 0);
                    textLines[i] = (x, 0, 1, 2 * 4 + a);

                    if (obj.HasDuration)
                    {
                        float width = obj.Duration * MS_TO_PX;

                        objDurationBars[i] = (x + NoteSize, 0, 1, 2 * c + a);
                        objDurationBarsSecondary[i] = (Math.Max(width - NoteSize, 0), 1, 0);
                        objDurationMarkers[i] = (x + width, 0, 1, 2 * c + a);

                        if (Math.Abs(mousex - x - width) < NoteSize / 4 && Math.Abs(mousey - CellGap - NoteSize / 2) < NoteSize / 4)
                            hoveringDuration ??= obj;

                        if (hoveringDuration == obj)
                            objDurationHovers = (x + width, 0, 1, 2 * 5 + 1);
                        else if (draggingDuration == obj)
                            objDurationSelects = (x + width, 0, 1, 2 * 6 + 1);
                    }

                    if (mousex > x && mousex < x + NoteSize && mousey > CellGap && mousey < CellGap + NoteSize)
                        hoveringObject ??= obj;

                    if (hoveringObject == obj)
                        objHovers = (x, 0, 1, 2 * 5 + 1);
                    else if (obj.Selected)
                    {
                        if (Dragging)
                            dragLines[selectIndex++] = (cursorPos - currentPos + obj.DragStartMs * MS_TO_PX, 0, 1, 2 * 7 + 1);
                        else
                            objSelects[selectIndex++] = (x, 0, 1, 2 * 6 + 1);
                    }

                    if (CurrentMap.RenderMode == ObjectRenderMode.Special && obj.Ms <= currentTime - sfxOffset)
                        toPlay = obj;

                    if (x - 8 > (lastRenderedText ?? float.MinValue))
                    {
                        string objStr = $"{obj.Name ?? "null"} {indices[obj.ID]:##,###}";
                        string msStr = $"{obj.Ms:##,##0}ms";

                        color1Strings.Add((x + 3, objStr));
                        color1Length += objStr.Length;

                        color2Strings.Add((x + 3, msStr));
                        color2Length += msStr.Length;

                        lastRenderedText = x;
                        objCount++;
                    }
                }

                objConstant.UploadData(objConstants);
                objIcon.UploadData(objIcons, objIconsSecondary);
                objHover.UploadData([objHovers]);
                objSelect.UploadData(objSelects);

                objDurationMarker.UploadData(objDurationMarkers);
                objDurationBar.UploadData(objDurationBars, objDurationBarsSecondary);
                objDurationHover.UploadData([objDurationHovers]);
                objDurationSelect.UploadData([objDurationSelects]);

                textLine.UploadData(textLines);
                dragLine.UploadData(dragLines);
            }

            float selectXStart = cursorPos - currentPos + selectMsStart * MS_TO_PX;
            float selectXEnd = cursorPos - currentPos + selectMsEnd * MS_TO_PX;
            Vector4 selectOffset = new();

            if (selecting)
                selectOffset = (selectXStart, 0, selectXEnd - selectXStart, 5);
            selectBox.UploadData([selectOffset]);

            float beatDiv = Settings.beatDivisor.Value.Value + 1f;
            float multiplier = beatDiv % 1 == 0 ? 1 : 1  / (beatDiv % 1);
            int divisor = (int)Math.Round(beatDiv * multiplier);

            bool multipleOfTwo = divisor % 2 == 0;
            int divisorReset = (divisor - 1) / 2 * 2;

            int numPoints = CurrentMap.TimingPoints.Count;
            int totalBeats = 0; // total full beat ticks to render
            int totalHalfBeats = 0; // total half beat ticks to render (not full beats)
            int totalSubBeats = 0; // total sub-beat ticks to render (not full or half beats)

            // (numBeats, numHalfBeats, numSubBeats)
            Vector3i[] pointMetrics = new Vector3i[numPoints];

            for (int i = 0; i < numPoints; i++)
            {
                TimingPoint point = CurrentMap.TimingPoints[i];
                if (point.BPM <= 0 || point.Ms > totalTime)
                    continue;

                double nextMs = i + 1 < numPoints ? Math.Min(CurrentMap.TimingPoints[i + 1].Ms, totalTime) : totalTime;
                double totalMs = nextMs - point.Ms;

                double msIncrement = 60000 / point.BPM * multiplier;

                int numBeats = (int)(totalMs / msIncrement);
                int numHalfBeats = multipleOfTwo ? (int)(totalMs / (msIncrement / 2) - numBeats) : 0;
                int numSubBeats = (int)(totalMs / (msIncrement / divisor) - numBeats - numHalfBeats);

                pointMetrics[i] = (numBeats, numHalfBeats, numSubBeats);

                totalBeats += numBeats;
                totalHalfBeats += numHalfBeats;
                totalSubBeats += numSubBeats;
            }

            Vector3i currentMetrics = Vector3i.Zero;

            Vector4[] bpmStarts = new Vector4[numPoints];
            Vector4[] bpmBeats = new Vector4[totalBeats];
            Vector4[] bpmHalfBeats = new Vector4[totalHalfBeats];
            Vector4[] bpmSubBeats = new Vector4[totalSubBeats];

            Vector4 bpmHovers = Vector4.Zero;
            Vector4 bpmSelects = Vector4.Zero;

            for (int i = 0; i < numPoints; i++)
            {
                TimingPoint point = CurrentMap.TimingPoints[i];
                if (point.BPM <= 0)
                    continue;

                Vector3i metrics = pointMetrics[i];

                double msIncrement = 60000 / point.BPM * multiplier;
                double halfIncrement = msIncrement / 2;
                double subIncrement = msIncrement / divisor;

                double currentIncrement = subIncrement;
                float currentX = cursorPos - currentPos + point.Ms * MS_TO_PX;

                bpmStarts[i] = (currentX, 0, 1, 8);

                if (mousex > currentX && mousex < currentX + 72 && mousey > rect.Height && mousey < rect.Height + 52)
                    hoveringObject ??= point;

                if (hoveringObject == point)
                    bpmHovers = (currentX, 0, 1, 2 * 5 + 1);
                else if (selectedPoint == point)
                    bpmSelects = (currentX, 0, 1, 2 * 6 + 1);

                // full beat ticks
                for (int j = 0; j < metrics.X; j++)
                {
                    float x = currentX + (float)(msIncrement * (j + 1) * MS_TO_PX);
                    bpmBeats[currentMetrics.X + j] = (x, 0, 1, 2 * 1 + 1);
                }

                // half beat ticks
                for (int j = 0; j < metrics.Y; j++)
                {
                    float x = currentX + (float)((msIncrement * j + halfIncrement) * MS_TO_PX);
                    bpmHalfBeats[currentMetrics.Y + j] = (x, 0, 1, 2 * 2 + 1);
                }

                // sub beat ticks
                for (int j = 0; j < metrics.Z; j++)
                {
                    int curTick = j % divisorReset;

                    if (curTick == 0 && j > 0)
                        currentIncrement += subIncrement;
                    else if (curTick + 1 == divisor / 2 && multipleOfTwo)
                        currentIncrement += subIncrement;

                    float x = currentX + (float)((msIncrement * j + currentIncrement) * MS_TO_PX);
                    bpmSubBeats[currentMetrics.Z + j] = (x, 0, 1, 2 * 0 + 1);
                }

                currentMetrics += metrics;
            }

            bpmStart.UploadData(bpmStarts);
            bpmBeat.UploadData(bpmBeats);
            bpmHalfBeat.UploadData(bpmHalfBeats);
            bpmSubBeat.UploadData(bpmSubBeats);

            bpmHover.UploadData([bpmHovers]);
            bpmSelect.UploadData([bpmSelects]);

            Vector4[] staticLines =
            [
                (cursorPos - currentPos, 0, 1, 2 * 1 + 1),
                (endPos, 0, 1, 2 * 8 + 1),
                (cursorPos, 0, 1, 2 * 4 + 0.75f)
            ];

            staticLine.UploadData(staticLines);

            color1Data = new Vector4[color1Length];
            RenderText(color1Strings, color1Data, objCount);

            color2Data = new Vector4[color2Length];
            RenderText(color2Strings, color2Data, objCount);

            if (toPlay != lastPlayedObject)
            {
                lastPlayedObject = toPlay;

                if (toPlay != null && MusicPlayer.IsPlaying && MainWindow.Focused)
                    SoundPlayer.Play("hit");
            }

            if (Settings.metronome.Value && MainWindow.Focused)
            {
                float ms = currentTime - sfxOffset;
                float beat = Timing.GetClosestBeat(ms);

                if (beat != lastPlayedTick && beat <= ms && beat > 0 && MusicPlayer.IsPlaying)
                {
                    lastPlayedTick = beat;

                    SoundPlayer.Play("metronome");
                }
            }
        }

        public override float[] Draw()
        {
            selectBox.UploadStaticData(GLVerts.Rect(0, 0, 1, rect.Height, 1f, 1f, 1f, 1f));
            staticLine.UploadStaticData(GLVerts.Line(0, 0, 0, rect.Height, 1, 1f, 1f, 1f, 1f));

            RectangleF note = new(0, CellGap, NoteSize, NoteSize);

            List<float> noteVerts = [];
            noteVerts.AddRange(GLVerts.Rect(note, 1f, 1f, 1f, 1 / 20f));
            noteVerts.AddRange(GLVerts.Outline(note, 1, 1f, 1f, 1f, 1f));

            for (int i = 0; i < 9; i++)
            {
                float gridX = CellPos(2 - i % 3);
                float gridY = CellPos(i / 3);

                noteVerts.AddRange(GLVerts.Outline(gridX, gridY, 9, 9, 1, 1f, 1f, 1f, 0.45f));
            }

            noteConstant.UploadStaticData(noteVerts.ToArray());
            noteLocation.UploadStaticData(GLVerts.Rect(0, 0, NoteSize * 0.2f, NoteSize * 0.2f, 1f, 1f, 1f, 1f));
            noteHover.UploadStaticData(GLVerts.Outline(-4, CellGap - 4, NoteSize + 8, NoteSize + 8, 1, 1f, 1f, 1f, 1f));
            noteSelect.UploadStaticData(GLVerts.Outline(-4, CellGap - 4, NoteSize + 8, NoteSize + 8, 1, 1f, 1f, 1f, 1f));

            List<float> objVerts = [];
            objVerts.AddRange(GLVerts.CircleOutline(NoteSize / 2, NoteSize / 2 + CellGap, NoteSize / 2, 2, 20, 0, 1f, 1f, 1f, 1f));
            objVerts.AddRange(GLVerts.Circle(NoteSize / 2, NoteSize / 2 + CellGap, NoteSize / 2, 20, 0, 1f, 1f, 1f, 1 / 20f));

            objConstant.UploadStaticData(objVerts.ToArray());
            objIcon.UploadStaticData(GLVerts.Icon(NoteSize / 8, NoteSize / 8 + CellGap, NoteSize * 3 / 4f, NoteSize * 3 / 4f));
            objHover.UploadStaticData(GLVerts.CircleOutline(NoteSize / 2, NoteSize / 2 + CellGap, NoteSize / 2 + 4, 1, 20, 0, 1f, 1f, 1f, 1f));
            objSelect.UploadStaticData(GLVerts.CircleOutline(NoteSize / 2, NoteSize / 2 + CellGap, NoteSize / 2 + 4, 1, 20, 0, 1f, 1f, 1f, 1f));

            objDurationMarker.UploadStaticData(GLVerts.Circle(0, NoteSize / 2 + CellGap, NoteSize / 4, 4, 0, 1f, 1f, 1f, 1f));
            objDurationBar.UploadStaticData(GLVerts.Line(0, NoteSize / 2 + CellGap, 1, NoteSize / 2 + CellGap, 4, 1f, 1f, 1f, 1f));
            objDurationHover.UploadStaticData(GLVerts.CircleOutline(0, NoteSize / 2 + CellGap, NoteSize / 4 + 4, 1, 4, 0, 1f, 1f, 1f, 1f));
            objDurationSelect.UploadStaticData(GLVerts.CircleOutline(0, NoteSize / 2 + CellGap, NoteSize / 4 + 4, 1, 4, 0, 1f, 1f, 1f, 1f));

            textLine.UploadStaticData(GLVerts.Line(0.5f, rect.Height + 3, 0.5f, rect.Height + 28, 1, 1f, 1f, 1f, 1f));
            dragLine.UploadStaticData(GLVerts.Line(0, 0, 0, rect.Height, 1, 1f, 1f, 1f, 1f));

            float beatTickGap = rect.Height - NoteSize - CellGap;

            bpmStart.UploadStaticData(GLVerts.Line(0, 0, 0, rect.Height + 58, 1, 1f, 1f, 1f, 1f));
            bpmBeat.UploadStaticData(GLVerts.Line(0, rect.Height, 0, rect.Height - beatTickGap, 1, 1f, 1f, 1f, 1f));
            bpmHalfBeat.UploadStaticData(GLVerts.Line(0, rect.Height - 3 * beatTickGap / 5, 0, rect.Height, 1, 1f, 1f, 1f, 1f));
            bpmSubBeat.UploadStaticData(GLVerts.Line(0, rect.Height - 3 * beatTickGap / 10, 0, rect.Height, 1, 1f, 1f, 1f, 1f));

            bpmHover.UploadStaticData(GLVerts.Outline(-4, rect.Height, 80, 60, 1, 1f, 1f, 1f, 1f));
            bpmSelect.UploadStaticData(GLVerts.Outline(-4, rect.Height, 80, 60, 1, 1f, 1f, 1f, 1f));

            Shader.Sprite.Uniform2("SpriteSize", 1f / MainWindow.SpriteSize.X, 1f / MainWindow.SpriteSize.Y);

            List<float> verts = [];
            verts.AddRange(GLVerts.Rect(rect, 0.15f, 0.15f, 0.15f, Settings.trackOpacity.Value / 255f));
            verts.AddRange(GLVerts.Outline(rect, 1, 0.2f, 0.2f, 0.2f));

            return verts.ToArray();
        }

        public override void PreRender(float mousex, float mousey, float frametime)
        {
            base.PreRender(mousex, mousey, frametime);

            if (selecting)
            {
                float cursorPos = rect.Width * Settings.cursorPos.Value.Value / 100f;
                float currentMs = Settings.currentTime.Value.Value;
                float mouseMs = (mousex - cursorPos - NoteSize / 2f) / MS_TO_PX + currentMs;
                selectMsEnd = MathHelper.Clamp(mouseMs, 0, Settings.currentTime.Value.Max);

                float start = Math.Min(selectMsStart, selectMsEnd);
                float end = Math.Max(selectMsStart, selectMsEnd);
                (int low, int high) = CurrentMap.Notes.SearchRange(start, end);
                
                List<Note> selected = new();
                if (CurrentMap.Notes.Count > 0)
                    selected = CurrentMap.Notes.Take(new Range(low, high)).ToList();

                CurrentMap.Notes.Selected = new(selected);
            }
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            base.Render(mousex, mousey, frametime);

            if (Settings.waveform.Value)
            {
                float cursorPos = rect.Width * Settings.cursorPos.Value.Value / 100f;
                float currentPos = Settings.currentTime.Value.Value * MS_TO_PX;
                float maxPos = Settings.currentTime.Value.Max * MS_TO_PX;

                Vector2 pos = (-currentPos + cursorPos, cursorPos - currentPos + maxPos + 1);
                Waveform.Render(pos, (StartPositionRelative, EndPositionRelative), rect.Height);
            }
            
            UpdateInstanceData(mousex, mousey);

            selectBox.Render();
            staticLine.Render();

            if (CurrentMap.RenderMode == ObjectRenderMode.Notes)
            {
                noteConstant.Render();
                noteLocation.Render();
                noteHover.Render();
                noteSelect.Render();
            }
            else
            {
                spriteSheet.Activate();

                objConstant.Render();
                objIcon.Render();
                objHover.Render();
                objSelect.Render();

                objDurationMarker.Render();
                objDurationBar.Render();
                objDurationHover.Render();
                objDurationSelect.Render();
            }

            textLine.Render();
            dragLine.Render();

            bpmStart.Render();
            bpmBeat.Render();
            bpmHalfBeat.Render();
            bpmSubBeat.Render();

            bpmHover.Render();
            bpmSelect.Render();
        }

        private const string FONT = "main";

        public override void PostRender(float mousex, float mousey, float frametime)
        {
            base.PostRender(mousex, mousey, frametime);
            
            FontRenderer.SetActive(FONT);

            FontRenderer.SetColor(Settings.color1.Value);
            FontRenderer.RenderData(FONT, color1Data);

            FontRenderer.SetColor(Settings.color2.Value);
            FontRenderer.RenderData(FONT, color2Data);
        }

        public override void MouseMove(float x, float y)
        {
            base.MouseMove(x, y);

            float currentTime = Settings.currentTime.Value.Value;
            float totalTime = Settings.currentTime.Value.Max;
            float cursorPos = Settings.cursorPos.Value.Value;

            float cursorMs = (x - rect.Width * cursorPos / 100f - NoteSize / 2f) / MS_TO_PX + currentTime;
            float offset = dragMsStart - cursorMs;

            float bpm = Timing.GetCurrentBpm(cursorMs).BPM;

            if (bpm > 0)
            {
                float stepX = 60f / bpm * MS_TO_PX / Settings.beatDivisor.Value.Value;
                float threshold = MathHelper.Clamp(stepX / 1.75f, 1f, 12f);
                float snappedMs = Timing.GetClosestBeat(cursorMs);

                if (Math.Abs(snappedMs - cursorMs) * MS_TO_PX <= threshold)
                    offset = dragMsStart - snappedMs;
            }

            if (draggingObjects.FirstOrDefault() is TimingPoint point)
            {
                float stepX = 60f / bpm * MS_TO_PX / Settings.beatDivisor.Value.Value;
                float threshold = MathHelper.Clamp(stepX / 1.75f, 1f, 12f);
                float snappedMs = Timing.GetClosestBeat(cursorMs, true);
                float snappedNote = CurrentMap.Notes.GetClosest(cursorMs);

                if (Math.Abs(snappedNote - cursorMs) < Math.Abs(snappedMs - cursorMs))
                    snappedMs = snappedNote;
                if (Math.Abs(snappedMs - cursorMs) * MS_TO_PX <= threshold)
                    offset = dragMsStart - snappedMs;
                if (Math.Abs(cursorMs) * MS_TO_PX <= threshold)
                    offset = 0;

                point.Ms = (long)Math.Min(point.DragStartMs - offset, totalTime);

                CurrentMap.SortTimings(false);
            }
            else if (draggingDuration != null)
            {
                draggingDuration.Duration = (long)Math.Max(dragMsStart - offset - draggingDuration.Ms, 0);
            }
            else if (draggingObjects.Count > 0)
            {
                foreach (MapObject obj in draggingObjects)
                    obj.Ms = (long)MathHelper.Clamp(obj.DragStartMs - offset, 0, totalTime);

                switch (CurrentMap.RenderMode)
                {
                    case ObjectRenderMode.Notes:
                        CurrentMap.Notes.Sort();
                        break;
                    case ObjectRenderMode.VFX:
                        CurrentMap.VfxObjects.Sort();
                        break;
                    case ObjectRenderMode.Special:
                        CurrentMap.SpecialObjects.Sort();
                        break;
                }
            }
            else if (Dragging)
            {
                float final = dragMsStart + (dragXStart - x) / MS_TO_PX;

                if (Timing.GetCurrentBpm(final).BPM > 0)
                    final = Timing.GetClosestBeat(final);

                Settings.currentTime.Value.Value = MathHelper.Clamp(final, 0, totalTime);
            }
        }

        private void ClearSelection()
        {
            selectedPoint = null;
            CurrentMap.Notes.ClearSelection();
            CurrentMap.VfxObjects.ClearSelection();
            CurrentMap.SpecialObjects.ClearSelection();
        }

        private List<MapObject> GetObjectsInRange(float start, float end)
        {
            int low, high;

            switch (CurrentMap.RenderMode)
            {
                case ObjectRenderMode.Notes:
                    (low, high) = CurrentMap.Notes.SearchRange(start, end);
                    return CurrentMap.Notes.Take(new Range(low, high)).Cast<MapObject>().ToList();
                case ObjectRenderMode.VFX:
                    (low, high) = CurrentMap.VfxObjects.SearchRange(start, end);
                    return CurrentMap.VfxObjects.Take(new Range(low, high)).ToList();
                case ObjectRenderMode.Special:
                    (low, high) = CurrentMap.SpecialObjects.SearchRange(start, end);
                    return CurrentMap.SpecialObjects.Take(new Range(low, high)).ToList();
            }

            return [];
        }

        private List<MapObject> GetSelected()
        {
            return CurrentMap.RenderMode switch
            {
                ObjectRenderMode.Notes => CurrentMap.Notes.Selected.Cast<MapObject>().ToList(),
                ObjectRenderMode.VFX => CurrentMap.VfxObjects.Selected,
                ObjectRenderMode.Special => CurrentMap.SpecialObjects.Selected,
                _ => []
            };
        }

        private void SetSelected(List<MapObject> selected)
        {
            ClearSelection();

            switch (CurrentMap.RenderMode)
            {
                case ObjectRenderMode.Notes:
                    CurrentMap.Notes.Selected = new(selected.Cast<Note>());
                    break;
                case ObjectRenderMode.VFX:
                    CurrentMap.VfxObjects.Selected = new(selected);
                    break;
                case ObjectRenderMode.Special:
                    CurrentMap.SpecialObjects.Selected = new(selected);
                    break;
            }
        }

        public override void MouseClickLeft(float x, float y)
        {
            base.MouseClickLeft(x, y);

            if (hoveringObject is TimingPoint point)
            {
                draggingObjects = [point];
                draggingDuration = null;

                ClearSelection();

                selectedPoint = point;
            }
            else if (hoveringDuration != null)
            {
                draggingObjects = [];
                draggingDuration = hoveringDuration;

                ClearSelection();
            }
            else if (hoveringObject != null)
            {
                List<MapObject> selected = GetSelected();

                if (MainWindow.Instance.ShiftHeld)
                {
                    MapObject first = selected.FirstOrDefault() ?? hoveringObject;
                    MapObject last = hoveringObject;

                    long min = Math.Min(first.Ms, last.Ms);
                    long max = Math.Max(first.Ms, last.Ms);

                    selected = GetObjectsInRange(min, max);
                    selected.Insert(0, first);
                }
                else if (MainWindow.Instance.CtrlHeld)
                {
                    if (hoveringObject.Selected)
                        selected.Remove(hoveringObject);
                    else
                        selected.Add(hoveringObject);
                }
                else if (selected.Count == 0)
                    selected = [hoveringObject];

                SetSelected(selected);

                if (hoveringObject.Selected)
                {
                    draggingObjects = [hoveringObject, .. selected];
                    foreach (MapObject obj in draggingObjects)
                        obj.DragStartMs = obj.Ms;
                }

                draggingDuration = null;
            }
            else if (Dragging)
            {
                shouldReplay = MusicPlayer.IsPlaying;
                if (shouldReplay)
                    MusicPlayer.Pause();
            }

            float currentTime = Settings.currentTime.Value.Value;
            float cursorPos = Settings.cursorPos.Value.Value;

            dragMsStart = (x - rect.Width * cursorPos / 100f - NoteSize / 2f) / MS_TO_PX + currentTime;
            dragXStart = x;
        }

        public override void MouseUpLeft(float x, float y)
        {
            if (!Dragging)
            {
                base.MouseUpLeft(x, y);
                return;
            }

            float dragMsEnd = Settings.currentTime.Value.Value;
            long msDiff = (long)(dragMsEnd - dragMsStart);

            if (dragMsStart != dragMsEnd && draggingObjects.Count > 0)
            {
                foreach (MapObject obj in draggingObjects)
                    obj.Ms -= msDiff;

                if (draggingObjects[0] is TimingPoint point)
                    PointManager.Edit("MOVE POINT", point, n => n.Ms += msDiff);
                else
                {
                    switch (CurrentMap.RenderMode)
                    {
                        case ObjectRenderMode.Notes:
                            NoteManager.Edit("MOVE NOTE[S]", n => n.Ms += msDiff);
                            break;
                        case ObjectRenderMode.VFX:
                            VfxObjectManager.Edit("MOVE OBJECT[S]", n => n.Ms += msDiff);
                            break;
                        case ObjectRenderMode.Special:
                            SpecialObjectManager.Edit("MOVE OBJECT[S]", n => n.Ms += msDiff);
                            break;
                    }
                }
            }
            else if (dragMsStart != dragMsEnd && draggingDuration != null)
            {
                draggingDuration.Duration -= msDiff;

                switch (CurrentMap.RenderMode)
                {
                    case ObjectRenderMode.VFX:
                        VfxObjectManager.Edit("MOVE OBJ DURATION", [draggingDuration], n => n.Duration += msDiff);
                        break;
                    case ObjectRenderMode.Special:
                        SpecialObjectManager.Edit("MOVE OBJ DURATION", [draggingDuration], n => n.Duration += msDiff);
                        break;
                }
            }

            draggingObjects = [];

            if (shouldReplay)
                MusicPlayer.Play();

            base.MouseUpLeft(x, y);
        }

        public override void MouseClickRight(float x, float y)
        {
            base.MouseClickRight(x, y);

            if (Hovering)
            {
                float cursorPos = rect.Width * Settings.cursorPos.Value.Value / 100f;
                float currentMs = Settings.currentTime.Value.Value;
                float mouseMs = (x - cursorPos - NoteSize / 2f) / MS_TO_PX + currentMs;

                selectMsStart = MathHelper.Clamp(mouseMs, 0, Settings.currentTime.Value.Max);
                selecting = true;
            }
            else
                ClearSelection();
        }

        public override void MouseUpRight(float x, float y)
        {
            base.MouseUpRight(x, y);
            selecting = false;
        }

        public override void KeybindUsed(string keybind)
        {
            base.KeybindUsed(keybind);

            switch (keybind)
            {
                case "delete":
                    if (selectedPoint != null)
                        PointManager.Remove("DELETE POINT", selectedPoint);
                    break;
            }
        }
    }
}
