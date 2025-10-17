using New_SSQE.Audio;
using New_SSQE.Misc.Static;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;
using New_SSQE.Preferences;
using System.Drawing;

namespace New_SSQE.NewGUI.Windows
{
    internal partial class GuiWindowCreate
    {
        private static readonly string audioFilter = $"Audio Files ({MusicPlayer.SupportedExtensionsString})|{MusicPlayer.SupportedExtensionsString}";

        public static readonly GuiCheckbox NavSoundSpace = new(0, 0, 50, 50)
        {
            Text = TAG_SOUND_SPACE,
            TextSize = 30,
            Style = ControlStyle.Checkbox_Uncolored
        };
        public static readonly GuiCheckbox NavRhythia = new(0, 100, 50, 50)
        {
            Text = TAG_RHYTHIA,
            TextSize = 30,
            Style = ControlStyle.Checkbox_Uncolored
        };
        public static readonly GuiCheckbox NavNova = new(0, 200, 50, 50)
        {
            Text = TAG_NOVA,
            TextSize = 30,
            Style = ControlStyle.Checkbox_Uncolored
        };
        public static readonly ControlContainer GameNavs = new(320, 415, 180, 250, NavSoundSpace, NavRhythia, NavNova);
        public static readonly RadioCheckboxController NavController = new(Settings.createGame, NavSoundSpace, NavRhythia, NavNova);

        public static readonly GuiLabel AudioIDLabelSoundSpace = new(690, 370, 540, 50)
        {
            Text = "Roblox Audio ID",
            TextSize = 36
        };
        public static readonly GuiTextbox AudioIDBoxSoundSpace = new(790, 420, 340, 50)
        {
            TextSize = 36
        };
        public static readonly GuiLabel AudioPathLabelSoundSpace = new(690, 470, 540, 50)
        {
            Text = "Import Audio",
            TextSize = 36
        };
        public static readonly GuiPathBox AudioPathSoundSpace = new(690, 520, 540, 50)
        {
            Filter = audioFilter,
            Setting = Settings.audioPath,
            Text = "CHOOSE",
            TextSize = 28
        };
        public static readonly ControlContainer SoundSpaceContainer = new(AudioIDLabelSoundSpace, AudioIDBoxSoundSpace, AudioPathLabelSoundSpace, AudioPathSoundSpace);

        public static readonly GuiLabel AudioPathLabelRhythia = new(690, 370, 540, 50)
        {
            Text = "Import Audio",
            TextSize = 36
        };
        public static readonly GuiPathBox AudioPathRhythia = new(690, 420, 540, 50)
        {
            Filter = audioFilter,
            Setting = Settings.audioPath,
            Text = "CHOOSE",
            TextSize = 28
        };
        public static readonly GuiLabel OrLabelRhythia = new(690, 470, 540, 50)
        {
            Text = "[OR]",
            TextSize = 36
        };
        public static readonly GuiLabel OnlineLabelRhythia = new(690, 520, 540, 50)
        {
            Text = "Import Rhythia Online Map",
            TextSize = 36
        };
        public static readonly GuiTextbox OnlineBoxRhythia = new(690, 570, 540, 50)
        {
            TextSize = 36
        };
        public static readonly ControlContainer RhythiaContainer = new(AudioPathLabelRhythia, AudioPathRhythia, OrLabelRhythia, OnlineLabelRhythia, OnlineBoxRhythia);

        public static readonly GuiLabel AudioPathLabelNova = new(690, 370, 540, 50)
        {
            Text = "Import Audio",
            TextSize = 36
        };
        public static readonly GuiPathBox AudioPathNova = new(690, 420, 540, 50)
        {
            Filter = audioFilter,
            Setting = Settings.audioPath,
            Text = "CHOOSE",
            TextSize = 28
        };
        public static readonly ControlContainer NovaContainer = new(AudioPathNova, AudioPathLabelNova);

        public static readonly GuiButton CreateButton = new(750, 705, 420, 50)
        {
            Text = "CREATE MAP",
            TextSize = 38
        };
        public static readonly GuiButton BackButton = new(690, 920, 540, 75)
        {
            Text = "BACK TO MENU",
            TextSize = 48
        };
        public static readonly ControlContainer Persistent = new(CreateButton, BackButton);

        public static readonly GuiSquareTextured BackgroundSquare = new("menubg", Path.Combine(Assets.THIS, "background_menu.png"))
        {
            Color = Color.FromArgb(30, 30, 30)
        };
    }
}
