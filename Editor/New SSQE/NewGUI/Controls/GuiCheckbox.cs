using New_SSQE.NewGUI.Base;
using New_SSQE.Preferences;
using System.Drawing;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiCheckbox : InteractiveControl
    {
        private Setting<bool>? setting;
        private bool toggle;

        public Setting<bool>? Setting
        {
            get => setting;
            set
            {
                if (value != setting)
                {
                    setting = value;
                    shouldUpdate = true;
                }
            }
        }

        public bool Toggle
        {
            get => toggle;
            set
            {
                toggle = value;

                if (setting != null)
                    setting.Value = value;
            }
        }

        private float checkSize = 0f;

        public GuiCheckbox(float x, float y, float w, float h) : base(x, y, w, h)
        {
            if (setting != null)
                Toggle = setting.Value;

            Style = ControlStyle.Checkbox_Colored;
            PlayRightClickSound = false;
            CenterMode = CenterMode.Y;
        }

        public override float[] Draw()
        {
            Toggle = setting?.Value ?? Toggle;

            float width = Math.Min(rect.Width, rect.Height);
            float hGap = (rect.Height - width) / 2;
            RectangleF squareRect = new(rect.X, rect.Y + hGap, width, width);

            float cWidth = width * 0.75f * checkSize;
            float cGap = (width - cWidth) / 2;
            RectangleF checkRect = new(rect.X + cGap, rect.Y + hGap + cGap, cWidth, cWidth);

            TextColor = Style.Primary;
            xOffset = width * 1.15f;

            float[] fill = GLVerts.Rect(squareRect, Style.Tertiary);
            float[] outline = GLVerts.Outline(squareRect, 2f, Style.Quaternary);
            float[] check = GLVerts.Rect(checkRect, Style.Secondary);

            return [..fill, ..outline, ..check];
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
            if (Hovering)
                Toggle ^= true;

            base.MouseClickLeft(x, y);
        }
    }
}
