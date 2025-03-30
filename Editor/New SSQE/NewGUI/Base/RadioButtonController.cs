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

        public void Initialize()
        {
            void UpdateSelection(GuiButton control, string text)
            {
                if (active != null)
                    active.Text = texts[active];

                active = active == control ? null : control;
                initialIndex = active == null ? null : Array.IndexOf(controls, control);
                control.Text = active == control ? $"[{text}]" : text;

                SelectionChanged?.Invoke(active, new(Active));
            }

            foreach (GuiButton control in controls)
            {
                if (texts.TryGetValue(control, out string? value))
                    control.Text = value;

                string text = control.Text;
                texts.TryAdd(control, text);

                control.LeftClick += (s, e) => UpdateSelection(control, text);
            }

            if (initialIndex != null)
            {
                ClearSelection();

                GuiButton control = controls[initialIndex.Value];
                UpdateSelection(control, texts[control]);
            }
        }

        public void ClearSelection()
        {
            active = null;
            SelectionChanged?.Invoke(null, new(Active));
        }
    }
}
