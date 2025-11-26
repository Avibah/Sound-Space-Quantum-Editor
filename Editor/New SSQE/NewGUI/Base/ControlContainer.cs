using New_SSQE.NewGUI.Controls;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing;

namespace New_SSQE.NewGUI.Base
{
    internal class ControlContainer : InteractiveControl
    {
        private Control[] controls = [];
        private InteractiveControl[] interactives = [];

        public Control[] Children => [..controls];

        public bool ClipDescendants = false;

        public ControlContainer(float x, float y, float w, float h, params Control[] controls) : base(x, y, w, h)
        {
            SetControls(controls);
            Stretch = StretchMode.XY;

            PlayLeftClickSound = false;
            PlayRightClickSound = false;
        }

        public ControlContainer(params Control[] controls) : this(0, 0, 1920, 1080, controls) { }

        public void SetControls(params Control[] controls)
        {
            List<InteractiveControl> deferredInteractives = [];
            List<InteractiveControl> interactives = [];

            this.controls = [];
            this.interactives = [];

            Resize(1920, 1080);

            foreach (Control control in controls)
            {
                if (control is InteractiveControl interactive)
                {
                    if (control.RenderOnTop)
                        deferredInteractives.Add(interactive);
                    else
                        interactives.Add(interactive);
                }
            }

            this.controls = controls;
            this.interactives = [.. interactives, .. deferredInteractives];

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
            if (ClipDescendants)
                GLState.EnableScissor(rect);

            base.PreRender(mousex, mousey, frametime);

            foreach (Control control in controls)
            {
                if (control.Visible && !control.RenderOnTop)
                    control.PreRender(mousex, mousey, frametime);
            }

            if (ClipDescendants)
                GLState.DisableScissor();
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            if (!Visible)
                return;
            if (ClipDescendants)
                GLState.EnableScissor(rect);

            base.Render(mousex, mousey, frametime);

            foreach (Control control in controls)
            {
                if (control.Visible && !control.RenderOnTop)
                    control.Render(mousex, mousey, frametime);
            }

            if (ClipDescendants)
                GLState.DisableScissor();
        }

        public override void PostRender(float mousex, float mousey, float frametime)
        {
            if (!Visible)
                return;
            bool scissor = ClipDescendants;
            if (scissor)
                GLState.EnableScissor(rect);

            base.PostRender(mousex, mousey, frametime);
            List<Control> deferred = [];

            foreach (Control control in controls)
            {
                if (control.Visible)
                {
                    if (control.RenderOnTop)
                        deferred.Add(control);
                    else
                        control.PostRender(mousex, mousey, frametime);
                }
            }

            foreach (Control control in deferred)
            {
                control.PreRender(mousex, mousey, frametime);
                control.Render(mousex, mousey, frametime);
                control.PostRender(mousex, mousey, frametime);
            }

            if (scissor)
                GLState.DisableScissor();
        }

        public override void Resize(float screenWidth, float screenHeight)
        {
            base.Resize(screenWidth, screenHeight);

            foreach (Control control in controls)
            {
                control.RectOffset = (StartRect.X, StartRect.Y) + RectOffset;
                control.Resize(screenWidth, screenHeight);
            }
        }

        public override void MouseDownLeftGlobal(float x, float y)
        {
            if (!Visible)
                return;

            base.MouseDownLeftGlobal(x, y);

            for (int i = interactives.Length - 1; i >= 0; i--)
            {
                InteractiveControl control = interactives[i];

                if (control.Visible)
                    control.MouseDownLeftGlobal(x, y);
            }
        }

        public override void MouseDownRightGlobal(float x, float y)
        {
            if (!Visible)
                return;

            base.MouseDownRightGlobal(x, y);

            for (int i = interactives.Length - 1; i >= 0; i--)
            {
                InteractiveControl control = interactives[i];

                if (control.Visible)
                    control.MouseDownRightGlobal(x, y);
            }
        }

        public override void MouseUpLeftGlobal(float x, float y)
        {
            if (!Visible)
                return;

            base.MouseUpLeftGlobal(x, y);

            foreach (InteractiveControl control in interactives)
            {
                if (control.Visible)
                    control.MouseUpLeftGlobal(x, y);
            }
        }

