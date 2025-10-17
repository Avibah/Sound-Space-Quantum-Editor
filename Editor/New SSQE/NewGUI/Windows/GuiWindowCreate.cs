using New_SSQE.ExternalUtils;
using New_SSQE.Misc.Static;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewMaps;
using New_SSQE.NewMaps.Parsing;

namespace New_SSQE.NewGUI.Windows
{
    internal partial class GuiWindowCreate : GuiWindow
    {
        private const string TAG_SOUND_SPACE = "Sound Space";
        private const string TAG_RHYTHIA = "Rhythia";
        private const string TAG_NOVA = "Novastra/Phoenyx/Other";

        public GuiWindowCreate() : base(BackgroundSquare, Persistent, GameNavs, SoundSpaceContainer, RhythiaContainer, NovaContainer)
        {
            AudioPathSoundSpace.SelectedFile = "";
            AudioPathRhythia.SelectedFile = "";
            AudioPathNova.SelectedFile = "";
        }

        public override void Close()
        {
            base.Close();

            NavController.Disconnect();
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
