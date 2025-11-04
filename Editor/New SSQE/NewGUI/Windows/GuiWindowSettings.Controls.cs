using New_SSQE.ExternalUtils;
using New_SSQE.Misc.Static;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.CompoundControls;
using New_SSQE.NewGUI.Controls;
using New_SSQE.Preferences;
using System.Drawing;

namespace New_SSQE.NewGUI.Windows
{
    internal partial class GuiWindowSettings
    {
        public static readonly GuiButton BackButton = new(660, 930, 600, 100)
        {
            Text = "SAVE AND RETURN",
            TextSize = 54
        };
        public static readonly GuiButton ResetButton = new(700, 865, 500, 50)
        {
            Text = "RESET TO DEFAULT",
            TextSize = 30
        };
        public static readonly GuiButton OpenDirectoryButton = new(700, 810, 500, 50)
        {
            Text = "OPEN EDITOR FOLDER",
            TextSize = 30
        };
        public static readonly GuiButton KeybindsButton = new(700, 755, 500, 50)
        {
            Text = "CHANGE KEYBINDS",
            TextSize = 30
        };

        public static readonly GuiButton NavColors = new(0, 0, 200, 50)
        {
            Text = "COLORS",
            TextSize = 30
        };
        public static readonly GuiButton NavGraphics = new(0, 100, 200, 50)
        {
            Text = "GRAPHICS",
            TextSize = 30
        };
        public static readonly GuiButton NavMapping = new(0, 200, 200, 50)
        {
            Text = "MAPPING",
            TextSize = 30
        };
        public static readonly GuiButton NavPlayer = new(0, 300, 200, 50)
        {
            Text = "PLAYER",
            TextSize = 30
        };
        public static readonly GuiButton NavAudio = new(0, 400, 200, 50)
        {
            Text = "AUDIO",
            TextSize = 30
        };
        public static readonly GuiButton NavOther = new(0, 500, 200, 50)
        {
            Text = "OTHER",
            TextSize = 30
        };

        public static readonly RadioButtonController NavController = new(0, NavColors, NavGraphics, NavMapping, NavPlayer, NavAudio, NavOther);
        public static readonly ControlContainer SettingNavs = new(180, 80, 200, 550, NavColors, NavGraphics, NavMapping, NavPlayer, NavAudio, NavOther);



