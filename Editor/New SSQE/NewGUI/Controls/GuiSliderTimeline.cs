using New_SSQE.Audio;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Font;
using New_SSQE.NewGUI.Input;
using New_SSQE.NewGUI.Windows;
using New_SSQE.NewMaps;
using New_SSQE.Objects;
using New_SSQE.Objects.Other;
using New_SSQE.Preferences;
using OpenTK.Mathematics;
using System.Drawing;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiSliderTimeline : GuiSlider
    {
        private readonly Instance notes;
        private readonly Instance points;
        private readonly Instance objects;

        private Bookmark? hoveringBookmark = null;
        private Vector4[] hoveringBookmarkText = [];

        public GuiSliderTimeline(float x, float y, float w, float h) : base(x, y, w, h, Settings.currentTime)
        {
            notes = Instancing.Generate("timeline_notes", Shader.InstancedMain);
            points = Instancing.Generate("timeline_points", Shader.InstancedMain);
            objects = Instancing.Generate("timeline_objects", Shader.InstancedMain);

            canReset = false;
            PlayRightClickSound = false;
        }

        public void RefreshInstances()
        {
            shouldUpdate = true;
        }

        private void UpdateInstanceData()
        {
            shouldUpdate = false;
            RectangleF lineRect = new(rect.X + rect.Height / 2f, rect.Y + rect.Height / 2f - 1.5f, rect.Width - rect.Height, 3f);

            List<Vector4> noteVerts = [];
            List<Vector4> pointVerts = [];
            List<Vector4> objectVerts = [];

            float mult = lineRect.Width / setting.Value.Max;
            float prevX = -1;

            // notes
            for (int i = 0; i < Mapping.Current.Notes.Count; i++)
            {
                Note note = Mapping.Current.Notes[i];
                float x = lineRect.X + note.Ms * mult;

                if (x - 1 >= prevX)
                {
                    noteVerts.Add((x, 0, 1, 2 * 0 + 1));
                    prevX = x;
                }
            }

            prevX = -1;

            // points
            for (int i = 0; i < Mapping.Current.TimingPoints.Count; i++)
            {
                TimingPoint point = Mapping.Current.TimingPoints[i];
                float x = lineRect.X + point.Ms * mult;

                if (x - 1 >= prevX)
                {
                    pointVerts.Add((x, 0, 1, 2 * 0 + 1));
                    prevX = x;
                }
            }

            prevX = -1;

            // vfx objects
            for (int i = 0; i < Mapping.Current.VfxObjects.Count; i++)
            {
                MapObject obj = Mapping.Current.VfxObjects[i];
                float x = lineRect.X + obj.Ms * mult;

                if (x - 1 >= prevX)
                {
                    objectVerts.Add((x, 0, 1, 2 * 0 + 1));
                    prevX = x;
                }
            }

            prevX = -1;

            // special objects
            for (int i = 0; i < Mapping.Current.SpecialObjects.Count; i++)
            {
                MapObject obj = Mapping.Current.SpecialObjects[i];
                float x = lineRect.X + obj.Ms * mult;

                if (x - 1 >= prevX)
                {
                    objectVerts.Add((x, 0, 1, 2 * 0 + 1));
                    prevX = x;
                }
            }

            notes.UploadData([..noteVerts]);
            points.UploadData([..pointVerts]);
            objects.UploadData([..objectVerts]);
        }

        public override float[] Draw()
        {
            RectangleF lineRect = new(rect.X + rect.Height / 2f, rect.Y + rect.Height / 2f - 1.5f, rect.Width - rect.Height, 3f);
            float y = lineRect.Y + lineRect.Height / 2f;
            float widthDiff = MainWindow.Instance.ClientSize.X / 1920f;

            notes.UploadStaticData(GLVerts.Line(0, y + 5f, 0, y + 3f, 1 * widthDiff, Color.White));
            points.UploadStaticData(GLVerts.Line(0, y - 10f, 0, y - 6f, 2 * widthDiff, Color.White));
            objects.UploadStaticData(GLVerts.Line(0, y + 9f, 0, y + 7f, 2 * widthDiff, Color.White));

            List<float> bookmarkVerts = [];

            for (int i = 0; i < Mapping.Current.Bookmarks.Count; i++)
            {
                Bookmark bookmark = Mapping.Current.Bookmarks[i];

                float progress = bookmark.Ms / setting.Value.Max;
                float endProgress = bookmark.EndMs / setting.Value.Max;
                float x = lineRect.X + progress * lineRect.Width;
                float endX = lineRect.X + endProgress * lineRect.Width;

                bookmarkVerts.AddRange(GLVerts.Rect(x - 4f, y - 40f, 8f + (endX - x), 8f, hoveringBookmark == bookmark ? Style.Secondary : Style.Tertiary, 0.75f));
            }

            float[] main = base.Draw();
            float[] line = main[..36];
            float[] other = main[36..];

            float start = GuiWindowEditor.Track.StartPositionRelative * lineRect.Width;
            float width = (GuiWindowEditor.Track.EndPositionRelative - GuiWindowEditor.Track.StartPositionRelative) * lineRect.Width;

            float[] seLine = GLVerts.Rect(lineRect.X + start, lineRect.Y, width, lineRect.Height, Style.Tertiary);
            line = line.Concat(seLine).ToArray();

            return line.Concat(other).Concat(bookmarkVerts).ToArray();
        }

        public override void PreRender(float mousex, float mousey, float frametime)
        {
            if (shouldUpdate)
                UpdateInstanceData();

            base.PreRender(mousex, mousey, frametime);
            Update();
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            base.Render(mousex, mousey, frametime);

            notes.Render();
            points.Render();
            objects.Render();
        }

        public override void PostRender(float mousex, float mousey, float frametime)
        {
            base.PostRender(mousex, mousey, frametime);

            if (hoveringBookmark != null)
            {
                FontRenderer.SetColor(Style.Secondary);
                FontRenderer.RenderData("main", hoveringBookmarkText);
            }
        }

        public override void MouseMove(float x, float y)
        {
            base.MouseMove(x, y);

            RectangleF lineRect = new(rect.X + rect.Height / 2f, rect.Y + rect.Height / 2f - 1.5f, rect.Width - rect.Height, 3f);
            float yPos = lineRect.Y + lineRect.Height / 2f;

            hoveringBookmark = null;

            for (int i = 0; i < Mapping.Current.Bookmarks.Count; i++)
            {
                Bookmark bookmark = Mapping.Current.Bookmarks[i];

                float progress = bookmark.Ms / setting.Value.Max;
                float endProgress = bookmark.EndMs / setting.Value.Max;
                float xPos = lineRect.X + progress * lineRect.Width;
                float endX = lineRect.X + endProgress * lineRect.Width;

                RectangleF hitbox = new(xPos - 4f, yPos - 40f, 8f + (endX - xPos), 8f);
                if (hitbox.Contains(x, y))
                    hoveringBookmark = bookmark;
            }

            if (hoveringBookmark != null)
            {
                float progress = hoveringBookmark.Ms / setting.Value.Max;
                float xPos = lineRect.X + progress * lineRect.Width;
                float height = FontRenderer.GetHeight(16, "main");

                hoveringBookmarkText = FontRenderer.Print(xPos, yPos - 40f - height, hoveringBookmark.Text, 16, "main");
            }
        }

        private bool wasPlaying = false;

        public override void MouseClickLeft(float x, float y)
        {
            if (hoveringBookmark != null)
            {
                MusicPlayer.Pause();
                Settings.currentTime.Value.Value = KeybindManager.ShiftHeld ? hoveringBookmark.EndMs : hoveringBookmark.Ms;
            }
            else if (Hovering)
            {
                wasPlaying = MusicPlayer.IsPlaying;
                MusicPlayer.Pause();
            }

            base.MouseClickLeft(x, y);
        }

        public override void MouseUpLeft(float x, float y)
        {
            if (Dragging && wasPlaying && !Settings.pauseScroll.Value)
                MusicPlayer.Play();

            base.MouseUpLeft(x, y);
        }

        public override void Resize(float screenWidth, float screenHeight)
        {
            base.Resize(screenWidth, screenHeight);
            RefreshInstances();
        }
    }
}
