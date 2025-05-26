using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;
using System.Drawing;

namespace New_SSQE.NewGUI.Windows
{
    internal class GuiDialogExport : GuiWindowDialog
    {
        private static GuiDialogExport? Instance;

        private readonly GuiSquare BackgroundSquare = new(0, 0, 400, 400, Color.Black) { Stretch = StretchMode.XY };

        public GuiDialogExport() : base(760, 340, 400, 400)
        {
            SetControls(BackgroundSquare);

            if (Instance != null)
                Windowing.Close(Instance);
            Instance = this;

            CloseButton.LeftClick += (s, e) =>
            {
                Instance = null;
            };
        }

        public override void ConnectEvents()
        {
            
        }
    }
}
