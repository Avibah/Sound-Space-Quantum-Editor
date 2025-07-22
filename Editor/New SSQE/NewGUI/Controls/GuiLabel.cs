using New_SSQE.NewGUI.Base;
using New_SSQE.Preferences;
using System.Drawing;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiLabel : TextControl
    {
        private readonly Setting<Color>? setting;

        public GuiLabel(float x, float y, float w, float h, Setting<Color>? color = null, string text = "", int textSize = 0, string font = "main", CenterMode centerMode = CenterMode.XY) : base(x, y, w, h, text, textSize, font, centerMode)
        {
            setting = color;
            textColor = color?.Value ?? Color.White;
        }

        public override float[] Draw()
        {
            return [];
        }

        public override void Resize(float screenWidth, float screenHeight)
        {
            base.Resize(screenWidth, screenHeight);

            if (setting != null)
                SetColor(setting.Value);
        }

        public void SetColor(Color color)
        {
            textColor = color;
            Update();
        }

        public override void Reset()
        {
            base.Reset();

            text = startText;
            textSize = startTextSize;
            font = startFont;

            Update();
        }
    }
}
