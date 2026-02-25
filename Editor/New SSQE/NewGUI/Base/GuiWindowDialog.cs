using New_SSQE.NewGUI.Controls;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing;

namespace New_SSQE.NewGUI.Base
{
    internal abstract class GuiWindowDialog : GuiWindow
    {
        private const int CLOSE_WIDTH = 45;
        private const int TOPBAR_HEIGHT = 34;

        private static readonly Dictionary<string, GuiWindowDialog> Instances = [];
        private readonly string name;

        private readonly ControlContainer InnerContainer;

        public readonly GuiButtonTextured CloseButton;
        public readonly GuiButton DragStrip;

        public readonly GuiSquare Backdrop;
        public readonly GuiSquare BackgroundSquare;
        public readonly GuiSquare DragSquare;

        private Vector2 startPos = Vector2.Zero;
        private Vector2 startOff = Vector2.Zero;
        private bool dragging = false;

        public bool Modal = false;
        public bool Persistent = false;

        public GuiWindowDialog(float x, float y, float w, float h, params Control[] controls) : base(x - 1, y - (TOPBAR_HEIGHT - 2) + 1, w + 2, h + TOPBAR_HEIGHT)
        {
            InnerContainer = new(1, TOPBAR_HEIGHT - 1, w, h, controls);

            CloseButton = new(w - CLOSE_WIDTH + 1, 1, CLOSE_WIDTH, TOPBAR_HEIGHT - 2, new("DialogClose"))
            {
                Style = ControlStyle.Button_Close,
                Stretch = StretchMode.XY,
                LineThickness = 0,
                HoverIntensity = 0.9f,
                ClickIntensity = -0.2f,
                CornerDetail = 0
            };

            DragStrip = new(1, 1, w - CLOSE_WIDTH, TOPBAR_HEIGHT - 2)
            {
                PlayLeftClickSound = false,
                Style = ControlStyle.Transparent,
                Stretch = StretchMode.XY,
                Gradient = null,
            };

            Backdrop = new(0, 0, w + 2, h + TOPBAR_HEIGHT)
            {
                Color = Color.FromArgb(92, 90, 88),
                Stretch = StretchMode.XY
            };

            BackgroundSquare = new(1, TOPBAR_HEIGHT - 1, w, h)
            {
                Color = Color.FromArgb(76, 74, 72),
                Stretch = StretchMode.XY
            };

            DragSquare = new(1, 1, w - CLOSE_WIDTH, TOPBAR_HEIGHT - 2)
            {
                Color = Color.FromArgb(76, 74, 72),
                Stretch = StretchMode.XY
            };

            Container.ClipDescendants = true;
            base.SetControls(Backdrop, BackgroundSquare, DragSquare, DragStrip, CloseButton, InnerContainer);

            name = GetType().Name;

            if (Instances.TryGetValue(name, out GuiWindowDialog? value))
            {
                Windowing.Close(value);
                Instances[name] = this;
            }
            else
                Instances.Add(name, this);
        }

        public override void ConnectEvents()
        {
            CloseButton.LeftClick += (s, e) => CloseDialog();

            DragStrip.LeftDown += (s, e) =>
            {
                startPos = (e.X, e.Y);
                startOff = RectOffset;
                dragging = true;
            };
        }

        protected void CloseDialog()
        {
            Windowing.Close(this);
            Instances.Remove(name);
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

                RectangleF background = Backdrop.Origin;
                RectangleF origin = Origin;
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

        public virtual bool IsHovering() => Rect.Contains(PrevMouse.X, PrevMouse.Y);
    }
}
