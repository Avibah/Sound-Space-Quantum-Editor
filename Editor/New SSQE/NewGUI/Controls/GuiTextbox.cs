using New_SSQE.ExternalUtils;
using New_SSQE.NewGUI.Font;
using New_SSQE.NewGUI.Input;
using New_SSQE.Preferences;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using New_SSQE.NewGUI.Base;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiTextbox : InteractiveControl
    {
        protected int cursorPos = 0;

        private bool cursorShowing = false;
        private float cursorTime = 0;

        private readonly Setting<string>? setting;

        public GuiTextbox(float x, float y, float w, float h, Setting<string>? setting = null, string text = "", int textSize = 0, string font = "main", CenterMode centerMode = CenterMode.XY) : base(x, y, w, h, text, textSize, font, centerMode)
        {
            this.setting = setting;
            if (setting != null)
                SetText(setting.Value);

            Style = new(ControlStyles.Textbox_Colored);
            PlayLeftClickSound = false;
            PlayRightClickSound = false;
        }

        public override float[] Draw()
        {
            text = setting?.Value ?? text;
            SetColor(Style.Secondary);

            float[] fill = GLVerts.Rect(rect, Style.Primary);
            float[] outline = GLVerts.Outline(rect, 2, Style.Tertiary);
            if (!cursorShowing)
                return fill.Concat(outline).ToArray();

            string textBefore = "";
            if (cursorPos >= 0)
                textBefore = cursorPos >= text.Length ? text : text[..cursorPos];
            cursorPos = Math.Min(cursorPos, text.Length);

            float textBeforeWidth = FontRenderer.GetWidth(textBefore, TextSize, font);
            float textX = textBeforeWidth;
            float textY = 0;
            float textW = FontRenderer.GetWidth(text, TextSize, font);
            float textH = FontRenderer.GetHeight(TextSize, font);

            if (CenterMode.HasFlag(CenterMode.X))
                textX += rect.X + rect.Width / 2 - textW / 2;
            if (CenterMode.HasFlag(CenterMode.Y))
                textY += rect.Y + rect.Height / 2 - textH / 2;

            float[] cursor = GLVerts.Line(textX, textY, textX, textY + textH, 2, Style.Secondary);

            return fill.Concat(outline).Concat(cursor).ToArray();
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

            if (key == Keys.LeftControl || key == Keys.LeftAlt || key == Keys.LeftShift)
                return;
            if (key == Keys.RightControl || key == Keys.RightAlt || key == Keys.RightShift)
                return;

            bool ctrl = MainWindow.Instance.CtrlHeld;
            bool shift = MainWindow.Instance.ShiftHeld;

            cursorPos = Math.Clamp(cursorPos, 0, text.Length);

            switch (key)
            {
                case Keys.C when ctrl:
                    if (!string.IsNullOrWhiteSpace(text))
                        Clipboard.SetText(text);
                    break;

                case Keys.V when ctrl:
                    string clipboard = Clipboard.GetText();

                    if (!string.IsNullOrWhiteSpace(clipboard))
                    {
                        SetText(text.Insert(cursorPos, clipboard));
                        cursorPos += clipboard.Length;
                    }

                    break;

                case Keys.X when ctrl:
                    if (!string.IsNullOrWhiteSpace(text))
                        Clipboard.SetText(text);
                    SetText("");
                    cursorPos = 0;

                    break;

                case Keys.Left:
                    if (ctrl)
                        cursorPos = Math.Max(IndexOf(text, cursorPos - 1, false) + 1, 0);
                    else
                        cursorPos--;

                    break;

                case Keys.Right:
                    if (ctrl)
                        cursorPos = Math.Max(IndexOf(text, cursorPos + 1, true), 0);
                    else
                        cursorPos++;

                    break;

                case Keys.Backspace:
                    if (ctrl)
                    {
                        int index = Math.Max(IndexOf(text, cursorPos - 1, false), 0);
                        string word = text.Substring(index, Math.Min(cursorPos - index, text.Length - index));

                        SetText(text.Remove(index, word.Length));
                        cursorPos -= word.Length;
                    }
                    else if (text.Length > 0 && cursorPos > 0)
                    {
                        cursorPos--;
                        SetText(text.Remove(cursorPos, 1));
                    }

                    break;

                case Keys.Delete:
                    if (ctrl)
                    {
                        int index = Math.Max(IndexOf(text, cursorPos + 1, true), 0);
                        string word = text.Substring(Math.Clamp(cursorPos, 0, text.Length - 1), Math.Max(index - cursorPos, 0));

                        SetText(text.Remove(cursorPos, word.Length));
                    }
                    else if (text.Length > 0 && cursorPos < text.Length)
                        SetText(text.Remove(Math.Min(cursorPos, text.Length - 1), 1));

                    break;

                case Keys.Enter:
                case Keys.KeyPadEnter:
                    Focused = false;
                    InvokeTextEntered(new(text));
                    break;

                case Keys.Escape:
                    Focused = false;
                    break;

                default:
                    if (ctrl)
                        break;
                    string str = KeyConverter.GetCharFromInput(key, shift).ToString();

                    SetText(text.Insert(cursorPos, str));
                    cursorPos++;

                    break;
            }

            cursorPos = Math.Clamp(cursorPos, 0, text.Length);
            Update();

            if (setting != null)
                setting.Value = text;
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
                        store = text.Length;

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

        public override void MouseClickLeft(float x, float y)
        {
            cursorTime = 0;
            base.MouseClickLeft(x, y);
            if (text.Length == 0)
                return;

            // rough estimate of where the cursor clicked relative to the text
            int textWidth = FontRenderer.GetWidth(text, TextSize, font);
            float posX = x - rect.X - (rect.Width - textWidth) / 2;
            float letterWidth = textWidth / text.Length;

            posX = Math.Clamp(posX, 0, textWidth);
            cursorPos = (int)Math.Floor(posX / letterWidth + 0.3f);
        }
    }
}
