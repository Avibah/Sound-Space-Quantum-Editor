using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;

namespace New_SSQE.NewGUI.CompoundControls
{
    internal class GuiRadioPanelButton : ControlContainer
    {
        public Predicate<GuiButton?>? OnPanelButtonClicked = null;
        public RadioButtonController Controller;

        private readonly Dictionary<GuiButton, ControlContainer> panels = [];

        public GuiRadioPanelButton(int? activeIndex = null, params (GuiButton, ControlContainer)[] panelMap) : base()
        {
            foreach ((GuiButton, ControlContainer) item in panelMap)
                panels.Add(item.Item1, item.Item2);

            Controller = new(activeIndex, [.. panels.Keys]);

            SetControls([.. panels.Keys, .. panels.Values]);
        }

        public override void Reset()
        {
            base.Reset();

            Controller.SelectionChanged += (s, e) =>
            {
                if (OnPanelButtonClicked?.Invoke(e.Active) ?? false)
                    return;

                foreach (KeyValuePair<GuiButton, ControlContainer> item in panels)
                    item.Value.Visible = item.Key == e.Active;
            };
        }
    }
}
