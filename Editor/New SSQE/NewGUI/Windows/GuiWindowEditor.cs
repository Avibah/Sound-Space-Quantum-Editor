using New_SSQE.Audio;
using New_SSQE.ExternalUtils;
using New_SSQE.Misc.Dialogs;
using New_SSQE.Misc.Static;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;
using New_SSQE.NewGUI.Forms;
using New_SSQE.NewGUI.Input;
using New_SSQE.NewMaps;
using New_SSQE.NewMaps.Parsing;
using New_SSQE.Objects;
using New_SSQE.Objects.Managers;
using New_SSQE.Preferences;
using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;

namespace New_SSQE.NewGUI.Windows
{
    internal class GuiWindowEditor : GuiWindow
    {
        public static readonly GuiButton LNavOptions = new(10, 140, 175, 50, "OPTIONS", 31) { Stretch = StretchMode.X };
        public static readonly GuiButton LNavTiming = new(195, 140, 175, 50, "TIMING", 31) { Stretch = StretchMode.X };
        public static readonly GuiButton LNavPatterns = new(380, 140, 175, 50, "PATTERNS", 31) { Stretch = StretchMode.X };
        public static readonly GuiButton LNavPlayer = new(10, 946, 545, 50, "PLAYTEST", 31) { Stretch = StretchMode.X };
        public static readonly RadioButtonController LNavController = new(1, LNavOptions, LNavTiming, LNavPatterns, LNavPlayer);

        public static readonly GuiButton RNavSnapping = new(1365, 140, 175, 50, "SNAPPING", 31) { Stretch = StretchMode.X };
        public static readonly GuiButton RNavGraphics = new(1550, 140, 175, 50, "GRAPHICS", 31) { Stretch = StretchMode.X };
        public static readonly GuiButton RNavExport = new(1735, 140, 175, 50, "EXPORT", 31) { Stretch = StretchMode.X };
        public static readonly RadioButtonController RNavController = new(0, RNavSnapping, RNavGraphics, RNavExport);

        /*
         *************************************
         | BEGIN: Navs for standard maps:    |
         | Options, Timing, Patterns, Player |
         *************************************
         */

        // Options
        public static readonly GuiCheckbox Numpad = new(0, 10, 30, 30, Settings.numpad, "Use Numpad", 26) { Stretch = StretchMode.X };
        public static readonly GuiCheckbox SeparateClickTools = new(0, 50, 30, 30, Settings.separateClickTools, "Separate Click Modes", 26) { Stretch = StretchMode.X };
        public static readonly GuiButton SwapClickMode = new(0, 90, 200, 40, "Swap Click Mode", 26) { Stretch = StretchMode.X };
        public static readonly GuiCheckbox JumpOnPaste = new(0, 140, 30, 30, Settings.jumpPaste, "Jump on Paste", 26) { Stretch = StretchMode.X };
        public static readonly GuiCheckbox PauseOnScroll = new(0, 180, 30, 30, Settings.pauseScroll, "Pause on Seek", 26) { Stretch = StretchMode.X };
        public static readonly GuiButton EditMapVFX = new(0, 240, 200, 40, "Edit Map VFX", 26) { Stretch = StretchMode.X, Visible = false };
        public static readonly GuiButton EditSpecial = new(0, 290, 200, 40, "Edit Extra Objects", 26) { Stretch = StretchMode.X };

        public static readonly ControlContainer OptionsNav = new(10, 190, 545, 756, Numpad, SeparateClickTools, SwapClickMode, JumpOnPaste, PauseOnScroll, EditMapVFX, EditSpecial);

        // Timing
        public static readonly GuiTextboxNumeric ExportOffset = new(0, 50, 130, 40, Settings.exportOffset, false, false, "0", 31) { Stretch = StretchMode.X };
        public static readonly GuiTextboxNumeric SfxOffset = new(170, 50, 130, 40, Settings.sfxOffset, false, false, "0", 31) { Stretch = StretchMode.X };
        public static readonly GuiButton OpenTimings = new(0, 110, 210, 40, "OPEN BPM SETUP", 27) { Stretch = StretchMode.X };
        public static readonly GuiButton ImportIni = new(0, 160, 210, 40, "IMPORT INI", 27) { Stretch = StretchMode.X };
        public static readonly GuiCheckbox Metronome = new(0, 210, 30, 30, Settings.metronome, "Metronome", 26) { Stretch = StretchMode.X };
        public static readonly GuiButton OpenBookmarks = new(0, 280, 210, 40, "EDIT BOOKMARKS", 27) { Stretch = StretchMode.X };
        public static readonly GuiButton CopyBookmarks = new(0, 330, 210, 40, "COPY BOOKMARKS", 27) { Stretch = StretchMode.X };
        public static readonly GuiButton PasteBookmarks = new(0, 380, 210, 40, "PASTE BOOKMARKS", 27) { Stretch = StretchMode.X };

        public static readonly GuiLabel ExportOffsetLabel = new(0, 20, 100, 30, Settings.color1, "Export Offset:", 30, "main", CenterMode.None) { Stretch = StretchMode.X };
        public static readonly GuiLabel SfxOffsetLabel = new(170, 20, 100, 30, Settings.color1, "SFX Offset:", 30, "main", CenterMode.None) { Stretch = StretchMode.X };

        public static readonly ControlContainer TimingNav = new(10, 190, 545, 756, ExportOffset, SfxOffset, OpenTimings, ImportIni, Metronome,
            OpenBookmarks, CopyBookmarks, PasteBookmarks, ExportOffsetLabel, SfxOffsetLabel);

