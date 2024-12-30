using New_SSQE.Audio;
using New_SSQE.GUI.Shaders;
using New_SSQE.Maps;
using New_SSQE.Objects;
using New_SSQE.Objects.Managers;
using New_SSQE.Preferences;
using OpenTK.Mathematics;
using System.Drawing;

namespace New_SSQE.NewGUI.Controls
{
    internal enum TrackRenderMode
    {
        Notes,
        VFX,
        Special
    }

    internal class GuiTrack : InteractiveControl
    {
        private static float MS_TO_PX => CurrentMap.NoteStep / 1000f;
        private float NoteSize => (rect.Height - Settings.trackHeight.Value.Value) * 0.65f;
        private float CellGap => (rect.Height - Settings.trackHeight.Value.Value - NoteSize) / 2f;
        private float CellPos(float x) => x * NoteSize * 0.2f + NoteSize * 0.1f;

        private bool selecting = false;
        private float selectMsStart;
        private float selectMsEnd;

        private MapObject? hoveringObject;
        private List<MapObject> draggingObjects = [];
        private MapObject? lastPlayedObject;

        private Instance selectBox;

        private Instance noteConstant;
        private Instance noteLocation;
        private Instance noteHover;
        private Instance noteSelect;

        private Instance textLine;
        private Instance dragLine;

        public TrackRenderMode TrackRenderMode;

        public GuiTrack(float x, float y, float w, float h, TrackRenderMode trackRenderMode = TrackRenderMode.Notes) : base(x, y, w, h)
        {
            TrackRenderMode = trackRenderMode;

            selectBox = Instancing.Generate("track_selectBox", Shader.XScalingProgram);

            noteConstant = Instancing.Generate("track_noteConstant", Shader.TrackProgram);
            noteLocation = Instancing.Generate("track_noteLocation", Shader.TrackProgram);
            noteHover = Instancing.Generate("track_noteHover", Shader.TimelineProgram);
            noteSelect = Instancing.Generate("track_noteSelect", Shader.TimelineProgram);

            textLine = Instancing.Generate("track_textLine", Shader.TimelineProgram);
            dragLine = Instancing.Generate("track_dragLine", Shader.TimelineProgram);
        }

        private void UpdateInstanceData(float mousex, float mousey)
        {
            float currentTime = Settings.currentTime.Value.Value;
            float maxTime = Settings.currentTime.Value.Max;
            float divisor = Settings.beatDivisor.Value.Value + 1f;
            float sfxOffset = Settings.sfxOffset.Value;

            float currentPos = currentTime * MS_TO_PX;
            float cursorPos = rect.Width * Settings.cursorPos.Value.Value / 100f;

            float minMs = (-cursorPos - NoteSize) / MS_TO_PX + currentTime;
            float maxMs = (rect.Width - cursorPos) / MS_TO_PX + currentTime;

            int colorCount = Settings.noteColors.Value.Count;
            float? lastRenderedText = null;

            MapObject? toPlay = null;

            if (TrackRenderMode == TrackRenderMode.Notes)
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

                    noteConstants[i - low] = (x, 0, a, i % colorCount);
                    noteLocations[i - low] = (gridX, gridY, a, i % colorCount);
                    textLines[i - low] = (x, 0, a, 4);

                    // check if hovering

                    if (note.Selected)
                    {
                        if (Dragging)
                            dragLines[selectIndex++] = (cursorPos - currentPos + note.DragStartMs * MS_TO_PX, 0, 1, 7);
                        else
                            noteSelects[selectIndex++] = (x, 0, 1, 6);
                    }
                    else if (hoveringObject == note)
                        noteHovers = (x, 0, 1, 5);

                    if (note.Ms <= currentTime - sfxOffset)
                        toPlay = note;

                    if (x - 8 > (lastRenderedText ?? float.MinValue))
                    {
                        // render text

                        lastRenderedText = x;
                    }
                }

