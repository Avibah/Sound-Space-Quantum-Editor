using New_SSQE.NewGUI.Controls;
using New_SSQE.NewGUI.Base;
using New_SSQE.Preferences;
using System.Drawing;
using New_SSQE.ExternalUtils;
using New_SSQE.Misc.Dialogs;
using New_SSQE.Misc.Static;
using New_SSQE.Audio;

namespace New_SSQE.NewGUI.Windows
{
    internal class GuiWindowSettings : GuiWindow
    {
        public static readonly GuiButton BackButton = new(660, 930, 600, 100, "SAVE AND RETURN", 54, "square");
        public static readonly GuiButton ResetButton = new(700, 865, 500, 50, "RESET TO DEFAULT", 30, "square");
        public static readonly GuiButton OpenDirectoryButton = new(700, 810, 500, 50, "OPEN EDITOR FOLDER", 30, "square");
        public static readonly GuiButton KeybindsButton = new(700, 755, 500, 50, "CHANGE KEYBINDS", 30, "square");

        public static readonly GuiButton NavColors = new(0, 0, 200, 50, "COLORS", 30);
        public static readonly GuiButton NavGraphics = new(0, 100, 200, 50, "GRAPHICS", 30);
        public static readonly GuiButton NavMapping = new(0, 200, 200, 50, "MAPPING", 30);
        public static readonly GuiButton NavOther = new(0, 300, 200, 50, "OTHER", 30);

        public static readonly RadioButtonController NavController = new(0, NavColors, NavGraphics, NavMapping, NavOther);
        public static readonly ControlContainer SettingNavs = new(180, 80, 200, 550, NavColors, NavGraphics, NavMapping, NavOther);



        public static readonly GuiButton Color1Picker = new(0, 0, 200, 50, "PICK COLOR", 30, "square");
        public static readonly GuiLabel Color1Label = new(0, -30, 200, 26, null, "Color 1 (1/X BPM + Primary):", 30, "main", CenterMode.None);
        public static readonly GuiSquare Color1Square = new(210, 0, 75, 50, Settings.color1.Value) { Stretch = StretchMode.X };

        public static readonly GuiButton Color2Picker = new(0, 100, 200, 50, "PICK COLOR", 30, "square");
        public static readonly GuiLabel Color2Label = new(0, 70, 200, 26, null, "Color 2 (1/1 BPM + Secondary):", 30, "main", CenterMode.None);
        public static readonly GuiSquare Color2Square = new(210, 100, 75, 50, Settings.color2.Value) { Stretch = StretchMode.X };

        public static readonly GuiButton Color3Picker = new(0, 200, 200, 50, "PICK COLOR", 30, "square");
        public static readonly GuiLabel Color3Label = new(0, 170, 200, 26, null, "Color 3 (1/2 BPM + Preview):", 30, "main", CenterMode.None);
        public static readonly GuiSquare Color3Square = new(210, 200, 75, 50, Settings.color3.Value) { Stretch = StretchMode.X };

        public static readonly GuiButton Color4Picker = new(0, 300, 200, 50, "PICK COLOR", 30, "square");
        public static readonly GuiLabel Color4Label = new(0, 270, 200, 26, null, "Color 4 (Waveform):", 30, "main", CenterMode.None);
        public static readonly GuiSquare Color4Square = new(210, 300, 75, 50, Settings.color4.Value) { Stretch = StretchMode.X };

        public static readonly GuiButton Color5Picker = new(0, 400, 200, 50, "PICK COLOR", 30, "square");
        public static readonly GuiLabel Color5Label = new(0, 370, 200, 26, null, "Color 5 (Special Objects):", 30, "main", CenterMode.None);
        public static readonly GuiSquare Color5Square = new(210, 400, 75, 50, Settings.color5.Value) { Stretch = StretchMode.X };

        public static readonly GuiButton NoteColorPicker = new(0, 500, 200, 50, "ADD COLOR", 30, "square");
        public static readonly GuiLabel NoteColorLabel = new(0, 470, 200, 26, null, "Note Colors:", 30, "main", CenterMode.None);
        public static readonly GuiLabel NoteColorInfo = new(0, 555, 195, 26, null, "LMB: Remove\nRMB: Move left", 30, "main", CenterMode.None);
        public static readonly GuiSquare NoteColorHoverSquare = new(0, 0, 0, 0, Color.FromArgb(0, 127, 255), true);
        public static readonly ControlContainer NoteColorSquares = new(210, 500, 75, 50) { Stretch = StretchMode.X };

