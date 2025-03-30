using OpenTK.Windowing.GraphicsLibraryFramework;

namespace New_SSQE.NewGUI.Base
{
    internal class ClickEventArgs : EventArgs
    {
        public float X;
        public float Y;

        public ClickEventArgs(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    internal class TextEnteredEventArgs : EventArgs
    {
        public string Text;

        public TextEnteredEventArgs(string text)
        {
            Text = text;
        }
    }

    internal class ValueChangedEventArgs : EventArgs
    {
        public float Value;

        public ValueChangedEventArgs(float value)
        {
            Value = value;
        }
    }

    internal abstract class InteractiveControl : TextControl
    {
        public event EventHandler<ClickEventArgs>? LeftClick;
        public event EventHandler<ClickEventArgs>? RightClick;
        public event EventHandler<TextEnteredEventArgs>? TextEntered;
        public event EventHandler<ValueChangedEventArgs>? ValueChanged;

        public bool Hovering = false;
        public bool Dragging = false;
        public bool Focused = false;

        public InteractiveControl(float x, float y, float w, float h, string text = "", int textSize = 0, string font = "main", CenterMode centerMode = CenterMode.XY) : base(x, y, w, h, text, textSize, font, centerMode)
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
                LeftClick?.Invoke(this, new ClickEventArgs(x, y));
                MouseMove(x, y);
            }
            else
                Focused = false;
        }

        public virtual void MouseClickRight(float x, float y)
        {
            if (Hovering)
                RightClick?.Invoke(this, new ClickEventArgs(x, y));
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
        public virtual void KeybindUsed(string keybind) { }

        protected void InvokeTextEntered(TextEnteredEventArgs e) => TextEntered?.Invoke(this, e);
        protected void InvokeValueChanged(ValueChangedEventArgs e) => ValueChanged?.Invoke(this, e);
        protected void InvokeLeftClick(ClickEventArgs e) => LeftClick?.Invoke(this, e);
        protected void InvokeRightClick(ClickEventArgs e) => RightClick?.Invoke(this, e);

        public virtual void DisconnectAll()
        {
            LeftClick = null;
            RightClick = null;
            TextEntered = null;
            ValueChanged = null;
        }
    }
}
