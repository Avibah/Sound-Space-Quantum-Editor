using New_SSQE.Audio;
using New_SSQE.Misc;
using New_SSQE.NewGUI.Controls;
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
        protected ControlContainer Container => container;

        private readonly GuiSquare shadow = new()
        {
            RenderOnTop = true,
            Color = Color.FromArgb(63, 0, 0, 0),
            Visible = false
        };

        private bool connected = false;
        private bool enabled = true;
        
        public bool Enabled
        {
            get => enabled;
            private set
            {
                shadow.Visible = !value;
                enabled = value;
                MainWindow.Instance.ForceRender();
            }
        }

        public GuiWindow(float x, float y, float w, float h, params Control[] controls)
        {
            container = new(x, y, w, h, [.. controls, shadow]);
        }
        public GuiWindow(params Control[] controls) : this(0, 0, MainWindow.Instance.ClientSize.X, MainWindow.Instance.ClientSize.Y, controls) { }

        public abstract void ConnectEvents();

        protected Vector2 PrevMouse { get; private set; } = -Vector2.One;

        public virtual void Open()
        {
            if (!connected)
            {
                container.Reset();
                ConnectEvents();
            }

            connected = true;
        }

        public virtual void Close()
        {
            container.DisconnectAll();
        }

        public virtual void Enable() => Enabled = true;
        public virtual void Disable() => Enabled = false;

        public virtual void SetControls(params Control[] controls)
        {
            container.SetControls([.. controls, shadow]);
        }

        public virtual void Update()
        {
            container.Update();
        }
        
        public virtual void Render(float mousex, float mousey, float frametime)
        {
            container.Hovering = container.Rect.Contains(mousex, mousey);
            if ((mousex, mousey) != PrevMouse)
                container.MouseMove(mousex, mousey);
            PrevMouse = (mousex, mousey);

            container.PreRender(mousex, mousey, frametime);
            container.Render(mousex, mousey, frametime);
            container.PostRender(mousex, mousey, frametime);
        }

        public virtual void Resize(ResizeEventArgs e)
        {
            container.Resize(e.Width, e.Height);
        }

        public virtual void MouseDown(float x, float y, MouseButtonEventArgs e)
        {
            if (!enabled)
                return;

            PrevMouse = (x, y);

            if (e.Button == MouseButton.Left)
                container.MouseDownLeftGlobal(PrevMouse.X, PrevMouse.Y);
            else if (e.Button == MouseButton.Right)
                container.MouseDownRightGlobal(PrevMouse.X, PrevMouse.Y);
        }

        public virtual void MouseUp(float x, float y, MouseButtonEventArgs e)
        {
            PrevMouse = (x, y);

            if (e.Button == MouseButton.Left)
                container.MouseUpLeftGlobal(PrevMouse.X, PrevMouse.Y);
            else if (e.Button == MouseButton.Right)
                container.MouseUpRightGlobal(PrevMouse.X, PrevMouse.Y);
        }

        public virtual void MouseScroll(float x, float y, float delta)
        {
            if (!enabled)
                return;

            PrevMouse = (x, y);

            container.MouseScrollGlobal(PrevMouse.X, PrevMouse.Y, delta);
        }

        public virtual void KeyDown(Keys key)
        {
            if (!enabled)
                return;

            container.KeyDown(key);
            KeybindManager.Run(key);
        }

        public virtual void KeyUp(Keys key)
        {
            container.KeyUp(key);
        }

        public virtual void KeybindUsed(string keybind)
        {
            if (!enabled)
                return;

            container.KeybindUsed(keybind);
        }

        public virtual void TextInput(string str)
        {
            if (!enabled)
                return;

            container.TextInput(str);
        }

        public virtual void FileDrop(string file)
        {
            if (!enabled)
                return;

            bool loaded = true;
            Map? prev = Mapping.Current;

            if (SoundEngine.SupportedExtensions.Contains(Path.GetExtension(file)))
            {
                string id = FormatUtils.FixID(Path.GetFileNameWithoutExtension(file));
                if (file != Assets.CachedAt($"{id}.asset"))
                    File.Copy(file, Assets.CachedAt($"{id}.asset"), true);

                loaded = Mapping.Load(id);
            }
            else if (Path.GetExtension(file) != ".ini")
                loaded = Mapping.Load(file);

            if (!loaded && prev != null)
            {
                Mapping.Current = prev;
                Mapping.Open();
            }
        }

        public virtual bool TextboxFocused() => enabled && container.TextboxFocused();
        public virtual bool HoveringInteractive(InteractiveControl? exclude = null) => enabled && container.HoveringInteractive(exclude);
        public virtual InteractiveControl? GetHoveringInteractive(InteractiveControl? exclude = null) => enabled ? container.GetHoveringInteractive(exclude) : null;
        public virtual bool ShouldConsumeScroll() => enabled && container.ShouldConsumeScroll();

        protected virtual void SetOffset(Vector2 offset)
        {
            container.RectOffset = offset;
            container.Resize(MainWindow.Instance.ClientSize.X, MainWindow.Instance.ClientSize.Y);
        }

        protected virtual RectangleF Origin => container.Origin;
        protected virtual Vector2 RectOffset => container.RectOffset;
        protected virtual RectangleF Rect => container.Rect;
    }
}
