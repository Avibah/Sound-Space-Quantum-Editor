using New_SSQE.Preferences;
using OpenTK.Graphics.OpenGL;
using SkiaSharp;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiButtonToggle : GuiButtonTextured
    {
        private readonly Setting<bool> setting;

        public GuiButtonToggle(float x, float y, float w, float h, Setting<bool> setting, string texture, SKBitmap? bmp = null, bool smooth = false, TextureUnit unit = TextureUnit.Texture3) : base(x, y, w, h, texture, bmp, smooth, unit)
        {
            this.setting = setting;
            textures[0].TileSize = (2, 1);
            textures[0].TileIndex = setting.Value ? 1 : 0;
        }

        public override void MouseClickLeft(float x, float y)
        {
            if (Hovering)
            {
                setting.Value ^= true;
                textures[0].TileIndex = setting.Value ? 1 : 0;
            }

            base.MouseClickLeft(x, y);
        }
    }
}
