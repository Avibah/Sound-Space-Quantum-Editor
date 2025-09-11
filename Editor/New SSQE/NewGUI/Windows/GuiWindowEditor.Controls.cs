using New_SSQE.Misc.Static;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;
using New_SSQE.Preferences;
using System.Drawing;

namespace New_SSQE.NewGUI.Windows
{
    internal partial class GuiWindowEditor
    {
        public static readonly GuiButton LNavOptions = new(10, 140, 175, 50, "OPTIONS", 31);
        public static readonly GuiButton LNavTiming = new(195, 140, 175, 50, "TIMING", 31);
        public static readonly GuiButton LNavPatterns = new(380, 140, 175, 50, "PATTERNS", 31);
        public static readonly GuiButton LNavPlayer = new(10, 946, 545, 50, "PLAYTEST", 31);
        public static readonly RadioButtonController LNavController = new(1, LNavOptions, LNavTiming, LNavPatterns, LNavPlayer);

        public static readonly GuiButton RNavSnapping = new(1365, 140, 175, 50, "SNAPPING", 31);
        public static readonly GuiButton RNavGraphics = new(1550, 140, 175, 50, "GRAPHICS", 31);
        public static readonly GuiButton RNavExport = new(1735, 140, 175, 50, "EXPORT", 31);
        public static readonly RadioButtonController RNavController = new(0, RNavSnapping, RNavGraphics, RNavExport);

        /*
         *************************************
         | BEGIN: Navs for standard maps:    |
         | Options, Timing, Patterns, Player |
         *************************************
         */

        // Options
        public static readonly GuiCheckbox Numpad = new(0, 10, 30, 30, Settings.numpad, "Use Numpad", 26);
        public static readonly GuiCheckbox SeparateClickTools = new(0, 50, 30, 30, Settings.separateClickTools, "Separate Click Modes", 26);
        public static readonly GuiButton SwapClickMode = new(0, 90, 200, 40, "Swap Click Mode", 26);
        public static readonly GuiCheckbox JumpOnPaste = new(0, 140, 30, 30, Settings.jumpPaste, "Jump on Paste", 26);
        public static readonly GuiCheckbox PauseOnScroll = new(0, 180, 30, 30, Settings.pauseScroll, "Pause on Seek", 26);
        public static readonly GuiButton EditMapVFX = new(0, 240, 200, 40, "Edit Map VFX", 26) { Visible = false };
        public static readonly GuiButton EditSpecial = new(0, 290, 200, 40, "Edit Extra Objects", 26);

        public static readonly ControlContainer OptionsNav = new(10, 190, 545, 756, Numpad, SeparateClickTools, SwapClickMode, JumpOnPaste, PauseOnScroll, EditMapVFX, EditSpecial);

        // Timing
        public static readonly GuiTextboxNumeric ExportOffset = new(0, 50, 130, 40, Settings.exportOffset, false, false, "0", 31);
        public static readonly GuiTextboxNumeric SfxOffset = new(170, 50, 130, 40, Settings.sfxOffset, false, false, "0", 31);
        public static readonly GuiTextboxNumeric MusicOffset = new(340, 50, 130, 40, Settings.musicOffset, false, false, "0", 31);
        public static readonly GuiButton OpenTimings = new(0, 110, 210, 40, "OPEN BPM SETUP", 27);
        public static readonly GuiButton ImportIni = new(0, 160, 210, 40, "IMPORT INI", 27);
        public static readonly GuiCheckbox Metronome = new(0, 210, 30, 30, Settings.metronome, "Metronome", 26);
        public static readonly GuiButton OpenBookmarks = new(0, 280, 210, 40, "EDIT BOOKMARKS", 27);
        public static readonly GuiButton CopyBookmarks = new(0, 330, 210, 40, "COPY BOOKMARKS", 27);
        public static readonly GuiButton PasteBookmarks = new(0, 380, 210, 40, "PASTE BOOKMARKS", 27);

        public static readonly GuiLabel ExportOffsetLabel = new(0, 20, 100, 30, Settings.color1, "Export Offset:", 30, "main", CenterMode.None);
        public static readonly GuiLabel SfxOffsetLabel = new(170, 20, 100, 30, Settings.color1, "SFX Offset:", 30, "main", CenterMode.None);
        public static readonly GuiLabel MusicOffsetLabel = new(340, 20, 100, 30, Settings.color1, "Music Offset:", 30, "main", CenterMode.None);

