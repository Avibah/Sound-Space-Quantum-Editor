using New_SSQE.NewGUI.Base;
using New_SSQE.Preferences;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiTextboxNumeric : GuiTextbox
    {
        private readonly Setting<float>? setting;
        private readonly bool isFloat;
        private readonly bool isPositive;

        private string prevText = "";
        private int prevCursor = 0;

        public Vector2 Bounds = (float.MinValue, float.MaxValue);

        public GuiTextboxNumeric(float x, float y, float w, float h, Setting<float>? setting = null, bool isFloat = false, bool isPositive = false, string text = "0", int textSize = 0, string font = "main", CenterMode centerMode = CenterMode.XY) : base(x, y, w, h, null, text, textSize, font, centerMode)
        {
            this.setting = setting;
            this.isFloat = isFloat;
            this.isPositive = isPositive;

            if (setting != null)
                SetText(setting.Value.ToString());
            if (isPositive)
                Bounds = (0, float.MaxValue);
        }

        public override void KeyDown(Keys key)
        {
            if (!Focused)
                return;

            bool ctrl = MainWindow.Instance.CtrlHeld;
            cursorPos = Math.Clamp(cursorPos, 0, text.Length);

            switch (key)
            {
                case Keys.C when ctrl:
                case Keys.V when ctrl:
                case Keys.X when ctrl:
                case Keys.Left:
                case Keys.Right:
                case Keys.Backspace:
                case Keys.Delete:
                case Keys.Enter:
                case Keys.KeyPadEnter:
                case Keys.Escape:
                    prevText = text;
                    prevCursor = cursorPos;
                    base.KeyDown(key);
                    break;
            }
        }

        public override void TextInput(string str)
        {
            if (isPositive && str == "-")
                return;

            prevText = text;
            prevCursor = cursorPos;
            base.TextInput(str);
        }

        protected override void FinishInput()
        {
            float numFloat = 0;
            int numInt = 0;

            string temp = text;
            if (string.IsNullOrEmpty(temp) || temp == "-" || temp == Program.Culture.NumberFormat.NumberDecimalSeparator) 
                temp = "0";

            if (isFloat ? float.TryParse(temp, out numFloat) : int.TryParse(temp, out numInt))
            {
                float value = numFloat + numInt;
                
                if (value < Bounds.X)
                {
                    value = Bounds.X;
                    if (Bounds.X != float.MinValue)
                        SetText(value.ToString());
                }

                if (value > Bounds.Y)
                {
                    value = Bounds.Y;
                    if (Bounds.Y != float.MaxValue)
                        SetText(value.ToString());
                }

                if (setting != null)
                {
                    float prevSetting = setting.Value;

                    setting.Value = value;

                    if (prevSetting != setting.Value)
                        InvokeValueChanged(new(value));
                }
                else
                    InvokeValueChanged(new(value));
            }
            else
            {
                cursorPos = prevCursor;
                SetText(prevText);
            }

            cursorPos = Math.Clamp(cursorPos, 0, text.Length);
            Update();
        }
    }
}
