using New_SSQE.Audio;
using New_SSQE.NewGUI.Base;
using New_SSQE.Preferences;
using System.Drawing;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiCheckbox : InteractiveControl
    {
        private bool _toggle;
        public bool Toggle
        {
            get => _toggle;
            set
            {
                _toggle = value;

                if (setting != null)
                    setting.Value = value;
            }
        }

        private readonly Setting<bool>? setting;
        private float checkSize = 0f;

        public GuiCheckbox(float x, float y, float w, float h, Setting<bool>? setting = null, string text = "", int textSize = 0, string font = "main") : base(x, y, w, h, text, textSize, font, CenterMode.Y)
        {
            if (setting != null)
                Toggle = setting.Value;

            this.setting = setting;

            Style = new(ControlStyles.Checkbox_Colored);
        }

        public override float[] Draw()
        {
            Toggle = setting?.Value ?? Toggle;

            float width = Math.Min(rect.Width, rect.Height);
            float hGap = (rect.Height - width) / 2;
            RectangleF squareRect = new(rect.X, rect.Y + hGap, width, width);

            float[] fill = GLVerts.Rect(squareRect, Style.Tertiary);
            float[] outline = GLVerts.Outline(squareRect, 2, Style.Quaternary);

            float cWidth = width * 0.75f * checkSize;
            float cGap = (width - cWidth) / 2;
            RectangleF checkRect = new(rect.X + cGap, rect.Y + hGap + cGap, cWidth, cWidth);

            SetColor(Style.Primary);
            float[] check = GLVerts.Rect(checkRect, Style.Secondary);

            xOffset = width * 1.15f;

            return fill.Concat(outline).Concat(check).ToArray();
        }

        public override void PreRender(float mousex, float mousey, float frametime)
        {
            base.PreRender(mousex, mousey, frametime);

            float prevSize = checkSize;
            checkSize = Toggle ? Math.Min(1, checkSize + frametime * 8) : Math.Max(0, checkSize - frametime * 8);

            if (checkSize != prevSize)
                Update();
        }

        public override void MouseClickLeft(float x, float y)
        {
            base.MouseClickLeft(x, y);

            if (Hovering)
            {
                SoundPlayer.Play(Settings.clickSound.Value);
                Toggle ^= true;
            }
        }
    }
}
