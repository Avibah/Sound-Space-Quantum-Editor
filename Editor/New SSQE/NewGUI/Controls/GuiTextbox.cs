using New_SSQE.NewGUI.Font;
using New_SSQE.Preferences;
using OpenTK.Windowing.GraphicsLibraryFramework;
using New_SSQE.NewGUI.Base;
using New_SSQE.Services;
using OpenTK.Mathematics;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiTextbox : InteractiveControl
    {
        protected int cursorPos = 0;
        protected int topLine = 0;

        protected Vector2i RenderedCursor
        {
            get
            {
                int actualIndex = 0;
                int line = 0;
                int subLine = 0;
                string rendered = WrappedText;

                for (int i = 0; i < cursorPos; i++)
                {
                    if (i >= Text.Length)
                    {
                        subLine++;
                        continue;
                    }

                    while (Text[i] != rendered[actualIndex])
                    {
                        actualIndex++;
                        line++;
                        subLine = 0;
                    }

                    if (Text[i] == '\n')
                    {
                        line++;
                        subLine = 0;
                    }
                    else
                        subLine++;

                    actualIndex++;
                }

                return (subLine, line);
            }
        }

        protected int GetCursorPos(Vector2i relativeCoords)
        {
            int line = 0;
            int renderPos = 0;
            int actualPos = 0;

            string rendered = WrappedText;

            while (line < relativeCoords.Y)
            {
                if (Text[actualPos] == rendered[renderPos])
                    actualPos++;
                if (rendered[renderPos] == '\n')
                    line++;

                renderPos++;
            }

            int inLine = relativeCoords.Y < WrappedText.Length ? Math.Min(WrappedText[relativeCoords.Y], relativeCoords.X) : 0;

            return actualPos + inLine;
        }

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

        public bool IsMultiLine = false;
        public bool IsReadOnly = false;

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

            float[] fill = GLVerts.Squircle(rect, cornerDetail, cornerRadius, Style.Primary);
            float[] outline = GLVerts.SquircleOutline(rect, lineThickness, cornerDetail, cornerRadius, Style.Tertiary);
            if (!cursorShowing)
                return [.. fill, .. outline];

            Vector2i cursor = RenderedCursor;
            string renderedText = WrappedText;
            string[] lines = renderedText.Split('\n');
            string textBefore = "";

            if (cursor.Y <= lines.Length)
                textBefore = lines[cursor.Y][..cursor.X];

            cursorPos = Math.Clamp(cursorPos, 0, Text.Length);

            float textBeforeWidth = FontRenderer.GetWidth(textBefore, TextSize, Font);
            float textX = textBeforeWidth + rect.X;
            float textY = rect.Y;
            float textW = FontRenderer.GetWidth(renderedText, TextSize, Font);
            float textH = FontRenderer.GetHeight(TextSize, Font);

            if (CenterMode.HasFlag(CenterMode.X))
                textX += rect.Width / 2 - textW / 2;
            else
                textX += TextPadding.X;

            if (CenterMode.HasFlag(CenterMode.Y))
                textY += rect.Height / 2 - textH * lines.Length / 2;
            else
                textY += TextPadding.Y;

            textY += textH * (cursor.Y - CurLine);

            float[] result = GLVerts.Line(textX, textY, textX, textY + textH, 2f, Style.Secondary);

            return [.. fill, .. outline, .. result];
        }

        public override void PreRender(float mousex, float mousey, float frametime)
        {
            base.PreRender(mousex, mousey, frametime);

            bool prevShowing = cursorShowing;
            cursorShowing = Focused && (int)cursorTime % 2 == 0;

            if (cursorShowing != prevShowing)
                shouldUpdate = true;

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

                case Keys.V when ctrl && !IsReadOnly:
                    string clipboard = Clipboard.GetText();

                    if (!string.IsNullOrWhiteSpace(clipboard))
                    {
                        Text = Text.Insert(cursorPos, clipboard);
                        cursorPos += clipboard.Length;
                    }

                    break;

                case Keys.X when ctrl && !IsReadOnly:
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

                    Vector2i renderedL = RenderedCursor;
                    if (renderedL.Y < CurLine)
                        CurLine = renderedL.Y;

                    break;

                case Keys.Right:
                    if (ctrl)
                        cursorPos = Math.Max(IndexOf(Text, cursorPos + 1, true), 0);
                    else
                        cursorPos++;

                    Vector2i renderedR = RenderedCursor;
                    if (renderedR.Y >= CurLine + MaxLines)
                        CurLine = renderedR.Y - MaxLines;

                    break;

                case Keys.Up:
                    Vector2i cursorU = RenderedCursor;
                    
                    if (cursorU.Y == 0)
                        cursorPos = 0;
                    else
                    {
                        Vector2i newPos = (cursorU.X, cursorU.Y - 1);
                        if (newPos.Y < CurLine)
                            CurLine = newPos.Y;
                        cursorPos = GetCursorPos(newPos);
                    }

                    break;

                case Keys.Down:
                    Vector2i cursorD = RenderedCursor;
                    string[] rendered = WrappedText.Split('\n');

                    if (cursorD.Y == rendered.Length - 1)
                        cursorPos = rendered[cursorD.Y].Length;
                    else
                    {
                        Vector2i newPos = (cursorD.X, cursorD.Y + 1);
                        if (newPos.Y >= CurLine + MaxLines)
                            CurLine = newPos.Y - MaxLines;
                        cursorPos = GetCursorPos(newPos);
                    }

                    break;

                case Keys.Backspace when !IsReadOnly:
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

                case Keys.Delete when !IsReadOnly:
                    if (ctrl)
                    {
                        int index = Math.Max(IndexOf(Text, cursorPos + 1, true), 0);
                        string word = Text.Substring(Math.Clamp(cursorPos, 0, Text.Length - 1), Math.Max(index - cursorPos, 0));

                        Text = Text.Remove(cursorPos, word.Length);
                    }
                    else if (Text.Length > 0 && cursorPos < Text.Length)
                        Text = Text.Remove(Math.Min(cursorPos, Text.Length - 1), 1);

                    break;

                case Keys.Enter when !IsReadOnly:
                case Keys.KeyPadEnter when !IsReadOnly:
                    if (IsMultiLine)
                        TextInput("\n");
                    else
                    {
                        Focused = false;
                        InvokeTextEntered(new(Text));
                    }

                    break;

                case Keys.Escape:
                    Focused = false;
                    break;
            }

            FinishInput();
        }

        public override void TextInput(string str)
        {
            if (!Focused || IsReadOnly)
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
                    int store = Math.Max(set.IndexOf(' ', current + 1), set.IndexOf('\n', current + 1));

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
            shouldUpdate = true;
        }

        public override void MouseScroll(float x, float y, float delta)
        {
            base.MouseScroll(x, y, delta);

            if (delta < 0)
            {
                if (CurLine > 0)
                    CurLine--;
            }
            else
            {
                if (CurLine + MaxLines < WrappedText.Split('\n').Length - 1)
                    CurLine++;
            }
        }
    }
}
