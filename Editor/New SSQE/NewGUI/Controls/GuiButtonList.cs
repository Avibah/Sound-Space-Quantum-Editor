using New_SSQE.NewGUI.Base;
using New_SSQE.Preferences;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiButtonList : GuiButton
    {
        private readonly Setting<ListSetting> setting;
        private readonly string prefix;

        public GuiButtonList(float x, float y, float w, float h, Setting<ListSetting> setting, string prefix = "", int textSize = 0, string font = "main", CenterMode centerMode = CenterMode.XY) : base(x, y, w, h, "", textSize, font, centerMode)
        {
            this.setting = setting;
            this.prefix = prefix;

            PlayRightClickSound = true;

            if (Array.IndexOf(setting.Value.Possible, setting.Value.Current) < 0)
                setting.Value.Current = setting.Value.Possible[0];
            SetText(prefix + setting.Value.Current.ToString().ToUpper());
        }

        private void UpdateSetting(bool right)
        {
            ListSetting list = setting.Value;
            string[] possible = list.Possible;

            int index = Array.IndexOf(possible, list.Current);
            index = index >= 0 ? index : possible.Length - 1;

            int newIndex = (possible.Length + index + (right ? -1 : 1)) % possible.Length;
            list.Current = possible[newIndex];

            SetText(prefix + list.Current.ToString().ToUpper());
            Update();
        }

        public override void MouseClickLeft(float x, float y)
        {
            if (Hovering)
                UpdateSetting(true);

            base.MouseClickLeft(x, y);
        }

        public override void MouseClickRight(float x, float y)
        {
            if (Hovering)
                UpdateSetting(false);

            base.MouseClickRight(x, y);
        }
    }
}
