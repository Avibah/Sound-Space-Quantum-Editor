using New_SSQE.NewGUI.Shaders;
using OpenTK.Graphics;
using System.Drawing;

namespace New_SSQE.NewGUI
{
    internal abstract class Control : IDisposable
    {
        protected Shader shader;
        protected Texture[] textures;

        protected int textureIndex;

        protected readonly RectangleF startRect;
        protected RectangleF rect;

        protected VertexArrayHandle vao;
        protected BufferHandle vbo;
        protected int vertexCount;

        public bool Visible = true;
        public bool Square = false;

        public Control(RectangleF rect)
        {
            shader = Shader.Default;
            textures = [];
            textureIndex = 0;

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
            shader.Enable();
            GLState.DrawTriangles(vao, 0, vertexCount);

            if (textureIndex >= 0 && textureIndex < textures.Length)
                textures[textureIndex].Render();
        }

        public virtual void PostRender(float mousex, float mousey, float frametime) { }

        public virtual void Resize(float screenWidth, float screenHeight)
        {
            float widthDiff = screenWidth / 1920;
            float heightDiff = screenHeight / 1080;

            float x = startRect.X * widthDiff;
            float y = startRect.Y * heightDiff;
            float w = startRect.Width * widthDiff;
            float h = startRect.Height * heightDiff;

            if (Square)
            {
                if (w < h)
                {
                    x += (h - w) / 2;
                    w = h;
                }
                else
                {
                    y += (w - h) / 2;
                    h = w;
                }
            }

            SetRect(x, y, w, h);
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
