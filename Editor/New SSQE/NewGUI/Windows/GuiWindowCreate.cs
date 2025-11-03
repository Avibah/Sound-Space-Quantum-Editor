using New_SSQE.Misc.Static;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewMaps;
using New_SSQE.NewMaps.Parsing;

namespace New_SSQE.NewGUI.Windows
{
    internal partial class GuiWindowCreate : GuiWindow
    {
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
                string path =
                    NavController.Active == NavSoundSpace ? AudioPathSoundSpace.SelectedFile :
                    NavController.Active == NavRhythia ? AudioPathRhythia.SelectedFile :
                    NavController.Active == NavNova ? AudioPathNova.SelectedFile : "";

                string id = FormatUtils.FixID(Path.GetFileNameWithoutExtension(path));

                if (NavController.Active == NavSoundSpace && !string.IsNullOrWhiteSpace(AudioIDBoxSoundSpace.Text))
                {
                    id = FormatUtils.FixID(AudioIDBoxSoundSpace.Text);

                    if (Mapping.LoadAudio(id))
                        Mapping.Load(id);
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
                SoundSpaceContainer.Visible = e.Active == NavSoundSpace;
                RhythiaContainer.Visible = e.Active == NavRhythia;
                NovaContainer.Visible = e.Active == NavNova;
            };

            NavController.Initialize();
        }
    }
}
