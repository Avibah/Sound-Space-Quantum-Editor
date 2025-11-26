using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;
using New_SSQE.Preferences;

namespace New_SSQE.NewGUI.CompoundControls
{
    internal class GuiButtonDropdown : ControlContainer
    {
        public readonly GuiButton MainButton;
        public readonly GuiButton[] Options;
        public readonly GuiScrollingList OptionList;

        public new ControlStyle Style
        {
            get => MainButton.Style;
            set
            {
                MainButton.Style = value;
                foreach (GuiButton option in Options)
                    option.Style = value;
            }
        }

        public new string Text
        {
            get => MainButton.Text;
            set => MainButton.Text = value;
        }

        public new string Prefix
        {
            get => MainButton.Prefix;
            set => MainButton.Prefix = value;
        }

        public new string Suffix
        {
            get => MainButton.Suffix;
            set => MainButton.Suffix = value;
        }

        public new float TextSize
        {
            get => MainButton.TextSize;
            set
            {
                MainButton.TextSize = value;
                foreach (GuiButton option in Options)
                    option.TextSize = value;
            }
        }

        public new string Font
        {
            get => MainButton.Font;
            set
            {
                MainButton.Font = value;
                foreach (GuiButton option in Options)
                    option.Font = value;
            }
        }

        public new CenterMode CenterMode
        {
            get => MainButton.CenterMode;
            set
            {
                MainButton.CenterMode = value;
                foreach (GuiButton option in Options)
                    option.CenterMode = value;
            }
        }

        private readonly Setting<ListSetting> setting;

        public GuiButtonDropdown(float x, float y, float w, float h, Setting<ListSetting> setting, int? toShow = null) : base(x, y, w, h)
        {
            int numShown = Math.Min(toShow ?? setting.Value.Possible.Length, setting.Value.Possible.Length);

            MainButton = new(0, 0, w, h)
            {
                CornerDetail = 1,
                CornerRadius = 0
            };

            List<GuiButton> buttons = [];

            for (int i = 0; i < setting.Value.Possible.Length; i++)
            {
                string option = setting.Value.Possible[i];
                GuiButton button = new(0, h * i, w, h)
                {
                    Text = option.ToUpper(),
                    CornerDetail = 1,
                    CornerRadius = 0
                };
                buttons.Add(button);
            }

            Options = [.. buttons];
            OptionList = new(0, h, w, h * numShown, Options)
            {
                Visible = false
            };

            SetControls(MainButton, OptionList);
            this.setting = setting;
            RenderOnTop = true;
        }

        public override void Update()
        {
            Text = setting.Value.Current.ToUpper();
            base.Update();
        }

        public override void Reset()
        {
            base.Reset();

            MainButton.LeftClick += (s, e) =>
            {
                OptionList.Visible ^= true;
            };

            for (int i = 0; i < Options.Length; i++)
            {
                int index = i;

                Options[index].LeftClick += (s, e) =>
                {
                    OptionList.Visible = false;
                    setting.Value.Current = setting.Value.Possible[index];
                    shouldUpdate = true;
                };
            }
        }

        public override void MouseDownLeftGlobal(float x, float y)
        {
            base.MouseDownLeftGlobal(x, y);

            if (!MainButton.Hovering && !OptionList.Hovering)
                OptionList.Visible = false;
        }
    }
}