        // Patterns
        public static readonly GuiButton HFlip = new(0, 20, 175, 40, "HORIZONTAL FLIP", 27) { Stretch = StretchMode.X };
        public static readonly GuiButton VFlip = new(185, 20, 175, 40, "VERTICAL FLIP", 27) { Stretch = StretchMode.X };
        public static readonly GuiNumberBox RotateBox = new(0, 110, 100, 40, 5, null, true, false, "90", 31) { Stretch = StretchMode.X };
        public static readonly GuiButton RotateButton = new(110, 110, 100, 40, "ROTATE", 27) { Stretch = StretchMode.X };
        public static readonly GuiNumberBox ScaleBox = new(0, 190, 100, 40, 5, null, true, false, "150", 31) { Stretch = StretchMode.X };
        public static readonly GuiButton ScaleButton = new(110, 190, 100, 40, "SCALE", 27) { Stretch = StretchMode.X };
        public static readonly GuiCheckbox ApplyOnPaste = new(0, 250, 30, 30, Settings.applyOnPaste, "Apply Rotate/Scale on Paste", 27) { Stretch = StretchMode.X };
        public static readonly GuiCheckbox ClampSR = new(0, 290, 30, 30, Settings.clampSR, "Clamp Rotate/Scale in Bounds", 27) { Stretch = StretchMode.X };
        public static readonly GuiCheckbox PasteReversed = new(0, 330, 30, 30, Settings.pasteReversed, "Paste Reversed", 27) { Stretch = StretchMode.X };
        public static readonly GuiButton StoreNodes = new(0, 390, 175, 40, "STORE NODES", 27) { Stretch = StretchMode.X };
        public static readonly GuiButton ClearNodes = new(185, 390, 175, 40, "CLEAR NODES", 27) { Stretch = StretchMode.X };
        public static readonly GuiCheckbox CurveBezier = new(0, 450, 30, 30, Settings.curveBezier, "Curve Bezier", 27) { Stretch = StretchMode.X };
        public static readonly GuiNumberBox BezierBox = new(0, 520, 100, 40, 1, Settings.bezierDivisor, false, true, "4", 27) { Stretch = StretchMode.X };
        public static readonly GuiButton BezierButton = new(110, 520, 100, 40, "DRAW", 27) { Stretch = StretchMode.X };

        public static readonly GuiLabel RotateLabel = new(0, 80, 175, 30, Settings.color1, "Rotate by Degrees:", 30, "main", CenterMode.None) { Stretch = StretchMode.X };
        public static readonly GuiLabel ScaleLabel = new(0, 160, 100, 30, Settings.color1, "Scale by Percent:", 30, "main", CenterMode.None) { Stretch = StretchMode.X };
        public static readonly GuiLabel DrawBezierLabel = new(0, 490, 100, 30, Settings.color1, "Draw Bezier with Divisor:", 30, "main", CenterMode.None) { Stretch = StretchMode.X };

        public static readonly ControlContainer PatternsNav = new(10, 190, 545, 756, HFlip, VFlip, RotateBox, RotateButton, ScaleBox, ScaleButton, ApplyOnPaste, ClampSR, PasteReversed,
            StoreNodes, ClearNodes, CurveBezier, BezierBox, BezierButton, RotateLabel, ScaleLabel, DrawBezierLabel);

        // Player
        public static readonly GuiButtonList CameraMode = new(0, 50, 150, 40, Settings.cameraMode, "", 27) { Stretch = StretchMode.X };
        public static readonly GuiTextboxNumeric NoteScale = new(175, 50, 100, 40, Settings.noteScale, true, false, "1", 31) { Stretch = StretchMode.X };
        public static readonly GuiTextboxNumeric CursorScale = new(300, 50, 100, 40, Settings.cursorScale, true, false, "1", 31) { Stretch = StretchMode.X };
        public static readonly GuiCheckbox LockCursor = new(0, 110, 30, 30, Settings.lockCursor, "Lock Cursor Within Grid", 27) { Stretch = StretchMode.X };
        public static readonly GuiCheckbox GridGuides = new(0, 160, 30, 30, Settings.gridGuides, "Grid Guides", 27) { Stretch = StretchMode.X };
        public static readonly GuiTextboxNumeric Sensitivity = new(0, 240, 115, 40, Settings.sensitivity, true, true, "1", 31) { Stretch = StretchMode.X };
        public static readonly GuiTextboxNumeric Parallax = new(135, 240, 115, 40, Settings.parallax, true, true, "1", 31) { Stretch = StretchMode.X };
        public static readonly GuiTextboxNumeric FieldOfView = new(270, 240, 115, 40, Settings.fov, true, true, "70", 31) { Stretch = StretchMode.X };
        public static readonly GuiTextboxNumeric ApproachDistance = new(0, 325, 150, 40, Settings.approachDistance, true, true, "1", 31) { Stretch = StretchMode.X };
        public static readonly GuiTextboxNumeric HitWindow = new(235, 325, 150, 40, Settings.hitWindow, false, true, "55", 31) { Stretch = StretchMode.X };
        public static readonly GuiSlider PlayerApproachRate = new(0, 410, 400, 32, Settings.playerApproachRate) { Style = ControlStyle.None, Stretch = StretchMode.X };
        public static readonly GuiCheckbox ApproachFade = new(0, 460, 30, 30, Settings.approachFade, "Approach Fade", 27) { Stretch = StretchMode.X };
        public static readonly GuiButton FromStart = new(0, 520, 200, 40, "PLAY FROM START", 27) { Stretch = StretchMode.X };
        public static readonly GuiButton PlayMap = new(210, 520, 200, 40, "PLAY HERE", 27) { Stretch = StretchMode.X };

