using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using New_SSQE.ExternalUtils;
using New_SSQE.Misc;
using New_SSQE.Misc.Dialogs;
using New_SSQE.NewMaps.Parsing;
using New_SSQE.Preferences;
using OpenFileDialog = New_SSQE.Misc.Dialogs.OpenFileDialog;

namespace New_SSQE.NewGUI
{
    public partial class ExportSSPM : Window
    {
        public static ExportSSPM? Instance;

        public ExportSSPM()
        {
            Instance = this;
            Icon = new(new Bitmap(Path.Combine(Assets.TEXTURES, "Empty.png")));

            InitializeComponent();

            MapIDBox.IsReadOnly = !MainWindow.DebugVersion;

            MapperBox.Text = Settings.mappers.Value;
            SongNameBox.Text = Settings.songName.Value;
            UseCover.IsChecked = Settings.useCover.Value;
            CoverPathBox.Text = Settings.cover.Value;
            CustomDifficultyBox.Text = Settings.customDifficulty.Value;

            foreach (ComboBoxItem item in DifficultyBox.Items)
            {
                if (item?.Content.ToString() == Settings.difficulty.Value)
                    DifficultyBox.SelectedItem = item;
            }

            CreateID();
        }

        private string GetSongName()
        {
            return !string.IsNullOrWhiteSpace(SongNameBox.Text) ? SongNameBox.Text : "Untitled Song";
        }

        private string[] GetMappers()
        {
            if (MapperBox.Text == null)
                return new string[] { "None" };

            string[] mappers = MapperBox.Text.Split("\n");
            for (int i = 0; i < mappers.Length; i++)
                mappers[i] = mappers[i].Replace("\r", "");

            List<string> mappersList = new();

            foreach (string mapper in mappers)
                if (!string.IsNullOrWhiteSpace(mapper))
                    mappersList.Add(mapper);

            return mappersList.Count > 0 ? mappersList.ToArray() : new string[] { "None" };
        }

        private string prevString = "";

        private void CreateID()
        {
            string songName = GetSongName();
            string mappers = string.Join(" ", GetMappers());
            string curString = FormatUtils.FixID($"{mappers} - {songName}");

            if (curString != prevString)
            {
                MapIDBox.Text = curString;
                prevString = curString;
            }
        }

        public static void UpdateID()
        {
            Instance?.CreateID();
        }

        public static void ShowWindow()
        {
            Instance?.Close();

            ExportSSPM window = new();

            window.Show();

            if (Platforms.IsLinux)
            {
                window.Topmost = true;
                BackgroundWindow.YieldWindow(window);
            }
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            SSPM.Metadata["songId"] = MapIDBox.Text ?? "";
            SSPM.Metadata["mapName"] = GetSongName();
            SSPM.Metadata["mappers"] = string.Join("\n", GetMappers());
            SSPM.Metadata["coverPath"] = (UseCover.IsChecked ?? false) ? CoverPathBox.Text : "";
            ComboBoxItem? item = DifficultyBox.SelectedItem as ComboBoxItem;
            SSPM.Metadata["difficulty"] = FormatUtils.Difficulties.ContainsKey(item?.Content.ToString() ?? "") ? (item?.Content.ToString() ?? "") : "N/A";
            SSPM.Metadata["customDifficulty"] = CustomDifficultyBox.Text;

            Settings.mappers.Value = SSPM.Metadata["mappers"];
            Settings.songName.Value = SSPM.Metadata["mapName"];
            Settings.difficulty.Value = SSPM.Metadata["difficulty"];
            Settings.useCover.Value = UseCover.IsChecked ?? false;
            Settings.cover.Value = CoverPathBox.Text;
            Settings.customDifficulty.Value = CustomDifficultyBox.Text;

            Close();

            SSPM.Export();
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult result = new OpenFileDialog()
            {
                Title = "Select Cover Image",
                Filter = "PNG Images (*.png)|*.png"
            }.Show(Settings.coverPath, out string fileName);

            if (result == DialogResult.OK)
                CoverPathBox.Text = fileName;
            else
                CoverPathBox.Text = "Default";
        }
    }
}
