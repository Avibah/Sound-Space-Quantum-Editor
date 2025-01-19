using New_SSQE.GUI;
using New_SSQE.GUI.Shaders;
using New_SSQE.Maps;
using New_SSQE.Misc.Static;
using New_SSQE.Objects;
using New_SSQE.Objects.Managers;
using New_SSQE.Preferences;
using OpenTK.Graphics;
using OpenTK.Mathematics;
using System.Drawing;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiGrid3d : View3dControl
    {
        private readonly VertexArrayHandle backgroundVAO;
        private readonly BufferHandle backgroundVBO;
        private int backgroundVertexCount;

        private readonly Instance noteConstant;

        public GuiGrid3d(float x, float y, float w, float h) : base(x, y, w, h)
        {
            (backgroundVAO, backgroundVBO) = GLState.NewVAO_VBO(2, 4);

            noteConstant = Instancing.Generate("grid3d_noteConstant", Shader.VFXNoteProgram);
        }

        private void UpdateInstanceData(float mousex, float mousey)
        {
            int colorCount = Settings.noteColors.Value.Count;

            float currentTime = Settings.currentTime.Value.Value;
            float approachRate = (Settings.approachRate.Value.Value + 1f) / 10f;
            float maxMs = currentTime + 1000f / approachRate;

            ObjectList<Note> notes = CurrentMap.Notes;
            (int low, int high) = notes.SearchRange(currentTime, maxMs);
            int range = high - low;

            Vector4[] noteConstants = new Vector4[range];

            for (int i = low; i < high; i++)
            {
                Note note = notes[i];

                float progress = (note.Ms - currentTime) * approachRate / 1000f;
                noteConstants[range - (i - low) - 1] = (note.X - 1, note.Y - 1, progress * 24, i % colorCount);
            }

            noteConstant.UploadData(noteConstants);
        }

        public override float[] Draw()
        {
            List<float> verts = [];
            verts.AddRange(GLVerts.Outline(-1.5f, -1.5f, 3f, 3f, 0.02f, 0.5f, 0.5f, 0.5f, 0.5f));
            verts.AddRange(GLVerts.Line(-0.5f, -1.5f, -0.5f, 1.5f, 0.01f, 0.5f, 0.5f, 0.5f, 0.5f));
            verts.AddRange(GLVerts.Line(0.5f, -1.5f, 0.5f, 1.5f, 0.01f, 0.5f, 0.5f, 0.5f, 0.5f));
            verts.AddRange(GLVerts.Line(-1.5f, -0.5f, 1.5f, -0.5f, 0.01f, 0.5f, 0.5f, 0.5f, 0.5f));
            verts.AddRange(GLVerts.Line(-1.5f, 0.5f, 1.5f, 0.5f, 0.01f, 0.5f, 0.5f, 0.5f, 0.5f));
            backgroundVertexCount = verts.Count / 6;

            GLState.BufferData(backgroundVBO, verts.ToArray());

            noteConstant.UploadStaticData(GLVerts.Outline(-0.375f, -0.375f, 0.75f, 0.75f, 0.125f, 1f, 1f, 1f, 1f));

            return [];
        }

        public override void PreRender(float mousex, float mousey, float frametime)
        {
            base.PreRender(mousex, mousey, frametime);

            float brightness = 0.5f;
            float contrast = 0.5f;
            float saturation = 0.5f;
            float blur = 0;
            float fov = 1;
            Color tint = Color.White;
            Vector3 position = Vector3.Zero;
            float rotation = 0;
            float approachRate = 1;
            string text = "";

            ObjectList<MapObject> objects = CurrentMap.VfxObjects;
            MapObject?[] curObjects = new MapObject?[12];

            float currentTime = Settings.currentTime.Value.Value;

            for (int i = 0; i < objects.Count; i++)
            {
                MapObject obj = objects[i];
                float ms = currentTime;

                if (obj.Ms > currentTime)
                    continue;
                if (obj.Ms + obj.Duration >= currentTime)
                    curObjects[obj.ID] = obj;

                if (i + 1 < objects.Count)
                {
                    MapObject next = objects[i + 1];

                    if (next.ID == obj.ID)
                        ms = Math.Min(ms, next.Ms);
                }

                float progress = (ms - obj.Ms) / obj.Duration;

                switch (obj.ID)
                {
                    case 2 when obj is Brightness temp:
                        brightness = (float)Easing.Process(brightness, temp.Intensity, progress, temp.Style, temp.Direction);
                        break;

                    case 3 when obj is Contrast temp:
                        contrast = (float)Easing.Process(contrast, temp.Intensity, progress, temp.Style, temp.Direction);
                        break;

                    case 4 when obj is Saturation temp:
                        saturation = (float)Easing.Process(saturation, temp.Intensity, progress, temp.Style, temp.Direction);
                        break;

                    case 5 when obj is Blur temp:
                        blur = (float)Easing.Process(blur, temp.Intensity, progress, temp.Style, temp.Direction);
                        break;

                    case 6 when obj is FOV temp:
                        fov = (float)Easing.Process(fov, temp.Intensity, progress, temp.Style, temp.Direction);
                        break;

                    case 7 when obj is Tint temp:
                        int r = (int)Easing.Process(tint.R, temp.Color.R, progress, temp.Style, temp.Direction);
                        int g = (int)Easing.Process(tint.G, temp.Color.G, progress, temp.Style, temp.Direction);
                        int b = (int)Easing.Process(tint.B, temp.Color.B, progress, temp.Style, temp.Direction);

                        tint = Color.FromArgb(r, g, b);
                        break;

                    case 8 when obj is Position temp:
                        float x = (float)Easing.Process(position.X, temp.Pos.X, progress, temp.Style, temp.Direction);
                        float y = (float)Easing.Process(position.Y, temp.Pos.Y, progress, temp.Style, temp.Direction);
                        float z = (float)Easing.Process(position.Z, temp.Pos.Z, progress, temp.Style, temp.Direction);

                        position = (x, y, z);
                        break;

                    case 9 when obj is Rotation temp:
                        rotation = (float)Easing.Process(rotation, temp.Degrees, progress, temp.Style, temp.Direction);
                        break;

                    case 10 when obj is ARFactor temp:
                        approachRate = (float)Easing.Process(approachRate, temp.Factor, progress, temp.Style, temp.Direction);
                        break;

                    case 11 when obj is Text temp:
                        if (progress <= 1)
                            text = temp.String;
                        break;
                }
            }

            fov = MathHelper.DegreesToRadians(70 * fov);

            if (fov > 0 && fov < MathHelper.Pi)
            {
                Box2i window = MainWindow.Instance.ClientRectangle;

                Matrix4 mProjection = Matrix4.CreatePerspectiveFieldOfView(fov, window.Size.X / (float)window.Size.Y, 0.1f, 1000);
                Matrix4 mRotation = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotation)) * Matrix4.CreateRotationY(MathHelper.Pi);
                Vector3 vLook = (mRotation * -Vector4.UnitZ).Xyz;
                Vector3 vCameraPos = -Vector3.UnitZ * 5.5f + vLook * (1.25f, 1.25f, 0);
                Matrix4 mView = Matrix4.CreateTranslation(-vCameraPos) * mRotation;
                Matrix4 mTranslation = Matrix4.CreateTranslation(position * (-1, 1, 1));

                Shader.SetPVT(mProjection, mView, mTranslation);
                Shader.SetBCSBT(brightness, contrast, saturation, blur, (tint.R / 255f, tint.G / 255f, tint.B / 255f));
                Shader.SetBlur(blur);
            }

            if (MainWindow.Instance.CurrentWindow is GuiWindowEditor editor)
            {
                editor.SetVFXToast(text);

                for (int i = 2; i < curObjects.Length; i++)
                    editor.IconSet[i - 2].Visible = curObjects[i] != null;
            }
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            base.Render(mousex, mousey, frametime);
            UpdateInstanceData(mousex, mousey);

            BeginFBORender();

            GLState.EnableProgram(Shader.VFXGridProgram);
            GLState.DrawTriangles(backgroundVAO, 0, backgroundVertexCount);

            noteConstant.Render();

            EndFBORender();
        }
    }
}
