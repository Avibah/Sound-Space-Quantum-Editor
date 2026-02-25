using New_SSQE.Misc;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewMaps;
using New_SSQE.NewMaps.Parsing;

namespace New_SSQE.NewGUI.Windows
{
    internal partial class GuiWindowCreate : GuiWindow
    {
        public GuiWindowCreate() : base(BackgroundSquare, Persistent, GameNavs)
        {
            AudioPathSoundSpace.SelectedFile = "";
            AudioPathRhythia.SelectedFile = "";
            AudioPathNova.SelectedFile = "";
        }

        public override void Close()
        {
            base.Close();

            GameNavs.Disconnect();
        }

        public override void ConnectEvents()
        {
            CreateButton.LeftClick += (s, e) =>
            {
                string path =
                    GameNavs.Active == NavSoundSpace ? AudioPathSoundSpace.SelectedFile :
                    GameNavs.Active == NavRhythia ? AudioPathRhythia.SelectedFile :
                    GameNavs.Active == NavNova ? AudioPathNova.SelectedFile : "";

                string id = FormatUtils.FixID(Path.GetFileNameWithoutExtension(path));
                if (GameNavs.Active == NavSoundSpace && !string.IsNullOrWhiteSpace(AudioIDBoxSoundSpace.Text))
                    id = FormatUtils.FixID(AudioIDBoxSoundSpace.Text);

                if (File.Exists(path))
                    File.Copy(path, Assets.CachedAt($"{id}.asset"), true);

                Mapping.Load(id);
            };

            BackButton.LeftClick += (s, e) => Windowing.Open<GuiWindowMenu>();
        }
    }
}
