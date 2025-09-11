using New_SSQE.Misc.Static;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;
using New_SSQE.Preferences;
using System.Drawing;

namespace New_SSQE.NewGUI.Windows
{
    internal partial class GuiWindowCreate
    {
        public static readonly GuiCheckbox NavSoundSpace = new(0, 0, 50, 50, null, TAG_SOUND_SPACE, 30) { Style = ControlStyle.Checkbox_Uncolored };
        public static readonly GuiCheckbox NavRhythia = new(0, 100, 50, 50, null, TAG_RHYTHIA, 30) { Style = ControlStyle.Checkbox_Uncolored };
        public static readonly GuiCheckbox NavNova = new(0, 200, 50, 50, null, TAG_NOVA, 30) { Style = ControlStyle.Checkbox_Uncolored };
        public static readonly ControlContainer GameNavs = new(320, 415, 180, 250, NavSoundSpace, NavRhythia, NavNova);
        public static readonly RadioCheckboxController NavController = new(Settings.createGame, NavSoundSpace, NavRhythia, NavNova);

        public static readonly GuiLabel AudioIDLabelSoundSpace = new(690, 370, 540, 50, null, "Roblox Audio ID", 36);
        public static readonly GuiTextbox AudioIDBoxSoundSpace = new(790, 420, 340, 50, null, "", 36);
        public static readonly GuiLabel AudioPathLabelSoundSpace = new(690, 470, 540, 50, null, "Import Audio", 36);
        public static readonly GuiPathBox AudioPathSoundSpace = new(690, 520, 540, 50, audioFilter, Settings.audioPath, null, "CHOOSE", 28);
        public static readonly ControlContainer SoundSpaceContainer = new(AudioIDLabelSoundSpace, AudioIDBoxSoundSpace, AudioPathLabelSoundSpace, AudioPathSoundSpace);

        public static readonly GuiLabel AudioPathLabelRhythia = new(690, 370, 540, 50, null, "Import Audio", 36);
        public static readonly GuiPathBox AudioPathRhythia = new(690, 420, 540, 50, audioFilter, Settings.audioPath, null, "CHOOSE", 28);
        public static readonly GuiLabel OrLabelRhythia = new(690, 470, 540, 50, null, "[OR]", 36);
        public static readonly GuiLabel OnlineLabelRhythia = new(690, 520, 540, 50, null, "Import Rhythia Online Map", 36);
        public static readonly GuiTextbox OnlineBoxRhythia = new(690, 570, 540, 50, null, "", 36);
        public static readonly ControlContainer RhythiaContainer = new(AudioPathLabelRhythia, AudioPathRhythia, OrLabelRhythia, OnlineLabelRhythia, OnlineBoxRhythia);

        public static readonly GuiLabel AudioPathLabelNova = new(690, 370, 540, 50, null, "Import Audio", 36);
        public static readonly GuiPathBox AudioPathNova = new(690, 420, 540, 50, audioFilter, Settings.audioPath, null, "CHOOSE", 28);
        public static readonly ControlContainer NovaContainer = new(AudioPathNova, AudioPathLabelNova);

        public static readonly GuiButton CreateButton = new(750, 705, 420, 50, "CREATE MAP", 38);
        public static readonly GuiButton BackButton = new(690, 920, 540, 75, "BACK TO MENU", 48);
        public static readonly ControlContainer Persistent = new(CreateButton, BackButton);

        public static readonly GuiSquareTextured BackgroundSquare = new("menubg", Path.Combine(Assets.THIS, "background_menu.png"), Color.FromArgb(30, 30, 30));
    }
}