        public static readonly GuiLabel CameraModeLabel = new(0, 20, 100, 30, Settings.color1, "Camera Mode:", 30, "main", CenterMode.None) { Stretch = StretchMode.X };
        public static readonly GuiLabel NoteScaleLabel = new(175, 20, 100, 30, Settings.color1, "Note Size:", 30, "main", CenterMode.None) { Stretch = StretchMode.X };
        public static readonly GuiLabel CursorScaleLabel = new(300, 20, 100, 30, Settings.color1, "Cursor Size:", 30, "main", CenterMode.None) { Stretch = StretchMode.X };
        public static readonly GuiLabel SensitivityLabel = new(0, 210, 100, 30, Settings.color1, "Sensitivity:", 30, "main", CenterMode.None) { Stretch = StretchMode.X };
        public static readonly GuiLabel ParallaxLabel = new(135, 210, 100, 30, Settings.color1, "Parallax:", 30, "main", CenterMode.None) { Stretch = StretchMode.X };
        public static readonly GuiLabel FieldOfViewLabel = new(270, 210, 100, 30, Settings.color1, "FOV:", 30, "main", CenterMode.None) { Stretch = StretchMode.X };
        public static readonly GuiLabel ApproachDistanceLabel = new(0, 295, 100, 30, Settings.color1, "Approach Distance:", 30, "main", CenterMode.None) { Stretch = StretchMode.X };
        public static readonly GuiLabel HitWindowLabel = new(235, 295, 100, 30, Settings.color1, "Hit Window:", 30, "main", CenterMode.None) { Stretch = StretchMode.X };
        public static readonly GuiLabel PlayerApproachRateLabel = new(0, 380, 400, 32, Settings.color1, "", 30) { Stretch = StretchMode.X };

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

        public static readonly GuiButton SpecialNavBeat = new(0, 0, 150, 30, "Beats", 26) { Stretch = StretchMode.X };
        public static readonly GuiButton SpecialNavMine = new(0, 40, 150, 30, "Mines", 26) { Stretch = StretchMode.X };
        public static readonly GuiButton SpecialNavGlide = new(0, 80, 150, 30, "Glides", 26) { Stretch = StretchMode.X };
        public static readonly GuiButton SpecialNavLyric = new(0, 120, 150, 30, "Lyrics", 26) { Stretch = StretchMode.X };
        public static readonly RadioButtonController SpecialNavController = new(null, SpecialNavBeat, SpecialNavMine, SpecialNavGlide, SpecialNavLyric);

        public static readonly ControlContainer SpecialNavNova = new(10, 200, 545, 756, SpecialNavBeat, SpecialNavMine, SpecialNavGlide/*, SpecialNavLyric*/) { Stretch = StretchMode.XY };
        public static readonly GuiButtonList GameSwitch = new(10, 946, 545, 50, Settings.modchartGame, "GAME: ", 31) { Stretch = StretchMode.X };
        public static readonly GuiButton SpecialNavExit = new(10, 140, 545, 50, "CLOSE EXTRA OBJECTS", 31) { Stretch = StretchMode.X };
        public static readonly GuiLabel LyricPreview = new(0, 860, 1920, 56, Settings.color2, "", 48) { Stretch = StretchMode.X };

        public static readonly GuiTextbox LyricBox = new(0, 0, 370, 30, null, "", 26) { Stretch = StretchMode.X };
        public static readonly GuiCheckbox LyricFadeIn = new(0, 40, 30, 30, null, "Fade In", 26) { Stretch = StretchMode.X };
        public static readonly GuiCheckbox LyricFadeOut = new(0, 80, 30, 30, null, "Fade Out", 26) { Stretch = StretchMode.X };
        public static readonly GuiButton LyricCreate = new(0, 120, 370, 30, "", 26) { Stretch = StretchMode.X };
        public static readonly GuiLabel LyricInfo = new(0, 160, 370, 400, null, string.Join('\n',
            "Lyric usage:",
            "> \"-\" prefix: Clear previous line",
            "> \"-\" suffix: Apply next lyric as a syllable",
            "    (Without a space in between)",
            "> Press 'Enter' in the lyric textbox to quickly place a lyric"), 20, "main", CenterMode.None) { Stretch = StretchMode.X };
        public static readonly ControlContainer LyricNav = new(175, 200, 370, 756, LyricBox, LyricFadeIn, LyricFadeOut, LyricCreate, LyricInfo) { Visible = false };

        public static readonly ControlContainer SpecialMapNavs = new(SpecialNavNova, GameSwitch, SpecialNavExit, LyricNav, LyricPreview) { Visible = false };

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
        public static readonly GuiCheckbox Quantum = new(245, 10, 30, 30, Settings.enableQuantum, "Quantum", 27) { Stretch = StretchMode.X };
        public static readonly GuiCheckbox QuantumGridSnap = new(245, 50, 30, 30, Settings.quantumGridSnap, "Snap to Grid", 27) { Stretch = StretchMode.X };
        public static readonly GuiCheckbox AutoAdvance = new(245, 90, 30, 30, Settings.autoAdvance, "Auto-Advance", 27) { Stretch = StretchMode.X };
        public static readonly GuiSlider BeatSnapDivisor = new(245, 170, 250, 32, Settings.beatDivisor) { Style = ControlStyle.None, ShiftIncrement = 0.5f, Stretch = StretchMode.X };
        public static readonly GuiSlider QuantumSnapDivisor = new(245, 250, 250, 32, Settings.quantumSnapping) { Style = ControlStyle.None, Stretch = StretchMode.X };

        public static readonly GuiLabel BeatDivisorLabel = new(245, 140, 250, 32, Settings.color1, "", 30) { Stretch = StretchMode.X };
        public static readonly GuiLabel SnappingLabel = new(245, 220, 250, 32, Settings.color1, "", 30) { Stretch = StretchMode.X };

