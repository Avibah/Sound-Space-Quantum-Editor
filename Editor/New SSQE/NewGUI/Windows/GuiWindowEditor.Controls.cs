using New_SSQE.Misc.Static;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;
using New_SSQE.Preferences;
using System.Drawing;

namespace New_SSQE.NewGUI.Windows
{
    internal partial class GuiWindowEditor
    {
        public static readonly GuiButton LNavOptions = new(10, 140, 175, 50)
        {
            Text = "OPTIONS",
            TextSize = 31
        };
        public static readonly GuiButton LNavTiming = new(195, 140, 175, 50)
        {
            Text = "TIMING",
            TextSize = 31
        };
        public static readonly GuiButton LNavPatterns = new(380, 140, 175, 50)
        {
            Text = "PATTERNS",
            TextSize = 31
        };
        public static readonly GuiButton LNavPlayer = new(10, 946, 545, 50)
        {
            Text = "PLAYTEST",
            TextSize = 31
        };
        public static readonly RadioButtonController LNavController = new(1, LNavOptions, LNavTiming, LNavPatterns, LNavPlayer);

        public static readonly GuiButton RNavSnapping = new(1365, 140, 175, 50)
        {
            Text = "SNAPPING",
            TextSize = 31
        };
        public static readonly GuiButton RNavGraphics = new(1550, 140, 175, 50)
        {
            Text = "GRAPHICS",
            TextSize = 31
        };
        public static readonly GuiButton RNavExport = new(1735, 140, 175, 50)
        {
            Text = "EXPORT",
            TextSize = 31
        };
        public static readonly RadioButtonController RNavController = new(0, RNavSnapping, RNavGraphics, RNavExport);

        /*
         *************************************
         | BEGIN: Navs for standard maps:    |
         | Options, Timing, Patterns, Player |
         *************************************
         */

        // Options
        public static readonly GuiCheckbox Numpad = new(0, 10, 30, 30)
        {
            Setting = Settings.numpad,
            Text = "Use Numpad",
            TextSize = 26
        };
        public static readonly GuiCheckbox SeparateClickTools = new(0, 50, 30, 30)
        {
            Setting = Settings.separateClickTools,
            Text = "Separate Click Tools",
            TextSize = 26
        };
        public static readonly GuiButton SwapClickMode = new(0, 90, 200, 40)
        {
            Text = "Swap Click mode",
            TextSize = 26
        };
        public static readonly GuiCheckbox JumpOnPaste = new(0, 140, 30, 30)
        {
            Setting = Settings.jumpPaste,
            Text = "Jump on Paste",
            TextSize = 26
        };
        public static readonly GuiCheckbox PauseOnScroll = new(0, 180, 30, 30)
        {
            Setting = Settings.pauseScroll,
            Text = "Pase on Seek",
            TextSize = 26
        };
        public static readonly GuiButton EditMapVFX = new(0, 240, 200, 40)
        {
            Text = "Edit Map VFX",
            TextSize = 26,
            Visible = false
        };
        public static readonly GuiButton EditSpecial = new(0, 290, 200, 40)
        {
            Text = "Edit Extra Objects",
            TextSize = 26,
        };
        public static readonly ControlContainer OptionsNav = new(10, 190, 545, 756, Numpad, SeparateClickTools, SwapClickMode, JumpOnPaste, PauseOnScroll, EditMapVFX, EditSpecial);

        // Timing
        public static readonly GuiTextboxNumeric ExportOffset = new(0, 50, 130, 40)
        {
            Setting = Settings.exportOffset,
            Text = "0",
            TextSize = 31
        };
        public static readonly GuiTextboxNumeric SfxOffset = new(170, 50, 130, 40)
        {
            Setting = Settings.sfxOffset,
            Text = "0",
            TextSize = 31
        };
        public static readonly GuiTextboxNumeric MusicOffset = new(340, 50, 130, 40)
        {
            Setting = Settings.musicOffset,
            Text = "0",
            TextSize = 31
        };
        public static readonly GuiButton OpenTimings = new(0, 110, 210, 40)
        {
            Text = "OPEN BPM SETUP",
            TextSize = 27
        };
        public static readonly GuiButton ImportIni = new(0, 160, 210, 40)
        {
            Text = "IMPORT INI",
            TextSize = 27
        };
        public static readonly GuiCheckbox Metronome = new(0, 210, 30, 30)
        {
            Setting = Settings.metronome,
            Text = "Metronome",
            TextSize = 26
        };
        public static readonly GuiButton OpenBookmarks = new(0, 280, 210, 40)
        {
            Text = "EDIT BOOKMARKS",
            TextSize = 27
        };
        public static readonly GuiButton CopyBookmarks = new(0, 330, 210, 40)
        {
            Text = "COPY BOOKMARKS",
            TextSize = 27
        };
        public static readonly GuiButton PasteBookmarks = new(0, 380, 210, 40)
        {
            Text = "PASTE BOOKMARKS",
            TextSize = 27
        };

