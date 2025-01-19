using OpenTK.Graphics.OpenGL;
using SkiaSharp;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiButtonTextured : GuiButton
    {
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
