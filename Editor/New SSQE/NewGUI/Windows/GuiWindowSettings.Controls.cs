using New_SSQE.ExternalUtils;
using New_SSQE.Misc.Static;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;
using New_SSQE.Preferences;
using System.Drawing;

namespace New_SSQE.NewGUI.Windows
{
    internal partial class GuiWindowSettings
    {
        public static readonly GuiButton BackButton = new(660, 930, 600, 100, "SAVE AND RETURN", 54, "main");
        public static readonly GuiButton ResetButton = new(700, 865, 500, 50, "RESET TO DEFAULT", 30, "main");
        public static readonly GuiButton OpenDirectoryButton = new(700, 810, 500, 50, "OPEN EDITOR FOLDER", 30, "main");
        public static readonly GuiButton KeybindsButton = new(700, 755, 500, 50, "CHANGE KEYBINDS", 30, "main");

        public static readonly GuiButton NavColors = new(0, 0, 200, 50, "COLORS", 30);
        public static readonly GuiButton NavGraphics = new(0, 100, 200, 50, "GRAPHICS", 30);
        public static readonly GuiButton NavMapping = new(0, 200, 200, 50, "MAPPING", 30);
        public static readonly GuiButton NavPlayer = new(0, 300, 200, 50, "PLAYER", 30);
        public static readonly GuiButton NavOther = new(0, 400, 200, 50, "OTHER", 30);

        public static readonly RadioButtonController NavController = new(0, NavColors, NavGraphics, NavMapping, NavPlayer, NavOther);
        public static readonly ControlContainer SettingNavs = new(180, 80, 200, 550, NavColors, NavGraphics, NavMapping, NavPlayer, NavOther);



        public static readonly GuiButton Color1Picker = new(0, 0, 200, 50, "PICK COLOR", 30, "main");
        public static readonly GuiLabel Color1Label = new(0, -30, 200, 26, null, "Color 1 (1/X BPM + Primary):", 30, "main", CenterMode.None);
        public static readonly GuiSquare Color1Square = new(210, 0, 75, 50, Settings.color1.Value);

        public static readonly GuiButton Color2Picker = new(0, 100, 200, 50, "PICK COLOR", 30, "main");
        public static readonly GuiLabel Color2Label = new(0, 70, 200, 26, null, "Color 2 (1/1 BPM + Secondary):", 30, "main", CenterMode.None);
        public static readonly GuiSquare Color2Square = new(210, 100, 75, 50, Settings.color2.Value);

        public static readonly GuiButton Color3Picker = new(0, 200, 200, 50, "PICK COLOR", 30, "main");
        public static readonly GuiLabel Color3Label = new(0, 170, 200, 26, null, "Color 3 (1/2 BPM + Preview):", 30, "main", CenterMode.None);
        public static readonly GuiSquare Color3Square = new(210, 200, 75, 50, Settings.color3.Value);

        public static readonly GuiButton Color4Picker = new(0, 300, 200, 50, "PICK COLOR", 30, "main");
        public static readonly GuiLabel Color4Label = new(0, 270, 200, 26, null, "Color 4 (Waveform):", 30, "main", CenterMode.None);
        public static readonly GuiSquare Color4Square = new(210, 300, 75, 50, Settings.color4.Value);

        public static readonly GuiButton Color5Picker = new(0, 400, 200, 50, "PICK COLOR", 30, "main");
        public static readonly GuiLabel Color5Label = new(0, 370, 200, 26, null, "Color 5 (Special Objects):", 30, "main", CenterMode.None);
        public static readonly GuiSquare Color5Square = new(210, 400, 75, 50, Settings.color5.Value);

        public static readonly GuiButton NoteColorPicker = new(0, 500, 200, 50, "ADD COLOR", 30, "main");
        public static readonly GuiLabel NoteColorLabel = new(0, 470, 200, 26, null, "Note Colors:", 30, "main", CenterMode.None);
        public static readonly GuiLabel NoteColorInfo = new(0, 555, 195, 26, null, "LMB: Remove\nRMB: Move left", 30, "main", CenterMode.None);
        public static readonly GuiSquare NoteColorHoverSquare = new(0, 0, 0, 0, Color.FromArgb(0, 127, 255), true);
        public static readonly ControlContainer NoteColorSquares = new(210, 500, 75, 50);

        public static readonly GuiNumberBox EditorBGOpacityTextbox = new(650, 0, 200, 50, 5, Settings.editorBGOpacity, false, true, "255", 34) { Style = ControlStyle.Textbox_Uncolored, Bounds = (0, 255) };
        public static readonly GuiLabel EditorBGOpacityLabel = new(650, -30, 200, 26, null, "Editor Background Opacity:", 30, "main", CenterMode.None);
        public static readonly GuiSquare EditorBGOpacitySquare = new(860, 0, 75, 50);