        public static readonly GuiLabel ExportOffsetLabel = new(0, 20, 100, 30)
        {
            ColorSetting = Settings.color1,
            Text = "Export Offset:",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiLabel SfxOffsetLabel = new(170, 20, 100, 30)
        {
            ColorSetting = Settings.color1,
            Text = "SFX Offset:",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiLabel MusicOffsetLabel = new(340, 20, 100, 30)
        {
            ColorSetting = Settings.color1,
            Text = "Music Offset:",
            TextSize = 30,
            CenterMode = CenterMode.None
        };

        public static readonly ControlContainer TimingNav = new(10, 190, 545, 756, ExportOffset, SfxOffset, MusicOffset, OpenTimings, ImportIni, Metronome,
            OpenBookmarks, CopyBookmarks, PasteBookmarks, ExportOffsetLabel, SfxOffsetLabel, MusicOffsetLabel);

        // Patterns
        public static readonly GuiButton HFlip = new(0, 20, 175, 40)
        {
            Text = "HORIZONTAL FLIP",
            TextSize = 27
        };
        public static readonly GuiButton VFlip = new(185, 20, 175, 40)
        {
            Text = "VERTICAL FLIP",
            TextSize = 27
        };
        public static readonly GuiNumberBox RotateBox = new(0, 110, 100, 40)
        {
            Increment = 5,
            IsFloat = true,
            Text = "90",
            TextSize = 31
        };
        public static readonly GuiButton RotateButton = new(110, 110, 100, 40)
        {
            Text = "ROTATE",
            TextSize = 27
        };
        public static readonly GuiNumberBox ScaleBox = new(0, 190, 100, 40)
        {
            Increment = 5,
            IsFloat = true,
            Text = "150",
            TextSize = 31
        };
        public static readonly GuiButton ScaleButton = new(110, 190, 100, 40)
        {
            Text = "SCALE",
            TextSize = 27
        };
        public static readonly GuiCheckbox ApplyOnPaste = new(0, 250, 30, 30)
        {
            Setting = Settings.applyOnPaste,
            Text = "Apply Rotate/Scale on Paste",
            TextSize = 27
        };
        public static readonly GuiCheckbox ClampSR = new(0, 290, 30, 30)
        {
            Setting = Settings.clampSR,
            Text = "Clamp Rotate/Scale in Bounds",
            TextSize = 27
        };
        public static readonly GuiCheckbox PasteReversed = new(0, 330, 30, 30)
        {
            Setting = Settings.pasteReversed,
            Text = "Paste Reversed",
            TextSize = 27
        };
        public static readonly GuiButton StoreNodes = new(0, 390, 175, 40)
        {
            Text = "STORE NODES",
            TextSize = 27
        };
        public static readonly GuiButton ClearNodes = new(185, 390, 175, 40)
        {
            Text = "CLEAR NODES",
            TextSize = 27
        };
        public static readonly GuiCheckbox CurveBezier = new(0, 450, 30, 30)
        {
            Setting = Settings.curveBezier,
            Text = "Curve Bezier",
            TextSize = 27
        };
        public static readonly GuiNumberBox BezierBox = new(0, 520, 100, 40)
        {
            Increment = 1,
            Setting = Settings.bezierDivisor,
            IsPositive = true,
            Text = "4",
            TextSize = 27,
            Bounds = (1, float.MaxValue)
        };
        public static readonly GuiButton BezierButton = new(110, 520, 100, 40)
        {
            Text = "DRAW",
            TextSize = 27
        };

        public static readonly GuiLabel RotateLabel = new(0, 80, 175, 30)
        {
            ColorSetting = Settings.color1,
            Text = "Rotate by Degrees:",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiLabel ScaleLabel = new(0, 160, 100, 30)
        {
            ColorSetting = Settings.color1,
            Text = "Scale by Percent:",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiLabel DrawBezierLabel = new(0, 490, 100, 30)
        {
            ColorSetting = Settings.color1,
            Text = "Draw Bezier with Divisor:",
            TextSize = 30,
            CenterMode = CenterMode.None
        };

        public static readonly ControlContainer PatternsNav = new(10, 190, 545, 756, HFlip, VFlip, RotateBox, RotateButton, ScaleBox, ScaleButton, ApplyOnPaste, ClampSR, PasteReversed,
            StoreNodes, ClearNodes, CurveBezier, BezierBox, BezierButton, RotateLabel, ScaleLabel, DrawBezierLabel);

        // Player
        public static readonly GuiButtonList CameraMode = new(0, 50, 150, 40, Settings.cameraMode)
        {
            TextSize = 27
        };
        public static readonly GuiTextboxNumeric NoteScale = new(175, 50, 100, 40)
        {
            Setting = Settings.noteScale,
            IsFloat = true,
            Text = "1",
            TextSize = 31
        };
        public static readonly GuiTextboxNumeric CursorScale = new(300, 50, 100, 40)
        {
            Setting = Settings.cursorScale,
            IsFloat = true,
            Text = "1",
            TextSize = 31
        };
        public static readonly GuiCheckbox LockCursor = new(0, 110, 30, 30)
        {
            Setting = Settings.lockCursor,
            Text = "Lock Cursor Within Grid",
            TextSize = 27
        };
        public static readonly GuiCheckbox GridGuides = new(0, 160, 30, 30)
        {
            Setting = Settings.gridGuides,
            Text = "Grid Guides",
            TextSize = 27
        };
        public static readonly GuiTextboxNumeric Sensitivity = new(0, 240, 115, 40)
        {
            Setting = Settings.sensitivity,
            IsFloat = true,
            IsPositive = true,
            Text = "1",
            TextSize = 31
        };
        public static readonly GuiTextboxNumeric Parallax = new(135, 240, 115, 40)
        {
            Setting = Settings.parallax,
            IsFloat = true,
            IsPositive = true,
            Text = "1",
            TextSize = 31
        };
        public static readonly GuiTextboxNumeric FieldOfView = new(270, 240, 115, 40)
        {
            Setting = Settings.fov,
            IsFloat = true,
            IsPositive = true,
            Text = "70",
            TextSize = 31
        };
        public static readonly GuiTextboxNumeric ApproachDistance = new(0, 325, 150, 40)
        {
            Setting = Settings.approachDistance,
            IsFloat = true,
            IsPositive = true,
            Text = "1",
            TextSize = 31
        };
        public static readonly GuiTextboxNumeric HitWindow = new(235, 325, 150, 40)
        {
            Setting = Settings.hitWindow,
            IsPositive = true,
            Text = "55",
            TextSize = 31
        };
        public static readonly GuiSlider PlayerApproachRate = new(0, 410, 400, 32, Settings.playerApproachRate)
        {
            Style = ControlStyle.None
        };
        public static readonly GuiCheckbox ApproachFade = new(0, 460, 30, 30)
        {
            Setting = Settings.approachFade,
            Text = "Approach Fade",
            TextSize = 27
        };
        public static readonly GuiButton FromStart = new(0, 520, 200, 40)
        {
            Text = "PLAY FROM START",
            TextSize = 27
        };
        public static readonly GuiButton PlayMap = new(210, 520, 200, 40)
        {
            Text = "PLAY HERE",
            TextSize = 27
        };

        public static readonly GuiLabel CameraModeLabel = new(0, 20, 100, 30)
        {
            ColorSetting = Settings.color1,
            Text = "Camera Mode:",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiLabel NoteScaleLabel = new(175, 20, 100, 30)
        {
            ColorSetting = Settings.color1,
            Text = "Note Size:",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiLabel CursorScaleLabel = new(300, 20, 100, 30)
        {
            ColorSetting = Settings.color1,
            Text = "Cursor Size:",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiLabel SensitivityLabel = new(0, 210, 100, 30)
        {
            ColorSetting = Settings.color1,
            Text = "Sensitivity:",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiLabel ParallaxLabel = new(135, 210, 100, 30)
        {
            ColorSetting = Settings.color1,
            Text = "Parallax:",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiLabel FieldOfViewLabel = new(270, 210, 100, 30)
        {
            ColorSetting = Settings.color1,
            Text = "FOV:",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiLabel ApproachDistanceLabel = new(0, 295, 100, 30)
        {
            ColorSetting = Settings.color1,
            Text = "Approach Distance:",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiLabel HitWindowLabel = new(235, 295, 100, 30)
        {
            ColorSetting = Settings.color1,
            Text = "Hit Window:",
            TextSize = 30,
            CenterMode = CenterMode.None
        };
        public static readonly GuiLabel PlayerApproachRateLabel = new(0, 380, 400, 32)
        {
            ColorSetting = Settings.color1,
            TextSize = 30
        };

        public static readonly ControlContainer PlayerNav = new(10, 190, 545, 836, CameraMode, NoteScale, CursorScale, LockCursor, GridGuides, Sensitivity, Parallax, FieldOfView,
            ApproachDistance, HitWindow, PlayerApproachRate, ApproachFade, FromStart, PlayMap, CameraModeLabel, NoteScaleLabel, CursorScaleLabel, SensitivityLabel,
            ParallaxLabel, FieldOfViewLabel, ApproachDistanceLabel, HitWindowLabel, PlayerApproachRateLabel);

        public static readonly ControlContainer StandardMapNavs = new(LNavOptions, LNavTiming, LNavPatterns, LNavPlayer,
            OptionsNav, TimingNav, PatternsNav, PlayerNav);

        /*
         *************************************
         | END: Navs for standard maps:      |
         | Options, Timing, Patterns, Player |
         *************************************
         */

        /*
         *************************************
         | BEGIN: Special map controls       |
         *************************************
         */

        public static readonly GuiButton SpecialNavBeat = new(0, 0, 150, 30)
        {
            Text = "Beats",
            TextSize = 26
        };
        public static readonly GuiButton SpecialNavMine = new(0, 40, 150, 30)
        {
            Text = "Mines",
            TextSize = 26
        };
        public static readonly GuiButton SpecialNavGlide = new(0, 80, 150, 30)
        {
            Text = "Glides",
            TextSize = 26
        };
        public static readonly GuiButton SpecialNavLyric = new(0, 120, 150, 30)
        {
            Text = "Lyrics",
            TextSize = 26
        };
        public static readonly GuiButton SpecialNavFever = new(0, 160, 150, 30)
        {
            Text = "Fevers",
            TextSize = 26
        };
        public static readonly GuiButton SpecialNavNotes = new(0, 700, 150, 30)
        {
            Text = "Modify Notes",
            TextSize = 26
        };
        public static readonly RadioButtonController SpecialNavController = new(null, SpecialNavBeat, SpecialNavMine, SpecialNavGlide, SpecialNavLyric, SpecialNavFever, SpecialNavNotes);
        public static readonly ControlContainer SpecialNavNova = new(10, 200, 545, 756, SpecialNavBeat, SpecialNavMine, SpecialNavGlide, SpecialNavLyric, SpecialNavFever, SpecialNavNotes);

        public static readonly GuiButtonList GameSwitch = new(10, 946, 545, 50, Settings.modchartGame)
        {
            Text = "GAME: ",
            TextSize = 31
        };
        public static readonly GuiButton SpecialNavExit = new(10, 140, 545, 50)
        {
            Text = "CLOSE EXTRA OBJECTS",
            TextSize = 31
        };
        public static readonly GuiLabel LyricPreview = new(0, 860, 1920, 56)
        {
            ColorSetting = Settings.color2,
            TextSize = 48
        };
        public static readonly GuiSquare FeverPreview = new()
        {
            Color = Color.FromArgb(23, 255, 0, 0)
        };

        public static readonly GuiTextbox LyricBox = new(0, 0, 370, 30)
        {
            TextSize = 26
        };
        public static readonly GuiCheckbox LyricFadeIn = new(0, 40, 30, 30)
        {
            Text = "Fade In",
            TextSize = 26
        };
        public static readonly GuiCheckbox LyricFadeOut = new(0, 80, 30, 30)
        {
            Text = "Fade Out",
            TextSize = 26
        };
        public static readonly GuiButton LyricCreate = new(0, 120, 370, 30)
        {
            TextSize = 26
        };
        public static readonly GuiLabel LyricInfo = new(0, 160, 370, 400)
        {
            Text = string.Join('\n',
                "Lyric usage:",
                "> \"-\" prefix: Replace previous line",
                "> \"-\" suffix: Apply next lyric as a syllable",
                "    (Without a space in between)",
                "> Press 'Enter' in the lyric textbox to quickly place a lyric"),
            TextSize = 20,
            CenterMode = CenterMode.None
        };
        public static readonly ControlContainer LyricNav = new(175, 200, 370, 756, LyricBox, LyricFadeIn, LyricFadeOut, LyricCreate, LyricInfo)
        {
            Visible = false
        };

        public static readonly GuiCheckbox NoteEnableEasing = new(0, 0, 30, 30)
        {
            Text = "Enable Easing",
            TextSize = 26
        };
        public static readonly GuiButtonList NoteEasingStyle = new(0, 40, 370, 30, Settings.modchartStyle)
        {
            Text = "Easing Style: ",
            TextSize = 26
        };
        public static readonly GuiButtonList NoteEasingDirection = new(0, 80, 370, 30, Settings.modchartDirection)
        {
            Text = "Easing Direction: ",
            TextSize = 26
        };
        public static readonly GuiButton NoteApplyModifiers = new(0, 160, 370, 30)
        {
            Text = "Apply Modifications",
            TextSize = 26
        };
        public static readonly ControlContainer NoteNav = new(175, 200, 370, 756, NoteEnableEasing, NoteEasingStyle, NoteEasingDirection, NoteApplyModifiers)
        {
            Visible = false
        };
        public static readonly ControlContainer SpecialMapNavs = new(FeverPreview, SpecialNavNova, GameSwitch, SpecialNavExit, LyricNav, LyricPreview, NoteNav)
        {
            Visible = false
        };

        /*
         *************************************
         | END: Special map controls         |
         *************************************
         */

        /*
         *************************************
         | BEGIN: Navs for all maps:         |
         | Snapping, Graphics, Export        |
         *************************************
         */

        // Snapping
        public static readonly GuiCheckbox Quantum = new(245, 10, 30, 30)
        {
            Setting = Settings.enableQuantum,
            Text = "Quantum",
            TextSize = 27
        };
        public static readonly GuiCheckbox QuantumGridSnap = new(245, 50, 30, 30)
        {
            Setting = Settings.quantumGridSnap,
            Text = "Snap to Grid",
            TextSize = 27
        };
        public static readonly GuiCheckbox AutoAdvance = new(245, 90, 30, 30)
        {
            Setting = Settings.autoAdvance,
            Text = "Auto-Advance",
            TextSize = 27
        };
        public static readonly GuiSlider BeatSnapDivisor = new(245, 170, 250, 32, Settings.beatDivisor)
        {
            Style = ControlStyle.None,
            ShiftIncrement = 0.5f
        };
        public static readonly GuiSlider QuantumSnapDivisor = new(245, 250, 250, 32, Settings.quantumSnapping)
        {
            Style = ControlStyle.None
        };

        public static readonly GuiLabel BeatDivisorLabel = new(245, 140, 250, 32)
        {
            ColorSetting = Settings.color1,
            TextSize = 30
        };
        public static readonly GuiLabel SnappingLabel = new(245, 220, 250, 32)
        {
            ColorSetting = Settings.color1,
            TextSize = 30
        };
        public static readonly ControlContainer SnappingNav = new(1365, 190, 545, 756, Quantum, QuantumGridSnap, AutoAdvance, BeatSnapDivisor, QuantumSnapDivisor, BeatDivisorLabel, SnappingLabel);

        // Graphics
        public static readonly GuiCheckbox Autoplay = new(245, 10, 30, 30)
        {
            Setting = Settings.autoplay,
            Text = "Autoplay",
            TextSize = 26
        };
        public static readonly GuiCheckbox SmoothAutoplay = new(245, 50, 30, 30)
        {
            Setting = Settings.smoothAutoplay,
            Text = "Smooth Autoplay Cursor",
            TextSize = 26
        };
        public static readonly GuiCheckbox ApproachSquares = new(245, 90, 30, 30)
        {
            Setting = Settings.approachSquares,
            Text = "Approach Squares",
            TextSize = 26
        };
        public static readonly GuiCheckbox GridNumbers = new(245, 130, 30, 30)
        {
            Setting = Settings.gridNumbers,
            Text = "Grid Numbers",
            TextSize = 26
        };
        public static readonly GuiCheckbox GridLetters = new(245, 170, 30, 30)
        {
            Setting = Settings.gridLetters,
            Text = "Grid Letters",
            TextSize = 26
        };
        public static readonly GuiCheckbox QuantumGridLines = new(245, 210, 30, 30)
        {
            Setting = Settings.quantumGridLines,
            Text = "Quantum Grid Lines",
            TextSize = 26
        };
        public static readonly GuiSlider ApproachRate = new(245, 290, 250, 32, Settings.approachRate)
        {
            Style = ControlStyle.None
        };
        public static readonly GuiSlider TrackCursorPos = new(245, 370, 250, 32, Settings.cursorPos)
        {
            Style = ControlStyle.None
        };

        public static readonly GuiLabel ApproachRateLabel = new(245, 260, 250, 32)
        {
            ColorSetting = Settings.color1,
            TextSize = 28
        };
        public static readonly GuiLabel CursorPosLabel = new(245, 340, 250, 32)
        {
            ColorSetting = Settings.color1,
            TextSize = 28
        };

        public static readonly ControlContainer GraphicsNav = new(1365, 190, 545, 756, Autoplay, ApproachSquares, GridNumbers, GridLetters, QuantumGridLines, ApproachRate, TrackCursorPos,
            ApproachRateLabel, CursorPosLabel, SmoothAutoplay);

        // Export
        public static readonly GuiButton SaveButton = new(275, 20, 100, 40)
        {
            Text = "SAVE",
            TextSize = 27
        };
        public static readonly GuiButton SaveAsButton = new(385, 20, 100, 40)
        {
            Text = "SAVE AS",
            TextSize = 27
        };
        public static readonly GuiButtonList ExportSwitch = new(255, 90, 250, 40, Settings.exportType)
        {
            TextSize = 27
        };
        public static readonly GuiButton ExportButton = new(275, 140, 210, 40)
        {
            Text = "EXPORT MAP",
            TextSize = 27
        };
        public static readonly GuiTextbox ReplaceIDBox = new(275, 230, 210, 40)
        {
            TextSize = 27
        };
        public static readonly GuiButton ReplaceID = new(275, 280, 210, 40)
        {
            Text = "REPLACE",
            TextSize = 27
        };
        public static readonly GuiButton ConvertAudio = new(275, 350, 210, 40)
        {
            Text = "CONVERT TO MP3",
            TextSize = 27
        };
        public static readonly GuiButton CalculateSR = new(0, 20, 210, 40)
        {
            Text = "CALCULATE SR/RP",
            TextSize = 27
        };

        public static readonly GuiLabel ReplaceIDLabel = new(275, 190, 210, 40)
        {
            ColorSetting = Settings.color1,
            Text = "Replace Audio ID",
            TextSize = 30
        };
        public static readonly GuiLabel SRLabel = new(10, 70, 190, 320)
        {
            ColorSetting = Settings.color1,
            TextSize = 28,
            CenterMode = CenterMode.None
        };

        public static readonly ControlContainer ExportNav = new(1365, 190, 545, 756, SaveButton, SaveAsButton, ExportSwitch, ExportButton, ReplaceIDBox, ReplaceID, ConvertAudio,
            CalculateSR, ReplaceIDLabel, SRLabel);
        public static readonly ControlContainer ConstantMapNavs = new(RNavSnapping, RNavGraphics, RNavExport, SnappingNav, GraphicsNav, ExportNav);

        /*
         *************************************
         | END: Navs for all maps:           |
         | Snapping, Graphics, Export        |
         *************************************
         */

        // Main
        public static readonly GuiButton CopyButton = new(810, 273, 300, 42)
        {
            Text = "COPY MAP DATA",
            TextSize = 27
        };
        public static readonly GuiButton BackButton = new(810, 774, 300, 42)
        {
            Text = "BACK TO MENU",
            TextSize = 27
        };

        public static readonly GuiSlider Tempo = new(1408, 1016, 512, 64, Settings.tempo)
        {
            Style = ControlStyle.None
        };
        public static readonly GuiSliderTimeline Timeline = new(0, 1016, 1344, 64)
        {
            Style = ControlStyle.None
        };
        public static readonly GuiSlider MusicVolume = new(1856, 760, 40, 256, Settings.masterVolume)
        {
            Reverse = true,
            Style = ControlStyle.None
        };
        public static readonly GuiButtonToggle MusicMute = new(1868, 750, 16, 16, Settings.muteMusic, new("MuteUnmute"));
        public static readonly GuiSlider SfxVolume = new(1792, 760, 40, 256, Settings.sfxVolume)
        {
            Reverse = true,
            Style = ControlStyle.None
        };
        public static readonly GuiButtonToggle SfxMute = new(1804, 750, 16, 16, Settings.muteSfx, new("MuteUnmute"));
        public static readonly GuiButtonPlayPause PlayPause = new(1344, 1016, 64, 64);

        public static readonly GuiLabelToast ToastLabel = new(0, 1000, 1920, 64)
        {
            TextSize = 42
        };
        public static readonly GuiLabel ZoomLabel = new(565, 140, 80, 30)
        {
            ColorSetting = Settings.color1,
            Text = "Zoom:",
            TextSize = 32,
            CenterMode = CenterMode.None
        };
        public static readonly GuiLabel ZoomValueLabel = new(640, 140, 80, 30)
        {
            ColorSetting = Settings.color2,
            TextSize = 32,
            CenterMode = CenterMode.None
        };
        public static readonly GuiLabel ClickModeLabel = new(810, 816, 300, 42)
        {
            ColorSetting = Settings.color1,
            TextSize = 30
        };

        public static readonly GuiLabel TempoLabel = new(1408, 1048, 512, 30)
        {
            ColorSetting = Settings.color1,
            TextSize = 30
        };
        public static readonly GuiLabel MusicVolumeLabel = new(1856, 720, 40, 30)
        {
            ColorSetting = Settings.color1,
            Text = "Music",
            TextSize = 24
        };
        public static readonly GuiLabel MusicVolumeValueLabel = new(1856, 996, 40, 30)
        {
            ColorSetting = Settings.color1,
            TextSize = 24
        };
        public static readonly GuiLabel SfxVolumeLabel = new(1792, 720, 40, 30)
        {
            ColorSetting = Settings.color1,
            Text = "SFX",
            TextSize = 24
        };
        public static readonly GuiLabel SfxVolumeValueLabel = new(1792, 996, 40, 30)
        {
            ColorSetting = Settings.color1,
            TextSize = 24
        };

        public static readonly GuiLabel CurrentTimeLabel = new(0, 1048, 64, 32)
        {
            ColorSetting = Settings.color1,
            TextSize = 26
        };
        public static readonly GuiLabel CurrentMsLabel = new(0, 1012, 32, 32)
        {
            ColorSetting = Settings.color1,
            TextSize = 26
        };
        public static readonly GuiLabel TotalTimeLabel = new(1280, 1048, 64, 32)
        {
            ColorSetting = Settings.color1,
            TextSize = 26
        };
        public static readonly GuiLabel NotesLabel = new(0, 1048, 1344, 32)
        {
            ColorSetting = Settings.color1,
            TextSize = 30
        };

        public static readonly GuiGrid Grid = new(810, 390, 300, 300)
        {
            Stretch = StretchMode.None
        };
        public static readonly GuiTrack Track = new(0, 0, 1920, 86);

        public static readonly GuiSquareTextured BackgroundSquare = new("editorbg", Path.Combine(Assets.THIS, "background_editor.png"));
    }
}
