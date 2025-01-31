using SkiaSharp;
using System.Drawing;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiSquareTextured : GuiSquare
    {
        public GuiSquareTextured(float x, float y, float w, float h, string texture, string? fileSource = null, Color? backupColor = null) : base(x, y, w, h, backupColor ?? Color.Black, false)
        {
            if (fileSource != null && File.Exists(fileSource))
            {
                using FileStream fs = File.OpenRead(fileSource);
                textures = [new(texture, SKBitmap.Decode(fs))];
            }
            else
                textures = [new(texture)];
        }
        public GuiSquareTextured(string texture, string? fileSource = null, Color? backupColor = null) : this(0, 0, 1920, 1080, texture, fileSource, backupColor) { }

        public override float[] Draw()
        {
            textures[0].Draw(rect);
            return base.Draw();
        }
    }
}
