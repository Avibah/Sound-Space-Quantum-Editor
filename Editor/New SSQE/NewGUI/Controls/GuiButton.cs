using New_SSQE.NewGUI.Base;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiButton : InteractiveControl
    {
        private float hoverTime = 0f;
        
        public GuiButton(float x, float y, float w, float h, string text = "", int textSize = 0, string font = "main", CenterMode centerMode = CenterMode.XY) : base(x, y, w, h, text, textSize, font, centerMode)
        {
            Style = ControlStyle.Button_Uncolored;
            PlayRightClickSound = false;
        }

        public override float[] Draw()
        {
            float[] fill = GLVerts.Rect(rect, Style.Primary);
            float[] outline = GLVerts.Outline(rect, 2f, Style.Secondary);
            float[] mask = GLVerts.Rect(rect, Style.Tertiary, hoverTime);

            return fill.Concat(outline).Concat(mask).ToArray();
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
