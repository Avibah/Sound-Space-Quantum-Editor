using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;
using New_SSQE.Preferences;
using OpenTK.Mathematics;

namespace New_SSQE.NewGUI.CompoundControls
{
    internal class GuiNumberBox : ControlContainer
    {
        public readonly GuiTextboxNumeric ValueBox;
        public readonly GuiButtonTextured UpButton;
        public readonly GuiButtonTextured DownButton;

        public float Value = 0;

        public Vector2 Bounds
        {
            get => ValueBox.Bounds;
            set => ValueBox.Bounds = value;
        }

        public new ControlStyle Style
        {
            get => ValueBox.Style;
            set => ValueBox.Style = value;
        }

        public new string Text
        {
            get => ValueBox.Text;
            set
            {
                if (!float.TryParse(value, out Value))
                    Value = 0;
                ValueBox.Text = value;
            }
        }

        public new float TextSize
        {
            get => ValueBox.TextSize;
            set => ValueBox.TextSize = value;
        }

        public new string Font
        {
            get => ValueBox.Font;
            set => ValueBox.Font = value;
        }

        public new CenterMode CenterMode
        {
            get => ValueBox.CenterMode;
            set => ValueBox.CenterMode = value;
        }

        public Setting<float>? Setting
        {
            get => ValueBox.Setting;
            set
            {
                if (value != null)
                    Value = value.Value;
                ValueBox.Setting = value;
            }
        }

        public bool IsFloat
        {
            get => ValueBox.IsFloat;
            set => ValueBox.IsFloat = value;
        }

        public bool IsPositive
        {
            get => ValueBox.IsPositive;
            set => ValueBox.IsPositive = value;
        }

        private float increment;
        
        public float Increment
        {
            get => increment;
            set
            {
                value = Math.Abs(value);

                if (value != increment)
                {
                    increment = value;
                    shouldUpdate = true;
                }
            }
        }

        public GuiNumberBox(float x, float y, float w, float h) : base(x, y, w, h)
        {
            ValueBox = new(0, 0, w - w / 8 - 2, h)
            {
                Stretch = StretchMode.XY
            };

            UpButton = new(w - w / 8, 0, w / 8, h / 2, new("Arrows"))
            {
                Stretch = StretchMode.XY,
                TileSize = (2, 1)
            };

            DownButton = new(w - w / 8, h / 2, w / 8, h / 2, new("Arrows"))
            {
                Stretch = StretchMode.XY,
                TileSize = (2, 1),
                TileIndex = 1
            };

            SetControls(ValueBox, UpButton, DownButton);
        }

        public override void Reset()
        {
            base.Reset();

            UpButton.LeftClick += (s, e) => IncrementUp();
            DownButton.LeftClick += (s, e) => IncrementDown();

            ValueBox.ValueChanged += (s, e) =>
            {
                Value = e.Value;
                InvokeValueChanged(new(Value));
            };

            Value = Setting?.Value ?? Value;
        }

        public float ApplyIncrement(float increment)
        {
            Value += increment;

            if (IsPositive)
                Value = Math.Max(Value, this.increment);
            if (!IsFloat)
                Value = (int)Value;

            Value = Math.Clamp(Value, Bounds.X, Bounds.Y);
            Value = (float)Math.Round(Value, 3);

            if (Setting != null)
                Setting.Value = Value;
            ValueBox.Text = Value.ToString();
            InvokeValueChanged(new(Value));

            return Value;
        }
        public float IncrementUp() => ApplyIncrement(increment);
        public float IncrementDown() => ApplyIncrement(-increment);
    }
}
