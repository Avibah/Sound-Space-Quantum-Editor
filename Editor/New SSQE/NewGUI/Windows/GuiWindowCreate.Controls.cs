using New_SSQE.Audio;
using New_SSQE.Misc;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.CompoundControls;
using New_SSQE.NewGUI.Controls;
using New_SSQE.Preferences;
using System.Drawing;

namespace New_SSQE.NewGUI.Windows
{
    internal partial class GuiWindowCreate
    {
        private static readonly string audioFilter = $"Audio Files ({SoundEngine.SupportedExtensionsString})|{SoundEngine.SupportedExtensionsString}";

        public static readonly GuiCheckbox NavSoundSpace = new(0, 0, 50, 50)
        {
            Text = "Sound Space",
            TextSize = 30,
            Style = ControlStyle.Checkbox_Uncolored
        };
        public static readonly GuiCheckbox NavRhythia = new(0, 100, 50, 50)
        {
            Text = "Rhythia",
            TextSize = 30,
            Style = ControlStyle.Checkbox_Uncolored
        };
        public static readonly GuiCheckbox NavNova = new(0, 200, 50, 50)
        {
            Text = "Novastra/Phoenyx/Other",
            TextSize = 30,
            Style = ControlStyle.Checkbox_Uncolored
        };

        public static readonly GuiLabel AudioIDLabelSoundSpace = new(370, -45, 540, 50)
        {
            Text = "Roblox Audio ID",
            TextSize = 36
        };
        public static readonly GuiTextbox AudioIDBoxSoundSpace = new(470, 5, 340, 50)
        {
            TextSize = 36
        };
        public static readonly GuiLabel AudioPathLabelSoundSpace = new(370, 55, 540, 50)
        {
            Text = "Import Audio",
            TextSize = 36
        };
        public static readonly GuiPathBox AudioPathSoundSpace = new(370, 105, 540, 50)
        {
            Filter = audioFilter,
            Setting = Settings.audioPath,
            Text = "CHOOSE",
            TextSize = 28
        };
        public static readonly ControlContainer SoundSpaceContainer = new(AudioIDLabelSoundSpace, AudioIDBoxSoundSpace, AudioPathLabelSoundSpace, AudioPathSoundSpace);

        public static readonly GuiLabel AudioPathLabelRhythia = new(370, -45, 540, 50)
        {
            Text = "Import Audio",
            TextSize = 36
        };
        public static readonly GuiPathBox AudioPathRhythia = new(370, 5, 540, 50)
        {
            Filter = audioFilter,
            Setting = Settings.audioPath,
            Text = "CHOOSE",
            TextSize = 28
        };
        public static readonly GuiLabel OnlineDisclaimerRhythia = new(370, 85, 540, 100)
        {
            Text = string.Join('\n',
                "Rhythia Online is no longer supported for importing",
                "and should not be used for new work.",
                "",
                "discord.gg/rhythia -> #Announcements"
            ),
            TextSize = 20,
            CenterMode = CenterMode.XY
        };
        public static readonly ControlContainer RhythiaContainer = new(AudioPathLabelRhythia, AudioPathRhythia, OnlineDisclaimerRhythia);

        public static readonly GuiLabel AudioPathLabelNova = new(370, -45, 540, 50)
        {
            Text = "Import Audio",
            TextSize = 36
        };
        public static readonly GuiPathBox AudioPathNova = new(370, 5, 540, 50)
        {
            Filter = audioFilter,
            Setting = Settings.audioPath,
            Text = "CHOOSE",
            TextSize = 28
        };
        public static readonly ControlContainer NovaContainer = new(AudioPathNova, AudioPathLabelNova);

        public static readonly GuiRadioPanelCheckbox GameNavs = new(320, 415, 180, 250, Settings.createGame,
            (NavSoundSpace, SoundSpaceContainer),
            (NavRhythia, RhythiaContainer),
            (NavNova, NovaContainer)
        );

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

        public static readonly GuiSquareTextured BackgroundSquare = new("menubg", Assets.ThisAt("background_menu.png"))
        {
            Color = Color.FromArgb(30, 30, 30)
        };
    }
}
