using New_SSQE.Misc;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.CompoundControls;
using New_SSQE.NewGUI.Controls;
using System.Drawing;

namespace New_SSQE.NewGUI.Windows
{
    internal partial class GuiWindowMenu
    {
        public static readonly GuiLabel ChangelogLabel = new(60, 195, 200, 40)
        {
            Text = "CHANGELOG",
            TextSize = 42,
            CenterMode = CenterMode.None
        };
        public static readonly GuiLabel SoundSpaceLabel = new(35, 0, 750, 100)
        {
            Text = "SOUND SPACE",
            TextSize = 156,
            Font = "square",
            CenterMode = CenterMode.None
        };
        public static readonly GuiLabel QuantumEditorLabel = new(615, 140, 150, 40)
        {
            Text = "QUANTUM EDITOR",
            TextSize = 38,
            Font = "square",
            CenterMode = CenterMode.None
        };
        public static readonly GuiLabel Changelog = new(60, 230, 890, 715)
        {
            TextWrapped = true,
            TextSize = 20,
            Font = "semibold",
            CenterMode = CenterMode.None
        };
        public static readonly GuiButton FeedbackButton = new(35, 140, 100, 40)
        {
            Text = "Feedback?",
            TextSize = 20
        };

        public static readonly GuiButton CreateButton = new(1190, 180, 600, 100)
        {
            Text = "CREATE NEW MAP",
            TextSize = 54
        };
        public static readonly GuiButton LoadButton = new(1190, 295, 600, 100)
        {
            Text = "LOAD MAP",
            TextSize = 54
        };
        public static readonly GuiButton ImportButton = new(1190, 410, 600, 100)
        {
            Text = "PASTE MAP",
            TextSize = 54
        };
        public static readonly GuiButton SettingsButton = new(1190, 525, 600, 100)
        {
            Text = "SETTINGS",
            TextSize = 54
        };

        public static readonly GuiButton AutosavedButton = new(1190, 640, 600, 100)
        {
            Text = "AUTOSAVED MAP",
            TextSize = 54
        };
        public static readonly GuiButton LastMapButton = new(1190, 755, 600, 100)
        {
            Text = "EDIT LAST MAP",
            TextSize = 54
        };

        public static readonly GuiScrollingList ChangelogPanel = new(60, 230, 890, 715, Changelog)
        {
            ScrollScale = 2,
            Style = ControlStyle.Slider_Uncolored
        };
        public static readonly GuiSquare ChangelogBackdrop1 = new(35, 180, 950, 790)
        {
            Color = Color.FromArgb(40, 0, 0, 0)
        };
        public static readonly GuiSquare ChangelogBackdrop2 = new(55, 230, 900, 715)
        {
            Color = Color.FromArgb(50, 0, 0, 0)
        };

        public static readonly GuiSquare MapSelectBackdrop = new(0, 1040, 1920, 40)
        {
            Color = Color.FromArgb(50, 0, 0, 0)
        };
        public static readonly GuiButton NavLeft = new(0, 1040, 40, 40)
        {
            Text = "<",
            TextSize = 34
        };
        public static readonly GuiButton NavRight = new(1880, 1040, 40, 40)
        {
            Text = ">",
            TextSize = 34
        };

        public static readonly GuiButton MapSelect0 = new(40, 1040, 368, 40)
        {
            TextSize = 20
        };
        public static readonly GuiButton MapSelect1 = new(408, 1040, 368, 40)
        {
            TextSize = 20
        };
        public static readonly GuiButton MapSelect2 = new(776, 1040, 368, 40)
        {
            TextSize = 20
        };
        public static readonly GuiButton MapSelect3 = new(1144, 1040, 368, 40)
        {
            TextSize = 20
        };
        public static readonly GuiButton MapSelect4 = new(1512, 1040, 368, 40)
        {
            TextSize = 20
        };

        public static readonly GuiButton MapClose0 = new(40, 1040, 40, 40)
        {
            Text = "X",
            TextSize = 26
        };
        public static readonly GuiButton MapClose1 = new(408, 1040, 40, 40)
        {
            Text = "X",
            TextSize = 26
        };
        public static readonly GuiButton MapClose2 = new(776, 1040, 40, 40)
        {
            Text = "X",
            TextSize = 26
        };
        public static readonly GuiButton MapClose3 = new(1144, 1040, 40, 40)
        {
            Text = "X",
            TextSize = 26
        };
        public static readonly GuiButton MapClose4 = new(1512, 1040, 40, 40)
        {
            Text = "X",
            TextSize = 26
        };

        public static readonly GuiSquareTextured BackgroundSquare = new("menubg", Assets.ThisAt("background_menu.png"))
        {
            Color = Color.FromArgb(30, 30, 30)
        };

        private static readonly List<(GuiButton, GuiButton)> mapSelects =
        [
            (MapSelect0, MapClose0),
            (MapSelect1, MapClose1),
            (MapSelect2, MapClose2),
            (MapSelect3, MapClose3),
            (MapSelect4, MapClose4)
        ];
    }
}
