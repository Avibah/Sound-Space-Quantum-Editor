using New_SSQE.NewGUI.Base;
using New_SSQE.Preferences;
using System.Drawing;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiCheckbox : InteractiveControl
    {
        public EventHandler<ValueChangedEventArgs<bool>>? ValueChanged;

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
                Animator.Reversed = !value;

                if (setting != null)
                    setting.Value = value;
            }
        }

        public GuiCheckbox(float x, float y, float w, float h) : base(x, y, w, h)
        {
            if (setting != null)
                Toggle = setting.Value;

            Style = ControlStyle.Checkbox_Colored;
            PlayRightClickSound = false;
            CenterMode = CenterMode.Y;

            CornerRadius = 0.5f;
            Animator.AddKey("CheckSize", 0.125f);
        }

        public override void Reset()
        {
            base.Reset();
            Animator.Play();
        }

        public override float[] Draw()
        {
            Toggle = setting?.Value ?? Toggle;

            float width = Math.Min(rect.Width, rect.Height);
            float hGap = (rect.Height - width) / 2;
            RectangleF squareRect = new(rect.X, rect.Y + hGap, width, width);

            float cWidth = width * 0.75f * Animator["CheckSize"];
            float cGap = (width - cWidth) / 2;
            RectangleF checkRect = new(rect.X + cGap, rect.Y + hGap + cGap, cWidth, cWidth);

            TextColor = Style.Primary;
            xOffset = width * 1.15f;

            float[] fill = GLVerts.Squircle(squareRect, CornerDetail, CornerRadius, Style.Tertiary);
            float[] outline = GLVerts.SquircleOutline(squareRect, 2f, CornerDetail, CornerRadius, Style.Quaternary);
            float[] check = GLVerts.Squircle(checkRect, CornerDetail, CornerRadius, Style.Secondary);

            return [..fill, ..outline, ..check];
        }

        public override void MouseClickLeft(float x, float y)
        {
            Toggle ^= true;
            ValueChanged?.Invoke(this, new(Toggle));
            base.MouseClickLeft(x, y);
        }

        public override void DisconnectAll()
        {
            base.DisconnectAll();
            ValueChanged = null;
        }
    }
}