        public static readonly GuiTextboxNumeric EditorBGOpacityTextbox = new(650, 0, 200, 50, Settings.editorBGOpacity, false, true, "255", 34) { Style = new(ControlStyles.Textbox_Uncolored), Bounds = (0, 255) };
        public static readonly GuiLabel EditorBGOpacityLabel = new(650, -30, 200, 26, null, "Editor Background Opacity:", 30, "main", CenterMode.None);
        public static readonly GuiSquare EditorBGOpacitySquare = new(860, 0, 75, 50) { Stretch = StretchMode.X };

        public static readonly GuiTextboxNumeric GridOpacityTextbox = new(650, 100, 200, 50, Settings.gridOpacity, false, true, "255", 34) { Style = new(ControlStyles.Textbox_Uncolored), Bounds = (0, 255) };
        public static readonly GuiLabel GridOpacityLabel = new(650, 70, 200, 26, null, "Grid Opacity:", 30, "main", CenterMode.None);
        public static readonly GuiSquare GridOpacitySquare = new(860, 100, 75, 50) { Stretch = StretchMode.X };

        public static readonly GuiTextboxNumeric TrackOpacityTextbox = new(650, 200, 200, 50, Settings.trackOpacity, false, true, "255", 34) { Style = new(ControlStyles.Textbox_Uncolored), Bounds = (0, 255) };
        public static readonly GuiLabel TrackOpacityLabel = new(650, 170, 200, 26, null, "Track Opacity:", 30, "main", CenterMode.None);
        public static readonly GuiSquare TrackOpacitySquare = new(860, 200, 75, 50) { Stretch = StretchMode.X };

        public static readonly ControlContainer ColorsNav = new(500, 80, 1220, 700, Color1Picker, Color1Label, Color1Square, Color2Picker, Color2Label, Color2Square,
            Color3Picker, Color3Label, Color3Square, Color4Picker, Color4Label, Color4Square, Color5Picker, Color5Label, Color5Square,
            NoteColorPicker, NoteColorLabel, NoteColorInfo, NoteColorHoverSquare, NoteColorSquares,
            EditorBGOpacityTextbox, EditorBGOpacityLabel, EditorBGOpacitySquare, GridOpacityTextbox, GridOpacityLabel, GridOpacitySquare,
            TrackOpacityTextbox, TrackOpacityLabel, TrackOpacitySquare);



