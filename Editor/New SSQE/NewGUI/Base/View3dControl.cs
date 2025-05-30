﻿using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace New_SSQE.NewGUI.Base
{
    internal abstract class View3dControl : InteractiveControl
    {
        private int fbo;
        private int msaa_fbo;
        private int rbo;
        private int fbo_tex;

        private static int ViewVAO;
        private static int ViewVBO;

        private bool renderBegan = false;
        private bool renderEnded = false;

        public View3dControl(float x, float y, float w, float h) : base(x, y, w, h)
        {
            GLState.CreateFBO(out fbo, out msaa_fbo, out rbo, out fbo_tex);

            (ViewVAO, ViewVBO) = GLState.NewVAO_VBO(2, 2);
            GLState.BufferData(ViewVBO, GLVerts.TextureWithoutAlpha(-1, -1, 2, 2));
        }

        public override void Update()
        {
            base.Update();

            Box2i screenRect = MainWindow.Instance.ClientRectangle;
            float x = 2 * (rect.X / screenRect.Width) - 1;
            float y = 2 * (rect.Y / screenRect.Height) - 1;
            float w = 2 * (rect.Width / screenRect.Width);
            float h = 2 * (rect.Height / screenRect.Height);

            GLState.BufferData(ViewVBO, GLVerts.TextureWithoutAlpha(x, y, w, h));
            GLState.AllocateFBO(w, h, msaa_fbo, rbo, fbo_tex);
        }

        public void BeginFBORender()
        {
            renderBegan = true;

            GLState.BeginFBORender(rect, msaa_fbo);
        }

        public void EndFBORender()
        {
            renderEnded = true;

            GLState.EndFBORender(rect, fbo, msaa_fbo);
            GLState.EnableTextureUnit(Shader.FBOTexture, TextureUnit.Texture3);
            GLState.EnableTexture(fbo_tex);
            GLState.DrawTriangles(ViewVAO, 0, 6);
        }

        public override void PostRender(float mousex, float mousey, float frametime)
        {
            base.PostRender(mousex, mousey, frametime);

            if (renderBegan && !renderEnded)
                throw new InvalidOperationException("Framebuffer render must be stopped if started!");
            if (!renderBegan && renderEnded)
                throw new InvalidOperationException("Framebuffer render must be started if stopped!");

            renderBegan = false;
            renderEnded = false;
        }
    }
}
