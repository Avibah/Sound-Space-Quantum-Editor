using SkiaSharp;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiSquareTextured : GuiSquare
    {
        public GuiSquareTextured(float x, float y, float w, float h, string texture, string? fileSource = null) : base(x, y, w, h)
        {
            if (fileSource != null && File.Exists(fileSource))
            {
                using FileStream fs = File.OpenRead(fileSource);
                textures = [new(texture, SKBitmap.Decode(fs))];
            }
            else
                textures = [new(texture)];
        }
        public GuiSquareTextured(string texture, string? fileSource = null) : this(0, 0, 1920, 1080, texture, fileSource) { }

        public override float[] Draw()
        {
            if (textures.Length > 0)
                textures[0].Draw(rect, Color.A / 255f);
            return base.Draw();
        }
    }
}
