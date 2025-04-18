using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SkiaSharp;
using System.Drawing;

namespace New_SSQE.NewGUI.Base
{
    internal class Texture : IDisposable
    {
        private readonly TextureHandle texture;
        private readonly TextureUnit texUnit;

        private readonly VertexArrayHandle vao;
        private readonly BufferHandle vbo;

        public Shader Shader = Shader.Texture;

        private RectangleF rect = RectangleF.Empty;

        private Vector2i _tileSize = Vector2i.One;
        public Vector2i TileSize
        {
            get => _tileSize;
            set
            {
                if (_tileSize != value)
                {
                    _tileSize = value;
                    Draw(rect);
                }
            }
        }

        private int _tileIndex;
        public int TileIndex
        {
            get => _tileIndex;
            set
            {
                if (_tileIndex != value)
                {
                    _tileIndex = value;
                    Draw(rect);
                }
            }
        }

        public Texture(string texture, SKBitmap? img = null, bool smooth = false, TextureUnit unit = TextureUnit.Texture0)
        {
            this.texture = Texturing.Generate(texture, img, smooth, unit);
            texUnit = unit;

            (vao, vbo) = GLState.NewVAO_VBO(2, 2, 1);

            if (this.texture == TextureHandle.Zero)
                Dispose();
        }

        public Texture(string texture) : this(texture, null, false, TextureUnit.Texture0) { }

        public void Draw(RectangleF rect, float alpha = 1)
        {
            if (_disposed)
                return;

            int tileX = _tileIndex % _tileSize.X;
            int tileY = _tileIndex / _tileSize.X;
            float tileWidth = 1f / _tileSize.X;
            float tileHeight = 1f / _tileSize.Y;

            float[] vertices = GLVerts.Texture(rect.X, rect.Y, rect.Width, rect.Height, tileX * tileWidth, tileY * tileHeight, tileWidth, tileHeight, alpha);

            GLState.BufferData(vbo, vertices);
            this.rect = rect;
        }

        public void Draw(float x, float y, float w, float h, float alpha = 1) => Draw(new(x, y, w, h), alpha);

        public void Activate()
        {
            if (_disposed)
                return;

            GLState.EnableTextureUnit(Shader, texUnit);
            GLState.EnableTexture(texture);
        }

        public void Render()
        {
            if (_disposed)
                return;

            Activate();
            GLState.DrawTriangles(vao, 0, 6);
        }


        private bool _disposed = false;

        public void Dispose()
        {
            if (_disposed)
                return;

            GLState.Clean(texture);
            GLState.Clean(vbo);
            GLState.Clean(vao);

            _disposed = true;
        }
    }
}
