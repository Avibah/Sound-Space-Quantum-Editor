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

        public Texture[] Textures => [..textures];

        protected int textureIndex = 0;

        protected RectangleF StartRect { get; private set; }
        protected RectangleF rect;

        protected bool shouldUpdate = false;

        protected int vao;
        protected int vbo;
        protected int vertexCount;

        public bool Visible = true;
        public bool RenderOnTop = false;
        public StretchMode Stretch = StretchMode.XY;
        public int CornerDetail = 8;
        public float CornerRadius = 0.125f;

        public Vector2 RectOffset = Vector2.Zero;
        public Gradient? Gradient = null;

        public Control(RectangleF rect)
        {
            StartRect = rect;
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

        public virtual void PreRender(float mousex, float mousey, float frametime)
        {
            if (shouldUpdate)
            {
                Update();
                shouldUpdate = false;
            }
        }

        public virtual void Render(float mousex, float mousey, float frametime)
        {
            Gradient?.PreRender(mousex, mousey, frametime);

            if (vertexCount > 0)
            {
                shader.Enable();
                GLState.DrawTriangles(vao, 0, vertexCount);
            }

            if (textureIndex >= 0 && textureIndex < textures.Length)
                textures[textureIndex].Render();
            
            Gradient?.Render(mousex, mousey, frametime);
            Gradient?.PostRender(mousex, mousey, frametime);
        }

        public virtual void PostRender(float mousex, float mousey, float frametime) { }

        public virtual void Resize(float screenWidth, float screenHeight)
        {
            float widthDiff = screenWidth / 1920;
            float heightDiff = screenHeight / 1080;

            float x = StartRect.X * widthDiff;
            float y = StartRect.Y * heightDiff;
            float w = StartRect.Width * widthDiff;
            float h = StartRect.Height * heightDiff;
            
            float min = Math.Min(widthDiff, heightDiff);

            if (!Stretch.HasFlag(StretchMode.X))
            {
                float width = StartRect.Width * min;
                x += (w - width) / 2;
                w = width;
            }

            if (!Stretch.HasFlag(StretchMode.Y))
            {
                float height = StartRect.Height * min;
                y += (h - height) / 2;
                h = height;
            }

            SetRect(x + RectOffset.X * widthDiff, y + RectOffset.Y * heightDiff, w, h);
            Gradient?.SetRect(rect);
        }

        public virtual void SetRect(RectangleF rect)
        {
            this.rect = rect;
            shouldUpdate = true;
        }

        public virtual void SetRect(float x, float y, float w, float h) => SetRect(new(x, y, w, h));

        public virtual void SetOrigin(RectangleF rect)
        {
            StartRect = rect;
        }

        public virtual void SetOrigin(float x, float y, float w, float h) => SetOrigin(new(x, y, w, h));

        public virtual RectangleF GetRect() => rect;

        public virtual RectangleF GetOrigin() => StartRect;

        public virtual void Reset() { }

        /// <summary>
        /// Returns extents of inner content relative to the control's origin (left, up, right, down)
        /// </summary>
        public virtual Vector4 GetExtents() => (0, 0, StartRect.Width, StartRect.Height);
    }
}
