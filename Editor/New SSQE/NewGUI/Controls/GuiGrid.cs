using New_SSQE.Audio;
using New_SSQE.Misc.Static;
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
    internal class GuiGrid : InteractiveControl
    {
        private const string FONT = "main";

        private static readonly Dictionary<IndividualObjectMode, Dictionary<Vector2, MapObject>> objectLookup = new()
        {
            {IndividualObjectMode.Beat, new()
            {
                {(1, 1), new Beat(0) }
            } },
            {IndividualObjectMode.Mine, new()
            {
                {(0, 0), new Mine(0, 0, 0) },
                {(0, 1), new Mine(0, 1, 0) },
                {(0, 2), new Mine(0, 2, 0) },
                {(1, 0), new Mine(1, 0, 0) },
                {(1, 1), new Mine(1, 1, 0) },
                {(1, 2), new Mine(1, 2, 0) },
                {(2, 0), new Mine(2, 0, 0) },
                {(2, 1), new Mine(2, 1, 0) },
                {(2, 2), new Mine(2, 2, 0) }
            } },
            {IndividualObjectMode.Glide, new()
            {
                {(1, 2), new Glide(0, GlideDirection.Up) },
                {(1, 0), new Glide(0, GlideDirection.Down) },
                {(2, 1), new Glide(0, GlideDirection.Left) },
                {(0, 1), new Glide(0, GlideDirection.Right) }
            } },
            {IndividualObjectMode.Fever, new()
            {
                {(1, 1), new Fever(0, 0) }
            } }
        };

        private readonly int bezierPreviewLineVAO;
        private readonly int bezierPreviewLineVBO;

        private float CellSize => rect.Width / 3f;
        private float NoteSize => CellSize * 0.75f;
        private float PreviewSize => CellSize * 0.65f;
        private float NoteGap => (CellSize - NoteSize) / 2f;
        private float PreviewGap => (CellSize - PreviewSize) / 2f;

        private float[] objectSizes = new float[16];
        private float[] objectGaps = new float[16];

        private void UpdateObjectMetrics()
        {
            objectSizes = new float[16];
            objectSizes[14] = CellSize * 0.6f;

            objectGaps = new float[16];
            objectGaps[14] = (CellSize - objectSizes[14]) / 2f;
        }

        private readonly Instance autoplayCursor;

        private readonly Instance noteConstant;
        private readonly Instance noteApproach;
        private readonly Instance noteHover;
        private readonly Instance noteSelect;
        private readonly Instance notePreview;

        private readonly Instance beatConstant;
        private readonly Instance beatApproach;

        private readonly Instance mineConstant;
        private readonly Instance mineApproach;
        private readonly Instance mineHover;
        private readonly Instance mineSelect;

        private readonly Instance glideConstant;
        private readonly Instance glideApproach;

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

        public static Vector2 CellBounds => Settings.enableQuantum.Value ? (-0.85f, 2.85f) : (0, 2);

        private bool respectObjectMode;

        public bool RespectObjectMode
        {
            get => respectObjectMode;
            set
            {
                if (value != respectObjectMode)
                {
                    respectObjectMode = value;
                    shouldUpdate = true;
                }
            }
        }

        public GuiGrid(float x, float y, float w, float h) : base(x, y, w, h)
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

            mineConstant = Instancing.Generate("grid_mineConstant", Shader.InstancedMain);
            mineApproach = Instancing.Generate("grid_mineApproach", Shader.InstancedMain);
            mineHover = Instancing.Generate("grid_mineHover", Shader.InstancedMain);
            mineSelect = Instancing.Generate("grid_mineSelect", Shader.InstancedMain);

            glideConstant = Instancing.Generate("grid_glideConstant", Shader.InstancedMainExtra, true);
            glideApproach = Instancing.Generate("grid_glideApproach", Shader.InstancedMain);

            Style = ControlStyle.Grid_Colored;

            PlayLeftClickSound = false;
            PlayRightClickSound = false;

            UpdateObjectMetrics();
        }

        public override void Resize(float screenWidth, float screenHeight)
        {
            base.Resize(screenWidth, screenHeight);
            UpdateObjectMetrics();
        }

        private Vector2 MouseToGridSpaceUnclamped(float mousex, float mousey)
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

            x = x - 1 / increment + 1;
            y = y - 1 / increment + 1;

            return (2 - x, 2 - y);
        }

        private Vector2 MouseToGridSpace(float mousex, float mousey)
        {
            Vector2 pos = MouseToGridSpaceUnclamped(mousex, mousey);

            float x = Math.Clamp(pos.X, CellBounds.X, CellBounds.Y);
            float y = Math.Clamp(pos.Y, CellBounds.X, CellBounds.Y);

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

            (int low, int high) = notes.SearchRange(currentTime, maxMs);
            int range = high - low;

            Vector4[] noteConstants = new Vector4[range];
            Vector4[] noteApproaches = new Vector4[range];
            Vector4 noteHovers = Vector4.Zero;
            Vector4[] noteSelects = new Vector4[notes.Selected.Count];

            int selectIndex = 0;

            bool gridNumbers = Settings.gridNumbers.Value;
            int gridNumberSize = (int)(28 * TextScale);
            hoveringXY = null;

            bool isSpecialNotes = Mapping.Current.RenderMode == ObjectRenderMode.Special;
            bool isSelectMode = Settings.ClickMode.HasFlag(ClickMode.Select);

            for (int i = low; i < high; i++)
            {
                Note note = notes[i];
                int c = i % colorCount;

                float x = rect.X + (2 - note.X) * CellSize + NoteGap;
                float y = rect.Y + (2 - note.Y) * CellSize + NoteGap;

                float progress = 1 - (float)Math.Min(1, (note.Ms - currentTime) * approachRate / 750);

                if (isSpecialNotes && note.EnableEasing)
                    progress = (float)Easing.Process(0, 1, progress, note.Style, note.Direction);
                else
                    progress = (float)Math.Min(1, Math.Pow(progress, 2));
                progress = Math.Clamp(progress, 0, 1);

                float approachSize = 4 + NoteSize + NoteSize * (1 - progress) * 2 + 0.5f;

                noteConstants[i - low] = (x, y, 1, 2 * c + progress);
                noteApproaches[i - low] = (x - approachSize / 2 + NoteSize / 2, y - approachSize / 2 + NoteSize / 2, approachSize, 2 * c + progress);

                if (isSelectMode)
                {
                    if (Math.Abs(mousex - NoteSize / 2 - x) <= NoteSize / 2 && Math.Abs(mousey - NoteSize / 2 - y) <= NoteSize / 2)
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

                    gridNumStrings.Add(((x + NoteSize / 2 - width / 2, y + NoteSize / 2 - height / 2, 1 - progress), numText));
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
                Note defaultNote = new(1, 1, 0);
                int index = Math.Max(0, low - 1);

                Note lastFar = index > 0 && index < notes.Count ? notes[index - 1] : defaultNote;
                Note last = index < notes.Count ? notes[index] : defaultNote;
                Note next = index + 1 < notes.Count ? notes[index + 1] : last;
                Note nextFar = index + 2 < notes.Count ? notes[index + 2] : next;

                if (currentTime > next.Ms)
                {
                    lastFar = last;
                    last = next;
                }

                if (currentTime < last.Ms)
                {
                    nextFar = next;
                    next = last;
                }

                long msDiff = next.Ms - last.Ms;
                float timePos = currentTime - last.Ms;
                float t = msDiff == 0 ? 1 : timePos / msDiff;

                float x = CellSize / 2 + rect.X;
                float y = CellSize / 2 + rect.Y;

                if (Settings.smoothAutoplay.Value)
                {
                    Vector2 p0 = (lastFar.X, lastFar.Y);
                    Vector2 p1 = (last.X, last.Y);
                    Vector2 p2 = (next.X, next.Y);
                    Vector2 p3 = (nextFar.X, nextFar.Y);

                    Vector2 result = 0.5f * (
                        2 * p1 +
                        (-p0 + p2) * t +
                        (2 * p0 - 5 * p1 + 4 * p2 - p3) * MathF.Pow(t, 2) +
                        (-p0 + 3 * p1 - 3 * p2 + p3) * MathF.Pow(t, 3)
                    );

                    x += (2 - result.X) * CellSize;
                    y += (2 - result.Y) * CellSize;
                }
                else
                {
                    t = (float)Math.Sin(t * MathHelper.PiOver2);

                    float lastX = (2 - last.X) * CellSize;
                    float lastY = (2 - last.Y) * CellSize;

                    float nextX = (2 - next.X) * CellSize;
                    float nextY = (2 - next.Y) * CellSize;

                    x += lastX + (nextX - lastX) * t;
                    y += lastY + (nextY - lastY) * t;
                }

                float widthDiff = MainWindow.Instance.ClientSize.X / 1920f;
                float heightDiff = MainWindow.Instance.ClientSize.Y / 1080f;

                float width = (float)Math.Sin(t * MathHelper.Pi) * 8 + 16;
                width *= Math.Min(widthDiff, heightDiff);

                autoplayCursor.UploadData([(x - width / 2, y - width / 2, width, 2 * 4 + 1)]);
            }

            if (Mapping.Current.RenderMode == ObjectRenderMode.Special)
            {
                if (Mapping.Current.ObjectMode != IndividualObjectMode.Note)
                    hoveringXY = null;
                bool shouldCheckID = respectObjectMode && Mapping.Current.ObjectMode != IndividualObjectMode.Disabled;

                List<Vector4> beatConstants = [];
                List<Vector4> beatApproaches = [];

                List<Vector4> mineConstants = [];
                List<Vector4> mineApproaches = [];
                Vector4 mineHovers = Vector4.Zero;
                List<Vector4> mineSelects = [];

                List<Vector4> glideConstants = [];
                List<Vector3> glideConstantsSecondary = [];
                List<Vector4> glideApproaches = [];

                ObjectList<MapObject> objects = Mapping.Current.SpecialObjects;
                (low, high) = objects.SearchRange(currentTime, maxMs);

                List<int> indices = [];

                for (int i = 0; i < high; i++)
                {
                    MapObject obj = objects[i];
                    for (int j = indices.Count; j <= obj.ID; j++)
                        indices.Add(0);
                    indices[obj.ID]++;

                    if (obj.Ms < currentTime)
                        continue;
                    if (shouldCheckID && obj.ID != (int)Mapping.Current.ObjectMode)
                        continue;

                    float progress = (float)Math.Min(1, (float)Math.Pow(1 - Math.Min(1, (obj.Ms - currentTime) * approachRate / 750), 2));

                    if (obj is XYMapObject xy)
                    {
                        float size = objectSizes[obj.ID];
                        float gap = objectGaps[obj.ID];

                        float x = rect.X + (2 - xy.X) * CellSize + gap;
                        float y = rect.Y + (2 - xy.Y) * CellSize + gap;

                        float approachSize = 4 + size + size * (1 - progress) * 2 + 0.5f;

                        if (Settings.ClickMode.HasFlag(ClickMode.Select))
                        {
                            if (Math.Abs(mousex - size / 2 - x) <= size / 2 && Math.Abs(mousey - size / 2 - y) <= size / 2)
                                hoveringXY ??= xy;
                        }

                        switch (obj.ID)
                        {
                            case 14:
                                mineConstants.Add((x, y, 1, 2 * 8 + progress));
                                mineApproaches.Add((x - approachSize / 2 + size / 2, y - approachSize / 2 + size / 2, approachSize, 2 * 8 + progress));

                                if (hoveringXY == xy)
                                    mineHovers = (x, y, 1, 2 * 5 + 1);
                                if (xy.Selected)
                                    mineSelects.Add((x, y, 1, 2 * 6 + progress));
                                break;
                        }
                    }
                    else
                    {
                        float approachSize = 4 + rect.Width + rect.Width * (1 - progress) + 0.5f;

                        switch (obj.ID)
                        {
                            case 12:
                                beatConstants.Add((rect.X, rect.Y, 1, 2 * 4 + progress));
                                beatApproaches.Add((rect.X + rect.Width / 2 - approachSize / 2, rect.Y + rect.Height / 2 - approachSize / 2, approachSize, 2 * 4 + progress));
                                break;
                            case 13 when obj is Glide glide:
                                glideConstants.Add((rect.X + rect.Width / 2, rect.Y + rect.Height / 2, 1, 2 * 4 + progress));
                                glideConstantsSecondary.Add((1, 1, (int)glide.Direction * MathHelper.PiOver2));
                                glideApproaches.Add((rect.X + rect.Width / 2 - approachSize / 2, rect.Y + rect.Height / 2 - approachSize / 2, approachSize, 2 * 4 + progress));
                                break;
                        }
                    }
                }

                beatConstant.UploadData(beatConstants.ToArray());
                beatApproach.UploadData(beatApproaches.ToArray());

                mineConstant.UploadData(mineConstants.ToArray());
                mineApproach.UploadData(mineApproaches.ToArray());
                mineHover.UploadData([mineHovers]);
                mineSelect.UploadData(mineSelects.ToArray());

                glideConstant.UploadData(glideConstants.ToArray(), glideConstantsSecondary.ToArray());
                glideApproach.UploadData(glideApproaches.ToArray());
            }

            List<Vector4> notePreviews = [];
            bezierPositions = [];

            if (hoveringCell != null && hoveringXY == null && Settings.ClickMode.HasFlag(ClickMode.Place))
            {
                Vector2 hover = hoveringCell ?? Vector2.One;
                float x = rect.X + (2 - hover.X) * CellSize + PreviewGap;
                float y = rect.Y + (2 - hover.Y) * CellSize + PreviewGap;

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

                    float x = rect.X + (2 - note.X) * CellSize + PreviewGap;
                    float y = rect.Y + (2 - note.Y) * CellSize + PreviewGap;

                    notePreviews.Add((x, y, 1, 2 * 2 + 1));
                    bezierPositions.Add((x + PreviewSize / 2, y + PreviewSize / 2));
                }
            }

            notePreview.UploadData(notePreviews.ToArray());
        }

        public override float[] Draw()
        {
            bool squircles = Settings.gridSquircles.Value;
            int cornerDetail = (int)Settings.gridSquircleDetail.Value;
            float cornerRadius = Settings.gridSquircleRadius.Value;

            List<float> autoplayVerts = [];
            autoplayVerts.AddRange(GLVerts.Outline(0, 0, 1, 1, 0.075f, Style.Quaternary));
            autoplayVerts.AddRange(GLVerts.Rect(0, 0, 1, 1, Style.Quaternary, 0.25f));
            autoplayCursor.UploadStaticData(autoplayVerts.ToArray());

            List<float> noteVerts = [];

            if (squircles)
            {
                noteVerts.AddRange(GLVerts.SquircleOutline(0, 0, NoteSize, NoteSize, 2, cornerDetail, cornerRadius, 1f, 1f, 1f, 1f));
                noteVerts.AddRange(GLVerts.Squircle(0, 0, NoteSize, NoteSize, cornerDetail, cornerRadius, 1f, 1f, 1f, 0.15f));
            }
            else
            {
                noteVerts.AddRange(GLVerts.Outline(0, 0, NoteSize, NoteSize, 2, 1f, 1f, 1f, 1f));
                noteVerts.AddRange(GLVerts.Rect(0, 0, NoteSize, NoteSize, 1f, 1f, 1f, 0.15f));
            }
            
            noteConstant.UploadStaticData(noteVerts.ToArray());

            if (squircles)
            {
                noteApproach.UploadStaticData(GLVerts.SquircleOutline(0, 0, 1, 1, 0.0125f, cornerDetail, cornerRadius, 1f, 1f, 1f, 1f));
                noteHover.UploadStaticData(GLVerts.SquircleOutline(-4, -4, NoteSize + 8, NoteSize + 8, 2, cornerDetail, cornerRadius, 1f, 1f, 1f, 0.25f));
                noteSelect.UploadStaticData(GLVerts.SquircleOutline(-4, -4, NoteSize + 8, NoteSize + 8, 2, cornerDetail, cornerRadius, 1f, 1f, 1f, 1f));
            }
            else
            {
                noteApproach.UploadStaticData(GLVerts.Outline(0, 0, 1, 1, 0.0125f, 1f, 1f, 1f, 1f));
                noteHover.UploadStaticData(GLVerts.Outline(-4, -4, NoteSize + 8, NoteSize + 8, 2, 1f, 1f, 1f, 0.25f));
                noteSelect.UploadStaticData(GLVerts.Outline(-4, -4, NoteSize + 8, NoteSize + 8, 2, 1f, 1f, 1f, 1f));
            }

            List<float> previewVerts = [];

            if (squircles)
            {
                previewVerts.AddRange(GLVerts.SquircleOutline(0, 0, PreviewSize, PreviewSize, 2, cornerDetail, cornerRadius, 1f, 1f, 1f, 0.09375f));
                previewVerts.AddRange(GLVerts.Squircle(0, 0, PreviewSize, PreviewSize, cornerDetail, cornerRadius, 1f, 1f, 1f, 0.125f));
            }
            else
            {
                previewVerts.AddRange(GLVerts.Outline(0, 0, PreviewSize, PreviewSize, 2, 1f, 1f, 1f, 0.09375f));
                previewVerts.AddRange(GLVerts.Rect(0, 0, PreviewSize, PreviewSize, 1f, 1f, 1f, 0.125f));
            }
            
            notePreview.UploadStaticData(previewVerts.ToArray());

            beatConstant.UploadStaticData(GLVerts.Outline(-5, -5, rect.Width + 10, rect.Height + 10, 3, Settings.color5.Value));
            beatApproach.UploadStaticData(GLVerts.Outline(0, 0, 1, 1, 0.0075f, Settings.color5.Value));

            float mineSize = objectSizes[14];

            List<float> mineVerts = [];
            mineVerts.AddRange(GLVerts.PolygonOutline(mineSize / 2, mineSize / 2, mineSize / 2, 2, 4, 0, 1f, 1f, 1f, 1f));
            mineVerts.AddRange(GLVerts.Polygon(mineSize / 2, mineSize / 2, mineSize / 2, 4, 0, 1f, 1f, 1f, 0.15f));
            mineConstant.UploadStaticData(mineVerts.ToArray());

            mineApproach.UploadStaticData(GLVerts.PolygonOutline(0.5f, 0.5f, 0.5f, 0.0125f, 4, 0, 1f, 1f, 1f, 1f));
            mineHover.UploadStaticData(GLVerts.PolygonOutline(mineSize / 2, mineSize / 2, mineSize / 2 + 4, 2, 4, 0, 1f, 1f, 1f, 0.25f));
            mineSelect.UploadStaticData(GLVerts.PolygonOutline(mineSize / 2, mineSize / 2, mineSize / 2 + 4, 2, 4, 0, 1f, 1f, 1f, 1f));

            List<float> glideVerts = [];
            glideVerts.AddRange(GLVerts.Outline(-rect.Width / 2 - 5, -rect.Height / 2 - 5, rect.Width + 10, rect.Height + 10, 3, Settings.color5.Value));
            glideVerts.AddRange(GLVerts.Line(-rect.Width / 2 + 10, -rect.Height / 2 + 13, rect.Width / 2 - 10, -rect.Height / 2 + 13, 6, Settings.color5.Value));
            glideVerts.AddRange(GLVerts.PolygonOutline(0, -rect.Height / 5, rect.Width / 10, 8, 3, -30, Settings.color5.Value));
            glideVerts.AddRange(GLVerts.PolygonOutline(0, rect.Height / 5, rect.Width / 10, 8, 3, -30, Settings.color5.Value));
            glideConstant.UploadStaticData(glideVerts.ToArray());

            glideApproach.UploadStaticData(GLVerts.Outline(0, 0, 1, 1, 0.0075f, Settings.color5.Value));

            List<float> verts = [];
            verts.AddRange(GLVerts.Rect(rect, Style.Primary, Settings.gridOpacity.Value / 255f));
            verts.AddRange(GLVerts.Outline(rect, 1, Style.Secondary));

            for (int i = 1; i < 3; i++)
            {
                float x = rect.X + rect.Width / 3 * i;
                float y = rect.Y + rect.Height / 3 * i;

                verts.AddRange(GLVerts.Line(x, rect.Y, x, rect.Bottom, 1, Style.Tertiary));
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
                    int gx = key.Value.Item1;
                    int gy = key.Value.Item2;

                    if (Mapping.Current.RenderMode != ObjectRenderMode.Notes && Mapping.Current.ObjectMode != IndividualObjectMode.Note)
                    {
                        if (!objectLookup.TryGetValue(Mapping.Current.ObjectMode, out Dictionary<Vector2, MapObject>? subLookup))
                            continue;
                        if (subLookup == null || !subLookup.ContainsKey((2 - gx, 2 - gy)))
                            continue;
                    }

                    string letter = key.Key.ToString().Replace("KeyPad", "");

                    float x = rect.X + gx * CellSize + CellSize / 2;
                    float y = rect.Y + gy * CellSize + CellSize / 2;

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
                float currentTime = Settings.currentTime.Value.Value;
                long ms = Timing.GetClosestBeat(currentTime);

                if (ms > 0 && ms <= currentTime && Mapping.Current.Notes.FirstOrDefault(n => Math.Abs(n.Ms - ms) < 2) == null)
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

            if (Mapping.Current.RenderMode == ObjectRenderMode.Notes || Mapping.Current.ObjectMode == IndividualObjectMode.Note)
            {
                noteSelect.Render();
                noteHover.Render();
                notePreview.Render();
            }
            else if (Mapping.Current.RenderMode == ObjectRenderMode.Special)
            {
                beatConstant.Render();
                if (Settings.approachSquares.Value)
                    beatApproach.Render();

                mineConstant.Render();
                if (Settings.approachSquares.Value)
                    mineApproach.Render();
                mineSelect.Render();
                mineHover.Render();

                glideConstant.Render();
                if (Settings.approachSquares.Value)
                    glideApproach.Render();
            }

            if (Settings.autoplay.Value)
                autoplayCursor.Render();
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

                xOffset = Math.Max(CellBounds.X, minX + xOffset) - minX;
                xOffset = Math.Min(CellBounds.Y, maxX + xOffset) - maxX;
                yOffset = Math.Max(CellBounds.X, minY + yOffset) - minY;
                yOffset = Math.Min(CellBounds.Y, maxY + yOffset) - maxY;

                for (int i = 1; i < draggingXY.Count; i++)
                {
                    draggingXY[i].X += xOffset;
                    draggingXY[i].Y += yOffset;
                }
            }
        }

        public override void MouseClickLeftGlobal(float x, float y)
        {
            base.MouseClickLeftGlobal(x, y);

            if ((hoveringXY == null && Settings.ClickMode == ClickMode.Both) || Settings.ClickMode == ClickMode.Place)
            {
                if ((Mapping.Current.RenderMode != ObjectRenderMode.Notes && Mapping.Current.ObjectMode !=  IndividualObjectMode.Note) || hoveringCell == null)
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
                List<XYMapObject> selected = new(Mapping.Current.SelectedObjects.Where(n => n is XYMapObject).Cast<XYMapObject>());

                if (MainWindow.Instance.ShiftHeld)
                {
                    XYMapObject first = selected.FirstOrDefault() ?? hoveringXY;
                    XYMapObject last = hoveringXY;

                    long min = Math.Min(first.Ms, last.Ms);
                    long max = Math.Max(first.Ms, last.Ms);

                    selected = Mapping.Current.GetObjectsInRange(min, max).Cast<XYMapObject>().ToList();
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
                else if (selected.Count == 0 || !selected.Contains(hoveringXY))
                    selected = [hoveringXY];

                Mapping.Current.SelectedObjects = new(selected);

                if (hoveringXY.Selected)
                {
                    draggingXY = [hoveringXY, ..selected];
                    dragCellStart = (hoveringXY.X, hoveringXY.Y);
                }

                if (!Hovering)
                    InvokeLeftClick(new(x, y));
            }
        }

        public override void MouseUpLeftGlobal(float x, float y)
        {
            base.MouseUpLeftGlobal(x, y);

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

                    switch (Mapping.Current.RenderMode)
                    {
                        case ObjectRenderMode.Notes:
                            Mapping.Current.Notes.Modify_Edit("MOVE NOTE[S]", n =>
                            {
                                n.X += cellDiff.X;
                                n.Y += cellDiff.Y;
                            });
                            break;
                        case ObjectRenderMode.VFX:
                            Mapping.Current.VfxObjects.Modify_Edit("MOVE OBJECT[S]", n =>
                            {
                                if (n is XYMapObject xy)
                                {
                                    xy.X += cellDiff.X;
                                    xy.Y += cellDiff.Y;
                                }
                            });
                            break;
                        case ObjectRenderMode.Special:
                            Mapping.Current.SpecialObjects.Modify_Edit("MOVE OBJECT[S]", n =>
                            {
                                if (n is XYMapObject xy)
                                {
                                    xy.X += cellDiff.X;
                                    xy.Y += cellDiff.Y;
                                }
                            });
                            break;
                    }
                }

                draggingXY = [];
            }
        }

        public override void KeybindUsed(string keybind)
        {
            base.KeybindUsed(keybind);
            if (!keybind.Contains("gridKey"))
                return;

            string rep = keybind.Replace("gridKey", "");
            string[] xy = rep.Split('|');

            int x = 2 - int.Parse(xy[0]);
            int y = 2 - int.Parse(xy[1]);

            long ms = Math.Min(Timing.GetClosestBeat(Settings.currentTime.Value.Value), (long)Settings.currentTime.Value.Max);
            if (ms < 0)
                ms = (long)Settings.currentTime.Value.Value;

            if (Mapping.Current.RenderMode == ObjectRenderMode.Notes || Mapping.Current.ObjectMode == IndividualObjectMode.Note)
            {
                Note note = new(x, y, ms);
                Mapping.Current.Notes.Modify_Add("ADD NOTE", note);
            }
            else if (Mapping.Current.RenderMode == ObjectRenderMode.Special &&
                objectLookup.TryGetValue(Mapping.Current.ObjectMode, out Dictionary<Vector2, MapObject>? subLookup) && subLookup != null)
            {
                if (!subLookup.TryGetValue((x, y), out MapObject? value) || value == null)
                    return;

                MapObject obj = value.Clone();
                obj.Ms = ms;

                Mapping.Current.SpecialObjects.Modify_Add($"ADD {obj.Name?.ToUpper()}", obj);
            }
            else
                return;

            if (Settings.autoAdvance.Value)
                Timing.Advance();
        }
    }
}
