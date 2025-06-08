using OpenTK.Mathematics;
using System.Drawing;

namespace New_SSQE.NewGUI.Base
{
    internal enum StretchMode
    {
        None = 1,
        X = 2,
        Y = 4,
        XY = 6
    }

    internal abstract class Control
    {
        protected Shader shader = Shader.Default;
        protected Texture[] textures = [];
        public ControlStyle Style = new();

        protected int textureIndex = 0;

        protected readonly RectangleF startRect;
        protected RectangleF rect;

        protected int vao;
        protected int vbo;
        protected int vertexCount;

        public bool Visible = true;
        public StretchMode Stretch = StretchMode.XY;

        public Vector2 RectOffset = Vector2.Zero;

        public Control(RectangleF rect)
        {
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
            if (vertexCount > 0)
            {
                shader.Enable();
                GLState.DrawTriangles(vao, 0, vertexCount);
            }

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
            
            float min = Math.Min(widthDiff, heightDiff);

            if (!Stretch.HasFlag(StretchMode.X))
            {
                float width = startRect.Width * min;
                x += (w - width) / 2;
                w = width;
            }

            if (!Stretch.HasFlag(StretchMode.Y))
            {
                float height = startRect.Height * min;
                y += (h - height) / 2;
                h = height;
            }

            SetRect(x + RectOffset.X * widthDiff, y + RectOffset.Y * heightDiff, w, h);
        }

        public virtual void SetRect(RectangleF rect)
        {
            this.rect = rect;
            Update();
        }

        public virtual void SetRect(float x, float y, float width, float height) => SetRect(new(x, y, width, height));

        public virtual RectangleF GetRect() => rect;

        public virtual RectangleF GetOrigin() => startRect;

        public virtual void Reset() { }
    }
}
