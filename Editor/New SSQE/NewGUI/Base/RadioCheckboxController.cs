using New_SSQE.NewGUI.Controls;
using New_SSQE.Preferences;

namespace New_SSQE.NewGUI.Base
{
    internal class RadioCheckboxEventArgs : EventArgs
    {
        public GuiCheckbox Active;

        public RadioCheckboxEventArgs(GuiCheckbox active)
        {
            Active = active;
        }
    }

    internal class RadioCheckboxController
    {
        public event EventHandler<RadioCheckboxEventArgs>? SelectionChanged;

        private readonly GuiCheckbox[] controls;
        private GuiCheckbox active;
        public GuiCheckbox Active => active;

        private readonly Setting<string>? setting;

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
                    SelectionChanged?.Invoke(this, new(control));

                    foreach (GuiCheckbox checkbox in controls)
                        checkbox.Toggle = checkbox == active;

                    if (setting != null)
                        setting.Value = active.Text;
                };
            }

            SelectionChanged?.Invoke(this, new(active));
        }

        public void Disconnect()
        {
            SelectionChanged = null;
        }
    }
}
