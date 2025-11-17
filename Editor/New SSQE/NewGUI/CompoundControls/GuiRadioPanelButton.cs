using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;

namespace New_SSQE.NewGUI.CompoundControls
{
    internal class GuiRadioPanelButton : ControlContainer
    {
        // Should return TRUE if the clicked button should not update the selection
        public Predicate<GuiButton?>? PanelButtonClickCallback = null;
        public RadioButtonController Controller;

        private readonly Dictionary<GuiButton, ControlContainer> panels = [];

        public GuiRadioPanelButton(float x, float y, float w, float h, int? activeIndex = null, params (GuiButton, ControlContainer)[] panelMap) : base(x, y, w, h)
        {
            foreach ((GuiButton, ControlContainer) item in panelMap)
                panels.Add(item.Item1, item.Item2);

            Controller = new(activeIndex, [.. panels.Keys]);

            SetControls([.. panels.Keys, .. panels.Values]);
        }

        public GuiRadioPanelButton(int? activeIndex = null, params (GuiButton, ControlContainer)[] panelMap) : this(0, 0, 1920, 1080, activeIndex, panelMap) { }

        public override void Reset()
        {
            base.Reset();

            Controller.SelectionChanged += (s, e) =>
            {
                if (PanelButtonClickCallback?.Invoke(e.Active) ?? false)
                    return;

                foreach (KeyValuePair<GuiButton, ControlContainer> item in panels)
                    item.Value.Visible = item.Key == e.Active;
            };

            Controller.Initialize();
        }

        public void Disconnect() => Controller.Disconnect();
    }
}
