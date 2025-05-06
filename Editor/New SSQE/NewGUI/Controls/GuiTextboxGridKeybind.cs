﻿using New_SSQE.NewGUI.Base;
using New_SSQE.Preferences;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiTextboxGridKeybind : GuiTextbox
    {
        private readonly int gridKey;

        public GuiTextboxGridKeybind(float x, float y, float w, float h, int gridKey, string text = "", int textSize = 0, string font = "main", CenterMode centerMode = CenterMode.XY) : base(x, y, w, h, null, text, textSize, font, centerMode)
        {
            this.gridKey = gridKey;
        }

        public override void KeyDown(Keys key)
        {
            if (!Focused)
                return;

            if (key == Keys.LeftControl || key == Keys.RightControl)
                return;
            if (key == Keys.LeftAlt || key == Keys.RightAlt)
                return;
            if (key == Keys.LeftShift || key == Keys.RightShift)
                return;

            if (key == Keys.Backspace)
                key = Keys.Delete;

            Settings.gridKeys.Value[gridKey] = key;

            SetText(key.ToString().ToUpper());
            cursorPos = text.Length;
        }

        public override void TextInput(string str) { }

        public override float[] Draw()
        {
            text = Settings.gridKeys.Value[gridKey].ToString().ToUpper();

            return base.Draw();
        }
    }
}
