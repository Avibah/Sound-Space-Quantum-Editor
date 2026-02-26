using New_SSQE.Misc;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;
using New_SSQE.NewGUI.Dialogs;
using New_SSQE.NewGUI.Font;
using New_SSQE.NewMaps;
using New_SSQE.NewMaps.Parsing;
using New_SSQE.Preferences;
using New_SSQE.Services;
using OpenTK.Windowing.Common;
using System.Drawing;

namespace New_SSQE.NewGUI.Windows
{
    internal partial class GuiWindowMenu : GuiWindow
    {
        private static string changelogCache = "";

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
            ChangelogPanel, ChangelogLabel, SoundSpaceLabel, QuantumEditorLabel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(changelogCache))
                {
                    changelogCache = "Loading...";

                    Task.Run(() =>
                    {
                        changelogCache = Network.DownloadString(Links.CHANGELOG);
                        AssembleChangelog();
                    });
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Failed to load changelog!", LogSeverity.WARN, ex);
                changelogCache = $"Failed to load changelog!\n\n{ex}";
            }

            AssembleChangelog();
            AssembleMapList();
        }

        private static void FixButtonsChangelog()
        {
            AutosavedButton.Visible = Settings.autosavedFile.Value != "";
            LastMapButton.Visible = File.Exists(Settings.lastFile.Value);

            RectangleF rect = LastMapButton.Rect;
            LastMapButton.SetRect(rect.X, AutosavedButton.Visible ? rect.Y : AutosavedButton.Rect.Y, rect.Width, rect.Height);
            LastMapButton.Update();

            AssembleChangelog();
        }

        public override void Open()
        {
            base.Open();
            FixButtonsChangelog();
        }

        public override void Resize(ResizeEventArgs e)
        {
            base.Resize(e);
            FixButtonsChangelog();
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            for (int i = 0; i < mapSelects.Count; i++)
            {
                GuiButton select = mapSelects[i].Item1;
                GuiButton close = mapSelects[i].Item2;

                close.Visible = select.Visible && select.Hovering;
                select.Text = close.Visible ? "Open Map" : mapNames[i];
            }

            base.Render(mousex, mousey, frametime);
        }

        public override void ConnectEvents()
        {
            FeedbackButton.LeftClick += (s, e) => Platforms.OpenLink(Links.FEEDBACK_FORM);

            CreateButton.LeftClick += (s, e) => Windowing.Open<GuiWindowCreate>();
            SettingsButton.LeftClick += (s, e) => Windowing.Open<GuiWindowSettings>();

            LoadButton.LeftClick += (s, e) =>
            {
                DialogResult result = new OpenFileDialog()
                {
                    Title = "Select Map File",
                    Filter = "Map Files (*.txt;*.qem;*.qemz;*.sspm;*.osu;*.nch;*.npk;*.phxm;*.rhym;*.phz;*.json)|*.txt;*.qem;*.qemz;*.sspm;*.osu;*.nch;*.npk;*.phxm;*.rhym;*.phz;*.json"
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

            string autosaveINI = Assets.TempAt("tempautosave.ini");

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
                Mapping.Current = Mapping.Cache[mapIndex + index];
                Mapping.Open();
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
        }

        private static void AssembleChangelog()
        {
            Changelog.Text = changelogCache;
            ChangelogPanel.Refresh();
        }

        private static void AssembleMapList()
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

                    select.Text = FontRenderer.TrimText(fileId, select.TextSize, select.Font, (int)select.Rect.Width - 10);
                    mapNames[i] = select.Text;
                }
            }
        }

        private static void ScrollMapList(bool right)
        {
            if (right)
                mapIndex++;
            else
                mapIndex--;

            AssembleMapList();
        }

        public override void MouseScroll(float x, float y, float delta)
        {
            base.MouseScroll(x, y, delta);

            bool hovering = NavLeft.Hovering || NavRight.Hovering;
            foreach ((GuiButton, GuiButton) nav in mapSelects)
                hovering |= nav.Item1.Hovering || nav.Item2.Hovering;

            if (hovering)
                ScrollMapList(delta > 0);
        }
    }
}
