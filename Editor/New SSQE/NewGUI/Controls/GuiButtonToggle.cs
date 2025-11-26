using New_SSQE.NewGUI.Base;
using New_SSQE.Preferences;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiButtonToggle : GuiButtonTextured
    {
        private readonly Setting<bool> setting;

        public GuiButtonToggle(float x, float y, float w, float h, Setting<bool> setting, Texture texture) : base(x, y, w, h, texture)
        {
            this.setting = setting;
            textures[0].TileSize = (2, 1);
            textures[0].TileIndex = setting.Value ? 1 : 0;
        }

        public override void MouseDownLeft(float x, float y)
        {
            setting.Value ^= true;
            textures[0].TileIndex = setting.Value ? 1 : 0;
            base.MouseDownLeft(x, y);
        }
    }
}
