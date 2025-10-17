using New_SSQE.NewGUI.Controls;

namespace New_SSQE.NewGUI.Base
{
    internal class RadioButtonEventArgs : EventArgs
    {
        public RadioButtonEventArgs()
        {
            
        }
    }

    internal class RadioButtonController
    {
        public event EventHandler<RadioButtonEventArgs>? SelectionChanged;

        private readonly Dictionary<GuiButton, string> texts = [];
        private readonly GuiButton[] controls;
        private GuiButton? active;

        private int? prevActive = null;

        public string Active => active != null ? texts[active] : "";

        public RadioButtonController(int? activeIndex, params GuiButton[] controls)
        {
            this.controls = controls;
            prevActive = activeIndex;
        }

        public void UpdateSelection(GuiButton control)
        {
            if (!texts.TryGetValue(control, out string? text))
                return;
            if (active != null)
                active.Text = texts[active];

            active = active == control ? null : control;
            prevActive = active == null ? null : Array.IndexOf(controls, control);
            control.Text = active == control ? $"[{text}]" : text;

            SelectionChanged?.Invoke(active, new());
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

            if (prevActive != null)
            {
                ClearSelection();
                UpdateSelection(controls[prevActive.Value]);
            }
        }

        public void ClearSelection()
        {
            if (active != null)
                active.Text = texts[active];

            active = null;
            SelectionChanged?.Invoke(null, new());
        }

        public void Disconnect()
        {
            SelectionChanged = null;
        }
    }
}
