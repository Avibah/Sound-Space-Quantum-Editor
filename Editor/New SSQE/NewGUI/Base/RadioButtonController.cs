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

        private readonly Dictionary<GuiButton, string> texts = [];
        private readonly GuiButton[] controls;
        private GuiButton? active;

        private int? initialIndex = null;

        public string Active => active != null ? texts[active] : "";

        public RadioButtonController(int? activeIndex, params GuiButton[] controls)
        {
            this.controls = controls;
            initialIndex = activeIndex;
        }

        public void UpdateSelection(GuiButton control)
        {
            if (!texts.TryGetValue(control, out string? text))
                return;
            if (active != null)
                active.Text = texts[active];

            active = active == control ? null : control;
            initialIndex = active == null ? null : Array.IndexOf(controls, control);
            control.Text = active == control ? $"[{text}]" : text;

            SelectionChanged?.Invoke(active, new(Active));
        }

        public void Initialize()
        {
            foreach (GuiButton control in controls)
            {
                if (texts.TryGetValue(control, out string? value))
                    control.Text = value;

                string text = control.Text;
                texts.TryAdd(control, text);

                control.LeftClick += (s, e) => UpdateSelection(control);
            }

            if (initialIndex != null)
            {
                ClearSelection();
                UpdateSelection(controls[initialIndex.Value]);
            }
        }

        public void ClearSelection()
        {
            active = null;
            SelectionChanged?.Invoke(null, new(Active));
        }
    }
}
