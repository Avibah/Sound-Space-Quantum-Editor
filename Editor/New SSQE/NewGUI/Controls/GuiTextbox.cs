using New_SSQE.ExternalUtils;
using New_SSQE.NewGUI.Font;
using New_SSQE.Preferences;
using OpenTK.Windowing.GraphicsLibraryFramework;
using New_SSQE.NewGUI.Base;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiTextbox : InteractiveControl
    {
        protected int cursorPos = 0;

        private bool cursorShowing = false;
        private float cursorTime = 0;

        private Setting<string>? setting;

        public Setting<string>? Setting
        {
            get => setting;
            set
            {
                if (value != setting)
                {
                    setting = value;
                    shouldUpdate = true;
                }

                if (value != null)
                    Text = value.Value;
            }
        }

        public GuiTextbox(float x, float y, float w, float h) : base(x, y, w, h)
        {
            Style = ControlStyle.Textbox_Colored;
            PlayLeftClickSound = false;
            PlayRightClickSound = false;
        }

        public override float[] Draw()
        {
            Text = setting?.Value ?? Text;
            TextColor = Style.Secondary;

            float[] fill = GLVerts.Squircle(rect, CornerDetail, CornerRadius, Style.Primary);
            float[] outline = GLVerts.SquircleOutline(rect, 2f, CornerDetail, CornerRadius, Style.Tertiary);
            if (!cursorShowing)
                return [..fill, ..outline];

            string textBefore = "";
            if (cursorPos >= 0)
                textBefore = cursorPos >= Text.Length ? Text : Text[..cursorPos];
            cursorPos = Math.Min(cursorPos, Text.Length);

            float textBeforeWidth = FontRenderer.GetWidth(textBefore, TextSize, Font);
            float textX = textBeforeWidth;
            float textY = 0;
            float textW = FontRenderer.GetWidth(Text, TextSize, Font);
            float textH = FontRenderer.GetHeight(TextSize, Font);

            if (CenterMode.HasFlag(CenterMode.X))
                textX += rect.X + rect.Width / 2 - textW / 2;
            if (CenterMode.HasFlag(CenterMode.Y))
                textY += rect.Y + rect.Height / 2 - textH / 2;

            float[] cursor = GLVerts.Line(textX, textY, textX, textY + textH, 2f, Style.Secondary);

            return [..fill, ..outline, ..cursor];
        }

        public override void PreRender(float mousex, float mousey, float frametime)
        {
            base.PreRender(mousex, mousey, frametime);

            bool prevShowing = cursorShowing;
            cursorShowing = Focused && (int)cursorTime % 2 == 0;

            if (cursorShowing != prevShowing)
                Update();

            cursorTime += frametime * 2;
        }

        public override void KeyDown(Keys key)
        {
            if (!Focused)
                return;
            base.KeyDown(key);

            bool ctrl = MainWindow.Instance.CtrlHeld;

            cursorPos = Math.Clamp(cursorPos, 0, Text.Length);
            cursorTime = 0;

            switch (key)
            {
                case Keys.C when ctrl:
                    if (!string.IsNullOrWhiteSpace(Text))
                        Clipboard.SetText(Text);
                    break;

                case Keys.V when ctrl:
                    string clipboard = Clipboard.GetText();

                    if (!string.IsNullOrWhiteSpace(clipboard))
                    {
                        Text = Text.Insert(cursorPos, clipboard);
                        cursorPos += clipboard.Length;
                    }

                    break;

                case Keys.X when ctrl:
                    if (!string.IsNullOrWhiteSpace(Text))
                        Clipboard.SetText(Text);
                    Text = "";
                    cursorPos = 0;

                    break;

                case Keys.Left:
                    if (ctrl)
                        cursorPos = Math.Max(IndexOf(Text, cursorPos - 1, false), 0);
                    else
                        cursorPos--;

                    break;

                case Keys.Right:
                    if (ctrl)
                        cursorPos = Math.Max(IndexOf(Text, cursorPos + 1, true), 0);
                    else
                        cursorPos++;

                    break;

                case Keys.Backspace:
                    if (ctrl)
                    {
                        int index = Math.Max(IndexOf(Text, cursorPos - 1, false), 0);
                        string word = Text.Substring(index, Math.Min(cursorPos - index, Text.Length - index));

                        Text = Text.Remove(index, word.Length);
                        cursorPos -= word.Length;
                    }
                    else if (Text.Length > 0 && cursorPos > 0)
                    {
                        cursorPos--;
                        Text = Text.Remove(cursorPos, 1);
                    }

                    break;

                case Keys.Delete:
                    if (ctrl)
                    {
                        int index = Math.Max(IndexOf(Text, cursorPos + 1, true), 0);
                        string word = Text.Substring(Math.Clamp(cursorPos, 0, Text.Length - 1), Math.Max(index - cursorPos, 0));

                        Text = Text.Remove(cursorPos, word.Length);
                    }
                    else if (Text.Length > 0 && cursorPos < Text.Length)
                        Text = Text.Remove(Math.Min(cursorPos, Text.Length - 1), 1);

                    break;

                case Keys.Enter:
                case Keys.KeyPadEnter:
                    Focused = false;
                    InvokeTextEntered(new(Text));
                    break;

                case Keys.Escape:
                    Focused = false;
                    break;
            }

            FinishInput();
        }

        public override void TextInput(string str)
        {
            if (!Focused)
                return;
            base.TextInput(str);

            if (MainWindow.Instance.CtrlHeld)
                return;

            Text = Text.Insert(cursorPos, str);
            cursorPos++;

            FinishInput();
        }

        protected virtual void FinishInput()
        {
            cursorPos = Math.Clamp(cursorPos, 0, Text.Length);
            Update();

            if (setting != null)
                setting.Value = Text;
        }

        private int IndexOf(string set, int stop, bool onceAfter)
        {
            int current = -1;

            void Run(bool next)
            {
                if (current + 1 < set.Length)
                {
                    int store = set.IndexOf(' ', current + 1);

                    if (next && store < 0)
                        store = Text.Length;

                    if (store > current && (store < stop || next || (next && stop + 1 == store)))
                    {
                        current = store;
                        Run(onceAfter && (store < stop || stop + 1 == store));
                    }
                }
            }

            Run(onceAfter);

            return current;
        }

        public override void MouseDownLeft(float x, float y)
        {
            cursorTime = 0;
            base.MouseDownLeft(x, y);
            if (Text.Length == 0)
                return;

            // rough estimate of where the cursor clicked relative to the text
            float textWidth = FontRenderer.GetWidth(Text, TextSize, Font);
            float posX = x - rect.X - (rect.Width - textWidth) / 2;
            float letterWidth = textWidth / Text.Length;

            posX = Math.Clamp(posX, 0, textWidth);
            cursorPos = (int)Math.Floor(posX / letterWidth + 0.3f);
            Update();
        }
    }
}
