using New_SSQE.NewGUI.Controls;

namespace New_SSQE.NewGUI.Base
{
    internal class RadioButtonEventArgs : EventArgs
    {
        public string Value;

        public RadioButtonEventArgs(string value)
        {
            Value = value;
        }
    }

    internal class RadioButtonController
    {
        public EventHandler<RadioButtonEventArgs>? SelectionChanged;

        private GuiButton? active;
        private Dictionary<GuiButton, string> texts = [];

        public string Active => active?.Text ?? "";

        public RadioButtonController(int? activeIndex, params GuiButton[] controls)
        {
            active = activeIndex != null ? controls[activeIndex.Value] : null;

            foreach (GuiButton control in controls)
            {
                string text = control.Text;
                texts.Add(control, text);

                control.LeftClick += (s, e) =>
                {
                    if (active != null)
                        active.Text = texts[active];

                    active = active == control ? null : control;
                    control.Text = active == control ? $"[{text}]" : text;

                    SelectionChanged?.Invoke(active, new(Active));
                };
            }
        }
    }
}
