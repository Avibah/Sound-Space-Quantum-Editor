using New_SSQE.Audio;
using New_SSQE.NewGUI.Base;
using New_SSQE.Preferences;
using OpenTK.Mathematics;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiButton : InteractiveControl
    {
        private float hoverTime = 0f;
        protected bool RightResponsive = false;
        
        public GuiButton(float x, float y, float w, float h, string text = "", int textSize = 0, string font = "main", CenterMode centerMode = CenterMode.XY) : base(x, y, w, h, text, textSize, font, centerMode)
        {
            Style = new(ControlStyles.Button_Uncolored);
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
            hoverTime = MathHelper.Clamp((hoverTime / 0.05f) + (Hovering ? 10 : -10) * frametime, 0, 1) * 0.05f;

            if (hoverTime != prevTime)
                Update();
        }

        public override void MouseClickLeft(float x, float y)
        {
            if (Hovering)
                SoundPlayer.Play(Settings.clickSound.Value);

            base.MouseClickLeft(x, y);
        }

        public override void MouseClickRight(float x, float y)
        {
            if (Hovering && RightResponsive)
                SoundPlayer.Play(Settings.clickSound.Value);

            base.MouseClickRight(x, y);
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
