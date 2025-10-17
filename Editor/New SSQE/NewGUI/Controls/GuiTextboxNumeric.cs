using New_SSQE.Preferences;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiTextboxNumeric : GuiTextbox
    {
        private Setting<float>? setting = null;
        private bool isFloat = false;
        private bool isPositive = false;

        public new Setting<float>? Setting
        {
            get => setting;
            set
            {
                if (value != setting)
                {
                    setting = value;
                    shouldUpdate = true;
                }

                Refresh();
            }
        }

        public bool IsFloat
        {
            get => isFloat;
            set
            {
                if (value != isFloat)
                {
                    isFloat = value;
                    shouldUpdate = true;
                }
            }
        }

        public bool IsPositive
        {
            get => isPositive;
            set
            {
                if (value != isPositive)
                {
                    isPositive = value;
                    shouldUpdate = true;
                }

                Refresh();
            }
        }

        private string prevText = "";
        private int prevCursor = 0;

        public Vector2 Bounds = (float.MinValue, float.MaxValue);

        public GuiTextboxNumeric(float x, float y, float w, float h) : base(x, y, w, h) { }

        private void Refresh()
        {
            if (setting != null)
                Text = setting.Value.ToString();
            if (isPositive)
                Bounds = (0, float.MaxValue);
        }

        public override void Reset()
        {
            base.Reset();
            Refresh();

            TextEntered += (s, e) =>
            {
                if (!float.TryParse(e.Text, out float value) || value < Bounds.X)
                    Text = Bounds.X.ToString();
            };
        }

        public override void KeyDown(Keys key)
        {
            if (!Focused)
                return;

            bool ctrl = MainWindow.Instance.CtrlHeld;
            cursorPos = Math.Clamp(cursorPos, 0, Text.Length);

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
                    prevText = Text;
                    prevCursor = cursorPos;
                    base.KeyDown(key);
                    break;
            }
        }

        public override void TextInput(string str)
        {
            if (isPositive && str == "-")
                return;

            prevText = Text;
            prevCursor = cursorPos;
            base.TextInput(str);
        }

        protected override void FinishInput()
        {
            float numFloat = 0;
            int numInt = 0;

            string temp = Text;
            if (string.IsNullOrEmpty(temp) || temp == "-" || temp == Program.Culture.NumberFormat.NumberDecimalSeparator) 
                temp = "0";

            if (isFloat ? float.TryParse(temp, out numFloat) : int.TryParse(temp, out numInt))
            {
                float value = Math.Max(numFloat + numInt, Bounds.X);

                if (value > Bounds.Y)
                {
                    value = Bounds.Y;
                    if (Bounds.Y != float.MaxValue)
                        Text = value.ToString();
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
                Text = prevText;
            }

            cursorPos = Math.Clamp(cursorPos, 0, Text.Length);
            Update();
        }
    }
}