        public static readonly ControlContainer SnappingNav = new(1365, 190, 545, 756, Quantum, QuantumGridSnap, AutoAdvance, BeatSnapDivisor, QuantumSnapDivisor, BeatDivisorLabel, SnappingLabel);

        // Graphics
        public static readonly GuiCheckbox Autoplay = new(245, 10, 30, 30, Settings.autoplay, "Autoplay", 26) { Stretch = StretchMode.X };
        public static readonly GuiCheckbox SmoothAutoplay = new(245, 50, 30, 30, Settings.smoothAutoplay, "Smooth Autoplay Cursor", 26) { Stretch = StretchMode.X };
        public static readonly GuiCheckbox ApproachSquares = new(245, 90, 30, 30, Settings.approachSquares, "Approach Squares", 26) { Stretch = StretchMode.X };
        public static readonly GuiCheckbox GridNumbers = new(245, 130, 30, 30, Settings.gridNumbers, "Grid Numbers", 26) { Stretch = StretchMode.X };
        public static readonly GuiCheckbox GridLetters = new(245, 170, 30, 30, Settings.gridLetters, "Grid Letters", 26) { Stretch = StretchMode.X };
        public static readonly GuiCheckbox QuantumGridLines = new(245, 210, 30, 30, Settings.quantumGridLines, "Quantum Grid Lines", 26) { Stretch = StretchMode.X };
        public static readonly GuiSlider ApproachRate = new(245, 290, 250, 32, Settings.approachRate) { Style = ControlStyle.None, Stretch = StretchMode.X };
        public static readonly GuiSlider TrackCursorPos = new(245, 370, 250, 32, Settings.cursorPos) { Style = ControlStyle.None, Stretch = StretchMode.X };

        public static readonly GuiLabel ApproachRateLabel = new(245, 260, 250, 32, Settings.color1, "", 28) { Stretch = StretchMode.X };
        public static readonly GuiLabel CursorPosLabel = new(245, 340, 250, 32, Settings.color1, "", 28) { Stretch = StretchMode.X };


        public static readonly ControlContainer GraphicsNav = new(1365, 190, 545, 756, Autoplay, ApproachSquares, GridNumbers, GridLetters, QuantumGridLines, ApproachRate, TrackCursorPos,
            ApproachRateLabel, CursorPosLabel, SmoothAutoplay);

        // Export
        public static readonly GuiButton SaveButton = new(275, 20, 100, 40, "SAVE", 27) { Stretch = StretchMode.X };
        public static readonly GuiButton SaveAsButton = new(385, 20, 100, 40, "SAVE AS", 27) { Stretch = StretchMode.X };
        public static readonly GuiButtonList ExportSwitch = new(255, 90, 250, 40, Settings.exportType, "", 27) { Stretch = StretchMode.X };
        public static readonly GuiButton ExportButton = new(275, 140, 210, 40, "EXPORT MAP", 27) { Stretch = StretchMode.X };
        public static readonly GuiTextbox ReplaceIDBox = new(275, 230, 210, 40, null, "", 27) { Stretch = StretchMode.X };
        public static readonly GuiButton ReplaceID = new(275, 280, 210, 40, "REPLACE", 27) { Stretch = StretchMode.X };
        public static readonly GuiButton ConvertAudio = new(275, 350, 210, 40, "CONVERT TO MP3", 27) { Stretch = StretchMode.X };
        public static readonly GuiButton CalculateSR = new(0, 20, 210, 40, "CALCULATE SR/RP", 27) { Stretch = StretchMode.X };

        public static readonly GuiLabel ReplaceIDLabel = new(275, 190, 210, 40, Settings.color1, "Replace Audio ID", 30) { Stretch = StretchMode.X };
        public static readonly GuiLabel SRLabel = new(10, 70, 190, 320, Settings.color1, "", 28, "main", CenterMode.None) { Stretch = StretchMode.X };

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

        public static readonly GuiSlider Tempo = new(1408, 1016, 512, 64, Settings.tempo) { Style = ControlStyle.None, Stretch = StretchMode.X };
        public static readonly GuiSliderTimeline Timeline = new(0, 1016, 1344, 64) { Style = ControlStyle.None, Stretch = StretchMode.X };
        public static readonly GuiSlider MusicVolume = new(1856, 760, 40, 256, Settings.masterVolume, true) { Style = ControlStyle.None };
        public static readonly GuiButtonToggle MusicMute = new(1868, 750, 16, 16, Settings.muteMusic, "MuteUnmute");
        public static readonly GuiSlider SfxVolume = new(1792, 760, 40, 256, Settings.sfxVolume, true) { Style = ControlStyle.None };
        public static readonly GuiButtonToggle SfxMute = new(1804, 750, 16, 16, Settings.muteSfx, "MuteUnmute");
        public static readonly GuiButtonPlayPause PlayPause = new(1344, 1016, 64, 64);

        public static readonly GuiLabelToast ToastLabel = new(0, 1000, 1920, 64, null, "", 42) { Stretch = StretchMode.X };
        public static readonly GuiLabel ZoomLabel = new(565, 140, 80, 30, Settings.color1, "Zoom: ", 32, "main", CenterMode.None);
        public static readonly GuiLabel ZoomValueLabel = new(640, 140, 80, 30, Settings.color2, "", 32, "main", CenterMode.None);
        public static readonly GuiLabel ClickModeLabel = new(810, 816, 300, 42, Settings.color1, "", 30);
        
