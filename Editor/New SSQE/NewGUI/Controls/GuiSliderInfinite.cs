using New_SSQE.Preferences;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiSliderInfinite : GuiSlider
    {
        private float min;
        private float max;

        public float Min
        {
            get => min;
            set
            {
                if (value != min)
                {
                    min = value;
                    shouldUpdate = true;
                }
            }
        }

        public float Max
        {
            get => max;
            set
            {
                if (value != max)
                {
                    max = value;
                    shouldUpdate = true;
                }
            }
        }

        public GuiSliderInfinite(float x, float y, float w, float h, Setting<SliderSetting> setting) : base(x, y, w, h, setting) { }

        public override void MouseMove(float x, float y)
        {
            base.MouseMove(x, y);

            if (Dragging)
            {
                bool horizontal = rect.Width > rect.Height;
                float width = horizontal ? MainWindow.Instance.ClientSize.X / 1920f : MainWindow.Instance.ClientSize.Y / 1080f;
                float value = setting.Value.Value + (horizontal ? MainWindow.Instance.Delta.X : MainWindow.Instance.Delta.Y) * setting.Value.Step / width;
                value = Math.Clamp(value, min, max);

                setting.Value.Value = value;
                setting.Value.Max = 2 * value;
            }
        }
    }
}
