using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;
using System.Drawing;

namespace New_SSQE.NewGUI.Forms
{
    internal class GuiDialogExport : GuiWindowDialog
    {
        private static readonly GuiSquare BackgroundSquare = new(0, 0, 400, 400, Color.Black);

        public GuiDialogExport() : base(760, 340, 400, 400, BackgroundSquare)
        {
            
        }

        public override void ConnectEvents()
        {

        }
    }
}
