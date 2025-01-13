using OpenTK.Windowing.GraphicsLibraryFramework;

namespace New_SSQE.NewGUI
{
    internal enum ClickType
    {
        Left,
        Right
    }

    internal class ClickEventArgs : EventArgs
    {
        public float X;
        public float Y;

        public ClickType ClickType;

        public ClickEventArgs(float x, float y, ClickType clickType)
        {
            X = x;
            Y = y;

            ClickType = clickType;
        }
    }

    internal class TextInputEventArgs : EventArgs
    {
        public string Text;

        public TextInputEventArgs(string text)
        {
            Text = text;
        }
    }

    internal abstract class InteractiveControl : TextControl
    {
        public event EventHandler LeftClick;
        public event EventHandler RightClick;
        public event EventHandler TextInput;

        public bool Hovering = false;
        public bool Dragging = false;
        public bool Focused = false;

        public InteractiveControl(float x, float y, float w, float h, string text = "", int textSize = 0, string font = "main", bool centered = true) : base(x, y, w, h, text, textSize, font, centered)
        {

        }

        public virtual void MouseEnter(float x, float y) => Hovering = true;
        public virtual void MouseLeave(float x, float y) => Hovering = false;

        public virtual void MouseClickLeft(float x, float y)
        {
            if (Hovering)
            {
                Focused = true;
                Dragging = true;
                LeftClick?.Invoke(this, new ClickEventArgs(x, y, ClickType.Left));
            }
            else
                Focused = false;
        }

        public virtual void MouseClickRight(float x, float y)
        {
            if (Hovering)
                RightClick?.Invoke(this, new ClickEventArgs(x, y, ClickType.Right));
        }

        public virtual void MouseUpLeft(float x, float y)
        {
            Dragging = false;
        }

        public virtual void MouseUpRight(float x, float y) { }

        public virtual void MouseMove(float x, float y) { }
        public virtual void MouseScroll(float x, float y, float delta) { }

        public virtual void KeyDown(Keys key) { }
        public virtual void KeyUp(Keys key) { }

        protected void InvokeTextInput(TextInputEventArgs e) => TextInput?.Invoke(this, e);
    }
}