        public static readonly ControlContainer TimingNav = new(10, 190, 545, 756, ExportOffset, SfxOffset, MusicOffset, OpenTimings, ImportIni, Metronome,
            OpenBookmarks, CopyBookmarks, PasteBookmarks, ExportOffsetLabel, SfxOffsetLabel, MusicOffsetLabel);

        // Patterns
        public static readonly GuiButton HFlip = new(0, 20, 175, 40, "HORIZONTAL FLIP", 27);
        public static readonly GuiButton VFlip = new(185, 20, 175, 40, "VERTICAL FLIP", 27);
        public static readonly GuiNumberBox RotateBox = new(0, 110, 100, 40, 5, null, true, false, "90", 31);
        public static readonly GuiButton RotateButton = new(110, 110, 100, 40, "ROTATE", 27);
        public static readonly GuiNumberBox ScaleBox = new(0, 190, 100, 40, 5, null, true, false, "150", 31);
        public static readonly GuiButton ScaleButton = new(110, 190, 100, 40, "SCALE", 27);
        public static readonly GuiCheckbox ApplyOnPaste = new(0, 250, 30, 30, Settings.applyOnPaste, "Apply Rotate/Scale on Paste", 27);
        public static readonly GuiCheckbox ClampSR = new(0, 290, 30, 30, Settings.clampSR, "Clamp Rotate/Scale in Bounds", 27);
        public static readonly GuiCheckbox PasteReversed = new(0, 330, 30, 30, Settings.pasteReversed, "Paste Reversed", 27);
        public static readonly GuiButton StoreNodes = new(0, 390, 175, 40, "STORE NODES", 27);
        public static readonly GuiButton ClearNodes = new(185, 390, 175, 40, "CLEAR NODES", 27);
        public static readonly GuiCheckbox CurveBezier = new(0, 450, 30, 30, Settings.curveBezier, "Curve Bezier", 27);
        public static readonly GuiNumberBox BezierBox = new(0, 520, 100, 40, 1, Settings.bezierDivisor, false, true, "4", 27) { Bounds = (1, float.MaxValue) };
        public static readonly GuiButton BezierButton = new(110, 520, 100, 40, "DRAW", 27);

        public static readonly GuiLabel RotateLabel = new(0, 80, 175, 30, Settings.color1, "Rotate by Degrees:", 30, "main", CenterMode.None);
        public static readonly GuiLabel ScaleLabel = new(0, 160, 100, 30, Settings.color1, "Scale by Percent:", 30, "main", CenterMode.None);
        public static readonly GuiLabel DrawBezierLabel = new(0, 490, 100, 30, Settings.color1, "Draw Bezier with Divisor:", 30, "main", CenterMode.None);

        public static readonly ControlContainer PatternsNav = new(10, 190, 545, 756, HFlip, VFlip, RotateBox, RotateButton, ScaleBox, ScaleButton, ApplyOnPaste, ClampSR, PasteReversed,
            StoreNodes, ClearNodes, CurveBezier, BezierBox, BezierButton, RotateLabel, ScaleLabel, DrawBezierLabel);

        // Player
        public static readonly GuiButtonList CameraMode = new(0, 50, 150, 40, Settings.cameraMode, "", 27);
        public static readonly GuiTextboxNumeric NoteScale = new(175, 50, 100, 40, Settings.noteScale, true, false, "1", 31);
        public static readonly GuiTextboxNumeric CursorScale = new(300, 50, 100, 40, Settings.cursorScale, true, false, "1", 31);
        public static readonly GuiCheckbox LockCursor = new(0, 110, 30, 30, Settings.lockCursor, "Lock Cursor Within Grid", 27);
        public static readonly GuiCheckbox GridGuides = new(0, 160, 30, 30, Settings.gridGuides, "Grid Guides", 27);
        public static readonly GuiTextboxNumeric Sensitivity = new(0, 240, 115, 40, Settings.sensitivity, true, true, "1", 31);
        public static readonly GuiTextboxNumeric Parallax = new(135, 240, 115, 40, Settings.parallax, true, true, "1", 31);
        public static readonly GuiTextboxNumeric FieldOfView = new(270, 240, 115, 40, Settings.fov, true, true, "70", 31);
        public static readonly GuiTextboxNumeric ApproachDistance = new(0, 325, 150, 40, Settings.approachDistance, true, true, "1", 31);
        public static readonly GuiTextboxNumeric HitWindow = new(235, 325, 150, 40, Settings.hitWindow, false, true, "55", 31);
        public static readonly GuiSlider PlayerApproachRate = new(0, 410, 400, 32, Settings.playerApproachRate) { Style = ControlStyle.None };
        public static readonly GuiCheckbox ApproachFade = new(0, 460, 30, 30, Settings.approachFade, "Approach Fade", 27);
        public static readonly GuiButton FromStart = new(0, 520, 200, 40, "PLAY FROM START", 27);
        public static readonly GuiButton PlayMap = new(210, 520, 200, 40, "PLAY HERE", 27);

