using New_SSQE.NewGUI.Controls;
using New_SSQE.NewGUI.Base;
using New_SSQE.Preferences;
using System.Drawing;
using New_SSQE.ExternalUtils;
using New_SSQE.Misc.Dialogs;

namespace New_SSQE.NewGUI.Windows
{
    internal class GuiWindowSettings : GuiWindow
    {
        public static readonly GuiButton BackButton = new(655, 930, 600, 100, "SAVE AND RETURN", 54, "square");
        public static readonly GuiButton ResetButton = new(700, 865, 500, 50, "RESET TO DEFAULT", 30, "square");
        public static readonly GuiButton OpenDirectoryButton = new(700, 810, 500, 50, "OPEN EDITOR FOLDER", 30, "square");
        public static readonly GuiButton KeybindsButton = new(700, 755, 500, 50, "CHANGE KEYBINDS", 30, "square");



        public static readonly GuiButton Color1Picker = new(180, 80, 200, 50, "PICK COLOR", 30, "square");
        public static readonly GuiLabel Color1Label = new(180, 50, 200, 26, null, "Color 1 (1/X BPM + Primary):", 30, "main", CenterMode.None);
        public static readonly GuiSquare Color1Square = new(390, 80, 75, 50, Settings.color1.Value) { Stretch = StretchMode.X };

        public static readonly GuiButton Color2Picker = new(180, 180, 200, 50, "PICK COLOR", 30, "square");
        public static readonly GuiLabel Color2Label = new(180, 150, 200, 26, null, "Color 2 (1/1 BPM + Secondary):", 30, "main", CenterMode.None);
        public static readonly GuiSquare Color2Square = new(390, 180, 75, 50, Settings.color2.Value) { Stretch = StretchMode.X };

        public static readonly GuiButton Color3Picker = new(180, 280, 200, 50, "PICK COLOR", 30, "square");
        public static readonly GuiLabel Color3Label = new(180, 250, 200, 26, null, "Color 3 (1/2 BPM + Preview):", 30, "main", CenterMode.None);
        public static readonly GuiSquare Color3Square = new(390, 280, 75, 50, Settings.color3.Value) { Stretch = StretchMode.X };

        public static readonly GuiButton Color4Picker = new(180, 380, 200, 50, "PICK COLOR", 30, "square");
        public static readonly GuiLabel Color4Label = new(180, 350, 200, 26, null, "Color 4 (Waveform):", 30, "main", CenterMode.None);
        public static readonly GuiSquare Color4Square = new(390, 380, 75, 50, Settings.color4.Value) { Stretch = StretchMode.X };

        public static readonly GuiButton Color5Picker = new(180, 480, 200, 50, "PICK COLOR", 30, "square");
        public static readonly GuiLabel Color5Label = new(180, 450, 200, 26, null, "Color 5 (Special Objects):", 30, "main", CenterMode.None);
        public static readonly GuiSquare Color5Square = new(390, 480, 75, 50, Settings.color5.Value) { Stretch = StretchMode.X };

        public static readonly GuiButton NoteColorPicker = new(180, 580, 200, 50, "ADD COLOR", 30, "square");
        public static readonly GuiLabel NoteColorLabel = new(180, 550, 200, 26, null, "Note Colors:", 30, "main", CenterMode.None);
        public static readonly GuiLabel NoteColorInfo = new(185, 635, 195, 26, null, "LMB: Remove\nRMB: Move left", 30, "main", CenterMode.None);
        public static readonly GuiSquare NoteColorHoverSquare = new(0, 0, 0, 0, Color.FromArgb(255, 0, 127), true);
        public static readonly ControlContainer NoteColorSquares = new(390, 580, 75, 50) { Stretch = StretchMode.X };



        public static readonly GuiTextboxNumeric EditorBGOpacityTextbox = new(180, 750, 200, 50, Settings.editorBGOpacity, false, true, "255", 34) { Style = new(ControlStyles.Textbox_Uncolored) };
        public static readonly GuiLabel EditorBGOpacityLabel = new(180, 720, 200, 26, null, "Editor Background Opacity:", 30, "main", CenterMode.None);
        public static readonly GuiSquare EditorBGOpacitySquare = new(390, 750, 75, 50) { Stretch = StretchMode.X };

