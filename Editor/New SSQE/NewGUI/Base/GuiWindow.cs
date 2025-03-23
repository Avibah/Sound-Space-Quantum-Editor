using New_SSQE.Audio;
using New_SSQE.FileParsing;
using New_SSQE.Misc.Static;
using New_SSQE.NewGUI.Controls;
using New_SSQE.NewGUI.Input;
using New_SSQE.NewMaps;
using New_SSQE.Preferences;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

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
        public static bool LockClick = false;

        public GuiWindow(float width, float height, params Control[] controls)
        {
            ConnectEvents();

            container = new(controls);
            container.Resize(width, height);
        }
        public GuiWindow(params Control[] controls) : this(MainWindow.Instance.ClientSize.X, MainWindow.Instance.ClientSize.Y, controls) { }

        public abstract void ConnectEvents();

        public virtual void Close()
        {
            container.DisconnectAll();
        }

        public virtual void SetControls(params Control[] controls)
        {
            container.SetControls(controls);
        }

        private Vector2 prevMouse = -Vector2.One;

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

            List<string> keybinds = Settings.CompareKeybind(key, KeybindManager.CtrlHeld, KeybindManager.AltHeld, KeybindManager.ShiftHeld);
            KeybindManager.Run(key);

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

        public virtual void FileDrop(FileDropEventArgs e)
        {
            for (int i = 0; i < e.FileNames.Length; i++)
            {
                string file = e.FileNames[i];

                if (File.Exists(file))
                {
                    bool loaded = true;
                    Map? prev = CurrentMap.Map;

                    if (MusicPlayer.SupportedExtensions.Contains(Path.GetExtension(file)))
                    {
                        string id = Exporting.FixID(Path.GetFileNameWithoutExtension(file));
                        if (file != Path.Combine(Assets.CACHED, $"{id}.asset"))
                            File.Copy(file, Path.Combine(Assets.CACHED, $"{id}.asset"), true);

                        loaded = MapManager.Load(id);
                    }
                    else if (Path.GetExtension(file) != ".ini")
                        loaded = MapManager.Load(file);

                    if (!loaded && prev != null)
                        prev.Open();
                }
            }
        }

        public virtual bool TextboxFocused()
        {
            foreach (Control control in container.Children)
            {
                if (control is GuiTextbox textbox && textbox.Focused)
                    return true;
            }

            return false;
        }
    }
}