        public static readonly GuiLabel CameraModeLabel = new(0, 20, 100, 30, Settings.color1, "Camera Mode:", 30, "main", CenterMode.None);
        public static readonly GuiLabel NoteScaleLabel = new(175, 20, 100, 30, Settings.color1, "Note Size:", 30, "main", CenterMode.None);
        public static readonly GuiLabel CursorScaleLabel = new(300, 20, 100, 30, Settings.color1, "Cursor Size:", 30, "main", CenterMode.None);
        public static readonly GuiLabel SensitivityLabel = new(0, 210, 100, 30, Settings.color1, "Sensitivity:", 30, "main", CenterMode.None);
        public static readonly GuiLabel ParallaxLabel = new(135, 210, 100, 30, Settings.color1, "Parallax:", 30, "main", CenterMode.None);
        public static readonly GuiLabel FieldOfViewLabel = new(270, 210, 100, 30, Settings.color1, "FOV:", 30, "main", CenterMode.None);
        public static readonly GuiLabel ApproachDistanceLabel = new(0, 295, 100, 30, Settings.color1, "Approach Distance:", 30, "main", CenterMode.None);
        public static readonly GuiLabel HitWindowLabel = new(235, 295, 100, 30, Settings.color1, "Hit Window:", 30, "main", CenterMode.None);
        public static readonly GuiLabel PlayerApproachRateLabel = new(0, 380, 400, 32, Settings.color1, "", 30);

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

        public static readonly GuiButton SpecialNavBeat = new(0, 0, 150, 30, "Beats", 26);
        public static readonly GuiButton SpecialNavMine = new(0, 40, 150, 30, "Mines", 26);
        public static readonly GuiButton SpecialNavGlide = new(0, 80, 150, 30, "Glides", 26);
        public static readonly GuiButton SpecialNavLyric = new(0, 120, 150, 30, "Lyrics", 26);
        public static readonly GuiButton SpecialNavFever = new(0, 160, 150, 30, "Fevers", 26);
        public static readonly GuiButton SpecialNavNotes = new(0, 700, 150, 30, "Modify Notes", 26);
        public static readonly RadioButtonController SpecialNavController = new(null, SpecialNavBeat, SpecialNavMine, SpecialNavGlide, SpecialNavLyric, SpecialNavFever, SpecialNavNotes);

        public static readonly ControlContainer SpecialNavNova = new(10, 200, 545, 756, SpecialNavBeat, SpecialNavMine, SpecialNavGlide, SpecialNavLyric, SpecialNavFever, SpecialNavNotes);
        public static readonly GuiButtonList GameSwitch = new(10, 946, 545, 50, Settings.modchartGame, "GAME: ", 31);
        public static readonly GuiButton SpecialNavExit = new(10, 140, 545, 50, "CLOSE EXTRA OBJECTS", 31);
        public static readonly GuiLabel LyricPreview = new(0, 860, 1920, 56, Settings.color2, "", 48);
        public static readonly GuiSquare FeverPreview = new(Color.FromArgb(23, 255, 0, 0));

        public static readonly GuiTextbox LyricBox = new(0, 0, 370, 30, null, "", 26);
        public static readonly GuiCheckbox LyricFadeIn = new(0, 40, 30, 30, null, "Fade In", 26);
        public static readonly GuiCheckbox LyricFadeOut = new(0, 80, 30, 30, null, "Fade Out", 26);
        public static readonly GuiButton LyricCreate = new(0, 120, 370, 30, "", 26);
        public static readonly GuiLabel LyricInfo = new(0, 160, 370, 400, null, string.Join('\n',
            "Lyric usage:",
            "> \"-\" prefix: Replace previous line",
            "> \"-\" suffix: Apply next lyric as a syllable",
            "    (Without a space in between)",
            "> Press 'Enter' in the lyric textbox to quickly place a lyric"), 20, "main", CenterMode.None);
        public static readonly ControlContainer LyricNav = new(175, 200, 370, 756, LyricBox, LyricFadeIn, LyricFadeOut, LyricCreate, LyricInfo) { Visible = false };

