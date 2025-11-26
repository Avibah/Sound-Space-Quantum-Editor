using New_SSQE.Audio;
using New_SSQE.Preferences;
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

    internal class ValueChangedEventArgs<T> : EventArgs
    {
        public T Value;

        public ValueChangedEventArgs(T value)
        {
            Value = value;
        }
    }

    internal abstract class InteractiveControl : TextControl
    {
        public event EventHandler<ClickEventArgs>? LeftClick;
        public event EventHandler<ClickEventArgs>? RightClick;
        public event EventHandler<TextEnteredEventArgs>? TextEntered;

        public bool Hovering = false;
        public bool Dragging = false;
        public bool Focused = false;
        public bool RightDragging = false;

        public bool PlayLeftClickSound = true;
        public bool PlayRightClickSound = true;
        public bool ConsumeScroll = false;

        public InteractiveControl(float x, float y, float w, float h) : base(x, y, w, h) { }

        public override void Reset()
        {
            base.Reset();
            if (Hovering)
                MouseLeave(-1, -1);

            Dragging = false;
            Focused = false;
            RightDragging = false;
        }

        public virtual void MouseEnter(float x, float y) => Hovering = true;
        public virtual void MouseLeave(float x, float y) => Hovering = false;

        public virtual void MouseDownLeft(float x, float y)
        {
            if (PlayLeftClickSound)
                SoundPlayer.Play(Settings.clickSound.Value);

            Focused = true;
            Dragging = true;
            MouseMove(x, y);

            if (this is not ControlContainer)
                Windowing.ButtonClicked = true;
        }

        public virtual void MouseDownLeftGlobal(float x, float y)
        {
            if (Hovering && !Windowing.ButtonClicked)
                MouseDownLeft(x, y);
            else
                Focused = false;
        }

        public virtual void MouseDownRight(float x, float y)
        {
            if (PlayRightClickSound)
                SoundPlayer.Play(Settings.clickSound.Value);

            RightDragging = true;
            Windowing.ButtonClicked = true;
        }

        public virtual void MouseDownRightGlobal(float x, float y)
        {
            if (Hovering)
                MouseDownRight(x, y);
        }

        public virtual void MouseUpLeft(float x, float y)
        {
            Dragging = false;
            if (Hovering)
                LeftClick?.Invoke(this, new ClickEventArgs(x, y));
        }

        public virtual void MouseUpLeftGlobal(float x, float y)
        {
            if (Dragging)
                MouseUpLeft(x, y);
        }

        public virtual void MouseUpRight(float x, float y)
        {
            RightDragging = false;
            if (Hovering)
                RightClick?.Invoke(this, new ClickEventArgs(x, y));
        }

        public virtual void MouseUpRightGlobal(float x, float y)
        {
            if (RightDragging)
                MouseUpRight(x, y);
        }

        public virtual void MouseMove(float x, float y) { }
        public virtual void MouseScroll(float x, float y, float delta) { }
        public virtual void MouseScrollGlobal(float x, float y, float delta)
        {
            if (Hovering)
                MouseScroll(x, y, delta);
        }

        public virtual void KeyDown(Keys key) { }
        public virtual void KeyUp(Keys key) { }
        public virtual void KeybindUsed(string keybind) { }
        public virtual void TextInput(string str) { }

        protected void InvokeTextEntered(TextEnteredEventArgs e) => TextEntered?.Invoke(this, e);
        protected void InvokeLeftClick(ClickEventArgs e) => LeftClick?.Invoke(this, e);
        protected void InvokeRightClick(ClickEventArgs e) => RightClick?.Invoke(this, e);

        public virtual void DisconnectAll()
        {
            LeftClick = null;
            RightClick = null;
            TextEntered = null;
        }

        public virtual bool ShouldConsumeScroll() => ConsumeScroll && Hovering;
    }
}
