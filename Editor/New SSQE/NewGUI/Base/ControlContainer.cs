using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing;

namespace New_SSQE.NewGUI.Base
{
    internal class ControlContainer : InteractiveControl
    {
        private Control[] controls = [];
        private InteractiveControl[] interactives = [];

        public Control[] Children => [..controls];

        public ControlContainer(float x, float y, float w, float h, params Control[] controls) : base(x, y, w, h)
        {
            SetControls(controls);
            Stretch = StretchMode.XY;
        }

        public ControlContainer(params Control[] controls) : this(0, 0, 1920, 1080, controls) { }

        public void SetControls(params Control[] controls)
        {
            this.controls = [];
            this.interactives = [];

            Resize(1920, 1080);

            List<InteractiveControl> interactives = [];

            foreach (Control control in controls)
            {
                if (control is InteractiveControl interactive)
                    interactives.Add(interactive);
            }

            this.controls = controls;
            this.interactives = [..interactives];

            Resize(MainWindow.Instance.ClientSize.X, MainWindow.Instance.ClientSize.Y);
        }

        public override float[] Draw() => [];

        public override void Update()
        {
            base.Update();

            foreach (Control control in controls)
                control.Update();
        }

        public override void PreRender(float mousex, float mousey, float frametime)
        {
            if (!Visible)
                return;

            base.PreRender(mousex, mousey, frametime);

            foreach (Control control in controls)
            {
                if (control.Visible)
                    control.PreRender(mousex, mousey, frametime);
            }
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            if (!Visible)
                return;

            base.Render(mousex, mousey, frametime);

            foreach (Control control in controls)
            {
                if (control.Visible)
                    control.Render(mousex, mousey, frametime);
            }
        }

        public override void PostRender(float mousex, float mousey, float frametime)
        {
            if (!Visible)
                return;

            base.PostRender(mousex, mousey, frametime);

            foreach (Control control in controls)
            {
                if (control.Visible)
                    control.PostRender(mousex, mousey, frametime);
            }
        }

        public override void Resize(float width, float height)
        {
            base.Resize(width, height);

            float wMult = rect.Width / startRect.Width;
            float hMult = rect.Height / startRect.Height;

            foreach (Control control in controls)
            {
                control.Resize(width, height);

                RectangleF controlRect = control.GetRect();
                RectangleF controlOrigin = control.GetOrigin();

                control.SetRect(rect.X + controlOrigin.X * wMult, rect.Y + controlOrigin.Y * hMult, controlRect.Width, controlRect.Height);
            }
        }

        public override void MouseClickLeft(float x, float y)
        {
            if (!Visible)
                return;

            base.MouseClickLeft(x, y);
            for (int i = interactives.Length - 1; i >= 0; i--)
            {
                InteractiveControl control = interactives[i];

                if (control.Visible)
                    control.MouseClickLeft(x, y);
            }
        }

        public override void MouseClickRight(float x, float y)
        {
            if (!Visible)
                return;

            base.MouseClickRight(x, y);

            for (int i = interactives.Length - 1; i >= 0; i--)
            {
                InteractiveControl control = interactives[i];

                if (control.Visible)
                    control.MouseClickRight(x, y);
            }
        }

        public override void MouseUpLeft(float x, float y)
        {
            if (!Visible)
                return;

            base.MouseUpLeft(x, y);

            foreach (InteractiveControl control in interactives)
            {
                if (control.Visible)
                    control.MouseUpLeft(x, y);
            }
        }

        public override void MouseUpRight(float x, float y)
        {
            if (!Visible)
                return;

            base.MouseUpRight(x, y);

            foreach (InteractiveControl control in interactives)
            {
                if (control.Visible)
                    control.MouseUpRight(x, y);
            }
        }

        public override void MouseMove(float x, float y)
        {
            if (!Visible)
                return;

            base.MouseMove(x, y);

            foreach (InteractiveControl control in interactives)
            {
                if (!control.Visible)
                    continue;

                if (control.Hovering && !control.GetRect().Contains(x, y))
                    control.MouseLeave(x, y);
                else if (!control.Hovering && control.GetRect().Contains(x, y))
                    control.MouseEnter(x, y);

                control.MouseMove(x, y);
            }
        }

        public override void MouseScroll(float x, float y, float delta)
        {
            if (!Visible)
                return;

            base.MouseScroll(x, y, delta);

            foreach (InteractiveControl control in interactives)
            {
                if (control.Visible)
                    control.MouseScroll(x, y, delta);
            }
        }

        public override void KeyDown(Keys key)
        {
            if (!Visible)
                return;

            base.KeyDown(key);

            foreach (InteractiveControl control in interactives)
            {
                if (control.Visible)
                    control.KeyDown(key);
            }
        }

        public override void KeyUp(Keys key)
        {
            if (!Visible)
                return;

            base.KeyUp(key);

            foreach (InteractiveControl control in interactives)
            {
                if (control.Visible)
                    control.KeyUp(key);
            }
        }

        public override void KeybindUsed(string keybind)
        {
            if (!Visible)
                return;

            base.KeybindUsed(keybind);

            foreach (InteractiveControl control in interactives)
            {
                if (control.Visible)
                    control.KeybindUsed(keybind);
            }
        }

        public override void DisconnectAll()
        {
            base.DisconnectAll();

            foreach (InteractiveControl control in interactives)
                control.DisconnectAll();
        }


        public override void Dispose()
        {
            base.Dispose();

            foreach (Control control in controls)
                control.Dispose();
        }
    }
}
