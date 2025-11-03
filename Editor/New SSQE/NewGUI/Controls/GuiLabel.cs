using New_SSQE.NewGUI.Base;
using New_SSQE.Preferences;
using System.Drawing;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiLabel : TextControl
    {
        private Setting<Color>? colorSetting;

        public Setting<Color>? ColorSetting
        {
            get => colorSetting;
            set
            {
                if (value != colorSetting)
                {
                    colorSetting = value;
                    shouldUpdate = true;
                }
            }
        }

        public GuiLabel(float x, float y, float w, float h) : base(x, y, w, h)
        {
            TextColor = colorSetting?.Value ?? Color.White;
        }

        public override float[] Draw()
        {
            return [];
        }

        public override void Update()
        {
            base.Update();

            if (colorSetting != null)
                TextColor = colorSetting.Value;
        }
    }
}
