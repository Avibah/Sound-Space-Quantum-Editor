using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SkiaSharp;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiButtonTextured : GuiButton
    {
        public Vector2i TileSize
        {
            get => textures[0].TileSize;
            set => textures[0].TileSize = value;
        }
        
        public int TileIndex
        {
            get => textures[0].TileIndex;
            set => textures[0].TileIndex = value;
        }

        public GuiButtonTextured(float x, float y, float w, float h, string texture, SKBitmap? img = null, bool smooth = false, TextureUnit unit = TextureUnit.Texture0) : base(x, y, w, h)
        {
            textures = [new(texture, img, smooth, unit)];
        }

        public override float[] Draw()
        {
            textures[0].Draw(rect);
            return base.Draw();
        }
    }
}
