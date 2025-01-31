using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;
using New_SSQE.NewMaps;
using New_SSQE.NewMaps.Parsing;
using System.Drawing;

namespace New_SSQE.NewGUI.Windows
{
    internal class GuiWindowCreate : GuiWindow
    {
        public static readonly GuiLabel IDLabel = new(832, 478, 256, 20, null, "Input Audio ID", 30);
        public static readonly GuiTextbox IDBox = new(832, 508, 256, 64, null, "", 30);

        public static readonly GuiButton CreateButton = new(832, 592, 256, 64, "CREATE", 38);
        public static readonly GuiButton ImportButton = new(832, 666, 256, 64, "IMPORT FILE", 38);
        public static readonly GuiButton BackButton = new(832, 740, 256, 64, "BACK", 38);

        public static readonly GuiSquareTextured BackgroundSquare = new("menubg", "background_menu.png", Color.FromArgb(30, 30, 30));

        public GuiWindowCreate() : base(BackgroundSquare, IDLabel, IDBox, CreateButton, ImportButton, BackButton)
        {
            
        }

        public override void ConnectEvents()
        {
            CreateButton.LeftClick += (s, e) =>
            {
                string audioId = IDBox.Text.Trim();
                audioId = FormatUtils.FixID(audioId);

                if (!string.IsNullOrWhiteSpace(audioId))
                    MapManager.Load(audioId);
            };

            ImportButton.LeftClick += (s, e) =>
            {
                string audioId = IDBox.Text.Trim();
                audioId = FormatUtils.FixID(audioId);

                if (MapManager.ImportAudio(audioId))
                    MapManager.Load(audioId);
            };

            BackButton.LeftClick += (s, e) => Windowing.SwitchWindow(new GuiWindowMenu());
        }
    }
}