        public static readonly GuiNumberBox GridOpacityTextbox = new(650, 100, 200, 50, 5, Settings.gridOpacity, false, true, "255", 34) { Style = ControlStyle.Textbox_Uncolored, Bounds = (0, 255) };
        public static readonly GuiLabel GridOpacityLabel = new(650, 70, 200, 26, null, "Grid Opacity:", 30, "main", CenterMode.None);
        public static readonly GuiSquare GridOpacitySquare = new(860, 100, 75, 50);

        public static readonly GuiNumberBox TrackOpacityTextbox = new(650, 200, 200, 50, 5, Settings.trackOpacity, false, true, "255", 34) { Style = ControlStyle.Textbox_Uncolored, Bounds = (0, 255) };
        public static readonly GuiLabel TrackOpacityLabel = new(650, 170, 200, 26, null, "Track Opacity:", 30, "main", CenterMode.None);
        public static readonly GuiSquare TrackOpacitySquare = new(860, 200, 75, 50);

        public static readonly ControlContainer ColorsNav = new(500, 80, 1220, 700, Color1Picker, Color1Label, Color1Square, Color2Picker, Color2Label, Color2Square,
            Color3Picker, Color3Label, Color3Square, Color4Picker, Color4Label, Color4Square, Color5Picker, Color5Label, Color5Square,
            NoteColorPicker, NoteColorLabel, NoteColorInfo, NoteColorHoverSquare, NoteColorSquares,
            EditorBGOpacityTextbox, EditorBGOpacityLabel, EditorBGOpacitySquare, GridOpacityTextbox, GridOpacityLabel, GridOpacitySquare,
            TrackOpacityTextbox, TrackOpacityLabel, TrackOpacitySquare);



        public static readonly GuiCheckbox WaveformCheckbox = new(0, 0, 45, 45, Settings.waveform, "Enable Waveform", 34) { Style = ControlStyle.Checkbox_Uncolored };
        public static readonly GuiCheckbox ClassicWaveformCheckbox = new(0, 60, 45, 45, Settings.classicWaveform, "Use Classic Waveform", 34) { Style = ControlStyle.Checkbox_Uncolored };
        public static readonly GuiNumberBox WaveformDetailTextbox = new(0, 150, 200, 50, 1, Settings.waveformDetail, false, true, "5", 30) { Style = ControlStyle.Textbox_Uncolored, Bounds = (1, 250) };
        public static readonly GuiLabel WaveformDetailLabel = new(0, 120, 200, 26, null, "Waveform Level of Detail:", 30, "main", CenterMode.None);

        public static readonly GuiCheckbox UseVSyncCheckbox = new(0, 300, 45, 45, Settings.useVSync, "Enable VSync", 34) { Style = ControlStyle.Checkbox_Uncolored };
        public static readonly GuiCheckbox LimitPlayerFPSCheckbox = new(0, 360, 45, 45, Settings.limitPlayerFPS, "Limit Player FPS", 34) { Style = ControlStyle.Checkbox_Uncolored };
        public static readonly GuiSlider FPSLimitSlider = new(0, 410, 400, 55, Settings.fpsLimit);
        public static readonly GuiLabel FPSLimitLabel = new(0, 460, 400, 55, null, "FPS Limit:", 34, "main", CenterMode.None);
        public static readonly GuiCheckbox LowerFPSInBackgroundCheckbox = new(0, 510, 45, 45, Settings.lowerBackgroundFPS, "Lower FPS in Background", 34) { Style = ControlStyle.Checkbox_Uncolored };

        public static readonly GuiCheckbox MSAACheckbox = new(700, 0, 45, 45, Settings.msaa, "Use Anti-Aliasing", 34) { Style = ControlStyle.Checkbox_Uncolored };
        public static readonly GuiLabel RestartLabel = new(700, 50, 200, 26, null, "Requires restart!", 30, "main", CenterMode.None);
        public static readonly GuiLabel FontScaleLabel = new(700, 120, 200, 26, null, "Font Scale:", 30, "main", CenterMode.None);
        public static readonly GuiNumberBox FontScaleTextbox = new(700, 150, 200, 50, 0.05f, Settings.fontScale, true, true, "1", 30) { Style = ControlStyle.Textbox_Uncolored, Bounds = (0.25f, 2.5f) };

        public static readonly GuiCheckbox GridSquircles = new(700, 300, 45, 45, Settings.gridSquircles, "Grid Squircles", 34) { Style = ControlStyle.Checkbox_Uncolored };
        public static readonly GuiNumberBox GridSquircleDetailTextbox = new(700, 390, 200, 50, 1, Settings.gridSquircleDetail, false, true, "8", 30) { Style = ControlStyle.Textbox_Uncolored, Bounds = (1, 16) };
        public static readonly GuiLabel GridSquircleDetailLabel = new(700, 360, 200, 26, null, "Grid Squircle Corner Detail:", 30, "main", CenterMode.None);
        public static readonly GuiNumberBox GridSquircleRadiusTextbox = new(700, 485, 200, 50, 0.025f, Settings.gridSquircleRadius, true, true, "0.125", 30) { Style = ControlStyle.Textbox_Uncolored, Bounds = (0, 0.5f) };
        public static readonly GuiLabel GridSquircleRadiusLabel = new(700, 455, 200, 26, null, "Grid Squircle Corner Radius:", 30, "main", CenterMode.None);
        public static readonly GuiSquare GridSquirclePreview = new(955, 395, 40, 40, Color.White, true)
        {
            Stretch = StretchMode.None,
            Rounded = true,
            CornerDetail = (int)Settings.gridSquircleDetail.Value,
            CornerRadius = Settings.gridSquircleRadius.Value
        };

