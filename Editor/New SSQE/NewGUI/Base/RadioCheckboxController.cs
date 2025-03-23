using New_SSQE.NewGUI.Controls;

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
        public EventHandler<RadioCheckboxEventArgs>? SelectionChanged;

        private GuiCheckbox[] controls;
        private GuiCheckbox active;
        public string Active => active.Text;

        public RadioCheckboxController(int activeIndex, params GuiCheckbox[] controls)
        {
            this.controls = controls;
            active = controls[activeIndex];
        }

        public void Initialize()
        {
            foreach (GuiCheckbox control in controls)
            {
                control.Toggle = control == active;

                control.LeftClick += (s, e) =>
                {
                    foreach (GuiCheckbox checkbox in controls)
                    {
                        if (control != checkbox)
                            checkbox.Toggle = false;
                    }

                    active = control;
                    SelectionChanged?.Invoke(control, new(control.Text));
                };
            }
        }
    }
}
