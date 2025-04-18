using New_SSQE.NewGUI.Base;
using New_SSQE.Preferences;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiNumberBox : ControlContainer
    {
        public readonly GuiTextboxNumeric ValueBox;
        public readonly GuiButtonTextured UpButton;
        public readonly GuiButtonTextured DownButton;

        public float Value = 0;

        private readonly Setting<float>? setting;
        private readonly float increment;

        private readonly bool isFloat;
        private readonly bool isPositive;

        public GuiNumberBox(float x, float y, float w, float h, float increment, Setting<float>? setting = null, bool isFloat = false, bool isPositive = false, string text = "0", int textSize = 0, string font = "main", CenterMode centerMode = CenterMode.XY) : base(x, y, w, h)
        {
            ValueBox = new(0, 0, w - w / 8 - 2, h, setting, isFloat, isPositive, text, textSize, font, centerMode) { Stretch = StretchMode.XY };
            UpButton = new(w - w / 8, 0, w / 8, h / 2, "Arrows") { Stretch = StretchMode.XY, TileSize = (2, 1) };
            DownButton = new(w - w / 8, h / 2, w / 8, h / 2, "Arrows") { Stretch = StretchMode.XY, TileSize = (2, 1), TileIndex = 1 };

            SetControls(ValueBox, UpButton, DownButton);

            if (setting != null)
                Value = setting.Value;
            else if (!float.TryParse(text, out Value))
                Value = 0;

            this.setting = setting;
            this.increment = Math.Abs(increment);

            this.isFloat = isFloat;
            this.isPositive = isPositive;
        }

        public override void Reset()
        {
            base.Reset();

            UpButton.LeftClick += (s, e) => IncrementUp();
            DownButton.LeftClick += (s, e) => IncrementDown();
        }

        public float Increment(float increment)
        {
            Value += increment;

            if (isPositive)
                Value = Math.Max(Value, this.increment);
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