        public static readonly GuiLabel TempoLabel = new(1408, 1048, 512, 30, Settings.color1, "", 30) { Stretch = StretchMode.X };
        public static readonly GuiLabel MusicVolumeLabel = new(1856, 720, 40, 30, Settings.color1, "Music", 24);
        public static readonly GuiLabel MusicVolumeValueLabel = new(1856, 996, 40, 30, Settings.color1, "", 24);
        public static readonly GuiLabel SfxVolumeLabel = new(1792, 720, 40, 30, Settings.color1, "SFX", 24);
        public static readonly GuiLabel SfxVolumeValueLabel = new(1792, 996, 40, 30, Settings.color1, "", 24);

        public static readonly GuiLabel CurrentTimeLabel = new(0, 1048, 64, 32, Settings.color1, "", 26);
        public static readonly GuiLabel CurrentMsLabel = new(0, 1012, 32, 32, Settings.color1, "", 26);
        public static readonly GuiLabel TotalTimeLabel = new(1280, 1048, 64, 32, Settings.color1, "", 26);
        public static readonly GuiLabel NotesLabel = new(0, 1048, 1344, 32, Settings.color1, "", 30) { Stretch = StretchMode.X };

        public static readonly GuiGrid Grid = new(810, 390, 300, 300);
        public static readonly GuiTrack Track = new(0, 0, 1920, 86) { Stretch = StretchMode.XY };

        public static readonly GuiSquareTextured BackgroundSquare = new("editorbg", Path.Combine(Assets.THIS, "background_editor.png")) { Stretch = StretchMode.XY };

        public GuiWindowEditor() : base(BackgroundSquare, StandardMapNavs, SpecialMapNavs, ConstantMapNavs, CopyButton, BackButton, Tempo,
            Timeline, MusicVolume, SfxVolume, PlayPause, ToastLabel, ZoomLabel, ZoomValueLabel, ClickModeLabel, TempoLabel,
            MusicVolumeLabel, MusicVolumeValueLabel, SfxVolumeLabel, SfxVolumeValueLabel, CurrentTimeLabel, CurrentMsLabel,
            MusicMute, SfxMute, TotalTimeLabel, NotesLabel, Grid, Track)
        {
            BackgroundSquare.SetColor(Color.FromArgb((int)Settings.editorBGOpacity.Value, 30, 30, 30));
            
            LNavPlayer.Visible = PlatformUtils.ExecutableExists("SSQE Player") || Settings.useRhythia.Value;

            BeatSnapDivisor.Update();
            QuantumSnapDivisor.Update();
            Timeline.Update();
            Tempo.Update();
        }

        public override void Close()
        {
            base.Close();

            LNavController.Disconnect();
            RNavController.Disconnect();
            SpecialNavController.Disconnect();
        }

