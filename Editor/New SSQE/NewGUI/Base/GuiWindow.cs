using New_SSQE.Audio;
using New_SSQE.Misc.Static;
using New_SSQE.NewGUI.Input;
using New_SSQE.NewMaps;
using New_SSQE.NewMaps.Parsing;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing;

namespace New_SSQE.NewGUI.Base
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
         * The default sizes of controls are based on a traditional 1920x1080 monitor,
         * so all positions/sizes should assume the window is fullscreen on a 1080p display.
         * Controls will be resized automatically.
         * 
         */

        private readonly ControlContainer container;

        private bool connected = false;
        public static bool LockClick = false;

        public GuiWindow(float x, float y, float w, float h, params Control[] controls)
        {
            container = new(x, y, w, h, controls);
            container.Reset();
        }
        public GuiWindow(params Control[] controls) : this(0, 0, MainWindow.Instance.ClientSize.X, MainWindow.Instance.ClientSize.Y, controls) { }

        public abstract void ConnectEvents();

        public virtual void Open()
        {
            if (!connected)
                ConnectEvents();
            connected = true;
        }

        public virtual void Close()
        {
            container.DisconnectAll();
        }

        public virtual void SetControls(params Control[] controls)
        {
            container.SetControls(controls);
        }

        protected Vector2 prevMouse { get; private set; } = -Vector2.One;

        public virtual void Update()
        {
            container.Update();
        }
        
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
            if (LockClick)
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
            KeybindManager.Run(key);
        }

        public virtual void KeyUp(Keys key)
        {
            container.KeyUp(key);
        }

        public virtual void KeybindUsed(string keybind)
        {
            container.KeybindUsed(keybind);
        }

        public virtual void TextInput(string str)
        {
            container.TextInput(str);
        }

        public virtual void FileDrop(string file)
        {
            bool loaded = true;
            Map? prev = Mapping.Current;

            if (MusicPlayer.SupportedExtensions.Contains(Path.GetExtension(file)))
            {
                string id = FormatUtils.FixID(Path.GetFileNameWithoutExtension(file));
                if (file != Path.Combine(Assets.CACHED, $"{id}.asset"))
                    File.Copy(file, Path.Combine(Assets.CACHED, $"{id}.asset"), true);

                loaded = Mapping.Load(id);
            }
            else if (Path.GetExtension(file) != ".ini")
                loaded = Mapping.Load(file);

            if (!loaded && prev != null)
                Mapping.Open(prev);
        }

        public virtual bool TextboxFocused() => container.TextboxFocused();
        public virtual bool HoveringInteractive(InteractiveControl? exclude = null) => container.HoveringInteractive(exclude);
        public virtual InteractiveControl? GetHoveringInteractive(InteractiveControl? exclude = null) => container.GetHoveringInteractive(exclude);

        protected virtual void SetOffset(Vector2 offset)
        {
            container.RectOffset = offset;
            container.Resize(MainWindow.Instance.ClientSize.X, MainWindow.Instance.ClientSize.Y);
        }

        protected virtual RectangleF GetOrigin() => container.GetOrigin();
        protected virtual Vector2 GetOffset() => container.RectOffset;
    }
}
