using New_SSQE.NewGUI.Input;
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

        public GuiTextboxNumeric(float x, float y, float w, float h, Setting<float>? setting = null, bool isFloat = false, bool isPositive = false, string text = "0", int textSize = 0, string font = "main", CenterMode centerMode = CenterMode.XY) : base(x, y, w, h, null, text, textSize, font, centerMode)
        {
            this.setting = setting;
            this.isFloat = isFloat;
            this.isPositive = isPositive;

            if (setting != null)
                SetText(setting.Value.ToString());
        }

        public override void KeyDown(Keys key)
        {
            if (!Focused)
                return;

            bool ctrl = MainWindow.Instance.CtrlHeld;
            bool shift = MainWindow.Instance.ShiftHeld;

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

                default:
                    if (ctrl)
                        break;
                    string str = KeyConverter.GetCharFromInput(key, shift).ToString();
                    string sep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                    
                    if (int.TryParse(str, out _) || (str == sep && !text.Contains(sep)) || (str == "-" && !text.Contains('-') && cursorPos == 0))
                    {
                        SetText(text.Insert(cursorPos, str));
                        cursorPos++;
                    }

                    break;
            }

            cursorPos = Math.Clamp(cursorPos, 0, text.Length);
            Update();

            float numFloat = 0;
            int numInt = 0;

            if (setting != null && (isFloat ? float.TryParse(text, out numFloat) : int.TryParse(text, out numInt)))
            {
                float value = numFloat + numInt;
                float prevSetting = setting.Value;

                if (!isPositive || value > 0)
                    setting.Value = value;
                if (prevSetting != setting.Value)
                    InvokeValueChanged(new(value));
            }
        }
    }
}
