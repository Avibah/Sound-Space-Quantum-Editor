using New_SSQE.Preferences;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiNumberBox : ControlContainer
    {
        public readonly GuiTextboxNumeric ValueBox;
        public readonly GuiButton UpButton;
        public readonly GuiButton DownButton;

        public float Value = 0;

        private readonly Setting<float>? setting;
        private readonly float increment;

        private readonly bool isFloat;
        private readonly bool isPositive;

        public GuiNumberBox(float x, float y, float w, float h, float increment, Setting<float>? setting = null, bool isFloat = false, bool isPositive = false, string text = "0", int textSize = 0, string font = "main", bool centered = true) : base(x, y, w, h)
        {
            ValueBox = new(x, y, w - w / 8, h, setting, isFloat, isPositive, text, textSize, font, centered);
            UpButton = new(x + w - w / 8, y, w / 8, h / 2, "^", 16, "square");
            DownButton = new(x + w - w / 8, y + h / 2, w / 8, h / 2, "v", 16, "square");

            SetControls(ValueBox, UpButton, DownButton);

            Value = setting == null ? 0 : setting.Value;

            this.setting = setting;
            this.increment = increment;

            this.isFloat = isFloat;
            this.isPositive = isPositive;

            UpButton.LeftClick += (s, e) => IncrementUp();
            DownButton.LeftClick += (s, e) => IncrementDown();
        }

        public float Increment(float increment)
        {
            Value += increment;

            if (!isPositive)
                Value = Math.Max(Value, 0);
            if (!isFloat)
                Value = (int)Value;

            if (setting != null)
                setting.Value = Value;
            ValueBox.SetText(Value.ToString());

            return Value;
        }
        public float IncrementUp() => Increment(increment);
        public float IncrementDown() => Increment(-increment);
    }
}
