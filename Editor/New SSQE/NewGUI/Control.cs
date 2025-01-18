using OpenTK.Graphics;
using New_SSQE.GUI.Shaders;
using System.Drawing;

namespace New_SSQE.NewGUI
{
    internal abstract class Control : IDisposable
    {
        protected ProgramHandle shader;
        protected Texture[] textures;

        protected int textureIndex;

        protected readonly RectangleF startRect;
        protected RectangleF rect;

        protected VertexArrayHandle vao;
        protected BufferHandle vbo;
        protected int vertexCount;

        public bool Visible = true;

        public Control(RectangleF rect)
        {
            shader = Shader.Program;
            textures = [];

            startRect = rect;
            this.rect = rect;

            (vao, vbo) = GLState.NewVAO_VBO(2, 4);
        }

        public Control(float x, float y, float w, float h) : this(new RectangleF(x, y, w, h)) { }

        public abstract float[] Draw();

        public virtual void Update()
        {
            float[] vertices = Draw();
            vertexCount = vertices.Length / 6;

            GLState.BufferData(vbo, vertices);
        }

        public virtual void PreRender(float mousex, float mousey, float frametime) { }

        public virtual void Render(float mousex, float mousey, float frametime)
        {
            GLState.EnableProgram(shader);
            GLState.DrawTriangles(vao, 0, vertexCount);
        }

        public virtual void PostRender(float mousex, float mousey, float frametime)
        {
            if (textureIndex >= 0 && textureIndex < textures.Length)
                textures[textureIndex].Render();
        }

        public virtual void Resize(float screenWidth, float screenHeight)
        {
            float widthDiff = screenWidth / 1920;
            float heightDiff = screenHeight / 1080;

            SetRect(startRect.X * widthDiff, startRect.Y * heightDiff,
                startRect.Width * widthDiff, startRect.Height * heightDiff);

            Update();
        }

        public virtual void SetRect(RectangleF rect) => this.rect = rect;

        public void SetRect(float x, float y, float width, float height) => SetRect(new(x, y, width, height));

        public virtual RectangleF GetRect() => rect;


        private bool _disposed = false;

        public virtual void Dispose()
        {
            if (_disposed)
                return;

            GLState.Clean(vbo);
            foreach (Texture tex in textures)
                tex.Dispose();

            _disposed = true;
        }
    }
}
