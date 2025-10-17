using New_SSQE.NewGUI.Base;
using New_SSQE.Preferences;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiTextboxKeybind : GuiTextbox
    {
        private readonly Setting<Keybind> setting;

        public GuiTextboxKeybind(float x, float y, float w, float h, Setting<Keybind> setting) : base(x, y, w, h)
        {
            this.setting = setting;
        }

        public override void KeyDown(Keys key)
        {
            if (!Focused)
                return;

            if (key == Keys.LeftControl || key == Keys.RightControl)
                return;
            if (key == Keys.LeftAlt || key == Keys.RightAlt)
                return;
            if (key == Keys.LeftShift || key == Keys.RightShift)
                return;

            if (key == Keys.Backspace)
                key = Keys.Delete;

            bool ctrl = MainWindow.Instance.CtrlHeld;
            bool alt = MainWindow.Instance.AltHeld;
            bool shift = MainWindow.Instance.ShiftHeld;

            setting.Value.Key = key;
            setting.Value.Ctrl = ctrl;
            setting.Value.Alt = alt;
            setting.Value.Shift = shift;

            Text = key.ToString().ToUpper();
            cursorPos = Text.Length;
        }

        public override void TextInput(string str) { }

        public override float[] Draw()
        {
            Text = setting.Value.Key.ToString().ToUpper();

            return base.Draw();
        }
    }
}