        public static readonly GuiTextboxNumeric GridOpacityTextbox = new(180, 850, 200, 50, Settings.gridOpacity, false, true, "255", 34) { Style = new(ControlStyles.Textbox_Uncolored) };
        public static readonly GuiLabel GridOpacityLabel = new(180, 820, 200, 26, null, "Grid Opacity:", 30, "main", CenterMode.None);
        public static readonly GuiSquare GridOpacitySquare = new(390, 850, 75, 50) { Stretch = StretchMode.X };

        public static readonly GuiTextboxNumeric TrackOpacityTextbox = new(180, 950, 200, 50, Settings.trackOpacity, false, true, "255", 34) { Style = new(ControlStyles.Textbox_Uncolored) };
        public static readonly GuiLabel TrackOpacityLabel = new(180, 920, 200, 26, null, "Track Opacity:", 30, "main", CenterMode.None);
        public static readonly GuiSquare TrackOpacitySquare = new(390, 950, 75, 50) { Stretch = StretchMode.X };



        public static readonly GuiCheckbox UseVSyncCheckbox = new(630, 380, 45, 45, Settings.useVSync, "Enable VSync", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly GuiCheckbox LimitPlayerFPSCheckbox = new(630, 440, 45, 45, Settings.limitPlayerFPS, "Limit Player FPS", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly GuiSlider FPSLimitSlider = new(630, 490, 400, 55, Settings.fpsLimit);
        public static readonly GuiLabel FPSLimitLabel = new(630, 540, 400, 55, null, "FPS Limit:", 34, "main", CenterMode.None);
        public static readonly GuiCheckbox LowerFPSInBackgroundCheckbox = new(630, 590, 45, 45, Settings.lowerBackgroundFPS, "Lower FPS in Background", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };

        public static readonly GuiCheckbox WaveformCheckbox = new(630, 80, 45, 45, Settings.waveform, "Enable Waveform", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly GuiCheckbox ClassicWaveformCheckbox = new(630, 140, 45, 45, Settings.classicWaveform, "Use Classic Waveform", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly GuiTextboxNumeric WaveformDetailTextbox = new(630, 230, 200, 50, Settings.waveformDetail, false, true, "5", 30) { Style = new(ControlStyles.Textbox_Uncolored) };
        public static readonly GuiLabel WaveformDetailLabel = new(630, 200, 200, 26, null, "Waveform Level of Detail:", 30, "main", CenterMode.None);

        public static readonly GuiCheckbox AutosaveCheckbox = new(1070, 140, 45, 45, Settings.enableAutosave, "Enable Autosave", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly GuiTextboxNumeric AutosaveIntervalTextbox = new(1070, 230, 200, 50, Settings.autosaveInterval, true, true, "5", 34) { Style = new(ControlStyles.Textbox_Uncolored) };
        public static readonly GuiLabel AutosaveIntervalLabel = new(1070, 200, 200, 26, null, "Autosave Interval (min):", 30, "main", CenterMode.None);

        public static readonly GuiCheckbox FullscreenPlayerCheckbox = new(1070, 380, 45, 45, Settings.fullscreenPlayer, "Open Player in Fullscreen", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly GuiCheckbox UseRhythiaCheckbox = new(1070, 440, 45, 45, Settings.useRhythia, "Use Rhythia as Player", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly GuiLabel RhythiaPathLabel = new(1070, 500, 200, 26, null, "", 30, "main", CenterMode.None);
        public static readonly GuiButton RhythiaPathButton = new(1070, 530, 200, 50, "CHANGE PATH", 30);

        public static readonly GuiCheckbox CheckForUpdatesCheckbox = new(1420, 80, 45, 45, Settings.checkUpdates, "Check For Updates", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly GuiCheckbox SkipDownloadCheckbox = new(1420, 140, 45, 45, Settings.skipDownload, "Skip Download From Roblox", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly GuiCheckbox CorrectOnCopyCheckbox = new(1420, 200, 45, 45, Settings.correctOnCopy, "Correct Errors on Copy", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly GuiCheckbox ReverseScrollCheckbox = new(1420, 260, 45, 45, Settings.reverseScroll, "Reverse Scroll Direction", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };



        public static readonly GuiCheckbox MSAACheckbox = new(1420, 650, 45, 45, Settings.msaa, "Use Anti-Aliasing", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly GuiLabel RestartLabel = new(1420, 700, 200, 26, null, "Requires restart!", 30, "main", CenterMode.None);

        public static readonly GuiSquareTextured BackgroundSquare = new("menubg", "background_menu.png", Color.FromArgb(30, 30, 30)) { Stretch = StretchMode.XY };



        public GuiWindowSettings() : base(BackgroundSquare, Color1Square, Color2Square, Color3Square, Color4Square, Color5Square, NoteColorSquares, NoteColorHoverSquare,
            EditorBGOpacitySquare, GridOpacitySquare, TrackOpacitySquare, BackButton, ResetButton, OpenDirectoryButton, KeybindsButton,
            Color1Picker, Color2Picker, Color3Picker, Color4Picker, Color5Picker, NoteColorPicker, RhythiaPathButton,
            WaveformCheckbox, ClassicWaveformCheckbox, AutosaveCheckbox, CheckForUpdatesCheckbox, SkipDownloadCheckbox, CorrectOnCopyCheckbox, ReverseScrollCheckbox,
            UseVSyncCheckbox, FullscreenPlayerCheckbox, LimitPlayerFPSCheckbox, UseRhythiaCheckbox, MSAACheckbox, LowerFPSInBackgroundCheckbox,
            FPSLimitSlider, EditorBGOpacityTextbox, GridOpacityTextbox, TrackOpacityTextbox, AutosaveIntervalTextbox, WaveformDetailTextbox,
            Color1Label, Color2Label, Color3Label, Color4Label, Color5Label, NoteColorLabel, NoteColorInfo,
            EditorBGOpacityLabel, GridOpacityLabel, TrackOpacityLabel, AutosaveIntervalLabel, WaveformDetailLabel,
            FPSLimitLabel, RhythiaPathLabel, RestartLabel)
        {
            RefreshNoteColors();
            RefreshRhythiaPath();
        }

        private void RefreshNoteColors()
        {
            List<Color> colors = Settings.noteColors.Value;
            GuiSquare[] squares = new GuiSquare[colors.Count];

            RectangleF rect = NoteColorSquares.GetOrigin();
            float colorWidth = rect.Width / colors.Count;

            for (int i = 0; i < colors.Count; i++)
                squares[i] = new(rect.X + colorWidth * i, rect.Y, colorWidth, rect.Height, colors[i]) { Stretch = StretchMode.XY };

            NoteColorSquares.SetControls(squares);
        }

        private void RefreshRhythiaPath()
        {
            string path = Settings.rhythiaPath.Value;
            int length = 15;

            int startLength = Math.Min(path.Length, length);
            int endLength = Math.Clamp(path.Length - length, 0, length);

            string start = path[..startLength];
            string end = path[(path.Length - endLength)..];
            string final = start;

            if (!string.IsNullOrWhiteSpace(end))
                final += $"{(path.Length > length * 2 ? "..." : "")}{end}";

            if (string.IsNullOrWhiteSpace(path))
                RhythiaPathLabel.SetText("Rhythia Path: NONE");
            else if (!File.Exists(path))
                RhythiaPathLabel.SetText("Rhythia Path: INVALID");
            else
                RhythiaPathLabel.SetText($"Rhythia Path: {final}");
        }

        private bool resetQuery = false;
        private long resetTime;

        private void RunResetQuery()
        {
            long time = DateTime.Now.Millisecond;
            resetTime = time;

            Task delay = Task.Delay(5000).ContinueWith(_ =>
            {
                if (resetTime == time)
                {
                    resetQuery = false;
                    ResetButton.SetText("RESET TO DEFAULT");
                }
            });
        }

        private int hoverIndex = -1;

        public override void Render(float mousex, float mousey, float frametime)
        {
            NoteColorHoverSquare.Visible = NoteColorSquares.Hovering;

            if (NoteColorSquares.Hovering)
            {
                List<Color> colors = Settings.noteColors.Value;
                RectangleF rect = NoteColorSquares.GetRect();

                float colorWidth = rect.Width / colors.Count;
                float x = (mousex - rect.X) / colorWidth;
                hoverIndex = (int)Math.Clamp(x, 0, colors.Count - 1);

                NoteColorHoverSquare.SetRect(rect.X + colorWidth * hoverIndex, rect.Y, colorWidth, rect.Height);
            }
            else
                hoverIndex = -1;

            Color1Square.SetColor(Settings.color1.Value);
            Color2Square.SetColor(Settings.color2.Value);
            Color3Square.SetColor(Settings.color3.Value);
            Color4Square.SetColor(Settings.color4.Value);
            Color5Square.SetColor(Settings.color5.Value);

            int editorBGOpacity = (int)Math.Clamp(Settings.editorBGOpacity.Value, 0, 255);
            EditorBGOpacityTextbox.SetText(editorBGOpacity.ToString());
            EditorBGOpacitySquare.SetColor(Color.FromArgb(editorBGOpacity, 255, 255, 255));

            int gridOpacity = (int)Math.Clamp(Settings.gridOpacity.Value, 0, 255);
            GridOpacityTextbox.SetText(gridOpacity.ToString());
            GridOpacitySquare.SetColor(Color.FromArgb(gridOpacity, 255, 255, 255));

            int trackOpacity = (int)Math.Clamp(Settings.trackOpacity.Value, 0, 255);
            TrackOpacityTextbox.SetText(trackOpacity.ToString());
            TrackOpacitySquare.SetColor(Color.FromArgb(trackOpacity, 255, 255, 255));

            SliderSetting fpsLimit = Settings.fpsLimit.Value;

            FPSLimitLabel.SetText($"FPS Limit: {(Math.Round(fpsLimit.Value) == Math.Round(fpsLimit.Max) ? "Unlimited" : Math.Round(fpsLimit.Value + 60))}");
            RestartLabel.Visible = MainWindow.MSAA != Settings.msaa.Value;

            base.Render(mousex, mousey, frametime);
        }

        public override void ConnectEvents()
        {
            BackButton.LeftClick += (s, e) =>
            {
                Settings.Save();
                Windowing.SwitchWindow(new GuiWindowMenu());
            };

            ResetButton.LeftClick += (s, e) =>
            {
                if (resetQuery)
                {
                    Settings.Reset();
                    Update();

                    WaveformDetailTextbox.Text = ((int)Settings.waveformDetail.Value).ToString();
                    AutosaveIntervalTextbox.Text = Settings.autosaveInterval.Value.ToString(Program.Culture);

                    resetQuery = false;
                    ResetButton.SetText("RESET TO DEFAULT");
                }
                else
                {
                    resetQuery = true;
                    ResetButton.SetText("ARE YOU SURE?");

                    RunResetQuery();
                }
            };

            OpenDirectoryButton.LeftClick += (s, e) => Platform.OpenDirectory();
            KeybindsButton.LeftClick += (s, e) => Windowing.SwitchWindow(new GuiWindowKeybinds());

            void OpenDialog(Setting<Color> setting)
            {
                ColorDialog dialog = new() { Color = setting.Value };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    setting.Value = dialog.Color;
                    Settings.RefreshColors();
                }
            }

            Color1Picker.LeftClick += (s, e) => OpenDialog(Settings.color1);
            Color2Picker.LeftClick += (s, e) => OpenDialog(Settings.color2);
            Color3Picker.LeftClick += (s, e) => OpenDialog(Settings.color3);
            Color4Picker.LeftClick += (s, e) => OpenDialog(Settings.color4);
            Color5Picker.LeftClick += (s, e) => OpenDialog(Settings.color5);

            NoteColorPicker.LeftClick += (s, e) =>
            {
                List<Color> noteColors = Settings.noteColors.Value;
                if (noteColors.Count >= 32)
                    return;

                ColorDialog dialog = new() { Color = Color.White };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    noteColors.Add(dialog.Color);
                    Settings.RefreshColors();
                    RefreshNoteColors();
                }
            };

            NoteColorSquares.LeftClick += (s, e) =>
            {
                List<Color> noteColors = Settings.noteColors.Value;

                if (hoverIndex >= 0 && noteColors.Count > 1)
                {
                    noteColors.RemoveAt(hoverIndex);
                    Settings.RefreshColors();
                    RefreshNoteColors();
                }
            };

            NoteColorSquares.RightClick += (s, e) =>
            {
                List<Color> noteColors = Settings.noteColors.Value;

                if (hoverIndex > 0 && noteColors.Count > 1)
                {
                    (noteColors[hoverIndex], noteColors[hoverIndex - 1]) = (noteColors[hoverIndex - 1], noteColors[hoverIndex]);
                    Settings.RefreshColors();
                    RefreshNoteColors();
                }
            };

            RhythiaPathButton.LeftClick += (s, e) =>
            {
                DialogResult result = new OpenFileDialog()
                {
                    Title = "Select Rhythia Executable",
                    Filter = Platform.ExecutableFilter
                }.RunWithSetting(Settings.rhythiaFolderPath, out string file);

                if (result == DialogResult.OK)
                {
                    Settings.rhythiaPath.Value = file;
                    RefreshRhythiaPath();
                }
            };
        }
    }
}
