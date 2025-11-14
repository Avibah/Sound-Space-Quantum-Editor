using New_SSQE.NewGUI.Controls;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing;

namespace New_SSQE.NewGUI.Base
{
    internal abstract class GuiWindowDialog : GuiWindow
    {
        private static readonly Dictionary<string, GuiWindowDialog> Instances = [];

        private readonly ControlContainer InnerContainer;

        public readonly GuiButton CloseButton;
        public readonly GuiButton DragStrip;

        private readonly GuiSquare BackgroundSquare;

        private Vector2 startPos = Vector2.Zero;
        private Vector2 startOff = Vector2.Zero;
        private bool dragging = false;

        public GuiWindowDialog(float x, float y, float w, float h, params Control[] controls) : base(x - 2, y - 18, w + 4, h + 20)
        {
            InnerContainer = new(0, 0, w, h, controls);

            CloseButton = new(w - 26, -18, 30, 18)
            {
                Text = "X",
                TextSize = 14,
                Font = "square",
                Style = ControlStyle.Transparent,
                Stretch = StretchMode.XY
            };

            DragStrip = new(-2, -18, w - 24, 18)
            {
                PlayLeftClickSound = false,
                Style = ControlStyle.Transparent,
                Stretch = StretchMode.XY
            };

            BackgroundSquare = new(-2, -18, w + 4, h + 20)
            {
                Color = Color.FromArgb(127, 127, 127),
                Stretch = StretchMode.XY
            };

            base.SetControls(BackgroundSquare, CloseButton, DragStrip, InnerContainer);

            string name = GetType().Name;

            if (Instances.TryGetValue(name, out GuiWindowDialog? value))
            {
                Windowing.Close(value);
                Instances[name] = this;
            }
            else
                Instances.Add(name, this);

            CloseButton.LeftClick += (s, e) =>
            {
                Windowing.Close(this);
                Instances.Remove(name);
            };

            DragStrip.LeftClick += (s, e) =>
            {
                startPos = (e.X, e.Y);
                startOff = GetOffset();
                dragging = true;
            };
        }

        public override void SetControls(params Control[] controls)
        {
            InnerContainer.SetControls(controls);
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            base.Render(mousex, mousey, frametime);

            if (dragging)
            {
                float x = (mousex - startPos.X) / MainWindow.ScaleX + startOff.X;
                float y = (mousey - startPos.Y) / MainWindow.ScaleY + startOff.Y;

                RectangleF background = BackgroundSquare.GetOrigin();
                RectangleF origin = GetOrigin();
                Vector2 topLeft = (background.X + origin.X, background.Y + origin.Y);
                x = Math.Clamp(x, -topLeft.X, 1920 - background.Width - topLeft.X);
                y = Math.Clamp(y, -topLeft.Y, 1080 - background.Height - topLeft.Y);

                SetOffset((x, y));
            }
        }

        public override void MouseUp(float x, float y, MouseButtonEventArgs e)
        {
            base.MouseUp(x, y, e);

            if (e.Button == MouseButton.Left && dragging)
            {
                dragging = false;
                startPos = Vector2.Zero;
                startOff = Vector2.Zero;
            }
        }

        public virtual bool IsHovering()
        {
            return BackgroundSquare.GetRect().Contains(prevMouse.X, prevMouse.Y);
        }
    }
}