        public static readonly GuiButton Color1Picker = new(0, 0, 200, 50)
        {
            Text = "PICK COLOR",
            TextSize = 30
        };
        public static readonly GuiLabel Color1Label = new(0, -30, 200, 26)
        {
            Text = "Color 1 (1/X BPM + Primary):",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiSquare Color1Square = new(210, 0, 75, 50)
        {
            Color = Settings.color1.Value
        };

        public static readonly GuiButton Color2Picker = new(0, 100, 200, 50)
        {
            Text = "PICK COLOR",
            TextSize = 30
        };
        public static readonly GuiLabel Color2Label = new(0, 70, 200, 26)
        {
            Text = "Color 2 (1/1 BPM + Secondary):",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiSquare Color2Square = new(210, 100, 75, 50)
        {
            Color = Settings.color2.Value
        };

        public static readonly GuiButton Color3Picker = new(0, 200, 200, 50)
        {
            Text = "PICK COLOR",
            TextSize = 30
        };
        public static readonly GuiLabel Color3Label = new(0, 170, 200, 26)
        {
            Text = "Color 3 (1/2 BPM + Preview):",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiSquare Color3Square = new(210, 200, 75, 50)
        {
            Color = Settings.color3.Value
        };

        public static readonly GuiButton Color4Picker = new(0, 300, 200, 50)
        {
            Text = "PICK COLOR",
            TextSize = 30
        };
        public static readonly GuiLabel Color4Label = new(0, 270, 200, 26)
        {
            Text = "Color 4 (Waveform):",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiSquare Color4Square = new(210, 300, 75, 50)
        {
            Color = Settings.color4.Value
        };

        public static readonly GuiButton Color5Picker = new(0, 400, 200, 50)
        {
            Text = "PICK COLOR",
            TextSize = 30
        };
        public static readonly GuiLabel Color5Label = new(0, 370, 200, 26)
        {
            Text = "Color 5 (Special Objects):",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiSquare Color5Square = new(210, 400, 75, 50)
        {
            Color = Settings.color5.Value
        };

        public static readonly GuiButton NoteColorPicker = new(0, 500, 200, 50)
        {
            Text = "ADD COLOR",
            TextSize = 30
        };
        public static readonly GuiLabel NoteColorLabel = new(0, 470, 200, 26)
        {
            Text = "Note Colors:",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiLabel NoteColorInfo = new(0, 555, 195, 26)
        {
            Text = "LMB: Remove\nRMB: Move left",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiSquare NoteColorHoverSquare = new(0, 0, 0, 0)
        {
            Color = Color.FromArgb(0, 127, 255),
            Outline = true
        };
        public static readonly ControlContainer NoteColorSquares = new(210, 500, 75, 50);

        public static readonly GuiNumberBox EditorBGOpacityTextbox = new(650, 0, 200, 50)
        {
            Increment = 5,
            Setting = Settings.editorBGOpacity,
            IsPositive = true,
            IsFloat = false,
            Text = "255",
            TextSize = 34,
            Style = ControlStyle.Textbox_Uncolored,
            Bounds = (0, 255)
        };
        public static readonly GuiLabel EditorBGOpacityLabel = new(650, -30, 200, 26)
        {
            Text = "Editor Background Opacity:",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiSquare EditorBGOpacitySquare = new(860, 0, 75, 50);

        public static readonly GuiNumberBox GridOpacityTextbox = new(650, 100, 200, 50)
        {
            Increment = 5,
            Setting = Settings.gridOpacity,
            IsPositive = true,
            IsFloat = false,
            Text = "255",
            TextSize = 34,
            Style = ControlStyle.Textbox_Uncolored,
            Bounds = (0, 255)
        };
        public static readonly GuiLabel GridOpacityLabel = new(650, 70, 200, 26)
        {
            Text = "Grid Opacity:",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiSquare GridOpacitySquare = new(860, 100, 75, 50);

        public static readonly GuiNumberBox TrackOpacityTextbox = new(650, 200, 200, 50)
        {
            Increment = 5,
            Setting = Settings.trackOpacity,
            IsPositive = true,
            IsFloat = false,
            Text = "255",
            TextSize = 34,
            Style = ControlStyle.Textbox_Uncolored,
            Bounds = (0, 255)
        };
        public static readonly GuiLabel TrackOpacityLabel = new(650, 170, 200, 26)
        {
            Text = "Track Opacity:",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiSquare TrackOpacitySquare = new(860, 200, 75, 50);

        public static readonly ControlContainer ColorsNav = new(500, 80, 1220, 700, Color1Picker, Color1Label, Color1Square, Color2Picker, Color2Label, Color2Square,
            Color3Picker, Color3Label, Color3Square, Color4Picker, Color4Label, Color4Square, Color5Picker, Color5Label, Color5Square,
            NoteColorPicker, NoteColorLabel, NoteColorInfo, NoteColorHoverSquare, NoteColorSquares,
            EditorBGOpacityTextbox, EditorBGOpacityLabel, EditorBGOpacitySquare, GridOpacityTextbox, GridOpacityLabel, GridOpacitySquare,
            TrackOpacityTextbox, TrackOpacityLabel, TrackOpacitySquare);



        public static readonly GuiCheckbox WaveformCheckbox = new(0, 0, 45, 45)
        {
            Setting = Settings.waveform,
            Text = "Enable Waveform",
            TextSize = 34,
            Style = ControlStyle.Checkbox_Uncolored
        };
        public static readonly GuiCheckbox ClassicWaveformCheckbox = new(0, 60, 45, 45)
        {
            Setting = Settings.classicWaveform,
            Text = "Use Classic Waveform",
            TextSize = 34,
            Style = ControlStyle.Checkbox_Uncolored
        };
        public static readonly GuiNumberBox WaveformDetailTextbox = new(0, 150, 200, 50)
        {
            Increment = 1,
            Setting = Settings.waveformDetail,
            IsPositive = true,
            IsFloat = false,
            Text = "5",
            TextSize = 30,
            Style = ControlStyle.Textbox_Uncolored,
            Bounds = (1, 250)
        };
        public static readonly GuiLabel WaveformDetailLabel = new(0, 120, 200, 26)
        {
            Text = "Waveform Level of Detail:",
            TextSize = 30,
            CenterMode = CenterMode.None
        };

        public static readonly GuiCheckbox UseVSyncCheckbox = new(0, 300, 45, 45)
        {
            Setting = Settings.useVSync,
            Text = "Enable VSync",
            TextSize = 34,
            Style = ControlStyle.Checkbox_Uncolored
        };
        public static readonly GuiCheckbox LimitPlayerFPSCheckbox = new(0, 360, 45, 45)
        {
            Setting = Settings.limitPlayerFPS,
            Text = "Limit Player FPS",
            TextSize = 34,
            Style = ControlStyle.Checkbox_Uncolored
        };
        public static readonly GuiSlider FPSLimitSlider = new(0, 410, 400, 55, Settings.fpsLimit);
        public static readonly GuiLabel FPSLimitLabel = new(0, 460, 400, 55)
        {
            Text = "FPS Limit:",
            TextSize = 34,
            CenterMode = CenterMode.None
        };
        public static readonly GuiCheckbox LowerFPSInBackgroundCheckbox = new(0, 510, 45, 45)
        {
            Setting = Settings.lowerBackgroundFPS,
            Text = "Lower FPS in Background",
            TextSize = 34,
            Style = ControlStyle.Checkbox_Uncolored
        };

        public static readonly GuiCheckbox MSAACheckbox = new(700, 0, 45, 45)
        {
            Setting = Settings.msaa,
            Text = "Use Anti-Aliasing",
            TextSize = 34,
            Style = ControlStyle.Checkbox_Uncolored
        };
        public static readonly GuiLabel RestartLabel = new(700, 50, 200, 26)
        {
            Text = "Requires restart!",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiLabel FontScaleLabel = new(700, 120, 200, 26)
        {
            Text = "Font Scale:",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiNumberBox FontScaleTextbox = new(700, 150, 200, 50)
        {
            Increment = 0.05f,
            Setting = Settings.fontScale,
            IsPositive = true,
            IsFloat = true,
            Text = "1",
            TextSize = 30,
            Style = ControlStyle.Textbox_Uncolored,
            Bounds = (0.25f, 2.5f)
        };

        public static readonly GuiCheckbox GridSquircles = new(700, 300, 45, 45)
        {
            Setting = Settings.gridSquircles,
            Text = "Grid Squircles",
            TextSize = 34,
            Style = ControlStyle.Checkbox_Uncolored
        };
        public static readonly GuiNumberBox GridSquircleDetailTextbox = new(700, 390, 200, 50)
        {
            Increment = 1,
            Setting = Settings.gridSquircleDetail,
            IsPositive = true,
            IsFloat = false,
            Text = "8",
            TextSize = 30,
            Style = ControlStyle.Textbox_Uncolored,
            Bounds = (1, 16)
        };
        public static readonly GuiLabel GridSquircleDetailLabel = new(700, 360, 200, 26)
        {
            Text = "Grid Squircle Corner Detail:",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiNumberBox GridSquircleRadiusTextbox = new(700, 485, 200, 50)
        {
            Increment = 0.025f,
            Setting = Settings.gridSquircleRadius,
            IsPositive = true,
            IsFloat = true,
            Text = "0.125",
            TextSize = 30,
            Style = ControlStyle.Textbox_Uncolored,
            Bounds = (0, 0.5f)
        };
        public static readonly GuiLabel GridSquircleRadiusLabel = new(700, 455, 200, 26)
        {
            Text = "Grid Squircle Corner Radius:",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiSquare GridSquirclePreview = new(955, 395, 40, 40)
        {
            Color = Color.White,
            Outline = true,
            Stretch = StretchMode.None,
            CornerDetail = (int)Settings.gridSquircleDetail.Value,
            CornerRadius = Settings.gridSquircleRadius.Value,
        };

        public static readonly ControlContainer GraphicsNav = new(500, 80, 1220, 700, WaveformCheckbox, ClassicWaveformCheckbox, WaveformDetailTextbox, WaveformDetailLabel,
            UseVSyncCheckbox, LimitPlayerFPSCheckbox, FPSLimitSlider, FPSLimitLabel, LowerFPSInBackgroundCheckbox, MSAACheckbox, RestartLabel, FontScaleLabel, FontScaleTextbox,
            GridSquircles, GridSquircleDetailTextbox, GridSquircleDetailLabel, GridSquircleRadiusTextbox, GridSquircleRadiusLabel, GridSquirclePreview);

        

        public static readonly GuiCheckbox AutosaveCheckbox = new(0, 0, 45, 45)
        {
            Setting = Settings.enableAutosave,
            Text = "Enable Autosave",
            TextSize = 34,
            Style = ControlStyle.Checkbox_Uncolored
        };
        public static readonly GuiNumberBox AutosaveIntervalTextbox = new(0, 90, 200, 50)
        {
            Increment = 1,
            Setting = Settings.autosaveInterval,
            IsPositive = true,
            IsFloat = true,
            Text = "5",
            TextSize = 34,
            Style = ControlStyle.Textbox_Uncolored,
            Bounds = (0.1f, 60)
        };
        public static readonly GuiLabel AutosaveIntervalLabel = new(0, 60, 200, 26)
        {
            Text = "Autosave Interval (min):",
            TextSize = 30,
            CenterMode = CenterMode.None
        };

        public static readonly GuiCheckbox CorrectOnCopyCheckbox = new(0, 215, 45, 45)
        {
            Setting = Settings.correctOnCopy,
            Text = "Correct Errors on Copy",
            TextSize = 34,
            Style = ControlStyle.Checkbox_Uncolored
        };
        public static readonly GuiCheckbox ReverseScrollCheckbox = new(0, 275, 45, 45)
        {
            Setting = Settings.reverseScroll,
            Text = "Reverse Scroll Direction",
            TextSize = 34,
            Style = ControlStyle.Checkbox_Uncolored
        };

        public static readonly ControlContainer MappingNav = new(500, 80, 1220, 700, AutosaveCheckbox, AutosaveIntervalTextbox, AutosaveIntervalLabel,
            CorrectOnCopyCheckbox, ReverseScrollCheckbox);



        public static readonly GuiCheckbox NavRhythia = new(0, 0, 45, 45)
        {
            Text = "Rhythia",
            TextSize = 34,
            Style = ControlStyle.Checkbox_Uncolored
        };
        public static readonly GuiCheckbox NavNova = new(0, 60, 45, 45)
        {
            Text = "Novastra",
            TextSize = 34,
            Style = ControlStyle.Checkbox_Uncolored
        };
        public static readonly GuiCheckbox NavSSQEPlayer = new(0, 120, 45, 45)
        {
            Text = "SSQE Player",
            TextSize = 34,
            Style = ControlStyle.Checkbox_Uncolored
        };

        public static readonly GuiPathBox RhythiaPathBox = new(300, 0, 750, 50)
        {
            Filter = PlatformUtils.ExecutableFilter,
            Folder = Settings.rhythiaFolderPath,
            Setting = Settings.rhythiaPath,
            Text = "RHYTHIA PATH",
            TextSize = 30
        };
        public static readonly GuiPathBox NovaPathBox = new(300, 60, 750, 50)
        {
            Filter = PlatformUtils.ExecutableFilter,
            Folder = Settings.novaFolderPath,
            Setting = Settings.novaPath,
            Text = "NOVASTRA PATH",
            TextSize = 30
        };
        public static readonly GuiCheckbox FullscreenPlayerCheckbox = new(0, 180, 45, 45)
        {
            Setting = Settings.fullscreenPlayer,
            Text = "Open SSQE Player in Fullscreen",
            TextSize = 34,
            Style = ControlStyle.Checkbox_Uncolored
        };

        public static readonly RadioCheckboxController PlaytestGameController = new(Settings.playtestGame, NavRhythia, NavNova, NavSSQEPlayer);
        public static readonly ControlContainer PlayerNav = new(500, 80, 1220, 700, NavRhythia, NavNova, NavSSQEPlayer, RhythiaPathBox, NovaPathBox, FullscreenPlayerCheckbox);



        public static readonly GuiCheckbox MonoCheckbox = new(0, 0, 45, 45)
        {
            Setting = Settings.monoAudio,
            Text = "Mono Audio",
            TextSize = 34,
            Style = ControlStyle.Checkbox_Uncolored
        };
        public static readonly GuiNumberBox PolyphonyTextbox = new(0, 90, 200, 50)
        {
            Increment = 1,
            Setting = Settings.maxPolyphony,
            IsPositive = true,
            IsFloat = false,
            Text = "1",
            TextSize = 34,
            Style = ControlStyle.Textbox_Uncolored,
            Bounds = (1, 16)
        };
        public static readonly GuiLabel PolyphonyLabel = new(0, 60, 200, 26)
        {
            Text = "Maximum Polyphony:",
            TextSize = 30,
            CenterMode = CenterMode.None
        };

        public static readonly ControlContainer AudioNav = new(500, 80, 1220, 700, MonoCheckbox, PolyphonyTextbox, PolyphonyLabel);



        public static readonly GuiCheckbox SkipDownloadCheckbox = new(0, 0, 45, 45)
        {
            Setting = Settings.skipDownload,
            Text = "Skip Download From Roblox",
            TextSize = 34,
            Style = ControlStyle.Checkbox_Uncolored
        };
        public static readonly GuiCheckbox CheckForUpdatesCheckbox = new(0, 60, 45, 45)
        {
            Setting = Settings.checkUpdates,
            Text = "Check For Updates",
            TextSize = 34,
            Style = ControlStyle.Checkbox_Uncolored
        };

        public static readonly ControlContainer OtherNav = new(500, 80, 1220, 700, SkipDownloadCheckbox, CheckForUpdatesCheckbox);



        public static readonly GuiSquareTextured BackgroundSquare = new("menubg", Path.Combine(Assets.THIS, "background_menu.png"))
        {
            Color = Color.FromArgb(30, 30, 30)
        };
    }
}