        public static readonly GuiCheckbox NoteEnableEasing = new(0, 0, 30, 30, null, "Enable Easing", 26);
        public static readonly GuiButtonList NoteEasingStyle = new(0, 40, 370, 30, Settings.modchartStyle, "Easing Style: ", 26);
        public static readonly GuiButtonList NoteEasingDirection = new(0, 80, 370, 30, Settings.modchartDirection, "Easing Direction: ", 26);
        public static readonly GuiButton NoteApplyModifiers = new(0, 160, 370, 30, "Apply Modifications", 26);
        public static readonly ControlContainer NoteNav = new(175, 200, 370, 756, NoteEnableEasing, NoteEasingStyle, NoteEasingDirection, NoteApplyModifiers) { Visible = false };

        public static readonly ControlContainer SpecialMapNavs = new(FeverPreview, SpecialNavNova, GameSwitch, SpecialNavExit, LyricNav, LyricPreview, NoteNav) { Visible = false };

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
        public static readonly GuiCheckbox Quantum = new(245, 10, 30, 30, Settings.enableQuantum, "Quantum", 27);
        public static readonly GuiCheckbox QuantumGridSnap = new(245, 50, 30, 30, Settings.quantumGridSnap, "Snap to Grid", 27);
        public static readonly GuiCheckbox AutoAdvance = new(245, 90, 30, 30, Settings.autoAdvance, "Auto-Advance", 27);
        public static readonly GuiSlider BeatSnapDivisor = new(245, 170, 250, 32, Settings.beatDivisor) { Style = ControlStyle.None, ShiftIncrement = 0.5f };
        public static readonly GuiSlider QuantumSnapDivisor = new(245, 250, 250, 32, Settings.quantumSnapping) { Style = ControlStyle.None };

        public static readonly GuiLabel BeatDivisorLabel = new(245, 140, 250, 32, Settings.color1, "", 30);
        public static readonly GuiLabel SnappingLabel = new(245, 220, 250, 32, Settings.color1, "", 30);

        public static readonly ControlContainer SnappingNav = new(1365, 190, 545, 756, Quantum, QuantumGridSnap, AutoAdvance, BeatSnapDivisor, QuantumSnapDivisor, BeatDivisorLabel, SnappingLabel);

        // Graphics
        public static readonly GuiCheckbox Autoplay = new(245, 10, 30, 30, Settings.autoplay, "Autoplay", 26);
        public static readonly GuiCheckbox SmoothAutoplay = new(245, 50, 30, 30, Settings.smoothAutoplay, "Smooth Autoplay Cursor", 26);
        public static readonly GuiCheckbox ApproachSquares = new(245, 90, 30, 30, Settings.approachSquares, "Approach Squares", 26);
        public static readonly GuiCheckbox GridNumbers = new(245, 130, 30, 30, Settings.gridNumbers, "Grid Numbers", 26);
        public static readonly GuiCheckbox GridLetters = new(245, 170, 30, 30, Settings.gridLetters, "Grid Letters", 26);
        public static readonly GuiCheckbox QuantumGridLines = new(245, 210, 30, 30, Settings.quantumGridLines, "Quantum Grid Lines", 26);
        public static readonly GuiSlider ApproachRate = new(245, 290, 250, 32, Settings.approachRate) { Style = ControlStyle.None };
        public static readonly GuiSlider TrackCursorPos = new(245, 370, 250, 32, Settings.cursorPos) { Style = ControlStyle.None };

        public static readonly GuiLabel ApproachRateLabel = new(245, 260, 250, 32, Settings.color1, "", 28);
        public static readonly GuiLabel CursorPosLabel = new(245, 340, 250, 32, Settings.color1, "", 28);


        public static readonly ControlContainer GraphicsNav = new(1365, 190, 545, 756, Autoplay, ApproachSquares, GridNumbers, GridLetters, QuantumGridLines, ApproachRate, TrackCursorPos,
            ApproachRateLabel, CursorPosLabel, SmoothAutoplay);

