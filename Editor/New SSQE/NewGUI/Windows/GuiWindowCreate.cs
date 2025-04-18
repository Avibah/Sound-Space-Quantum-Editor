using New_SSQE.Audio;
using New_SSQE.ExternalUtils;
using New_SSQE.Misc.Static;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;
using New_SSQE.NewMaps;
using New_SSQE.NewMaps.Parsing;
using New_SSQE.Preferences;
using System.Drawing;

namespace New_SSQE.NewGUI.Windows
{
    internal class GuiWindowCreate : GuiWindow
    {
        private const string TAG_SOUND_SPACE = "Sound Space";
        private const string TAG_RHYTHIA = "Rhythia";
        private const string TAG_NOVA = "Nova/Phoenyx/Other";

        private static readonly string audioFilter = $"Audio Files ({MusicPlayer.SupportedExtensionsString})|{MusicPlayer.SupportedExtensionsString}";

        public static readonly GuiCheckbox NavSoundSpace = new(0, 0, 50, 50, null, TAG_SOUND_SPACE, 30) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly GuiCheckbox NavRhythia = new(0, 100, 50, 50, null, TAG_RHYTHIA, 30) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly GuiCheckbox NavNova = new(0, 200, 50, 50, null, TAG_NOVA, 30) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly ControlContainer GameNavs = new(320, 415, 180, 250, NavSoundSpace, NavRhythia, NavNova);
        public static readonly RadioCheckboxController NavController = new(Settings.createGame, NavSoundSpace, NavRhythia, NavNova);

        public static readonly GuiLabel AudioIDLabelSoundSpace = new(690, 370, 540, 50, null, "Roblox Audio ID", 36);
        public static readonly GuiTextbox AudioIDBoxSoundSpace = new(790, 420, 340, 50, null, "", 36);
        public static readonly GuiLabel AudioPathLabelSoundSpace = new(690, 570, 540, 50, null, "Import Audio", 36);
        public static readonly GuiPathBox AudioPathSoundSpace = new(690, 620, 540, 50, audioFilter, null, "", 28);
        public static readonly ControlContainer SoundSpaceContainer = new(AudioIDLabelSoundSpace, AudioIDBoxSoundSpace, AudioPathLabelSoundSpace, AudioPathSoundSpace);

        public static readonly GuiLabel AudioPathLabelRhythia = new(690, 370, 540, 50, null, "Import Audio", 36);
        public static readonly GuiPathBox AudioPathRhythia = new(690, 420, 540, 50, audioFilter, null, "", 28);
        public static readonly GuiLabel OrLabelRhythia = new(690, 520, 540, 50, null, "[OR]", 36);
        public static readonly GuiLabel OnlineLabelRhythia = new(690, 570, 540, 50, null, "Import Rhythia Online Map", 36);
        public static readonly GuiTextbox OnlineBoxRhythia = new(690, 620, 540, 50, null, "", 36) { Stretch = StretchMode.X };
        public static readonly ControlContainer RhythiaContainer = new(AudioPathLabelRhythia, AudioPathRhythia, OrLabelRhythia, OnlineLabelRhythia, OnlineBoxRhythia);

        public static readonly GuiLabel AudioPathLabelNova = new(690, 370, 540, 50, null, "Import Audio", 36);
        public static readonly GuiPathBox AudioPathNova = new(690, 420, 540, 50, audioFilter, null, "", 28);
        public static readonly ControlContainer NovaContainer = new(AudioPathNova, AudioPathLabelNova);

        public static readonly GuiButton CreateButton = new(750, 705, 420, 50, "CREATE MAP", 38);
        public static readonly GuiButton BackButton = new(690, 920, 540, 75, "BACK TO MENU", 48);
        public static readonly ControlContainer Persistent = new(CreateButton, BackButton);

        public static readonly GuiSquareTextured BackgroundSquare = new("menubg", Path.Combine(Assets.THIS, "background_menu.png"), Color.FromArgb(30, 30, 30)) { Stretch = StretchMode.XY };

        public GuiWindowCreate() : base(BackgroundSquare, Persistent, GameNavs, SoundSpaceContainer, RhythiaContainer, NovaContainer)
        {
            AudioPathSoundSpace.SelectedFile = "";
            AudioPathRhythia.SelectedFile = "";
            AudioPathNova.SelectedFile = "";
        }

        public override void ConnectEvents()
        {
            CreateButton.LeftClick += (s, e) =>
            {
                string path = NavController.Active switch
                {
                    TAG_SOUND_SPACE => AudioPathSoundSpace.SelectedFile,
                    TAG_RHYTHIA => AudioPathRhythia.SelectedFile,
                    TAG_NOVA => AudioPathNova.SelectedFile,
                    _ => ""
                };

                string id = FormatUtils.FixID(Path.GetFileNameWithoutExtension(path));

                if (NavController.Active == "Sound Space" && !string.IsNullOrWhiteSpace(AudioIDBoxSoundSpace.Text))
                {
                    id = FormatUtils.FixID(AudioIDBoxSoundSpace.Text);

                    if (Mapping.LoadAudio(id))
                        Mapping.Load(id);
                }
                else if (NavController.Active == "Rhythia" && !string.IsNullOrWhiteSpace(OnlineBoxRhythia.Text))
                {
                    try
                    {
                        id = OnlineBoxRhythia.Text.Split("/").Last();
                        string url = Networking.GetBeatmapURLFromRhythiaMapID(int.Parse(id));
                        string file = Path.Combine(Assets.TEMP, "tempdownload.sspm");

                        Networking.DownloadFile(url, Path.Combine(Assets.TEMP, file));
                        Mapping.Load(file);
                    }
                    catch (Exception ex)
                    {
                        Logging.Log("Failed to download SSPM - R/O", LogSeverity.WARN, ex);
                        MessageBox.Show($"Failed to download map via Rhythia Online - {ex.Message}", MBoxIcon.Warning, MBoxButtons.OK);
                    }
                }
                else if (File.Exists(path))
                {
                    File.Copy(path, Path.Combine(Assets.CACHED, $"{id}.asset"), true);
                    Mapping.Load(id);
                }
            };

            BackButton.LeftClick += (s, e) => Windowing.SwitchWindow(new GuiWindowMenu());

            NavController.SelectionChanged += (s, e) =>
            {
                SoundSpaceContainer.Visible = e.Value == TAG_SOUND_SPACE;
                RhythiaContainer.Visible = e.Value == TAG_RHYTHIA;
                NovaContainer.Visible = e.Value == TAG_NOVA;
            };

            NavController.Initialize();
        }
    }
}
