using New_SSQE.Misc.Static;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;
using New_SSQE.Preferences;
using System.Drawing;

namespace New_SSQE.NewGUI.Windows
{
    internal partial class GuiWindowMenu
    {
        public static readonly GuiLabel ChangelogLabel = new(60, 195, 200, 40, null, "CHANGELOG", 42, "main", CenterMode.None);
        public static readonly GuiLabel SoundSpaceLabel = new(35, 0, 750, 100, null, "SOUND SPACE", 156, "square", CenterMode.None);
        public static readonly GuiLabel QuantumEditorLabel = new(615, 140, 150, 40, null, "QUANTUM EDITOR", 38, "square", CenterMode.None);
        public static readonly GuiLabel Changelog = new(60, 230, 890, 715, null, "", 20, "main", CenterMode.None);
        public static readonly GuiButton FeedbackButton = new(35, 140, 100, 40, "Feedback?", 20);

        public static readonly GuiButton CreateButton = new(1190, 180, 600, 100, "CREATE NEW MAP", 54, "main");
        public static readonly GuiButton LoadButton = new(1190, 295, 600, 100, "LOAD MAP", 54, "main");
        public static readonly GuiButton ImportButton = new(1190, 410, 600, 100, "PASTE MAP", 54, "main");
        public static readonly GuiButton SettingsButton = new(1190, 525, 600, 100, "SETTINGS", 54, "main");

        public static readonly GuiButton AutosavedButton = new(1190, 640, 600, 100, "AUTOSAVED MAP", 54, "main");
        public static readonly GuiButton LastMapButton = new(1190, 755, 600, 100, "EDIT LAST MAP", 54, "main");

        public static readonly GuiSlider ChangelogSlider = new(950, 230, 20, 720, Settings.changelogPosition) { Style = ControlStyle.Slider_Uncolored };
        public static readonly GuiSquare ChangelogBackdrop1 = new(35, 180, 950, 790, Color.FromArgb(40, 0, 0, 0));
        public static readonly GuiSquare ChangelogBackdrop2 = new(55, 230, 900, 715, Color.FromArgb(50, 0, 0, 0));

        public static readonly GuiSquare MapSelectBackdrop = new(0, 1040, 1920, 40, Color.FromArgb(50, 0, 0, 0));
        public static readonly GuiButton NavLeft = new(0, 1040, 40, 40, "<", 34);
        public static readonly GuiButton NavRight = new(1880, 1040, 40, 40, ">", 34);

        public static readonly GuiButton MapSelect0 = new(40, 1040, 368, 40, "", 20);
        public static readonly GuiButton MapSelect1 = new(408, 1040, 368, 40, "", 20);
        public static readonly GuiButton MapSelect2 = new(776, 1040, 368, 40, "", 20);
        public static readonly GuiButton MapSelect3 = new(1144, 1040, 368, 40, "", 20);
        public static readonly GuiButton MapSelect4 = new(1512, 1040, 368, 40, "", 20);

        public static readonly GuiButton MapClose0 = new(40, 1040, 40, 40, "X", 26);
        public static readonly GuiButton MapClose1 = new(408, 1040, 40, 40, "X", 26);
        public static readonly GuiButton MapClose2 = new(776, 1040, 40, 40, "X", 26);
        public static readonly GuiButton MapClose3 = new(1144, 1040, 40, 40, "X", 26);
        public static readonly GuiButton MapClose4 = new(1512, 1040, 40, 40, "X", 26);

        public static readonly GuiSquareTextured BackgroundSquare = new("menubg", Path.Combine(Assets.THIS, "background_menu.png"), Color.FromArgb(30, 30, 30));


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
