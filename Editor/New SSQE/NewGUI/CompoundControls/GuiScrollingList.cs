using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;
using New_SSQE.Preferences;
using OpenTK.Mathematics;
using System.Drawing;

namespace New_SSQE.NewGUI.CompoundControls
{
    internal class GuiScrollingList : ControlContainer
    {
        public readonly ControlContainer Container;
        public readonly GuiSquare SliderBackdrop;
        public readonly GuiSlider Slider;

        public new ControlStyle Style
        {
            get => Slider.Style;
            set => Slider.Style = value;
        }

        private readonly Setting<SliderSetting> setting = new SliderSetting(0, 0, 1);

        public GuiScrollingList(float x, float y, float w, float h, params Control[] controls) : base(x, y, w + Math.Min(20, w / 10), h)
        {
            Container = new(0, 0, w, h, controls)
            {
                ClipDescendants = true
            };

            SliderBackdrop = new(w, 0, Math.Min(20, w / 10), h)
            {
                Color = ControlStyle.Button_Uncolored.Primary,
                CornerRadius = 1
            };

            Slider = new(w, 0, Math.Min(20, w / 10), h, setting)
            {
                Style = ControlStyle.None
            };

            SetControls(Container, SliderBackdrop, Slider);
        }

        public override void Resize(float screenWidth, float screenHeight)
        {
            if (Container != null)
            {
                Container.Resize(1920, 1080);
                float curExtent = 0;
                setting.Value.Value = Math.Clamp(setting.Value.Value, 0, setting.Value.Max);

                foreach (Control control in Container.Children)
                {
                    Vector4 extents = control.GetExtents();
                    Vector2 offset = (extents.X, extents.Y + curExtent - setting.Value.Value);
                    curExtent += extents.Y + extents.W;

                    RectangleF origin = control.GetOrigin();
                    control.SetOrigin(offset.X, offset.Y, origin.Width, origin.Height);
                }

                setting.Value.Max = Math.Max(0, curExtent - StartRect.Height);
            }
            
            if (Slider != null)
            {
                Slider.Visible = setting.Value.Max > 0;
                SliderBackdrop.Visible = Slider.Visible;
                ConsumeScroll = Slider.Visible;
            }

            base.Resize(screenWidth, screenHeight);
        }

        public override void Reset()
        {
            base.Reset();

            Slider.ValueChanged += (s, e) => InvokeResize();
        }

        public override void MouseScroll(float x, float y, float delta)
        {
            setting.Value.Value = Math.Clamp(setting.Value.Value - 10 * delta, 0, setting.Value.Max);
            Slider.Update();
            InvokeResize();

            base.MouseScroll(x, y, delta);
        }

        public void AddControls(params Control[] controls)
        {
            List<Control> children = [.. Container.Children];
            
            foreach (Control control in controls)
            {
                if (!children.Contains(control))
                    children.Add(control);
            }

            Container.SetControls([.. children]);
            SetControls(Children);
        }

        public void RemoveControls(params Control[] controls)
        {
            List<Control> children = [.. Container.Children];

            foreach (Control control in controls)
                children.Remove(control);

            Container.SetControls([.. children]);
            SetControls(Children);
        }

        public void ClearControls()
        {
            Container.SetControls();
            SetControls(Children);
        }

        private void InvokeResize() => Resize(MainWindow.Instance.ClientSize.X, MainWindow.Instance.ClientSize.Y);
    }
}