        public override void ConnectEvents()
        {
            LNavController.SelectionChanged += (s, e) =>
            {
                bool options = OptionsNav.Visible;
                bool timing = TimingNav.Visible;
                bool patterns = PatternsNav.Visible;

                OptionsNav.Visible = e.Value == "OPTIONS";
                TimingNav.Visible = e.Value == "TIMING";
                PatternsNav.Visible = e.Value == "PATTERNS";
                PlayerNav.Visible = e.Value == "PLAYTEST";

                if (Settings.useRhythia.Value && e.Value == "PLAYTEST")
                {
                    if (options)
                        LNavController.UpdateSelection(LNavOptions);
                    else if (timing)
                        LNavController.UpdateSelection(LNavTiming);
                    else if (patterns)
                        LNavController.UpdateSelection(LNavPatterns);
                    else
                        LNavController.ClearSelection();
                }
            };

            if (Settings.useRhythia.Value)
            {
                LNavPlayer.LeftClick += (s, e) =>
                {
                    if (!File.Exists(Settings.rhythiaPath.Value))
                    {
                        Logging.Log($"Invalid Rhythia path - {Settings.rhythiaPath.Value}", LogSeverity.WARN);
                        ShowToast("INVALID RHYTHIA PATH [MENU > SETTINGS > MAPPING]", Settings.color1.Value);
                    }
                    else
                    {
                        try
                        {
                            if (MusicPlayer.IsPlaying)
                                MusicPlayer.Pause();

                            Settings.Save();
                            TXT.Write(Path.Combine(Assets.TEMP, "tempmap.txt"));

                            string[] args =
                            [
                                $"--a=\"{Path.GetFullPath(Path.Combine(Assets.CACHED, $"{Mapping.Current.SoundID}.asset")).Replace("\\", "/")}\"",
                                $"--t=\"{Path.GetFullPath(Path.Combine(Assets.TEMP, "tempmap.txt")).Replace("\\", "/")}\""
                            ];

                            Process.Start(Settings.rhythiaPath.Value, string.Join(' ', args));
                        }
                        catch (Exception ex)
                        {
                            Logging.Log("Failed to start Rhythia", LogSeverity.WARN, ex);
                            ShowToast("FAILED TO START RHYTHIA", Settings.color1.Value);
                        }
                    }
                };
            }

            RNavController.SelectionChanged += (s, e) =>
            {
                SnappingNav.Visible = e.Value == "SNAPPING";
                GraphicsNav.Visible = e.Value == "GRAPHICS";
                ExportNav.Visible = e.Value == "EXPORT";

                ConvertAudio.Visible = ExportNav.Visible && !PlatformUtils.IsLinux;
            };

            LNavController.Initialize();
            RNavController.Initialize();

            CopyButton.LeftClick += (s, e) =>
            {
                try
                {
                    if (KeybindManager.AltHeld)
                        Clipboard.SetText(TXT.CopyLegacy(Settings.correctOnCopy.Value));
                    else
                        Clipboard.SetText(TXT.Copy(Settings.correctOnCopy.Value));

                    ShowToast("COPIED TO CLIPBOARD", Settings.color1.Value);
                }
                catch (Exception ex)
                {
                    Logging.Log("Failed to copy", LogSeverity.WARN, ex);
                    ShowToast("FAILED TO COPY", Settings.color2.Value);
                }
            };

            BackButton.LeftClick += (s, e) => Windowing.SwitchWindow(new GuiWindowMenu());

            OpenTimings.LeftClick += (s, e) => TimingsWindow.ShowWindow();
            OpenTimings.BindKeybind("openTimings");
            OpenBookmarks.LeftClick += (s, e) => BookmarksWindow.ShowWindow();
            OpenBookmarks.BindKeybind("openBookmarks");

            HFlip.LeftClick += (s, e) => NoteManager.Edit("HORIZONTAL FLIP", Patterns.HorizontalFlip);
            HFlip.BindKeybind("hFlip");
            VFlip.LeftClick += (s, e) => NoteManager.Edit("VERTICAL FLIP", Patterns.VerticalFlip);
            VFlip.BindKeybind("vFlip");

            StoreNodes.LeftClick += (s, e) =>
            {
                Mapping.Current.BezierNodes = [];
                List<Note> selected = [..Mapping.Current.Notes.Selected.OrderBy(n => n.Ms)];
                int selectIndex = 0;
                
                for (int i = 0; i < Mapping.Current.Notes.Count; i++)
                {
                    if (selectIndex >= selected.Count)
                        break;

                    if (Mapping.Current.Notes[i] == selected[selectIndex])
                    {
                        Mapping.Current.BezierNodes.Add(i);
                        selectIndex++;
                    }
                }
            };
            StoreNodes.BindKeybind("storeNodes");
            ClearNodes.LeftClick += (s, e) => Mapping.Current.BezierNodes.Clear();
            BezierButton.LeftClick += (s, e) => Patterns.RunBezier();
            BezierButton.BindKeybind("drawBezier");

            RotateButton.LeftClick += (s, e) =>
            {
                float deg = RotateBox.Value;
                NoteManager.Edit($"ROTATE {deg}", n => Patterns.Rotate(n, deg));
            };

            ScaleButton.LeftClick += (s, e) =>
            {
                float scale = ScaleBox.Value;
                NoteManager.Edit($"SCALE {scale}%", n => Patterns.Scale(n, scale));
            };

            ImportIni.LeftClick += (s, e) =>
            {
                DialogResult result = new OpenFileDialog()
                {
                    Title = "Select .ini File",
                    Filter = "Map Property Files (*.ini)|*.ini"
                }.Show(Settings.defaultPath, out string fileName);

                if (result == DialogResult.OK)
                    INI.Read(fileName);
            };

            bool playerRunning = false;

            void Playtest(bool fromStart)
            {
                if (MusicPlayer.IsPlaying)
                    MusicPlayer.Pause();

                if (!playerRunning && PlatformUtils.ExecutableExists("SSQE Player"))
                {
                    Settings.Save();
                    TXT.Write(Path.Combine(Assets.TEMP, "tempmap.txt"));

                    Process process = PlatformUtils.RunExecutable("SSQE Player", $"{fromStart} false {KeybindManager.AltHeld}");
                    playerRunning = process != null;

                    if (process != null)
                    {
                        process.EnableRaisingEvents = true;
                        process.Exited += delegate { playerRunning = false; };
                    }
                }
            }

            PlayMap.LeftClick += (s, e) => Playtest(false);
            FromStart.LeftClick += (s, e) => Playtest(true);

            CopyBookmarks.LeftClick += (s, e) => Mapping.CopyBookmarks();
            PasteBookmarks.LeftClick += (s, e) => Mapping.PasteBookmarks();

            SaveButton.LeftClick += (s, e) =>
            {
                if (Mapping.Save())
                    ShowToast("SAVED", Settings.color1.Value);
            };
            SaveButton.BindKeybind("save");

            SaveAsButton.LeftClick += (s, e) =>
            {
                if (Mapping.SaveAs())
                    ShowToast("SAVED", Settings.color1.Value);
            };
            SaveAsButton.BindKeybind("saveAs");

            SwapClickMode.LeftClick += (s, e) => Settings.selectTool.Value ^= true;
            SwapClickMode.BindKeybind("switchClickTool");

            ReplaceID.LeftClick += (s, e) =>
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(ReplaceIDBox.Text))
                    {
                        DialogResult result = MessageBox.Show("Are you sure you want to replace this ID?\nIf an asset exists with this ID, it will be overwritten and this map will be saved.", MBoxIcon.Warning, MBoxButtons.Yes_No);
                        if (result != DialogResult.Yes)
                            return;

                        string id = ReplaceIDBox.Text;
                        MusicPlayer.Reset();

                        File.Move(Path.Combine(Assets.CACHED, $"{Mapping.Current.SoundID}.asset"), Path.Combine(Assets.CACHED, $"{id}.asset"));
                        Mapping.Current.SoundID = id;

                        Mapping.LoadAudio(id);
                        MusicPlayer.Volume = Settings.masterVolume.Value.Value;

                        if (Mapping.Current.FileName != null)
                            Mapping.Save();
                        else
                            Mapping.Autosave();

                        ShowToast($"REPLACED AUDIO ID WITH {id}", Settings.color1.Value);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Log("Failed to replace audio ID", LogSeverity.WARN, ex);
                    ShowToast("FAILED TO REPLACE ID", Settings.color1.Value);
                }
            };

            ExportButton.LeftClick += (s, e) =>
            {
                Windowing.OpenDialog(new GuiDialogExport());
                return;

                switch (Settings.exportType.Value.Current)
                {
                    case "Rhythia (SSPM)":
                        ExportSSPM.ShowWindow();
                        break;
                    case "Nova (NPK)":
                        ExportNOVA.ShowWindow();
                        break;
                }
            };

