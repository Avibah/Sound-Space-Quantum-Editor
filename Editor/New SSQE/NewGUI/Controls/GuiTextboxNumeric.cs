using New_SSQE.NewGUI.Base;
using New_SSQE.Preferences;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Globalization;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiTextboxNumeric : GuiTextbox
    {
        private readonly Setting<float>? setting;
        private readonly bool isFloat;
        private readonly bool isPositive;

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
                    base.KeyDown(key);
                    break;
            }
        }

        public override void TextInput(string str)
        {
            if (!Focused)
                return;
            if (MainWindow.Instance.CtrlHeld)
                return;

            string sep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            if (!isFloat && str == sep)
                return;

            if (int.TryParse(str, out _) || (str == sep && !text.Contains(sep)) || (!isPositive && str == "-" && !text.Contains('-') && cursorPos == 0))
                base.TextInput(str);
        }

        protected override void FinishInput()
        {
            cursorPos = Math.Clamp(cursorPos, 0, text.Length);
            Update();

            float numFloat = 0;
            int numInt = 0;

            if (isFloat ? float.TryParse(text, out numFloat) : int.TryParse(text, out numInt))
            {
                float value = Math.Clamp(numFloat + numInt, Bounds.X, Bounds.Y);

                if (setting != null)
                {
                    float prevSetting = setting.Value;

                    setting.Value = value;
                    SetText($"{value}");

                    if (prevSetting != setting.Value)
                        InvokeValueChanged(new(value));
                }
                else
                    InvokeValueChanged(new(value));
            }
        }
    }
}
