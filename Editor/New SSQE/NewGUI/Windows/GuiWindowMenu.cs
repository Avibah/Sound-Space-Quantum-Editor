using New_SSQE.ExternalUtils;
using New_SSQE.Misc.Dialogs;
using New_SSQE.Misc.Static;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;
using New_SSQE.NewGUI.Font;
using New_SSQE.NewMaps;
using New_SSQE.NewMaps.Parsing;
using New_SSQE.Preferences;
using OpenTK.Windowing.Common;
using System.Drawing;

namespace New_SSQE.NewGUI.Windows
{
    internal class GuiWindowMenu : GuiWindow
    {
        public static readonly GuiLabel ChangelogLabel = new(60, 195, 200, 40, null, "CHANGELOG", 42, "square", CenterMode.None);
        public static readonly GuiLabel SoundSpaceLabel = new(35, 0, 750, 100, null, "SOUND SPACE", 156, "square", CenterMode.None);
        public static readonly GuiLabel QuantumEditorLabel = new(615, 140, 150, 40, null, "QUANTUM EDITOR", 38, "square", CenterMode.None);
        public static readonly GuiLabel Changelog = new(60, 230, 890, 715, null, "", 20, "main", CenterMode.None);
        public static readonly GuiButton FeedbackButton = new(35, 140, 100, 40, "Feedback?", 20);

        public static readonly GuiButton CreateButton = new(1190, 180, 600, 100, "CREATE NEW MAP", 54, "square");
        public static readonly GuiButton LoadButton = new(1190, 295, 600, 100, "LOAD MAP", 54, "square");
        public static readonly GuiButton ImportButton = new(1190, 410, 600, 100, "PASTE MAP", 54, "square");
        public static readonly GuiButton SettingsButton = new(1190, 525, 600, 100, "SETTINGS", 54, "square");

        public static readonly GuiButton AutosavedButton = new(1190, 640, 600, 100, "AUTOSAVED MAP", 54, "square");
        public static readonly GuiButton LastMapButton = new(1190, 755, 600, 100, "EDIT LAST MAP", 54, "square");

        public static readonly GuiSlider ChangelogSlider = new(950, 230, 20, 720, Settings.changelogPosition) { Style = ControlStyle.Slider_Uncolored };
        public static readonly GuiSquare ChangelogBackdrop1 = new(35, 180, 950, 790, Color.FromArgb(40, 0, 0, 0));
        public static readonly GuiSquare ChangelogBackdrop2 = new(55, 230, 900, 715, Color.FromArgb(50, 0, 0, 0));

        public static readonly GuiSquare MapSelectBackdrop = new(0, 1040, 1920, 40, Color.FromArgb(50, 0, 0, 0));
        public static readonly GuiButton NavLeft = new(0, 1040, 40, 40, "<", 34);
        public static readonly GuiButton NavRight = new(1880, 1040, 40, 40, ">", 34);

        public static readonly GuiButton MapSelect0 = new(40, 1040, 368, 40, "", 20);
        public static readonly GuiButton MapSelect1 = new(408, 1040, 368, 40, "", 20);
        public static readonly GuiButton MapSelect2 = new(776, 1040, 368, 40, "", 20);
        public static readonly GuiButton MapSelect3 = new(1144, 1040, 368, 40, "", 20);
        public static readonly GuiButton MapSelect4 = new(1512, 1040, 368, 40, "", 20);

        public static readonly GuiButton MapClose0 = new(40, 1040, 40, 40, "X", 26);
        public static readonly GuiButton MapClose1 = new(408, 1040, 40, 40, "X", 26);
        public static readonly GuiButton MapClose2 = new(776, 1040, 40, 40, "X", 26);
        public static readonly GuiButton MapClose3 = new(1144, 1040, 40, 40, "X", 26);
        public static readonly GuiButton MapClose4 = new(1512, 1040, 40, 40, "X", 26);

        public static readonly GuiSquareTextured BackgroundSquare = new("menubg", Path.Combine(Assets.THIS, "background_menu.png"), Color.FromArgb(30, 30, 30));

        private static readonly List<(GuiButton, GuiButton)> mapSelects =
        [
            (MapSelect0, MapClose0),
            (MapSelect1, MapClose1),
            (MapSelect2, MapClose2),
            (MapSelect3, MapClose3),
            (MapSelect4, MapClose4)
        ];