            Dictionary<string, (float, Dictionary<string, float>)> srCache = [];
            bool calculating = false;

            CalculateSR.LeftClick += (s, e) =>
            {
                if (calculating)
                    return;
                calculating = true;

                Task.Run(() =>
                {
                    string data = TXT.CopyLegacy();
                    byte[] bytes = Encoding.UTF8.GetBytes(data[data.IndexOf(',')..]);
                    string hash = Encoding.UTF8.GetString(SHA512.HashData(bytes));

                    float sr;
                    Dictionary<string, float> rp;

                    if (srCache.TryGetValue(hash, out (float, Dictionary<string, float>) value))
                    {
                        sr = value.Item1;
                        rp = value.Item2;
                    }
                    else
                    {
                        Networking.GetDifficultyMetrics(data, out sr, out rp);
                        srCache.Add(hash, (sr, rp));
                    }

                    string result = sr > 0 ? $"SR: {Math.Round(sr, 2)}\n" : "";
                    string suffix = sr > 0 ? "RP" : "";

                    foreach (KeyValuePair<string, float> kvp in rp)
                        result += $"\n{kvp.Key}: {Math.Round(kvp.Value, 2)} {suffix}";

                    SRLabel.Text = result;
                    calculating = false;
                });
            };

            void ApproachRateChanged() => ApproachRateLabel.Text = $"Approach Rate: {Math.Round(Settings.approachRate.Value.Value + 1)}";
            ApproachRate.ValueChanged += (s, e) => ApproachRateChanged();
            ApproachRateChanged();

            void PlayerApproachRateChanged() => PlayerApproachRateLabel.Text = $"Player Approach Rate: {Math.Round(Settings.playerApproachRate.Value.Value + 1)}";
            PlayerApproachRate.ValueChanged += (s, e) => PlayerApproachRateChanged();
            PlayerApproachRateChanged();

            void SnappingChanged() => SnappingLabel.Text = $"Snapping: 3/{Math.Round(Settings.quantumSnapping.Value.Value + 3)}";
            QuantumSnapDivisor.ValueChanged += (s, e) => SnappingChanged();
            SnappingChanged();

            void CursorPosChanged() => CursorPosLabel.Text = $"Cursor Position: {Math.Abs(Math.Round(Settings.cursorPos.Value.Value))}%";
            TrackCursorPos.ValueChanged += (s, e) => CursorPosChanged();
            CursorPosChanged();

            void TempoChanged() => TempoLabel.Text = $"PLAYBACK SPEED - {Math.Round(Mapping.Current.Tempo * 100)}%";
            Tempo.ValueChanged += (s, e) => TempoChanged();
            TempoChanged();

            void MusicVolumeChanged() => MusicVolumeValueLabel.Text = $"{Math.Abs(Math.Round(Settings.masterVolume.Value.Value * 100))}";
            MusicVolume.ValueChanged += (s, e) => MusicVolumeChanged();
            MusicVolumeChanged();

            void SfxVolumeChanged() => SfxVolumeValueLabel.Text = $"{Math.Abs(Math.Round(Settings.sfxVolume.Value.Value * 100))}";
            SfxVolume.ValueChanged += (s, e) => SfxVolumeChanged();
            SfxVolumeChanged();

            void GameChanged()
            {
                switch (Settings.modchartGame.Value.Current)
                {
                    case "Nova":
                        SpecialNavNova.Visible = true;
                        break;
                    case "Rhythia":
                        SpecialNavNova.Visible = false;
                        break;
                }
            }

            GameSwitch.LeftClick += (s, e) => GameChanged();
            GameSwitch.RightClick += (s, e) => GameChanged();
            GameChanged();

            EditSpecial.LeftClick += (s, e) =>
            {
                StandardMapNavs.Visible = false;
                SpecialMapNavs.Visible = true;

                Mapping.RenderMode = ObjectRenderMode.Special;
            };

            SpecialNavExit.LeftClick += (s, e) =>
            {
                StandardMapNavs.Visible = true;
                SpecialMapNavs.Visible = false;

                Mapping.RenderMode = ObjectRenderMode.Notes;
            };

            SpecialNavController.SelectionChanged += (s, e) =>
            {
                IndividualObjectMode mode = e.Value switch
                {
                    "Beats" => IndividualObjectMode.Beat,
                    "Mines" => IndividualObjectMode.Mine,
                    "Glides" => IndividualObjectMode.Glide,
                    "Lyrics" => IndividualObjectMode.Lyric,
                    _ => IndividualObjectMode.Disabled
                };

                LyricNav.Visible = e.Value == "Lyrics";
                LyricPreview.Visible = e.Value == "Lyrics" || mode == IndividualObjectMode.Disabled;

                Mapping.ObjectMode = mode;
            };

            SpecialNavController.Initialize();

            Numpad.LeftClick += (s, e) => Settings.RefreshKeyMapping();

            void CreateLyric(string text)
            {
                List<MapObject> selected = Mapping.Current.SpecialObjects.Selected;
                long ms = Timing.GetClosestBeat(Settings.currentTime.Value.Value);
                ms = (long)Math.Clamp(ms >= 0 ? ms : Settings.currentTime.Value.Value, 0, Settings.currentTime.Value.Max);
                bool fadeIn = LyricFadeIn.Toggle;
                bool fadeOut = LyricFadeOut.Toggle;

                if (selected.Count == 1 && selected[0] is Lyric lyric)
                {
                    SpecialObjectManager.Edit("EDIT LYRIC", [lyric], n =>
                    {
                        if (n is Lyric lyric)
                        {
                            lyric.Text = text;
                            lyric.FadeIn = fadeIn;
                            lyric.FadeOut = fadeOut;
                        }
                    });
                }
                else
                {
                    Lyric toAdd = new(ms, text, fadeIn, fadeOut);
                    SpecialObjectManager.Add("ADD LYRIC", toAdd);

                    if (Settings.autoAdvance.Value)
                        Timing.Advance();
                }

                Mapping.ClearSelection();
                LyricBox.Text = "";
                LyricFadeIn.Toggle = false;
                LyricFadeOut.Toggle = false;
            }

