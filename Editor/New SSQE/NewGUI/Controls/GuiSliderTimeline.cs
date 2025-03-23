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

        public GuiSliderTimeline(float x, float y, float w, float h, bool reverse = false) : base(x, y, w, h, Settings.currentTime, reverse)
        {
            notes = Instancing.Generate("timeline_notes", Shader.InstancedMain);
            points = Instancing.Generate("timeline_points", Shader.InstancedMain);
            objects = Instancing.Generate("timeline_objects", Shader.InstancedMain);
        }

        public void UpdateInstanceData()
        {
            RectangleF lineRect = new(rect.X + rect.Height / 2f, rect.Y + rect.Height / 2f - 1.5f, rect.Width - rect.Height, 3f);

            Vector4[] noteVerts = new Vector4[CurrentMap.Notes.Count];
            Vector4[] pointVerts = new Vector4[CurrentMap.TimingPoints.Count];
            Vector4[] objectVerts = new Vector4[CurrentMap.VfxObjects.Count + CurrentMap.SpecialObjects.Count];

            float mult = lineRect.Width / setting.Value.Max;

            // notes
            for (int i = 0; i < noteVerts.Length; i++)
            {
                Note note = CurrentMap.Notes[i];
                float x = lineRect.X + note.Ms * mult;

                noteVerts[i] = (x, 0, 1, 2 * 0 + 1);
            }

            // points
            for (int i = 0; i < pointVerts.Length; i++)
            {
                TimingPoint point = CurrentMap.TimingPoints[i];
                float x = lineRect.X + point.Ms * mult;

                pointVerts[i] = (x, 0, 1, 2 * 0 + 1);
            }

            // vfx objects
            for (int i = 0; i < CurrentMap.VfxObjects.Count; i++)
            {
                MapObject obj = CurrentMap.VfxObjects[i];
                float x = lineRect.X + obj.Ms * mult;

                objectVerts[i] = (x, 0, 1, 2 * 0 + 1);
            }

            // special objects
            for (int i = 0; i < CurrentMap.SpecialObjects.Count; i++)
            {
                MapObject obj = CurrentMap.SpecialObjects[i];
                float x = lineRect.X + obj.Ms * mult;

                objectVerts[i + CurrentMap.VfxObjects.Count] = (x, 0, 1, 2 * 0 + 1);
            }

            notes.UploadData(noteVerts);
            points.UploadData(pointVerts);
            objects.UploadData(objectVerts);
        }

        public override float[] Draw()
        {
            RectangleF lineRect = new(rect.X + rect.Height / 2f, rect.Y + rect.Height / 2f - 1.5f, rect.Width - rect.Height, 3f);
            float y = lineRect.Y + lineRect.Height / 2f;

            notes.UploadStaticData(GLVerts.Line(0, y + 5f, 0, y + 3f, 1, Color.White));
            points.UploadStaticData(GLVerts.Line(0, y - 10f, 0, y - 6f, 2, Color.White));
            objects.UploadStaticData(GLVerts.Line(0, y + 9f, 0, y + 7f, 2, Color.White));

            List<float> bookmarkVerts = [];

            for (int i = 0; i < CurrentMap.Bookmarks.Count; i++)
            {
                Bookmark bookmark = CurrentMap.Bookmarks[i];

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
            base.PreRender(mousex, mousey, frametime);
            Update();
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            base.Render(mousex, mousey, frametime);
            UpdateInstanceData();

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

            for (int i = 0; i < CurrentMap.Bookmarks.Count; i++)
            {
                Bookmark bookmark = CurrentMap.Bookmarks[i];

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
    }
}
