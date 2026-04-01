using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using New_SSQE.Misc;
using New_SSQE.NewGUI.Dialogs;
using New_SSQE.NewMaps;
using New_SSQE.NewMaps.Parsing;
using New_SSQE.Preferences;
using New_SSQE.Services;
using OpenFileDialog = New_SSQE.NewGUI.Dialogs.OpenFileDialog;

namespace New_SSQE.NewGUI
{
    public partial class ExportPHXM : Window
    {
        private static ExportPHXM? Instance;

        public ExportPHXM()
        {
            Instance = this;
            Icon = new(new Bitmap(Assets.TexturesAt("Empty.png")));

            InitializeComponent();

            TitleBox.Text = Settings.songTitle.Value;
            ArtistBox.Text = Settings.songArtist.Value;
            MapperBox.Text = Settings.mappers.Value;
            if (string.IsNullOrWhiteSpace(MapperBox.Text))
                MapperBox.Text = Settings.mapCreator.Value;
            CoverPathBox.Text = Settings.cover.Value;
            VideoPathBox.Text = Settings.video.Value;
            ArtistLinkBox.Text = Mapping.Current.ArtistLinks.Values.FirstOrDefault() ?? "";
            RatingBox.Text = Settings.rating.Value.ToString();
            PlatformBox.Text = Settings.phxmPlatform.Value;
            CustomDifficultyBox.Text = Settings.customDifficulty.Value;

            foreach (ComboBoxItem item in DifficultyBox.Items)
            {
                if (item?.Content.ToString() == Settings.difficulty.Value)
                    DifficultyBox.SelectedItem = item;
            }
        }

        public static void ShowWindow()
        {
            Instance?.Close();

            ExportPHXM window = new();

            window.Show();

            if (Platforms.IsLinux)
            {
                window.Topmost = true;
                BackgroundWindow.YieldWindow(window);
            }
        }

        private string GetTitle()
        {
            return !string.IsNullOrWhiteSpace(TitleBox.Text) ? TitleBox.Text : "Untitled Song";
        }

        private string[] GetMappers()
        {
            if (MapperBox.Text == null)
                return ["None"];

            string[] mappers = MapperBox.Text.Split("\n");
            for (int i = 0; i < mappers.Length; i++)
                mappers[i] = mappers[i].Replace("\r", "");

            List<string> mappersList = [];

            foreach (string mapper in mappers)
                if (!string.IsNullOrWhiteSpace(mapper))
                    mappersList.Add(mapper);

            return mappersList.Count > 0 ? [.. mappersList] : ["None"];
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            PHXM.Metadata["songTitle"] = GetTitle();
            PHXM.Metadata["songArtist"] = ArtistBox.Text;
            PHXM.Metadata["mappers"] = string.Join('\n', GetMappers());
            PHXM.Metadata["coverPath"] = File.Exists(CoverPathBox.Text) ? CoverPathBox.Text : "";
            PHXM.Metadata["videoPath"] = File.Exists(VideoPathBox.Text) ? VideoPathBox.Text : "";
            PHXM.Metadata["rating"] = float.TryParse(RatingBox.Text, out _) ? RatingBox.Text : "0";
            PHXM.Metadata["artistLink"] = ArtistLinkBox.Text;
            PHXM.Metadata["artistPlatform"] = PlatformBox.Text;
            ComboBoxItem? item = DifficultyBox.SelectedItem as ComboBoxItem;
            PHXM.Metadata["difficulty"] = FormatUtils.Difficulties.ContainsKey(item?.Content.ToString() ?? "") ? (item?.Content.ToString() ?? "") : "N/A";
            PHXM.Metadata["difficultyName"] = CustomDifficultyBox.Text;

            Settings.songTitle.Value = PHXM.Metadata["songTitle"];
            Settings.romanizedTitle.Value = FormatUtils.FixASCII(Settings.songTitle.Value);
            Settings.songArtist.Value = PHXM.Metadata["songArtist"];
            Settings.romanizedArtist.Value = FormatUtils.FixASCII(Settings.songArtist.Value);
            Settings.songName.Value = $"{Settings.songArtist.Value} - {Settings.songTitle.Value}";
            Settings.mappers.Value = PHXM.Metadata["mappers"];
            Settings.coverPath.Value = PHXM.Metadata["coverPath"];
            Settings.useCover.Value = !string.IsNullOrWhiteSpace(Settings.coverPath.Value);
            Settings.video.Value = PHXM.Metadata["videoPath"];
            Settings.useVideo.Value = !string.IsNullOrWhiteSpace(Settings.video.Value);
            if (float.TryParse(PHXM.Metadata["rating"], out float rating))
                Settings.rating.Value = rating;
            Mapping.Current.ArtistLinks = new()
            {
                {Settings.songArtist.Value, PHXM.Metadata["artistLink"] }
            };
            Settings.phxmPlatform.Value = PHXM.Metadata["artistPlatform"];
            Settings.difficulty.Value = PHXM.Metadata["difficulty"];
            Settings.customDifficulty.Value = PHXM.Metadata["difficultyName"];

            Close();

            PHXM.Export();
        }

        private void SelectCover_Click(object sender, RoutedEventArgs e)
        {
            DialogResult result = new OpenFileDialog()
            {
                Title = "Select Cover Image",
                Filter = "Image Files (*.png;)|*.png"
            }.Show(Settings.coverPath, out string fileName);

            if (result == DialogResult.OK)
                CoverPathBox.Text = fileName;
            else
                CoverPathBox.Text = "";
        }

        private void SelectVideo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult result = new OpenFileDialog()
            {
                Title = "Select Video",
                Filter = "Video Files (*.mp4)|*.mp4"
            }.Show(Settings.video, out string fileName);

            if (result == DialogResult.OK)
                VideoPathBox.Text = fileName;
            else
                VideoPathBox.Text = "";
        }
    }
}