            LyricCreate.LeftClick += (s, e) => CreateLyric(LyricBox.Text);
            LyricBox.TextEntered += (s, e) =>
            {
                CreateLyric(e.Text);
                LyricBox.Focused = true;
            };
            LyricFadeIn.LeftClick += (s, e) => LyricFadeOut.Toggle = false;
            LyricFadeOut.LeftClick += (s, e) => LyricFadeIn.Toggle = false;

            MusicMute.LeftClick += (s, e) => MusicPlayer.Volume = Settings.masterVolume.Value.Value;
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            SliderSetting currentTime = Settings.currentTime.Value;

            ZoomValueLabel.Text = $"{Math.Round(Mapping.Current.Zoom * 100)}%";
            ClickModeLabel.Text = $"Click: {(Mapping.ClickMode == ClickMode.Select ? "Select" : "Place")}";
            ClickModeLabel.Visible = Mapping.ClickMode != ClickMode.Both;

            BeatDivisorLabel.Text = $"Beat Divisor: {Math.Round(Settings.beatDivisor.Value.Value * 10) / 10 + 1}";

            CurrentTimeLabel.Text = $"{(int)(currentTime.Value / 60000)}:{(int)(currentTime.Value % 60000 / 1000):0#}";
            TotalTimeLabel.Text = $"{(int)(currentTime.Max / 60000)}:{(int)(currentTime.Max % 60000 / 1000):0#}";

            float progress = currentTime.Value / currentTime.Max;
            RectangleF timelineRect = Timeline.GetRect();
            RectangleF currentMsRect = CurrentMsLabel.GetRect();

            CurrentMsLabel.SetRect(timelineRect.X + timelineRect.Height / 2 + (timelineRect.Width - timelineRect.Height) * progress - currentMsRect.Width / 2, currentMsRect.Y, currentMsRect.Width, currentMsRect.Height);
            CurrentMsLabel.Text = $"{(long)currentTime.Value:##,##0}ms";

            NotesLabel.Text = Mapping.RenderMode switch
            {
                ObjectRenderMode.Notes => $"{Mapping.Current.Notes.Count} Notes",
                ObjectRenderMode.VFX => $"{Mapping.Current.VfxObjects.Count} Objects",
                ObjectRenderMode.Special => $"{Mapping.Current.SpecialObjects.Count} Objects",
                _ => ""
            };

            if (LyricPreview.Visible)
            {
                LyricCreate.Text = Mapping.Current.SpecialObjects.Selected.Count == 1 ? "EDIT" : "CREATE";

                Lyric? prev = null;
                Lyric? current = null;
                Lyric? next = null;

                string text = "";
                float alpha = 1;

                Lyric[] lyrics = Mapping.Current.SpecialObjects.Where(n => n is Lyric).Cast<Lyric>().ToArray();

                for (int i = 0; i < lyrics.Length; i++)
                {
                    prev = current;
                    current = lyrics[i];
                    if (current.Ms > currentTime.Value)
                        break;

                    if (i + 1 < lyrics.Length)
                        next = lyrics[i + 1];
                    else
                        next = null;


                    if (current.FadeOut)
                        alpha = 1 - Math.Clamp((currentTime.Value - current.Ms) / 1000, 0, 1);
                    else if (current.FadeIn)
                        alpha = Math.Clamp((currentTime.Value - current.Ms) / 1000, 0, 1);
                    else
                        alpha = 1;

                    if (!string.IsNullOrWhiteSpace(current.Text))
                    {
                        if (prev != null && string.IsNullOrWhiteSpace(prev.Text))
                        {
                            text = current.Text;
                            if (text.StartsWith('-'))
                                text = text[1..];
                        }
                        else if (current.Text.StartsWith('-'))
                            text = current.Text[1..];
                        else if (text.EndsWith('-'))
                            text = text[..^1] + current.Text;
                        else if (!string.IsNullOrWhiteSpace(text))
                            text += ' ' + current.Text;
                        else
                            text = current.Text;
                    }
                    else
                        text = current.Text;
                }

                if (text.EndsWith('-'))
                    text = text[..^1];

                LyricPreview.Text = text;
                LyricPreview.SetColor(null, alpha);
            }

            base.Render(mousex, mousey, frametime);
        }

        public static void ShowToast(string text, Color? color = null)
        {
            ToastLabel.Show(text, color);
        }

        public override void MouseScroll(float delta)
        {
            base.MouseScroll(delta);

            if (KeybindManager.ShiftHeld)
            {
                SliderSetting setting = Settings.beatDivisor.Value;
                float step = setting.Step * (KeybindManager.CtrlHeld ? 1 : 2) * delta;

                setting.Value = Math.Clamp(setting.Value + step, 0f, setting.Max);
                BeatSnapDivisor.Update();
            }
            else if (KeybindManager.CtrlHeld)
                Mapping.IncrementZoom(delta);
            else
                Timing.Scroll(delta < 0 ^ Settings.reverseScroll.Value, Math.Abs(delta));
        }

        public override void FileDrop(string file)
        {
            if (Path.GetExtension(file) == ".ini")
                INI.Read(file);
            else
                base.FileDrop(file);
        }
    }
}