        public static readonly GuiCheckbox WaveformCheckbox = new(0, 0, 45, 45, Settings.waveform, "Enable Waveform", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly GuiCheckbox ClassicWaveformCheckbox = new(0, 60, 45, 45, Settings.classicWaveform, "Use Classic Waveform", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly GuiTextboxNumeric WaveformDetailTextbox = new(0, 150, 200, 50, Settings.waveformDetail, false, true, "5", 30) { Style = new(ControlStyles.Textbox_Uncolored), Bounds = (1,250) };
        public static readonly GuiLabel WaveformDetailLabel = new(0, 120, 200, 26, null, "Waveform Level of Detail:", 30, "main", CenterMode.None);

        public static readonly GuiCheckbox UseVSyncCheckbox = new(0, 300, 45, 45, Settings.useVSync, "Enable VSync", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly GuiCheckbox LimitPlayerFPSCheckbox = new(0, 360, 45, 45, Settings.limitPlayerFPS, "Limit Player FPS", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly GuiSlider FPSLimitSlider = new(0, 410, 400, 55, Settings.fpsLimit);
        public static readonly GuiLabel FPSLimitLabel = new(0, 460, 400, 55, null, "FPS Limit:", 34, "main", CenterMode.None);
        public static readonly GuiCheckbox LowerFPSInBackgroundCheckbox = new(0, 510, 45, 45, Settings.lowerBackgroundFPS, "Lower FPS in Background", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };

        public static readonly GuiCheckbox MSAACheckbox = new(700, 0, 45, 45, Settings.msaa, "Use Anti-Aliasing", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly GuiLabel RestartLabel = new(700, 50, 200, 26, null, "Requires restart!", 30, "main", CenterMode.None);

        public static readonly ControlContainer GraphicsNav = new(500, 80, 1220, 700, WaveformCheckbox, ClassicWaveformCheckbox, WaveformDetailTextbox, WaveformDetailLabel,
            UseVSyncCheckbox, LimitPlayerFPSCheckbox, FPSLimitSlider, FPSLimitLabel, LowerFPSInBackgroundCheckbox, MSAACheckbox, RestartLabel);



        public static readonly GuiCheckbox AutosaveCheckbox = new(0, 0, 45, 45, Settings.enableAutosave, "Enable Autosave", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly GuiTextboxNumeric AutosaveIntervalTextbox = new(0, 90, 200, 50, Settings.autosaveInterval, true, true, "5", 34) { Style = new(ControlStyles.Textbox_Uncolored), Bounds = (0.1f, 60) };
        public static readonly GuiLabel AutosaveIntervalLabel = new(0, 60, 200, 26, null, "Autosave Interval (min):", 30, "main", CenterMode.None);

        public static readonly GuiCheckbox CorrectOnCopyCheckbox = new(0, 215, 45, 45, Settings.correctOnCopy, "Correct Errors on Copy", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly GuiCheckbox ReverseScrollCheckbox = new(0, 275, 45, 45, Settings.reverseScroll, "Reverse Scroll Direction", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };

        public static readonly GuiCheckbox FullscreenPlayerCheckbox = new(0, 395, 45, 45, Settings.fullscreenPlayer, "Open Player in Fullscreen", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly GuiCheckbox UseRhythiaCheckbox = new(0, 455, 45, 45, Settings.useRhythia, "Use Rhythia as Player", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly GuiPathBox RhythiaPathBox = new(0, 515, 750, 50, Platform.ExecutableFilter, Settings.rhythiaFolderPath, Settings.rhythiaPath, "RHYTHIA PATH", 30);

        public static readonly ControlContainer MappingNav = new(500, 80, 1220, 700, AutosaveCheckbox, AutosaveIntervalTextbox, AutosaveIntervalLabel,
            CorrectOnCopyCheckbox, ReverseScrollCheckbox, FullscreenPlayerCheckbox, UseRhythiaCheckbox, RhythiaPathBox);



        public static readonly GuiCheckbox SkipDownloadCheckbox = new(0, 0, 45, 45, Settings.skipDownload, "Skip Download From Roblox", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly GuiCheckbox CheckForUpdatesCheckbox = new(0, 60, 45, 45, Settings.checkUpdates, "Check For Updates", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };
        public static readonly GuiCheckbox MonoCheckbox = new(0, 120, 45, 45, Settings.monoAudio, "Mono Audio", 34) { Style = new(ControlStyles.Checkbox_Uncolored) };

        public static readonly ControlContainer OtherNav = new(500, 80, 1220, 700, SkipDownloadCheckbox, CheckForUpdatesCheckbox, MonoCheckbox);



        public static readonly GuiSquareTextured BackgroundSquare = new("menubg", Path.Combine(Assets.THIS, "background_menu.png"), Color.FromArgb(30, 30, 30)) { Stretch = StretchMode.XY };


        public GuiWindowSettings() : base(BackgroundSquare, SettingNavs, ColorsNav, GraphicsNav, MappingNav, OtherNav,
            BackButton, ResetButton, OpenDirectoryButton, KeybindsButton)
        {
            RefreshNoteColors();
        }

        private static void RefreshNoteColors()
        {
            List<Color> colors = Settings.noteColors.Value;
            GuiSquare[] squares = new GuiSquare[colors.Count];

            RectangleF rect = NoteColorSquares.GetOrigin();
            float colorWidth = rect.Width / colors.Count;

            for (int i = 0; i < colors.Count; i++)
                squares[i] = new(colorWidth * i, 0, colorWidth, rect.Height, colors[i]) { Stretch = StretchMode.XY };

            NoteColorSquares.SetControls(squares);
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
            EditorBGOpacitySquare.SetColor(Color.FromArgb(editorBGOpacity, 255, 255, 255));

            int gridOpacity = (int)Math.Clamp(Settings.gridOpacity.Value, 0, 255);
            GridOpacitySquare.SetColor(Color.FromArgb(gridOpacity, 255, 255, 255));

            int trackOpacity = (int)Math.Clamp(Settings.trackOpacity.Value, 0, 255);
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

            static void OpenDialog(Setting<Color> setting)
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

            MonoCheckbox.LeftClick += (s, e) => MusicPlayer.Reload();

            NavController.SelectionChanged += (s, e) =>
            {
                ColorsNav.Visible = e.Value == "COLORS";
                GraphicsNav.Visible = e.Value == "GRAPHICS";
                MappingNav.Visible = e.Value == "MAPPING";
                OtherNav.Visible = e.Value == "OTHER";
            };

            NavController.Initialize();
        }
    }
}
