using New_SSQE.NewGUI.Base;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiButton : InteractiveControl
    {
        private float hoverTime = 0f;
        
        public GuiButton(float x, float y, float w, float h) : base(x, y, w, h)
        {
            Style = ControlStyle.Button_Uncolored;
            PlayRightClickSound = false;
        }

        public override float[] Draw()
        {
            float[] fill = GLVerts.Squircle(rect, CornerDetail, CornerRadius, Style.Primary);
            float[] outline = GLVerts.SquircleOutline(rect, 2f, CornerDetail, CornerRadius, Style.Secondary);
            float[] mask = GLVerts.Squircle(rect, CornerDetail, CornerRadius, Style.Tertiary, hoverTime);

            return [.. fill, .. outline, .. mask];
        }

        public override void PreRender(float mousex, float mousey, float frametime)
        {
            base.PreRender(mousex, mousey, frametime);

            float prevTime = hoverTime;
            hoverTime = Math.Clamp((hoverTime / 0.05f) + (Hovering ? 10 : -10) * frametime, 0, 1) * 0.05f;

            if (hoverTime != prevTime)
                Update();
        }

        private string keybind = "";

        public void BindKeybind(string keybind) => this.keybind = keybind;

        public override void KeybindUsed(string keybind)
        {
            base.KeybindUsed(keybind);

            if (keybind == this.keybind)
                InvokeLeftClick(new(0, 0));
        }
    }
}