        private static string changelogCache = "";
        private static List<string> changelogLines = [];

        private static int mapIndex = 0;
        private static readonly List<string> mapNames =
        [
            "",
            "",
            "",
            "",
            ""
        ];

        public GuiWindowMenu() : base(BackgroundSquare, ChangelogBackdrop1, ChangelogBackdrop2, MapSelectBackdrop,
            CreateButton, LoadButton, ImportButton, SettingsButton, AutosavedButton, LastMapButton, NavLeft, NavRight, FeedbackButton,
            MapSelect0, MapSelect1, MapSelect2, MapSelect3, MapSelect4, MapClose0, MapClose1, MapClose2, MapClose3, MapClose4,
            ChangelogSlider, ChangelogLabel, SoundSpaceLabel, QuantumEditorLabel, Changelog)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(changelogCache))
                    changelogCache = Networking.DownloadString(Links.CHANGELOG);
            }
            catch (Exception ex)
            {
                Logging.Log("Failed to load changelog!", LogSeverity.WARN, ex);
                changelogCache = $"Failed to load changelog!\n\n{ex}";
            }

            AssembleChangelog();
            AssembleMapList();

            Settings.changelogPosition.Value.Value = 0;
        }

        public override void Resize(ResizeEventArgs e)
        {
            base.Resize(e);

            AutosavedButton.Visible = Settings.autosavedFile.Value != "";
            LastMapButton.Visible = File.Exists(Settings.lastFile.Value);

            RectangleF rect = LastMapButton.GetRect();
            LastMapButton.SetRect(rect.X, AutosavedButton.Visible ? rect.Y : AutosavedButton.GetRect().Y, rect.Width, rect.Height);
            LastMapButton.Update();

            Settings.changelogPosition.Value.Value = 0;
            AssembleChangelog();
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            for (int i = 0; i < mapSelects.Count; i++)
            {
                GuiButton select = mapSelects[i].Item1;
                GuiButton close = mapSelects[i].Item2;

                close.Visible = select.Visible && select.Hovering;
                select.SetText(close.Visible ? "Open Map" : mapNames[i]);
            }

            base.Render(mousex, mousey, frametime);
        }

        public override void ConnectEvents()
        {
            ChangelogSlider.ValueChanged += UpdateChangelogText;
            UpdateChangelogText(null, new(Settings.changelogPosition.Value.Value));

            FeedbackButton.LeftClick += (s, e) => PlatformUtils.OpenLink(Links.FEEDBACK_FORM);

            CreateButton.LeftClick += (s, e) => Windowing.SwitchWindow(new GuiWindowCreate());
            SettingsButton.LeftClick += (s, e) => Windowing.SwitchWindow(new GuiWindowSettings());

            LoadButton.LeftClick += (s, e) =>
            {
                DialogResult result = new OpenFileDialog()
                {
                    Title = "Select Map File",
                    Filter = "Map Files (*.txt;*.sspm;*.osu;*.nch;*.npk;*.phxm;*.phz;*.json)|*.txt;*.sspm;*.osu;*.nch;*.npk;*.phxm;*.phz;*.json"
                }.Show(Settings.defaultPath, out string file);

                if (result == DialogResult.OK)
                    Mapping.Load(file);
            };

            ImportButton.LeftClick += (s, e) =>
            {
                string clipboard = Clipboard.GetText();

                if (!string.IsNullOrWhiteSpace(clipboard))
                    Mapping.Load(clipboard);
            };

            string autosaveINI = Path.Combine(Assets.TEMP, "tempautosave.ini");

            AutosavedButton.LeftClick += (s, e) =>
            {
                Mapping.Load(Settings.autosavedFile.Value);
                File.WriteAllText(autosaveINI, Settings.autosavedProperties.Value);
                INI.Read(autosaveINI);
            };

            LastMapButton.LeftClick += (s, e) => Mapping.Load(Settings.lastFile.Value);

            NavLeft.LeftClick += (s, e) => ScrollMapList(false);
            NavRight.LeftClick += (s, e) => ScrollMapList(true);

            void Open(int index)
            {
                if (mapIndex + index < 0 || mapIndex + index >= Mapping.Cache.Count)
                    return;
                Mapping.Open(Mapping.Cache[mapIndex + index]);
            }

            MapSelect0.LeftClick += (s, e) => Open(0);
            MapSelect1.LeftClick += (s, e) => Open(1);
            MapSelect2.LeftClick += (s, e) => Open(2);
            MapSelect3.LeftClick += (s, e) => Open(3);
            MapSelect4.LeftClick += (s, e) => Open(4);

            void Close(int index)
            {

                if (mapIndex + index < 0 || mapIndex + index >= Mapping.Cache.Count)
                    return;
                Mapping.Close(Mapping.Cache[mapIndex + index]);
                AssembleMapList();
            }

            MapClose0.LeftClick += (s, e) => Close(0);
            MapClose1.LeftClick += (s, e) => Close(1);
            MapClose2.LeftClick += (s, e) => Close(2);
            MapClose3.LeftClick += (s, e) => Close(3);
            MapClose4.LeftClick += (s, e) => Close(4);

            ChangelogSlider.ValueChanged += (s, e) => AssembleChangelog();
        }

        private void UpdateChangelogText(object? sender, ValueChangedEventArgs e)
        {
            SliderSetting setting = Settings.changelogPosition.Value;
            List<string> lines = [];

            for (int i = 0; i < changelogLines.Count; i++)
            {
                if (i >= setting.Value && i < setting.Value + Changelog.GetRect().Height / Changelog.TextSize / Settings.fontScale.Value / (FontRenderer.Unicode ? StbFont.UnicodeMult : 1) - 1)
                    lines.Add(changelogLines[i]);
            }

            Changelog.SetText(string.Join('\n', lines));
        }

        private void AssembleChangelog()
        {
            float width = Changelog.GetRect().Width;
            float height = Changelog.GetRect().Height;

            List<string> lines = [];

            foreach (string line in changelogCache.Split('\n'))
            {
                string newLine = line;

                while (FontRenderer.GetWidth(newLine, Changelog.TextSize, "main") > width && newLine.Contains(' '))
                {
                    int index = newLine.LastIndexOf(' ');

                    if (FontRenderer.GetWidth(newLine[..index], Changelog.TextSize, "main") <= width)
                        newLine = newLine.Remove(index, 1).Insert(index, "\n");
                    else
                        newLine = newLine.Remove(index, 1).Insert(index, "\\");
                }

                newLine = newLine.Replace('\\', ' ');

                foreach (string subLine in newLine.Split('\n'))
                    lines.Add(subLine);
            }

            SliderSetting setting = Settings.changelogPosition.Value;

            setting.Max = lines.Count - (int)(height / Changelog.TextSize / Settings.fontScale.Value);
            ChangelogSlider.Visible = setting.Max > 0;

            changelogLines = lines;
            UpdateChangelogText(null, new(Settings.changelogPosition.Value.Value));
        }

        private void AssembleMapList()
        {
            mapIndex = Math.Clamp(mapIndex, 0, Math.Max(Mapping.Cache.Count - mapSelects.Count, 0));

            NavLeft.Visible = mapIndex > 0;
            NavRight.Visible = mapIndex < Mapping.Cache.Count - mapSelects.Count;

            for (int i = 0; i < mapSelects.Count; i++)
            {
                GuiButton select = mapSelects[i].Item1;
                select.Visible = i + mapIndex < Mapping.Cache.Count;

                if (select.Visible)
                {
                    Map map = Mapping.Cache[i + mapIndex];
                    string fileId = (!map.IsSaved ? "[!] " : "") + map.FileID;

                    select.SetText(FontRenderer.TrimText(fileId, select.TextSize, select.Font, (int)select.GetRect().Width - 10));
                    mapNames[i] = select.Text;
                }
            }
        }

        private void ScrollMapList(bool right)
        {
            if (right)
                mapIndex++;
            else
                mapIndex--;

            AssembleMapList();
        }

        public override void MouseScroll(float delta)
        {
            base.MouseScroll(delta);

            bool hovering = NavLeft.Hovering || NavRight.Hovering;
            foreach ((GuiButton, GuiButton) nav in mapSelects)
                hovering |= nav.Item1.Hovering || nav.Item2.Hovering;

            if (hovering)
                ScrollMapList(delta > 0);
            else
            {
                int pos = (int)(Settings.changelogPosition.Value.Value + (delta < 0 ? 1 : -1));
                Settings.changelogPosition.Value.Value = Math.Clamp(pos, 0, Settings.changelogPosition.Value.Max);

                AssembleChangelog();
                ChangelogSlider.Update();
            }
        }
    }
}
