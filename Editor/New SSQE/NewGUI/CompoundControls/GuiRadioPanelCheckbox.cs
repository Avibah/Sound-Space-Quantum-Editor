using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;
using New_SSQE.Preferences;

namespace New_SSQE.NewGUI.CompoundControls
{
    internal class GuiRadioPanelCheckbox : ControlContainer
    {
        // Should return TRUE if the clicked checkbox should not update the selection
        public Predicate<GuiCheckbox?>? PanelCheckboxClickCallback = null;
        public RadioCheckboxController Controller;
        public GuiCheckbox Active => Controller.Active;

        private readonly Dictionary<GuiCheckbox, ControlContainer> panels = [];

        public GuiRadioPanelCheckbox(float x, float y, float w, float h, int activeIndex, params (GuiCheckbox, ControlContainer)[] panelMap) : base(x, y, w, h)
        {
            foreach ((GuiCheckbox, ControlContainer) item in panelMap)
                panels.Add(item.Item1, item.Item2);

            Controller = new(activeIndex, [.. panels.Keys]);

            SetControls([.. panels.Keys, .. panels.Values]);
        }

        public GuiRadioPanelCheckbox(int activeIndex, params (GuiCheckbox, ControlContainer)[] panelMap) : this(0, 0, 1920, 1080, activeIndex, panelMap) { }

        public GuiRadioPanelCheckbox(float x, float y, float w, float h, Setting<string> setting, params (GuiCheckbox, ControlContainer)[] panelMap) : base(x, y, w, h)
        {
            foreach ((GuiCheckbox, ControlContainer) item in panelMap)
                panels.Add(item.Item1, item.Item2);

            Controller = new(setting, [.. panels.Keys]);

            SetControls([.. panels.Keys, .. panels.Values]);
        }

        public GuiRadioPanelCheckbox(Setting<string> setting, params (GuiCheckbox, ControlContainer)[] panelMap) : this(0, 0, 1920, 1080, setting, panelMap) { }

        public override void Reset()
        {
            base.Reset();

            Controller.SelectionChanged += (s, e) =>
            {
                if (PanelCheckboxClickCallback?.Invoke(e.Active) ?? false)
                    return;

                foreach (KeyValuePair<GuiCheckbox, ControlContainer> item in panels)
                    item.Value.Visible = item.Key == e.Active;
            };

            Controller.Initialize();
        }

        public void Disconnect() => Controller.Disconnect();
    }
}
