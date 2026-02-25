using New_SSQE.Audio;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewMaps;
using New_SSQE.Preferences;
using OpenTK.Mathematics;
using System.Drawing;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiSlider : InteractiveControl
    {
        public EventHandler<ValueChangedEventArgs<float>>? ValueChanged;

        public float ShiftIncrement = 1;

        protected Setting<SliderSetting> setting;
        protected bool canReset = true;

        private bool reverse;

        private float prevValue = 0f;

        public bool Reverse
        {
            get => reverse;
            set
            {
                if (value != reverse)
                {
                    reverse = value;
                    shouldUpdate = true;
                }
            }
        }

        public GuiSlider(float x, float y, float w, float h, Setting<SliderSetting> setting) : base(x, y, w, h)
        {
            this.setting = setting;

            prevValue = setting.Value.Value;

            Style = ControlStyle.Slider_Uncolored;
            Animator.AddKey("HoverTime", 0.1f).Reversed = true;
        }

        public override float[] Draw()
        {
            bool horizontal = rect.Width > rect.Height;

            float progress = setting.Value.Max == 0 ? 0.5f : setting.Value.Value / setting.Value.Max;
            if (reverse)
                progress = 1 - progress;

            RectangleF lineRect = horizontal ? new(rect.X + rect.Height / 2, rect.Y + rect.Height / 2 - 1.5f, rect.Width - rect.Height, 3)
                : new(rect.X + rect.Width / 2 - 1.5f, rect.Y + rect.Width / 2, 3, rect.Height - rect.Width);
            Vector2 circlePos = (lineRect.X + lineRect.Width * (horizontal ? progress : 0.5f), lineRect.Y + lineRect.Height * (horizontal ? 0.5f : progress));

            float[] line = GLVerts.Squircle(lineRect, cornerDetail, cornerRadius, Style.Secondary);
            float[] circle = GLVerts.Polygon(circlePos.X, circlePos.Y, 4, 16, 0, Style.Primary);

            float hoverTime = Animator["HoverTime"];
            if (hoverTime > 0)
            {
                float[] hoverCircle = GLVerts.PolygonOutline(circlePos.X, circlePos.Y, 12 * hoverTime, 2, 6, 90 * hoverTime, Style.Primary);
                circle = [..circle, ..hoverCircle];
            }

            return [..line, ..circle];
        }

        public override void Reset()
        {
            base.Reset();
            Animator.Play();
        }

        public override void MouseEnter(float x, float y)
        {
            base.MouseEnter(x, y);
            Animator.SetReversed(false);
        }

        public override void MouseLeave(float x, float y)
        {
            base.MouseLeave(x, y);
            if (!Dragging)
                Animator.SetReversed(true);
        }

        public override void MouseUpLeft(float x, float y)
        {
            base.MouseUpLeft(x, y);
            if (!Hovering)
                Animator.SetReversed(true);
        }

        public override void MouseDownRight(float x, float y)
        {
            if (canReset)
            {
                setting.Value.Value = setting.Value.Default;
                UpdateSlider();
                Update();
            }

            base.MouseDownRight(x, y);
        }

        public override void MouseMove(float x, float y)
        {
            base.MouseMove(x, y);

            if (Dragging)
            {
                bool horizontal = rect.Width > rect.Height;
                float width = (rect.Height - rect.Width) * (horizontal ? -1 : 1);

                float step = setting.Value.Step / setting.Value.Max;
                if (!MainWindow.Instance.ShiftHeld)
                    step /= ShiftIncrement;

                float pos = horizontal ? rect.X + rect.Height / 2 : rect.Y + rect.Width / 2;
                float mouse = horizontal ? x : y;

                float progress = (float)Math.Round((horizontal ? mouse - pos : reverse ? (width - mouse + pos) : mouse - pos) / width / step) * step;
                setting.Value.Value = Math.Clamp(setting.Value.Max * progress, 0, setting.Value.Max);

                UpdateSlider();
                Update();
            }
        }

        private void UpdateSlider()
        {
            switch (setting.Name)
            {
                case "trackHeight":
                    break;

                case "sfxVolume":
                    SoundPlayer.Volume = setting.Value.Value;
                    break;

                case "masterVolume":
                    MusicPlayer.Volume = setting.Value.Value;
                    break;

                case "tempo":
                    Mapping.Current.Tempo = setting.Value.Value;
                    break;
            }

            if (setting.Value.Value != prevValue)
                ValueChanged?.Invoke(this, new(setting.Value.Value));
            prevValue = setting.Value.Value;
        }

        public override void DisconnectAll()
        {
            base.DisconnectAll();
            ValueChanged = null;
        }
    }
}
