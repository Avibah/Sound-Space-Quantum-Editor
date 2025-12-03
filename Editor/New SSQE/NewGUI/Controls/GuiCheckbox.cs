using New_SSQE.Misc.Static;
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
        private bool inverted;

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
                Animator.SetReversed("CheckSize", !value);

                if (setting != null)
                    setting.Value = value ^ inverted;
            }
        }

        public bool Inverted
        {
            get => inverted;
            set
            {
                if (inverted != value)
                {
                    inverted = value;
                    shouldUpdate = true;

                    if (setting != null)
                        Toggle = setting.Value ^ inverted;
                }
            }
        }

        public GuiCheckbox(float x, float y, float w, float h) : base(x, y, w, h)
        {
            if (setting != null)
                Toggle = setting.Value ^ inverted;

            Style = ControlStyle.Checkbox_Colored;
            PlayRightClickSound = false;
            CenterMode = CenterMode.Y;

            CornerRadius = 0.5f;
            Animator.AddKey("CheckSize", 0.125f);
            Animator.AddKey("HoverTime", 0.075f, EasingStyle.Linear);
            Animator.SetReversed("HoverTime", true);
        }

        public override void Reset()
        {
            base.Reset();
            Animator.Play();

            LeftClick += (s, e) =>
            {
                Toggle ^= true;
                ValueChanged?.Invoke(this, new(Toggle));
            };
        }

        public override float[] Draw()
        {
            Toggle = (setting?.Value ^ inverted) ?? Toggle;

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
            float[] mask = GLVerts.Squircle(squareRect, CornerDetail, CornerRadius, Style.Quinary, Animator["HoverTime"] * 0.05f);
            float[] clickMask = GLVerts.Squircle(squareRect, CornerDetail, CornerRadius, Style.Quinary, Dragging ? 0.04f : 0);

            return [.. fill, .. outline, .. check, .. mask, .. clickMask];
        }

        public override void DisconnectAll()
        {
            base.DisconnectAll();
            ValueChanged = null;
        }

        public override void MouseEnter(float x, float y)
        {
            base.MouseEnter(x, y);
            Animator.SetReversed("HoverTime", false);
        }

        public override void MouseLeave(float x, float y)
        {
            base.MouseLeave(x, y);
            Animator.SetReversed("HoverTime", true);
        }

        public override void MouseDownLeft(float x, float y)
        {
            shouldUpdate = true;
            base.MouseDownLeft(x, y);
        }

        public override void MouseUpLeft(float x, float y)
        {
            shouldUpdate = true;
            base.MouseUpLeft(x, y);
        }
    }
}