                noteConstant.UploadData(noteConstants);
                noteLocation.UploadData(noteLocations);
                noteHover.UploadData([noteHovers]);
                noteSelect.UploadData(noteSelects);

                textLine.UploadData(textLines);
                dragLine.UploadData(dragLines);
            }

            if (toPlay != lastPlayedObject)
            {
                lastPlayedObject = toPlay;

                if (toPlay != null && MusicPlayer.IsPlaying && MainWindow.Focused)
                    SoundPlayer.Play("hit");
            }
        }

        public override float[] Draw()
        {
            RectangleF note = new(0, CellGap, NoteSize, NoteSize);

            List<float> verts = new();
            verts.AddRange(GLVerts.Rect(note, 1f, 1f, 1f, 1 / 20f));
            verts.AddRange(GLVerts.Outline(note, 1, 1f, 1f, 1f, 1f));

            for (int i = 0; i < 9; i++)
            {
                float gridX = CellPos(2 - i % 3);
                float gridY = CellPos(i / 3);

                verts.AddRange(GLVerts.Outline(gridX, gridY, 9, 9, 1, 1f, 1f, 1f, 0.45f));
            }

            noteConstant.UploadStaticData(verts.ToArray());
            noteLocation.UploadStaticData(GLVerts.Rect(0, 0, NoteSize * 0.2f, NoteSize * 0.2f, 1f, 1f, 1f, 1f));
            noteHover.UploadStaticData(GLVerts.Outline(-4, CellGap - 4, NoteSize + 8, NoteSize + 8, 1, 1f, 1f, 1f, 1f));
            noteSelect.UploadStaticData(GLVerts.Outline(-4, CellGap - 4, NoteSize + 8, NoteSize + 8, 1, 1f, 1f, 1f, 1f));

            textLine.UploadStaticData(GLVerts.Line(0.5f, rect.Height + 3, 0.5f, rect.Height + 28, 1, 1f, 1f, 1f, 1f));
            dragLine.UploadStaticData(GLVerts.Line(0, 0, 0, rect.Height, 1, 1f, 1f, 1f, 1f));

            // waveform

            throw new NotImplementedException();
        }

        public override void PreRender(float mousex, float mousey, float frametime)
        {
            base.PreRender(mousex, mousey, frametime);

            if (selecting)
            {
                (int low, int high) = CurrentMap.Notes.SearchRange(selectMsStart, selectMsEnd);
                
                List<Note> selected = new();
                if (CurrentMap.Notes.Count > 0)
                    selected = CurrentMap.Notes.Take(new Range(low, high)).ToList();

                CurrentMap.Notes.Selected = new(selected);
            }
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            base.Render(mousex, mousey, frametime);
            
            // render waveform

            UpdateInstanceData(mousex, mousey);

            if (TrackRenderMode == TrackRenderMode.Notes)
            {
                noteConstant.Render();
                noteLocation.Render();
                noteHover.Render();
                noteSelect.Render();
            }

            textLine.Render();
            dragLine.Render();
        }

        public override void PostRender(float mousex, float mousey, float frametime)
        {
            base.PostRender(mousex, mousey, frametime);
            // render text
        }

        public override void MouseMove(float x, float y)
        {
            base.MouseMove(x, y);

            if (Dragging)
            {
                // drag track
            }
        }

        public override void MouseClickLeft(float x, float y)
        {
            base.MouseClickLeft(x, y);

            if (hoveringObject != null)
            {
                // select object
            }
        }

        public override void MouseUpLeft(float x, float y)
        {
            if (Dragging)
            {
                // select dragged objects or undrag track
            }

            base.MouseUpLeft(x, y);
        }

        public override void MouseClickRight(float x, float y)
        {
            base.MouseClickRight(x, y);

            if (Hovering)
            {
                selecting = true;
                // begin selecting multiple
            }
        }

        public override void MouseUpRight(float x, float y)
        {
            base.MouseUpRight(x, y);

            if (selecting)
            {
                // stop selecting
                
            }

            selecting = false;
        }
    }
}