        public static readonly ControlContainer GraphicsNav = new(500, 80, 1220, 700, WaveformCheckbox, ClassicWaveformCheckbox, WaveformDetailTextbox, WaveformDetailLabel,
            UseVSyncCheckbox, LimitPlayerFPSCheckbox, FPSLimitSlider, FPSLimitLabel, LowerFPSInBackgroundCheckbox, MSAACheckbox, RestartLabel, FontScaleLabel, FontScaleTextbox,
            GridSquircles, GridSquircleDetailTextbox, GridSquircleDetailLabel, GridSquircleRadiusTextbox, GridSquircleRadiusLabel, GridSquirclePreview);



        public static readonly GuiCheckbox AutosaveCheckbox = new(0, 0, 45, 45, Settings.enableAutosave, "Enable Autosave", 34) { Style = ControlStyle.Checkbox_Uncolored };
        public static readonly GuiNumberBox AutosaveIntervalTextbox = new(0, 90, 200, 50, 1, Settings.autosaveInterval, true, true, "5", 34) { Style = ControlStyle.Textbox_Uncolored, Bounds = (0.1f, 60) };
        public static readonly GuiLabel AutosaveIntervalLabel = new(0, 60, 200, 26, null, "Autosave Interval (min):", 30, "main", CenterMode.None);

        public static readonly GuiCheckbox CorrectOnCopyCheckbox = new(0, 215, 45, 45, Settings.correctOnCopy, "Correct Errors on Copy", 34) { Style = ControlStyle.Checkbox_Uncolored };
        public static readonly GuiCheckbox ReverseScrollCheckbox = new(0, 275, 45, 45, Settings.reverseScroll, "Reverse Scroll Direction", 34) { Style = ControlStyle.Checkbox_Uncolored };

        public static readonly ControlContainer MappingNav = new(500, 80, 1220, 700, AutosaveCheckbox, AutosaveIntervalTextbox, AutosaveIntervalLabel,
            CorrectOnCopyCheckbox, ReverseScrollCheckbox);


        public static readonly GuiCheckbox NavRhythia = new(0, 0, 45, 45, null, "Rhythia", 34) { Style = ControlStyle.Checkbox_Uncolored };
        public static readonly GuiCheckbox NavNova = new(0, 60, 45, 45, null, "Novastra", 34) { Style = ControlStyle.Checkbox_Uncolored };
        public static readonly GuiCheckbox NavSSQEPlayer = new(0, 120, 45, 45, null, "SSQE Player", 34) { Style = ControlStyle.Checkbox_Uncolored };

        public static readonly GuiPathBox RhythiaPathBox = new(300, 0, 750, 50, PlatformUtils.ExecutableFilter, Settings.rhythiaFolderPath, Settings.rhythiaPath, "RHYTHIA PATH", 30);
        public static readonly GuiPathBox NovaPathBox = new(300, 60, 750, 50, PlatformUtils.ExecutableFilter, Settings.novaFolderPath, Settings.novaPath, "NOVASTRA PATH", 30);
        public static readonly GuiCheckbox FullscreenPlayerCheckbox = new(0, 180, 45, 45, Settings.fullscreenPlayer, "Open SSQE Player in Fullscreen", 34) { Style = ControlStyle.Checkbox_Uncolored };

        public static readonly RadioCheckboxController PlaytestGameController = new(Settings.playtestGame, NavRhythia, NavNova, NavSSQEPlayer);
        public static readonly ControlContainer PlayerNav = new(500, 80, 1220, 700, NavRhythia, NavNova, NavSSQEPlayer, RhythiaPathBox, NovaPathBox, FullscreenPlayerCheckbox);



        public static readonly GuiCheckbox SkipDownloadCheckbox = new(0, 0, 45, 45, Settings.skipDownload, "Skip Download From Roblox", 34) { Style = ControlStyle.Checkbox_Uncolored };
        public static readonly GuiCheckbox CheckForUpdatesCheckbox = new(0, 60, 45, 45, Settings.checkUpdates, "Check For Updates", 34) { Style = ControlStyle.Checkbox_Uncolored };
        public static readonly GuiCheckbox MonoCheckbox = new(0, 120, 45, 45, Settings.monoAudio, "Mono Audio", 34) { Style = ControlStyle.Checkbox_Uncolored };

        public static readonly ControlContainer OtherNav = new(500, 80, 1220, 700, SkipDownloadCheckbox, CheckForUpdatesCheckbox, MonoCheckbox);



        public static readonly GuiSquareTextured BackgroundSquare = new("menubg", Path.Combine(Assets.THIS, "background_menu.png"), Color.FromArgb(30, 30, 30));
    }
}