        public override void MouseUpRightGlobal(float x, float y)
        {
            if (!Visible)
                return;

            base.MouseUpRightGlobal(x, y);

            foreach (InteractiveControl control in interactives)
            {
                if (control.Visible)
                    control.MouseUpRightGlobal(x, y);
            }
        }

        public override void MouseMove(float x, float y)
        {
            if (!Visible)
                return;

            base.MouseMove(x, y);
            bool canHover = !ClipDescendants || Hovering;

            foreach (InteractiveControl control in interactives)
            {
                if (control.Hovering && !control.GetRect().Contains(x, y))
                    control.MouseLeave(x, y);
                else if (control.Visible && canHover && !control.Hovering && control.GetRect().Contains(x, y))
                    control.MouseEnter(x, y);

                if (control.Visible)
                    control.MouseMove(x, y);
            }
        }

        public override void MouseScrollGlobal(float x, float y, float delta)
        {
            if (!Visible)
                return;

            base.MouseScrollGlobal(x, y, delta);

            foreach (InteractiveControl control in interactives)
            {
                if (control.Visible)
                    control.MouseScrollGlobal(x, y, delta);
            }
        }

        public override void KeyDown(Keys key)
        {
            if (!Visible)
                return;

            base.KeyDown(key);

            for (int i = interactives.Length - 1; i >= 0; i--)
            {
                InteractiveControl control = interactives[i];

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
            base.KeybindUsed(keybind);

            foreach (InteractiveControl control in interactives)
                control.KeybindUsed(keybind);
        }

        public override void TextInput(string str)
        {
            if (!Visible)
                return;

            base.TextInput(str);

            foreach (InteractiveControl control in interactives)
            {
                if (control.Visible)
                    control.TextInput(str);
            }
        }

        public bool TextboxFocused()
        {
            foreach (InteractiveControl control in interactives)
            {
                if (!control.Visible)
                    continue;

                if (control is ControlContainer container)
                {
                    if (container.TextboxFocused())
                        return true;
                }
                else if (control is GuiTextbox textbox && textbox.Focused)
                    return true;
            }

            return false;
        }

        public bool HoveringInteractive(InteractiveControl? exclude = null)
        {
            foreach (InteractiveControl control in interactives)
            {
                if (!control.Visible)
                    continue;

                if (control is ControlContainer container)
                {
                    if (container.HoveringInteractive(exclude))
                        return true;
                }
                else if (control.Hovering && control != exclude)
                    return true;
            }

            return false;
        }

        public InteractiveControl? GetHoveringInteractive(InteractiveControl? exclude = null)
        {
            foreach (InteractiveControl control in interactives)
            {
                if (!control.Visible)
                    continue;

                if (control is ControlContainer container)
                {
                    InteractiveControl? hovering = container.GetHoveringInteractive(exclude);

                    if (hovering != null)
                        return hovering;
                }
                else if (control.Hovering && control != exclude)
                    return control;
            }

            return null;
        }

        public override void DisconnectAll()
        {
            base.DisconnectAll();

            foreach (InteractiveControl control in interactives)
                control.DisconnectAll();
        }

        public override void Reset()
        {
            base.Reset();

            foreach (Control control in controls)
                control.Reset();
        }

        public override Vector4 GetExtents()
        {
            if (controls.Length == 0)
                return Vector4.Zero;

            Vector4 extents = Vector4.NegativeInfinity;

            foreach (Control control in controls)
            {
                RectangleF origin = control.GetOrigin();
                Vector4 cExtents = control.GetExtents() + (-origin.X, -origin.Y, origin.X, origin.Y);

                extents = Vector4.ComponentMax(extents, cExtents);
            }

            return extents;
        }

        public override bool ShouldConsumeScroll()
        {
            if (base.ShouldConsumeScroll())
                return true;

            foreach (InteractiveControl control in interactives)
            {
                if (control.ShouldConsumeScroll())
                    return true;
            }

            return false;
        }
    }
}
