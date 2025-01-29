using New_SSQE.GUI.Input;
using New_SSQE.Preferences;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing;

namespace New_SSQE.NewGUI.Windows
{
    internal abstract class GuiWindow
    {
        /*
         * IMPORTANT NOTES:
         * 
         * GuiWindows should make their base controls STATIC, ESPECIALLY if using one or more View3dControls (such as GuiGrid3d)!
         * View3dControls use OpenGL objects that aren't properly disposable, which can cause huge memory leaks if made more than once.
         * 
         * If necessary, non-static controls can be added to the window after creation, but try to keep this to a minimum and don't use
         * fullscreen View3dControls for this without checking for memory leaks afterwards.
         * 
         * 
         * The default sizes of controls are based on a standard 1920x1080 monitor,
         * so all positions/sizes should assume the window is fullscreen on a 1080p display.
         * Controls will be resized automatically.
         * 
         */

        private readonly ControlContainer container;
        public bool AwaitingButtonCompletion = false;

        public GuiWindow(RectangleF rect, params Control[] controls)
        {
            ConnectButtons();

            container = new(0, 0, 1920, 1080, controls);
            container.Resize(rect.Width, rect.Height);
        }
        public GuiWindow(float x, float y, float w, float h, params Control[] controls) : this(new(x, y, w, h), controls) { }
        public GuiWindow(params Control[] controls) : this(new(0, 0, MainWindow.Instance.ClientSize.X, MainWindow.Instance.ClientSize.Y), controls) { }

        public abstract void ConnectButtons();
        public virtual void Open() { }
        public virtual void Close() { }

        public virtual void SetControls(params Control[] controls)
        {
            container.SetControls(controls);
        }

        private Vector2 prevMouse = -Vector2.One;

        public virtual void Render(float mousex, float mousey, float frametime)
        {
            if ((mousex, mousey) != prevMouse)
                container.MouseMove(mousex, mousey);
            prevMouse = (mousex, mousey);

            container.PreRender(mousex, mousey, frametime);
            container.Render(mousex, mousey, frametime);
            container.PostRender(mousex, mousey, frametime);
        }

        public virtual void Resize(ResizeEventArgs e)
        {
            container.Resize(e.Width, e.Height);
        }

        public virtual void MouseDown(MouseButtonEventArgs e)
        {
            if (AwaitingButtonCompletion)
                return;

            if (e.Button == MouseButton.Left)
                container.MouseClickLeft(prevMouse.X, prevMouse.Y);
            else if (e.Button == MouseButton.Right)
                container.MouseClickRight(prevMouse.X, prevMouse.Y);
        }

        public virtual void MouseUp(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
                container.MouseUpLeft(prevMouse.X, prevMouse.Y);
            else if (e.Button == MouseButton.Right)
                container.MouseUpRight(prevMouse.X, prevMouse.Y);
        }

        public virtual void MouseScroll(float delta)
        {
            container.MouseScroll(prevMouse.X, prevMouse.Y, delta);
        }

        public virtual void KeyDown(Keys key)
        {
            container.KeyDown(key);

            List<string> keybinds = Settings.CompareKeybind(key, KeybindManager.CtrlHeld, KeybindManager.AltHeld, KeybindManager.ShiftHeld);

            foreach (string keybind in keybinds)
                container.KeybindUsed(keybind);
        }

        public virtual void KeyUp(Keys key)
        {
            container.KeyUp(key);
        }

        public virtual void Dispose()
        {
            container.Dispose();
        }
    }
}