        // Export
        public static readonly GuiButton SaveButton = new(275, 20, 100, 40, "SAVE", 27);
        public static readonly GuiButton SaveAsButton = new(385, 20, 100, 40, "SAVE AS", 27);
        public static readonly GuiButtonList ExportSwitch = new(255, 90, 250, 40, Settings.exportType, "", 27);
        public static readonly GuiButton ExportButton = new(275, 140, 210, 40, "EXPORT MAP", 27);
        public static readonly GuiTextbox ReplaceIDBox = new(275, 230, 210, 40, null, "", 27);
        public static readonly GuiButton ReplaceID = new(275, 280, 210, 40, "REPLACE", 27);
        public static readonly GuiButton ConvertAudio = new(275, 350, 210, 40, "CONVERT TO MP3", 27);
        public static readonly GuiButton CalculateSR = new(0, 20, 210, 40, "CALCULATE SR/RP", 27);

        public static readonly GuiLabel ReplaceIDLabel = new(275, 190, 210, 40, Settings.color1, "Replace Audio ID", 30);
        public static readonly GuiLabel SRLabel = new(10, 70, 190, 320, Settings.color1, "", 28, "main", CenterMode.None);

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
        public static readonly GuiButton CopyButton = new(810, 273, 300, 42, "COPY MAP DATA", 27);
        public static readonly GuiButton BackButton = new(810, 774, 300, 42, "BACK TO MENU", 27);

        public static readonly GuiSlider Tempo = new(1408, 1016, 512, 64, Settings.tempo) { Style = ControlStyle.None };
        public static readonly GuiSliderTimeline Timeline = new(0, 1016, 1344, 64) { Style = ControlStyle.None };
        public static readonly GuiSlider MusicVolume = new(1856, 760, 40, 256, Settings.masterVolume, true) { Style = ControlStyle.None };
        public static readonly GuiButtonToggle MusicMute = new(1868, 750, 16, 16, Settings.muteMusic, "MuteUnmute");
        public static readonly GuiSlider SfxVolume = new(1792, 760, 40, 256, Settings.sfxVolume, true) { Style = ControlStyle.None };
        public static readonly GuiButtonToggle SfxMute = new(1804, 750, 16, 16, Settings.muteSfx, "MuteUnmute");
        public static readonly GuiButtonPlayPause PlayPause = new(1344, 1016, 64, 64);

        public static readonly GuiLabelToast ToastLabel = new(0, 1000, 1920, 64, null, "", 42);
        public static readonly GuiLabel ZoomLabel = new(565, 140, 80, 30, Settings.color1, "Zoom: ", 32, "main", CenterMode.None);
        public static readonly GuiLabel ZoomValueLabel = new(640, 140, 80, 30, Settings.color2, "", 32, "main", CenterMode.None);
        public static readonly GuiLabel ClickModeLabel = new(810, 816, 300, 42, Settings.color1, "", 30);

        public static readonly GuiLabel TempoLabel = new(1408, 1048, 512, 30, Settings.color1, "", 30);
        public static readonly GuiLabel MusicVolumeLabel = new(1856, 720, 40, 30, Settings.color1, "Music", 24);
        public static readonly GuiLabel MusicVolumeValueLabel = new(1856, 996, 40, 30, Settings.color1, "", 24);
        public static readonly GuiLabel SfxVolumeLabel = new(1792, 720, 40, 30, Settings.color1, "SFX", 24);
        public static readonly GuiLabel SfxVolumeValueLabel = new(1792, 996, 40, 30, Settings.color1, "", 24);

        public static readonly GuiLabel CurrentTimeLabel = new(0, 1048, 64, 32, Settings.color1, "", 26);
        public static readonly GuiLabel CurrentMsLabel = new(0, 1012, 32, 32, Settings.color1, "", 26);
        public static readonly GuiLabel TotalTimeLabel = new(1280, 1048, 64, 32, Settings.color1, "", 26);
        public static readonly GuiLabel NotesLabel = new(0, 1048, 1344, 32, Settings.color1, "", 30);

        public static readonly GuiGrid Grid = new(810, 390, 300, 300) { Stretch = StretchMode.None };
        public static readonly GuiTrack Track = new(0, 0, 1920, 86);

        public static readonly GuiSquareTextured BackgroundSquare = new("editorbg", Path.Combine(Assets.THIS, "background_editor.png"));
    }
}
