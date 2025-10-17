using New_SSQE.Preferences;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiButtonList : GuiButton
    {
        private readonly Setting<ListSetting> setting;
        private string prefix = "";

        public string Prefix
        {
            get => prefix;
            set
            {
                if (value != prefix)
                {
                    prefix = value;
                    shouldUpdate = true;
                }
            }
        }

        public GuiButtonList(float x, float y, float w, float h, Setting<ListSetting> setting) : base(x, y, w, h)
        {
            this.setting = setting;
            PlayRightClickSound = true;

            RefreshSetting();
        }

        private void UpdateSetting(bool right)
        {
            ListSetting list = setting.Value;
            string[] possible = list.Possible;

            int index = Array.IndexOf(possible, list.Current);
            index = index >= 0 ? index : possible.Length - 1;

            int newIndex = (possible.Length + index + (right ? 1 : -1)) % possible.Length;
            list.Current = possible[newIndex];

            RefreshSetting();
            Update();
        }

        public void RefreshSetting()
        {
            ListSetting list = setting.Value;

            if (Array.IndexOf(list.Possible, list.Current) < 0)
                list.Current = list.Possible[0];
            Text = prefix + list.Current.ToString().ToUpper();
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
