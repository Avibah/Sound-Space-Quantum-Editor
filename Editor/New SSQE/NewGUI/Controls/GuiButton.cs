using New_SSQE.Misc.Static;
using New_SSQE.NewGUI.Base;
using System.Drawing;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiButton : InteractiveControl
    {
        public GuiButton(float x, float y, float w, float h) : base(x, y, w, h)
        {
            Style = ControlStyle.Button_Uncolored;
            PlayRightClickSound = false;

            Gradient = new(x, y, w, h)
            {
                Rotation = 90,
                StartColor = Color.FromArgb(0, 255, 255, 255),
                EndColor = Color.FromArgb(11, 255, 255, 255)
            };

            Animator.AddKey("HoverTime", 0.075f, EasingStyle.Linear);
            Animator.SetReversed(true);
        }

        public override float[] Draw()
        {
            float[] fill = GLVerts.Squircle(rect, CornerDetail, CornerRadius, Style.Primary);
            float[] outline = GLVerts.SquircleOutline(rect, 2f, CornerDetail, CornerRadius, Style.Secondary);
            float[] mask = GLVerts.Squircle(rect, CornerDetail, CornerRadius, Style.Tertiary, Animator["HoverTime"] * 0.05f);
            float[] clickMask = GLVerts.Squircle(rect, CornerDetail, CornerRadius, Style.Tertiary, Dragging ? 0.04f : 0);

            return [.. fill, .. outline, .. mask, .. clickMask];
        }

        public override void Reset()
        {
            base.Reset();
            Animator.Play();
        }

        public override void MouseEnter(float x, float y)
        {
            base.MouseEnter(x, y);
            Animator.SetReversed(false);
        }

        public override void MouseLeave(float x, float y)
        {
            base.MouseLeave(x, y);
            Animator.SetReversed(true);
        }

        private string keybind = "";

        public void BindKeybind(string keybind) => this.keybind = keybind;

        public override void KeybindUsed(string keybind)
        {
            base.KeybindUsed(keybind);

            if (keybind == this.keybind)
                InvokeLeftClick(new(0, 0));
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
