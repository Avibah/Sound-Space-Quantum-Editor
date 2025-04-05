using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using New_SSQE.ExternalUtils;
using New_SSQE.Misc.Dialogs;
using New_SSQE.Misc.Static;
using New_SSQE.NewMaps.Parsing;
using New_SSQE.Preferences;
using OpenFileDialog = New_SSQE.Misc.Dialogs.OpenFileDialog;

namespace New_SSQE.NewGUI
{
    public partial class ExportNOVA : Window
    {
        private static ExportNOVA? Instance;

        public ExportNOVA()
        {
            Instance = this;
            Icon = new(new Bitmap(Path.Combine(Assets.TEXTURES, "Empty.png")));

            InitializeComponent();

            TitleBox.Text = Settings.songTitle.Value;
            ArtistBox.Text = Settings.songArtist.Value;
            MapperBox.Text = Settings.mapCreator.Value;
            LinkBox.Text = Settings.mapCreatorPersonalLink.Value;
            CoverPathBox.Text = Settings.novaCover.Value;
            IconPathBox.Text = Settings.novaIcon.Value;
            SongOffsetBox.Text = Settings.songOffset.Value;
            PreviewStartBox.Text = Settings.previewStartTime.Value;
            PreviewDurationBox.Text = Settings.previewDuration.Value;
        }

        public static void ShowWindow()
        {
            Instance?.Close();

            ExportNOVA window = new();

            window.Show();

            if (Platform.IsLinux)
            {
                window.Topmost = true;
                BackgroundWindow.YieldWindow(window);
            }
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            NPK.Metadata["songOffset"] = long.TryParse(SongOffsetBox.Text, out long offset) ? offset.ToString() : "0";
            NPK.Metadata["songTitle"] = TitleBox.Text;
            NPK.Metadata["songArtist"] = ArtistBox.Text;
            NPK.Metadata["mapCreator"] = MapperBox.Text;
            NPK.Metadata["mapCreatorPersonalLink"] = LinkBox.Text;
            NPK.Metadata["previewStartTime"] = long.TryParse(PreviewStartBox.Text, out long start) ? start.ToString() : "0";
            NPK.Metadata["previewDuration"] = long.TryParse(PreviewDurationBox.Text, out long duration) ? duration.ToString() : "0";
            NPK.Metadata["coverPath"] = CoverPathBox.Text;
            NPK.Metadata["iconPath"] = IconPathBox.Text;

            Settings.songOffset.Value = NPK.Metadata["songOffset"];
            Settings.songTitle.Value = NPK.Metadata["songTitle"];
            Settings.songArtist.Value = NPK.Metadata["songArtist"];
            Settings.mapCreator.Value = NPK.Metadata["mapCreator"];
            Settings.mapCreatorPersonalLink.Value = NPK.Metadata["mapCreatorPersonalLink"];
            Settings.previewStartTime.Value = NPK.Metadata["previewStartTime"];
            Settings.previewDuration.Value = NPK.Metadata["previewDuration"];
            Settings.novaCover.Value = NPK.Metadata["coverPath"];
            Settings.novaIcon.Value = NPK.Metadata["iconPath"];

            Close();

            NPK.Export();
        }

        private void SelectCover_Click(object sender, RoutedEventArgs e)
        {
            DialogResult result = new OpenFileDialog()
            {
                Title = "Select Cover Image",
                Filter = "Image Files (*.png;*.jpg)|*.png;*.jpg"
            }.RunWithSetting(Settings.coverPath, out string fileName);

            if (result == DialogResult.OK)
                CoverPathBox.Text = fileName;
            else
                CoverPathBox.Text = "";
        }

        private void SelectIcon_Click(object sender, RoutedEventArgs e)
        {
            DialogResult result = new OpenFileDialog()
            {
                Title = "Select Profile Icon",
                Filter = "Image Files (*.png;*.jpg)|*.png;*.jpg"
            }.RunWithSetting(Settings.coverPath, out string fileName);

            if (result == DialogResult.OK)
                IconPathBox.Text = fileName;
            else
                IconPathBox.Text = "";
        }

        private void SongOffset_Click(object sender, RoutedEventArgs e)
        {
            SongOffsetBox.Text = ((long)Settings.currentTime.Value.Value).ToString();
        }

        private void PreviewStart_Click(object sender, RoutedEventArgs e)
        {
            long origin = long.TryParse(PreviewStartBox.Text, out long temp) ? temp : 0;
            long duration = long.TryParse(PreviewDurationBox.Text, out temp) ? temp : 0;
            PreviewStartBox.Text = ((long)Settings.currentTime.Value.Value).ToString();
            PreviewDurationBox.Text = (duration + origin - (long)Settings.currentTime.Value.Value).ToString();
        }

        private void PreviewDuration_Click(object sender, RoutedEventArgs e)
        {
            long start = long.TryParse(PreviewStartBox.Text, out long temp) ? temp : 0;
            PreviewDurationBox.Text = ((long)Settings.currentTime.Value.Value - start).ToString();
        }
    }
}
