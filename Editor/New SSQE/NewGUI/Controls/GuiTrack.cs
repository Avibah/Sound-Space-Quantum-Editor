using New_SSQE.Audio;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Font;
using New_SSQE.NewGUI.Windows;
using New_SSQE.NewMaps;
using New_SSQE.Objects;
using New_SSQE.Objects.Managers;
using New_SSQE.Objects.Other;
using New_SSQE.Preferences;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiTrack : InteractiveControl
    {
        private static float MS_TO_PX => Mapping.Current.Zoom * 0.5f;
        private static float PX_TO_MS => 2f / Mapping.Current.Zoom;
        private float NoteSize => rect.Height * 0.52f;
        private float CellGap => rect.Height * 0.14f;
        private float CellPos(float x) => x * NoteSize * 0.3f + NoteSize * 0.1f;

        private static bool RenderNotes => Mapping.Current.RenderMode == ObjectRenderMode.Notes || Mapping.Current.ObjectMode == IndividualObjectMode.Note;
        private static bool RenderSpecial => Mapping.Current.RenderMode == ObjectRenderMode.Special;
        private static bool RenderVFX => Mapping.Current.RenderMode == ObjectRenderMode.VFX;

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

        public float StartPositionRelative = 0f;
        public float EndPositionRelative = 1f;

        public GuiTrack(float x, float y, float w, float h) : base(x, y, w, h)
        {
            selectBox = Instancing.Generate("track_selectBox", Shader.InstancedMainExtra, true);
            staticLine = Instancing.Generate("track_staticLine", Shader.InstancedMain);

            noteConstant = Instancing.Generate("track_noteConstant", Shader.InstancedObject);
            noteLocation = Instancing.Generate("track_noteLocation", Shader.InstancedObject);
            noteHover = Instancing.Generate("track_noteHover", Shader.InstancedMain);
            noteSelect = Instancing.Generate("track_noteSelect", Shader.InstancedMain);

            objConstant = Instancing.Generate("track_objConstant", Shader.InstancedObject);
            objIcon = Instancing.Generate("track_objIcon", Shader.Sprite, true);
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

            bpmHover = Instancing.Generate("track_bpmHover", Shader.InstancedMain);
            bpmSelect = Instancing.Generate("track_bpmSelect", Shader.InstancedMain);

            spriteSheet = new("Sprites", null, true, TextureUnit.Texture2) { Shader = Shader.Sprite };

            Style = ControlStyle.Track_Colored;

            PlayLeftClickSound = false;
            PlayRightClickSound = false;
        }

        private void RenderText(List<(float, string)> text, Vector4[] data, bool primary, int first, int length, float yOffset = 0)
        {
            int offset = 0;

            for (int i = 0; i < Math.Min(text.Count, first + length); i++)
            {
                string str = text[i].Item2;

                if (i >= first)
                {
                    float x = text[i].Item1;
                    float y = primary ? rect.Height : rect.Height * 1.2f;

                    FontRenderer.PrintInto(data, offset, x, y + yOffset, str, (int)(rect.Height / 4), "main");
                }

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

            float minMs = (-cursorPos - NoteSize) * PX_TO_MS + currentTime;
            float maxMs = (rect.Width - cursorPos) * PX_TO_MS + currentTime;

            StartPositionRelative = Math.Max(0, (-cursorPos * PX_TO_MS + currentTime) / totalTime);
            EndPositionRelative = Math.Min(1, ((rect.Width - cursorPos) * PX_TO_MS + currentTime) / totalTime);

            int colorCount = Settings.noteColors.Value.Count;
            float? lastRenderedText = null;
            hoveringObject = null;
            hoveringDuration = null;

            MapObject? toPlay = null;
            int objCount = 0;

            List<(float, string)> color1Strings = [];
            List<(float, string)> color2Strings = [];

            int color1Length = 0;
            int color2Length = 0;

            if (RenderNotes)
            {
                ObjectList<Note> notes = Mapping.Current.Notes;
                (int low, int high) = notes.SearchRange(minMs, maxMs);
                int range = high - low;

                Vector4[] noteConstants = new Vector4[range];
                Vector4[] noteLocations = new Vector4[range];
                Vector4 noteHovers = Vector4.Zero;
                Vector4[] noteSelects = new Vector4[notes.Selected.Count];
                int selectIndex = 0;

                Vector4[] textLines = new Vector4[range];
                Vector4[] dragLines = new Vector4[noteSelects.Length];

                bool dragging = draggingObjects.Count > 0;

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

                    if (!dragging && mousex > x && mousex < x + NoteSize && mousey > CellGap && mousey < CellGap + NoteSize)
                        hoveringObject ??= note;

                    if (hoveringObject == note)
                        noteHovers = (x, 0, 1, 2 * 5 + 1);
                    else if (note.Selected)
                    {
                        if (dragging)
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
                ObjectList<MapObject> objects = RenderVFX ? Mapping.Current.VfxObjects : Mapping.Current.SpecialObjects;
                bool shouldCheckID = RenderSpecial && Mapping.Current.ObjectMode != IndividualObjectMode.Disabled;
                List<int> indices = [];

                int? first = null;
                int i;
                
                Vector4[] objConstants = new Vector4[objects.Count];
                Vector4[] objIcons = new Vector4[objects.Count];
                Vector3[] objIconsSecondary = new Vector3[objects.Count];
                Vector4 objHovers = Vector4.Zero;
                List<Vector4> objSelects = [];

                Vector4[] objDurationMarkers = new Vector4[objects.Count];
                Vector4[] objDurationBars = new Vector4[objects.Count];
                Vector3[] objDurationBarsSecondary = new Vector3[objects.Count];
                Vector4 objDurationHovers = Vector4.Zero;
                Vector4 objDurationSelects = Vector4.Zero;

                Vector4[] textLines = new Vector4[objects.Count];
                List<Vector4> dragLines = [];

                bool dragging = draggingObjects.Count > 0;

                for (i = 0; i < objects.Count; i++)
                {
                    MapObject obj = objects[i];
                    for (int j = indices.Count; j <= obj.ID; j++)
                        indices.Add(0);
                    indices[obj.ID]++;

                    if (obj.Duration + obj.Ms < minMs)
                        continue;
                    if (obj.Ms > maxMs)
                        break;
                    if (shouldCheckID && obj.ID != (int)Mapping.Current.ObjectMode)
                        continue;

                    first ??= i;

                    float x = cursorPos - currentPos + obj.Ms * MS_TO_PX;
                    float a = obj.Ms < currentTime - 1 ? 0.35f : 1f;

                    int c = (shouldCheckID ? indices[obj.ID] - 1 : i) % colorCount;

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
                        if (dragging)
                            dragLines.Add((cursorPos - currentPos + obj.DragStartMs * MS_TO_PX, 0, 1, 2 * 7 + 1));
                        else
                            objSelects.Add((x, 0, 1, 2 * 6 + 1));
                    }

                    if (RenderSpecial && obj.Ms <= currentTime - sfxOffset && obj.PlayHitsound)
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

                Range range = new(first ?? i, i);

                objConstant.UploadData(objConstants[range]);
                objIcon.UploadData(objIcons[range], objIconsSecondary[range]);
                objHover.UploadData([objHovers]);
                objSelect.UploadData([..objSelects]);

                objDurationMarker.UploadData(objDurationMarkers[range]);
                objDurationBar.UploadData(objDurationBars[range], objDurationBarsSecondary[range]);
                objDurationHover.UploadData([objDurationHovers]);
                objDurationSelect.UploadData([objDurationSelects]);

                textLine.UploadData(textLines[range]);
                dragLine.UploadData([..dragLines]);
            }

            float selectXStart = cursorPos - currentPos + selectMsStart * MS_TO_PX;
            float selectXEnd = cursorPos - currentPos + selectMsEnd * MS_TO_PX;
            Vector4 selectOffset = Vector4.Zero;
            Vector3 selectOffsetSecondary = Vector3.Zero;

            if (selecting)
            {
                selectOffset = (selectXStart, 0, 1, 2 * 5 + 0.2f);
                selectOffsetSecondary = (selectXEnd - selectXStart, 1, 0);
            }
            
            selectBox.UploadData([selectOffset], [selectOffsetSecondary]);

            float beatDiv = Settings.beatDivisor.Value.Value + 1f;
            float multiplier = beatDiv % 1 == 0 ? 1 : 1  / (beatDiv % 1);
            int divisor = (int)Math.Round(beatDiv * multiplier);

            bool multipleOfTwo = divisor % 2 == 0;
            int divisorReset = (divisor - 1) / 2 * 2;

            int numPoints = Mapping.Current.TimingPoints.Count;
            int totalBeats = 0; // total full beat ticks to render
            int totalHalfBeats = 0; // total half beat ticks to render (not full beats)
            int totalSubBeats = 0; // total sub-beat ticks to render (not full or half beats)

            // (numBeats, numHalfBeats, numSubBeats)
            Vector3i[] pointMetrics = new Vector3i[numPoints];

            for (int i = 0; i < numPoints; i++)
            {
                TimingPoint point = Mapping.Current.TimingPoints[i];
                if (point.BPM <= 0 || point.Ms > totalTime)
                    continue;
                
                double nextMs = i + 1 < numPoints ? Math.Min(Mapping.Current.TimingPoints[i + 1].Ms, totalTime) : totalTime;
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
            Vector3 bpmHoversSecondary = Vector3.Zero;
            Vector4 bpmSelects = Vector4.Zero;
            Vector3 bpmSelectsSecondary = Vector3.Zero;

            int validPoints = 0;

            for (int i = 0; i < numPoints; i++)
            {
                TimingPoint point = Mapping.Current.TimingPoints[i];
                if (point.BPM <= 0)
                    continue;

                Vector3i metrics = pointMetrics[i];

                double msIncrement = 60000 / point.BPM * multiplier;
                double halfIncrement = msIncrement / 2;
                double subIncrement = msIncrement / divisor;

                double currentIncrement = subIncrement;
                float currentX = cursorPos - currentPos + point.Ms * MS_TO_PX;

                bpmStarts[i] = (currentX, 0, 1, 2 * 8 + 1);

                if (mousex > currentX && mousex < currentX + 72 && mousey > rect.Height && mousey < rect.Height + 52)
                    hoveringObject ??= point;

                if (hoveringObject == point)
                    bpmHovers = (currentX, 0, 1, 2 * 5 + 1);
                else if (Mapping.Current.SelectedPoint == point)
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

                    float x = currentX + (float)((subIncrement * j + currentIncrement) * MS_TO_PX);
                    bpmSubBeats[currentMetrics.Z + j] = (x, 0, 1, 2 * 0 + 1);
                }

                currentMetrics += metrics;

                string pointStr = $"{point.BPM:##,###.###} BPM";
                string msStr = $"{point.Ms:##,##0}ms";

                color1Strings.Add((currentX + 3, pointStr));
                color1Length += pointStr.Length;

                color2Strings.Add((currentX + 3, msStr));
                color2Length += msStr.Length;

                validPoints++;
            }

            bpmStart.UploadData(bpmStarts);
            bpmBeat.UploadData(bpmBeats);
            bpmHalfBeat.UploadData(bpmHalfBeats);
            bpmSubBeat.UploadData(bpmSubBeats);

            bpmHover.UploadData([bpmHovers]);
            bpmSelect.UploadData([bpmSelects]);

            List<Vector4> staticLines =
            [
                (cursorPos - currentPos, 0, 1, 2 * 1 + 1),
                (endPos, 0, 1, 2 * 8 + 1),
                (cursorPos, 0, 1, 2 * 4 + 0.75f)
            ];

            if (selecting)
            {
                staticLines.Add((cursorPos - currentPos + selectMsStart * MS_TO_PX, 0, 1, 2 * 5 + 1));
                staticLines.Add((cursorPos - currentPos + selectMsEnd * MS_TO_PX, 0, 1, 2 * 5 + 1));
            }

            staticLine.UploadData([..staticLines]);

            color1Data = new Vector4[color1Length];
            RenderText(color1Strings, color1Data, true, 0, color1Strings.Count - validPoints);
            RenderText(color1Strings, color1Data, true, color1Strings.Count - validPoints, validPoints, rect.Height / 4);

            color2Data = new Vector4[color2Length];
            RenderText(color2Strings, color2Data, false, 0, color2Strings.Count - validPoints);
            RenderText(color2Strings, color2Data, false, color2Strings.Count - validPoints, validPoints, rect.Height / 4);

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
            selectBox.UploadStaticData(GLVerts.Rect(0, 0, 1, rect.Height, 1f, 1f, 1f, 0.2f));
            staticLine.UploadStaticData(GLVerts.Line(0, 0, 0, rect.Height, 1, 1f, 1f, 1f, 1f));

            RectangleF note = new(0, CellGap, NoteSize, NoteSize);

            List<float> noteVerts = [];
            noteVerts.AddRange(GLVerts.Rect(note, 1f, 1f, 1f, 1 / 20f));
            noteVerts.AddRange(GLVerts.Outline(note, 1, 1f, 1f, 1f, 1f));

            for (int i = 0; i < 9; i++)
            {
                float gridX = CellPos(2 - i % 3);
                float gridY = CellPos(i / 3) + CellGap;

                noteVerts.AddRange(GLVerts.Outline(gridX, gridY, NoteSize * 0.2f, NoteSize * 0.2f, 1, 1f, 1f, 1f, 0.45f));
            }

            noteConstant.UploadStaticData(noteVerts.ToArray());
            noteLocation.UploadStaticData(GLVerts.Rect(0, 0, NoteSize * 0.2f, NoteSize * 0.2f, 1f, 1f, 1f, 1f));
            noteHover.UploadStaticData(GLVerts.Outline(-4, CellGap - 4, NoteSize + 8, NoteSize + 8, 1, 1f, 1f, 1f, 1f));
            noteSelect.UploadStaticData(GLVerts.Outline(-4, CellGap - 4, NoteSize + 8, NoteSize + 8, 1, 1f, 1f, 1f, 1f));

            List<float> objVerts = [];
            objVerts.AddRange(GLVerts.PolygonOutline(NoteSize / 2, NoteSize / 2 + CellGap, NoteSize / 2, 2, 20, 0, 1f, 1f, 1f, 1f));
            objVerts.AddRange(GLVerts.Polygon(NoteSize / 2, NoteSize / 2 + CellGap, NoteSize / 2, 20, 0, 1f, 1f, 1f, 1 / 20f));

            objConstant.UploadStaticData(objVerts.ToArray());
            objIcon.UploadStaticData(GLVerts.Icon(NoteSize / 16, NoteSize / 16 + CellGap, NoteSize * 7 / 8f, NoteSize * 7 / 8f));
            objHover.UploadStaticData(GLVerts.PolygonOutline(NoteSize / 2, NoteSize / 2 + CellGap, NoteSize / 2 + 4, 1, 20, 0, 1f, 1f, 1f, 1f));
            objSelect.UploadStaticData(GLVerts.PolygonOutline(NoteSize / 2, NoteSize / 2 + CellGap, NoteSize / 2 + 4, 1, 20, 0, 1f, 1f, 1f, 1f));

            objDurationMarker.UploadStaticData(GLVerts.Polygon(0, NoteSize / 2 + CellGap, NoteSize / 4, 4, 0, 1f, 1f, 1f, 1f));
            objDurationBar.UploadStaticData(GLVerts.Line(0, NoteSize / 2 + CellGap, 1, NoteSize / 2 + CellGap, 4, 1f, 1f, 1f, 1f));
            objDurationHover.UploadStaticData(GLVerts.PolygonOutline(0, NoteSize / 2 + CellGap, NoteSize / 4 + 4, 1, 4, 0, 1f, 1f, 1f, 1f));
            objDurationSelect.UploadStaticData(GLVerts.PolygonOutline(0, NoteSize / 2 + CellGap, NoteSize / 4 + 4, 1, 4, 0, 1f, 1f, 1f, 1f));

            textLine.UploadStaticData(GLVerts.Line(0.5f, rect.Height + 2, 0.5f, rect.Height * 1.4f, 1, 1f, 1f, 1f, 1f));
            dragLine.UploadStaticData(GLVerts.Line(0, 0, 0, rect.Height, 1, 1f, 1f, 1f, 1f));

            float beatTickGap = rect.Height - NoteSize - CellGap;

            bpmStart.UploadStaticData(GLVerts.Line(0, 0, 0, rect.Height * 5 / 3, 1, 1f, 1f, 1f, 1f));
            bpmBeat.UploadStaticData(GLVerts.Line(0, rect.Height, 0, rect.Height - beatTickGap, 1, 1f, 1f, 1f, 1f));
            bpmHalfBeat.UploadStaticData(GLVerts.Line(0, rect.Height - 3 * beatTickGap / 5, 0, rect.Height, 1, 1f, 1f, 1f, 1f));
            bpmSubBeat.UploadStaticData(GLVerts.Line(0, rect.Height - 3 * beatTickGap / 10, 0, rect.Height, 1, 1f, 1f, 1f, 1f));

            bpmHover.UploadStaticData(GLVerts.Outline(-4, rect.Height, 80, 60, 1, 1f, 1f, 1f, 1f));
            bpmSelect.UploadStaticData(GLVerts.Outline(-4, rect.Height, 80, 60, 1, 1f, 1f, 1f, 1f));

            Shader.Sprite.Uniform2("SpriteSize", 1f / MainWindow.SpriteSize.X, 1f / MainWindow.SpriteSize.Y);

            List<float> verts = [];
            verts.AddRange(GLVerts.Rect(rect, Style.Tertiary, Settings.trackOpacity.Value / 255f));
            verts.AddRange(GLVerts.Outline(rect, 1, Style.Quaternary));

            return verts.ToArray();
        }

        public override void PreRender(float mousex, float mousey, float frametime)
        {
            base.PreRender(mousex, mousey, frametime);

            if (selecting)
            {
                float cursorPos = rect.Width * Settings.cursorPos.Value.Value / 100f;
                float currentMs = Settings.currentTime.Value.Value;
                selectMsEnd = (mousex - cursorPos) * PX_TO_MS + currentMs;

                float start = Math.Min(selectMsStart, selectMsEnd) - NoteSize * PX_TO_MS;
                float end = Math.Max(selectMsStart, selectMsEnd);

                Mapping.Current.SelectedObjects = Mapping.Current.GetObjectsInRange(start, end);
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
            
            if (RenderNotes)
            {
                noteLocation.Render();
                noteConstant.Render();
                noteHover.Render();
                noteSelect.Render();
            }
            else
            {
                spriteSheet.Activate();

                objIcon.Render();
                objConstant.Render();
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
            staticLine.Render();
        }

        private const string FONT = "main";

        public override void PostRender(float mousex, float mousey, float frametime)
        {
            base.PostRender(mousex, mousey, frametime);

            FontRenderer.SetActive(FONT);

            FontRenderer.SetColor(Style.Primary);
            FontRenderer.RenderData(FONT, color1Data);

            FontRenderer.SetColor(Style.Secondary);
            FontRenderer.RenderData(FONT, color2Data);
        }

        public override void MouseMove(float x, float y)
        {
            base.MouseMove(x, y);

            float currentTime = Settings.currentTime.Value.Value;
            float totalTime = Settings.currentTime.Value.Max;
            float cursorPos = Settings.cursorPos.Value.Value;

            float cursorMs = (x - rect.Width * cursorPos / 100f) * PX_TO_MS + currentTime;
            if (x == dragXStart)
                return;

            float bpm = Timing.GetCurrentBpm(cursorMs).BPM;
            float stepX = 60000f / bpm * MS_TO_PX / Settings.beatDivisor.Value.Value;
            float threshold = Math.Clamp(stepX / 1.75f, 1f, 12f);

            if (draggingObjects.Count > 0)
                cursorMs -= NoteSize / 2 * PX_TO_MS;

            if (draggingObjects.FirstOrDefault() is TimingPoint point)
            {
                float snappedMs = Timing.GetClosestBeat(cursorMs, point);
                float snappedNote = Mapping.Current.Notes.GetClosest(cursorMs);

                if (Math.Abs(snappedNote - cursorMs) < Math.Abs(snappedMs - cursorMs))
                    snappedMs = snappedNote;
                if (Math.Abs(snappedMs - cursorMs) * MS_TO_PX <= threshold)
                    cursorMs = snappedMs;
                if (Math.Abs(cursorMs) * MS_TO_PX <= threshold)
                    cursorMs = 0;

                point.Ms = (long)Math.Min(cursorMs, totalTime);

                Mapping.Current.SortTimings(false);
            }
            else
            {
                if (bpm > 0)
                {
                    float snappedMs = Timing.GetClosestBeat(cursorMs);

                    if (Math.Abs(snappedMs - cursorMs) * MS_TO_PX <= threshold)
                        cursorMs = snappedMs;
                }

                if (draggingDuration != null)
                {
                    draggingDuration.Duration = (long)Math.Max(cursorMs - draggingDuration.Ms, 0);
                }
                else if (draggingObjects.Count > 0)
                {
                    float offset = cursorMs - draggingObjects[0].DragStartMs;

                    foreach (MapObject obj in draggingObjects)
                        obj.Ms = (long)Math.Clamp(obj.DragStartMs + offset, 0, totalTime);

                    Mapping.Current.SortObjects();
                }
                else if (Dragging)
                {
                    float time = dragMsStart - (x - dragXStart) * PX_TO_MS;

                    if (Timing.GetCurrentBpm(time).BPM > 0)
                        time = Timing.GetClosestBeat(time);

                    Settings.currentTime.Value.Value = Math.Clamp(time, 0, totalTime);
                }
            }
        }

        public override void MouseClickLeft(float x, float y)
        {
            bool playCheck = false;

            if (hoveringObject is TimingPoint point)
            {
                draggingObjects = [point];
                draggingDuration = null;
                point.DragStartMs = point.Ms;

                Mapping.Current.ClearSelected();
                Mapping.Current.SelectedPoint = point;
            }
            else if (hoveringDuration != null)
            {
                draggingObjects = [];
                draggingDuration = hoveringDuration;
                draggingDuration.DragStartMs = draggingDuration.Duration;

                Mapping.Current.ClearSelected();
            }
            else if (hoveringObject != null)
            {
                List<MapObject> selected = Mapping.Current.SelectedObjects;

                if (MainWindow.Instance.ShiftHeld)
                {
                    MapObject first = selected.FirstOrDefault() ?? hoveringObject;
                    MapObject last = hoveringObject;

                    long min = Math.Min(first.Ms, last.Ms);
                    long max = Math.Max(first.Ms, last.Ms);

                    selected = Mapping.Current.GetObjectsInRange(min, max);
                    selected.Remove(first);
                    selected.Insert(0, first);
                }
                else if (MainWindow.Instance.CtrlHeld)
                {
                    if (hoveringObject.Selected)
                        selected.Remove(hoveringObject);
                    else
                        selected.Add(hoveringObject);
                }
                else if (!selected.Contains(hoveringObject))
                    selected = [hoveringObject];

                Mapping.Current.ClearSelected();
                Mapping.Current.SelectedObjects = selected;

                if (hoveringObject.Selected)
                {
                    draggingObjects = [hoveringObject, ..selected];
                    foreach (MapObject obj in draggingObjects)
                        obj.DragStartMs = obj.Ms;
                }

                draggingDuration = null;
            }
            else if (Hovering)
                playCheck = true;

            dragMsStart = Settings.currentTime.Value.Value;
            dragXStart = x;

            shouldReplay = playCheck && MusicPlayer.IsPlaying;
            if (shouldReplay)
                MusicPlayer.Pause();

            base.MouseClickLeft(x, y);
        }

        public override void MouseUpLeft(float x, float y)
        {
            if (draggingObjects.Count > 0)
            {
                long msDiff = draggingObjects[0].Ms - draggingObjects[0].DragStartMs;

                if (msDiff != 0)
                {
                    foreach (MapObject obj in draggingObjects)
                        obj.Ms = obj.DragStartMs;

                    if (draggingObjects[0] is TimingPoint point)
                        PointManager.Edit("MOVE POINT", point, n => n.Ms += msDiff);
                    else
                    {
                        switch (Mapping.Current.RenderMode)
                        {
                            case ObjectRenderMode.Notes:
                                Mapping.Current.Notes.Modify_Edit("MOVE NOTE[S]", n => n.Ms += msDiff);
                                break;
                            case ObjectRenderMode.VFX:
                                Mapping.Current.VfxObjects.Modify_Edit("MOVE OBJECT[S]", n => n.Ms += msDiff);
                                break;
                            case ObjectRenderMode.Special when RenderNotes:
                                Mapping.Current.Notes.Modify_Edit("MOVE NOTE[S]", n => n.Ms += msDiff);
                                break;
                            case ObjectRenderMode.Special:
                                Mapping.Current.SpecialObjects.Modify_Edit("MOVE OBJECT[S]", n => n.Ms += msDiff);
                                break;
                        }
                    }
                }
            }
            else if (draggingDuration != null)
            {
                long msDiff = draggingDuration.Duration - draggingDuration.DragStartMs;

                if (msDiff != 0)
                {
                    draggingDuration.Duration = draggingDuration.DragStartMs;

                    switch (Mapping.Current.RenderMode)
                    {
                        case ObjectRenderMode.VFX:
                            Mapping.Current.VfxObjects.Modify_Edit("MOVE OBJ DURATION", [draggingDuration], n => n.Duration += msDiff);
                            break;
                        case ObjectRenderMode.Special:
                            Mapping.Current.SpecialObjects.Modify_Edit("MOVE OBJ DURATION", [draggingDuration], n => n.Duration += msDiff);
                            break;
                    }
                }
            }

            draggingObjects = [];
            draggingDuration = null;

            if (shouldReplay)
                MusicPlayer.Play();

            base.MouseUpLeft(x, y);
        }

        public override void MouseClickRight(float x, float y)
        {
            base.MouseClickRight(x, y);

            Mapping.Current.SelectedPoint = null;

            if (Hovering)
            {
                float cursorPos = rect.Width * Settings.cursorPos.Value.Value / 100f;
                float currentMs = Settings.currentTime.Value.Value;

                selectMsStart = (x - cursorPos) * PX_TO_MS + currentMs;
                selecting = true;
            }
            else if ((Windowing.Current?.GetHoveringInteractive() ?? null) is not GuiButtonList)
                Mapping.Current.ClearSelected();
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
                    if (Mapping.Current.SelectedPoint != null)
                        PointManager.Remove("DELETE POINT", Mapping.Current.SelectedPoint);
                    else
                    {
                        switch (Mapping.Current.RenderMode)
                        {
                            case ObjectRenderMode.Notes:
                                Mapping.Current.Notes.Modify_Remove("DELETE NOTE[S]");
                                break;

                            case ObjectRenderMode.VFX:
                                Mapping.Current.VfxObjects.Modify_Remove("DELETE OBJECT[S]");
                                break;

                            case ObjectRenderMode.Special when RenderNotes:
                                Mapping.Current.Notes.Modify_Remove("DELETE NOTE[S]");
                                break;
                            case ObjectRenderMode.Special:
                                Mapping.Current.SpecialObjects.Modify_Remove("DELETE OBJECT[S]");
                                break;
                        }
                    }
                    break;
            }
        }

        public override void KeyDown(Keys key)
        {
            base.KeyDown(key);

            if (!Windowing.TextboxFocused())
            {
                if (key == Keys.Space && (!Dragging || draggingObjects.Count > 0 || draggingDuration != null) && !GuiWindowEditor.Timeline.Dragging)
                {
                    if (MusicPlayer.IsPlaying)
                        MusicPlayer.Pause();
                    else
                    {
                        if (Settings.currentTime.Value.Value >= Settings.currentTime.Value.Max - 1)
                            Settings.currentTime.Value.Value = 0;

                        MusicPlayer.Play();
                    }
                }

                if (key == Keys.Left || key == Keys.Right)
                    Timing.Scroll(key == Keys.Left);

                if (key == Keys.Escape)
                    Mapping.Current.ClearSelected();
            }
        }
    }
}
