using New_SSQE.NewGUI.Controls;
using New_SSQE.Preferences;

namespace New_SSQE.NewGUI.Base
{
    internal class RadioCheckboxEventArgs : EventArgs
    {
        public string Value;

        public RadioCheckboxEventArgs(string value)
        {
            Value = value;
        }
    }

    internal class RadioCheckboxController
    {
        public event EventHandler<RadioCheckboxEventArgs>? SelectionChanged;

        private GuiCheckbox[] controls;
        private GuiCheckbox active;
        public string Active => active.Text;

        private Setting<string>? setting;

        public RadioCheckboxController(int activeIndex, params GuiCheckbox[] controls)
        {
            this.controls = controls;
            active = controls[activeIndex];
        }

        public RadioCheckboxController(Setting<string> setting, params GuiCheckbox[] controls)
        {
            this.setting = setting;
            this.controls = controls;
            active = controls.FirstOrDefault(n => n.Text == setting.Value) ?? controls[0];
        }

        public void Initialize()
        {
            foreach (GuiCheckbox control in controls)
            {
                control.Toggle = control == active;

                control.LeftClick += (s, e) =>
                {
                    active = control;
                    SelectionChanged?.Invoke(control, new(control.Text));

                    foreach (GuiCheckbox checkbox in controls)
                        checkbox.Toggle = checkbox == active;

                    if (setting != null)
                        setting.Value = active.Text;
                };
            }

            SelectionChanged?.Invoke(active, new(active.Text));
        }

        public void Disconnect()
        {
            SelectionChanged = null;
        }
    }
}
