using New_SSQE.Misc.Static;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;
using New_SSQE.NewGUI.Input;
using New_SSQE.Preferences;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing;

namespace New_SSQE.NewGUI.Windows
{
    internal class GuiWindowKeybinds : GuiWindow
    {
        public static readonly GuiButton BackButton = new(655, 930, 600, 100, "RETURN TO SETTINGS", 52, "square");
        public static readonly GuiCheckbox CtrlIndicator = new(64, 828, 64, 64, null, "CTRL Held", 36) { Style = ControlStyle.Checkbox_Uncolored };
        public static readonly GuiCheckbox AltIndicator = new(64, 912, 64, 64, null, "ALT Held", 36) { Style = ControlStyle.Checkbox_Uncolored };
        public static readonly GuiCheckbox ShiftIndicator = new(64, 996, 64, 64, null, "SHIFT Held", 36) { Style = ControlStyle.Checkbox_Uncolored };
        public static readonly GuiLabel StaticKeysLabel = new(480, 375, 960, 555, null, "", 20);



        public static readonly GuiLabel HFlipLabel = new(1366, 100, 128, 26, null, "Horizontal Flip", 28, "main", CenterMode.None);
        public static readonly GuiLabel HFlipCAS = new(1642, 130, 256, 40, null, "", 28, "main", CenterMode.None);
        public static readonly GuiTextboxKeybind HFlipBox = new(1366, 130, 128, 40, Settings.hFlip, "", 28) { Style = ControlStyle.Textbox_Uncolored };
        public static readonly GuiButton HFlipReset = new(1504, 130, 128, 40, "RESET", 28);

        public static readonly GuiLabel VFlipLabel = new(1366, 180, 128, 26, null, "Vertical Flip", 28, "main", CenterMode.None);
        public static readonly GuiLabel VFlipCAS = new(1642, 210, 256, 40, null, "", 28, "main", CenterMode.None);
        public static readonly GuiTextboxKeybind VFlipBox = new(1366, 210, 128, 40, Settings.vFlip, "", 28) { Style = ControlStyle.Textbox_Uncolored };
        public static readonly GuiButton VFlipReset = new(1504, 210, 128, 40, "RESET", 28);

        public static readonly GuiLabel StoreNodesLabel = new(1366, 260, 128, 26, null, "Store Bezier Nodes", 28, "main", CenterMode.None);
        public static readonly GuiLabel StoreNodesCAS = new(1642, 290, 256, 40, null, "", 28, "main", CenterMode.None);
        public static readonly GuiTextboxKeybind StoreNodesBox = new(1366, 290, 128, 40, Settings.storeNodes, "", 28) { Style = ControlStyle.Textbox_Uncolored };
        public static readonly GuiButton StoreNodesReset = new(1504, 290, 128, 40, "RESET", 28);

        public static readonly GuiLabel AnchorNodeLabel = new(1366, 340, 128, 26, null, "Anchor Bezier Node", 28, "main", CenterMode.None);
        public static readonly GuiLabel AnchorNodeCAS = new(1642, 370, 256, 40, null, "", 28, "main", CenterMode.None);
        public static readonly GuiTextboxKeybind AnchorNodeBox = new(1366, 370, 128, 40, Settings.anchorNode, "", 28) { Style = ControlStyle.Textbox_Uncolored };
        public static readonly GuiButton AnchorNodeReset = new(1504, 370, 128, 40, "RESET", 28);

        public static readonly GuiLabel DrawBezierLabel = new(1366, 420, 128, 26, null, "Draw Bezier Curve", 28, "main", CenterMode.None);
        public static readonly GuiLabel DrawBezierCAS = new(1642, 450, 256, 40, null, "", 28, "main", CenterMode.None);
        public static readonly GuiTextboxKeybind DrawBezierBox = new(1366, 450, 128, 40, Settings.drawBezier, "", 28) { Style = ControlStyle.Textbox_Uncolored };
        public static readonly GuiButton DrawBezierReset = new(1504, 450, 128, 40, "RESET", 28);

        public static readonly GuiLabel SwitchClickLabel = new(150, 100, 128, 26, null, "Switch Click Function", 28, "main", CenterMode.None);
        public static readonly GuiLabel SwitchClickCAS = new(426, 130, 256, 40, null, "", 28, "main", CenterMode.None);
        public static readonly GuiTextboxKeybind SwitchClickBox = new(150, 130, 128, 40, Settings.switchClickTool, "", 28) { Style = ControlStyle.Textbox_Uncolored };
        public static readonly GuiButton SwitchClickReset = new(288, 130, 128, 40, "RESET", 28);

        public static readonly GuiLabel ToggleQuantumLabel = new(150, 180, 128, 26, null, "Toggle Quantum", 28, "main", CenterMode.None);
        public static readonly GuiLabel ToggleQuantumCAS = new(426, 210, 256, 40, null, "", 28, "main", CenterMode.None);
        public static readonly GuiTextboxKeybind ToggleQuantumBox = new(150, 210, 128, 40, Settings.quantum, "", 28) { Style = ControlStyle.Textbox_Uncolored };
        public static readonly GuiButton ToggleQuantumReset = new(288, 210, 128, 40, "RESET", 28);



        public static readonly GuiLabel OpenTimingsLabel = new(150, 300, 128, 26, null, "Open Timings", 28, "main", CenterMode.None);
        public static readonly GuiLabel OpenTimingsCAS = new(426, 330, 256, 40, null, "", 28, "main", CenterMode.None);
        public static readonly GuiTextboxKeybind OpenTimingsBox = new(150, 330, 128, 40, Settings.openTimings, "", 28) { Style = ControlStyle.Textbox_Uncolored };
        public static readonly GuiButton OpenTimingsReset = new(288, 330, 128, 40, "RESET", 28);

        public static readonly GuiLabel OpenBookmarksLabel = new(150, 380, 128, 26, null, "Open Bookmarks", 28, "main", CenterMode.None);
        public static readonly GuiLabel OpenBookmarksCAS = new(426, 410, 256, 40, null, "", 28, "main", CenterMode.None);
        public static readonly GuiTextboxKeybind OpenBookmarksBox = new(150, 410, 128, 40, Settings.openBookmarks, "", 28) { Style = ControlStyle.Textbox_Uncolored };
        public static readonly GuiButton OpenBookmarksReset = new(288, 410, 128, 40, "RESET", 28);

        public static readonly GuiLabel OpenDirectoryLabel = new(150, 460, 128, 26, null, "Open Directory", 28, "main", CenterMode.None);
        public static readonly GuiLabel OpenDirectoryCAS = new(426, 490, 256, 40, null, "", 28, "main", CenterMode.None);
        public static readonly GuiTextboxKeybind OpenDirectoryBox = new(150, 490, 128, 40, Settings.openDirectory, "", 28) { Style = ControlStyle.Textbox_Uncolored };
        public static readonly GuiButton OpenDirectoryReset = new(288, 490, 128, 40, "RESET", 28);

        public static readonly GuiLabel ExportSSPMLabel = new(150, 540, 128, 26, null, "Export SSPM", 28, "main", CenterMode.None);
        public static readonly GuiLabel ExportSSPMCAS = new(426, 570, 256, 40, null, "", 28, "main", CenterMode.None);
        public static readonly GuiTextboxKeybind ExportSSPMBox = new(150, 570, 128, 40, Settings.exportSSPM, "", 28) { Style = ControlStyle.Textbox_Uncolored };
        public static readonly GuiButton ExportSSPMReset = new(288, 570, 128, 40, "RESET", 28);

        public static readonly GuiLabel CreateBPMLabel = new(150, 620, 128, 26, null, "Create Timing Point", 28, "main", CenterMode.None);
        public static readonly GuiLabel CreateBPMCAS = new(426, 650, 256, 40, null, "", 28, "main", CenterMode.None);
        public static readonly GuiTextboxKeybind CreateBPMBox = new(150, 650, 128, 40, Settings.createBPM, "", 28) { Style = ControlStyle.Textbox_Uncolored };
        public static readonly GuiButton CreateBPMReset = new(288, 650, 128, 40, "RESET", 28);



        public static readonly GuiLabel GridLabel = new(778, 49, 128, 26, null, "Grid", 28, "main", CenterMode.None);
        public static readonly GuiTextboxGridKeybind Grid0Box = new(778, 75, 128, 40, 0, "", 28) { Style = ControlStyle.Textbox_Uncolored };
        public static readonly GuiButton Grid0Reset = new(778, 119, 128, 40, "RESET", 36);
        public static readonly GuiTextboxGridKeybind Grid1Box = new(916, 75, 128, 40, 1, "", 28) { Style = ControlStyle.Textbox_Uncolored };
        public static readonly GuiButton Grid1Reset = new(916, 119, 128, 40, "RESET", 36);
        public static readonly GuiTextboxGridKeybind Grid2Box = new(1054, 75, 128, 40, 2, "", 28) { Style = ControlStyle.Textbox_Uncolored };
        public static readonly GuiButton Grid2Reset = new(1054, 119, 128, 40, "RESET", 36);
        public static readonly GuiTextboxGridKeybind Grid3Box = new(778, 169, 128, 40, 3, "", 28) { Style = ControlStyle.Textbox_Uncolored };
        public static readonly GuiButton Grid3Reset = new(778, 213, 128, 40, "RESET", 36);
        public static readonly GuiTextboxGridKeybind Grid4Box = new(916, 169, 128, 40, 4, "", 28) { Style = ControlStyle.Textbox_Uncolored };
        public static readonly GuiButton Grid4Reset = new(916, 213, 128, 40, "RESET", 36);
        public static readonly GuiTextboxGridKeybind Grid5Box = new(1054, 169, 128, 40, 5, "", 28) { Style = ControlStyle.Textbox_Uncolored };
        public static readonly GuiButton Grid5Reset = new(1054, 213, 128, 40, "RESET", 36);
        public static readonly GuiTextboxGridKeybind Grid6Box = new(778, 263, 128, 40, 6, "", 28) { Style = ControlStyle.Textbox_Uncolored };
        public static readonly GuiButton Grid6Reset = new(778, 307, 128, 40, "RESET", 36);
        public static readonly GuiTextboxGridKeybind Grid7Box = new(916, 263, 128, 40, 7, "", 28) { Style = ControlStyle.Textbox_Uncolored };
        public static readonly GuiButton Grid7Reset = new(916, 307, 128, 40, "RESET", 36);
        public static readonly GuiTextboxGridKeybind Grid8Box = new(1054, 263, 128, 40, 8, "", 28) { Style = ControlStyle.Textbox_Uncolored };
        public static readonly GuiButton Grid8Reset = new(1054, 307, 128, 40, "RESET", 36);



        public static readonly GuiSquareTextured BackgroundSquare = new("menubg", Path.Combine(Assets.THIS, "background_menu.png"), Color.FromArgb(30, 30, 30)) { Stretch = StretchMode.XY };

        public GuiWindowKeybinds() : base(BackgroundSquare, BackButton, CtrlIndicator, AltIndicator, ShiftIndicator, StaticKeysLabel,
            HFlipLabel, HFlipCAS, HFlipBox, HFlipReset, VFlipLabel, VFlipCAS, VFlipBox, VFlipReset, StoreNodesLabel, StoreNodesCAS, StoreNodesBox, StoreNodesReset,
            AnchorNodeLabel, AnchorNodeCAS, AnchorNodeBox, AnchorNodeReset, DrawBezierLabel, DrawBezierCAS, DrawBezierBox, DrawBezierReset,
            SwitchClickLabel, SwitchClickCAS, SwitchClickBox, SwitchClickReset, ToggleQuantumLabel, ToggleQuantumCAS, ToggleQuantumBox, ToggleQuantumReset,
            OpenTimingsLabel, OpenTimingsCAS, OpenTimingsBox, OpenTimingsReset, OpenBookmarksLabel, OpenBookmarksCAS, OpenBookmarksBox, OpenBookmarksReset,
            OpenDirectoryLabel, OpenDirectoryCAS, OpenDirectoryBox, OpenDirectoryReset, ExportSSPMLabel, ExportSSPMCAS, ExportSSPMBox, ExportSSPMReset,
            CreateBPMLabel, CreateBPMCAS, CreateBPMBox, CreateBPMReset, GridLabel, Grid0Box,
            Grid0Reset, Grid1Box, Grid1Reset, Grid2Box, Grid2Reset, Grid3Box, Grid3Reset, Grid4Box, Grid4Reset,
            Grid5Box, Grid5Reset, Grid6Box, Grid6Reset, Grid7Box, Grid7Reset, Grid8Box, Grid8Reset)
        {
            StaticKeysLabel.SetText(string.Join('\n',
                "Static keybinds:",
                "",
                "> Zoom: CTRL + SCROLL",
                "",
                "> Beat Divisor: SHIFT + SCROLL",
                ">> Hold CTRL to increment by 0.5",
                "",
                "> Seek: SCROLL/LEFT/RIGHT",
                "> Play/Pause: SPACE",
                "",
                "> Select all: CTRL + A",
                "> Deselect all: ESCAPE",
                "",
                "> Delete: DELETE/BACKSPACE",
                "> Copy: CTRL + C",
                "> Paste: CTRL + V",
                "> Cut: CTRL + X",
                "> Undo: CTRL + Z",
                "> Redo: CTRL + Y",
                "",
                "> Fullscreen: F11",
                "> Save: CTRL + S",
                "> Save as: CTRL + SHIFT + S",
                "",
                "> Place stored patterns: 0-9",
                ">> Hold SHIFT to store selected notes as the key's pattern",
                ">> Hold CTRL to clear the key's pattern"
            ));
        }

        public override void ConnectEvents()
        {
            BackButton.LeftClick += (s, e) => Windowing.SwitchWindow(new GuiWindowSettings());

            void ResetKeybind(Setting<Keybind> setting, Keybind keybind, GuiTextboxKeybind control)
            {
                setting.Value = keybind;
                control.SetText(keybind.Key.ToString().ToUpper());
            }

            HFlipReset.LeftClick += (s, e) => ResetKeybind(Settings.hFlip, new(Keys.H, false, false, true), HFlipBox);
            VFlipReset.LeftClick += (s, e) => ResetKeybind(Settings.vFlip, new(Keys.V, false, false, true), VFlipBox);
            StoreNodesReset.LeftClick += (s, e) => ResetKeybind(Settings.storeNodes, new(Keys.S, false, false, true), StoreNodesBox);
            AnchorNodeReset.LeftClick += (s, e) => ResetKeybind(Settings.anchorNode, new(Keys.A, false, false, true), AnchorNodeBox);
            DrawBezierReset.LeftClick += (s, e) => ResetKeybind(Settings.drawBezier, new(Keys.D, false, false, true), DrawBezierBox);
            SwitchClickReset.LeftClick += (s, e) => ResetKeybind(Settings.switchClickTool, new(Keys.Tab, false, false, false), SwitchClickBox);
            ToggleQuantumReset.LeftClick += (s, e) => ResetKeybind(Settings.quantum, new(Keys.Q, true, false, false), ToggleQuantumBox);
            OpenTimingsReset.LeftClick += (s, e) => ResetKeybind(Settings.openTimings, new(Keys.T, true, false, false), OpenTimingsBox);
            OpenBookmarksReset.LeftClick += (s, e) => ResetKeybind(Settings.openBookmarks, new(Keys.B, true, false, false), OpenBookmarksBox);
            OpenDirectoryReset.LeftClick += (s, e) => ResetKeybind(Settings.openDirectory, new(Keys.D, true, false, true), OpenDirectoryBox);
            ExportSSPMReset.LeftClick += (s, e) => ResetKeybind(Settings.exportSSPM, new(Keys.E, true, true, false), ExportSSPMBox);
            CreateBPMReset.LeftClick += (s, e) => ResetKeybind(Settings.createBPM, new(Keys.B, true, false, true), CreateBPMBox);

            void ResetGridKey(int index, Keys key, GuiTextboxGridKeybind control)
            {
                Settings.gridKeys.Value[index] = key;
                control.SetText(key.ToString().ToUpper());
            }

            Grid0Reset.LeftClick += (s, e) => ResetGridKey(0, Keys.Q, Grid0Box);
            Grid1Reset.LeftClick += (s, e) => ResetGridKey(1, Keys.W, Grid1Box);
            Grid2Reset.LeftClick += (s, e) => ResetGridKey(2, Keys.E, Grid2Box);
            Grid3Reset.LeftClick += (s, e) => ResetGridKey(3, Keys.A, Grid3Box);
            Grid4Reset.LeftClick += (s, e) => ResetGridKey(4, Keys.S, Grid4Box);
            Grid5Reset.LeftClick += (s, e) => ResetGridKey(5, Keys.D, Grid5Box);
            Grid6Reset.LeftClick += (s, e) => ResetGridKey(6, Keys.Z, Grid6Box);
            Grid7Reset.LeftClick += (s, e) => ResetGridKey(7, Keys.X, Grid7Box);
            Grid8Reset.LeftClick += (s, e) => ResetGridKey(8, Keys.C, Grid8Box);
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            CtrlIndicator.Toggle = KeybindManager.CtrlHeld;
            AltIndicator.Toggle = KeybindManager.AltHeld;
            ShiftIndicator.Toggle = KeybindManager.ShiftHeld;

            HFlipCAS.SetText(CAS(Settings.hFlip.Value));
            VFlipCAS.SetText(CAS(Settings.vFlip.Value));
            SwitchClickCAS.SetText(CAS(Settings.switchClickTool.Value));
            ToggleQuantumCAS.SetText(CAS(Settings.quantum.Value));
            OpenTimingsCAS.SetText(CAS(Settings.openTimings.Value));
            OpenBookmarksCAS.SetText(CAS(Settings.openBookmarks.Value));
            StoreNodesCAS.SetText(CAS(Settings.storeNodes.Value));
            DrawBezierCAS.SetText(CAS(Settings.drawBezier.Value));
            AnchorNodeCAS.SetText(CAS(Settings.anchorNode.Value));
            OpenDirectoryCAS.SetText(CAS(Settings.openDirectory.Value));
            ExportSSPMCAS.SetText(CAS(Settings.exportSSPM.Value));
            CreateBPMCAS.SetText(CAS(Settings.createBPM.Value));

            base.Render(mousex, mousey, frametime);
        }

        private static string CAS(Keybind keybind)
        {
            List<string> cas = [];

            if (keybind.Ctrl)
                cas.Add("CTRL");
            if (keybind.Alt)
                cas.Add("ALT");
            if (keybind.Shift)
                cas.Add("SHIFT");

            return string.Join(" + ", cas);
        }
    }
}
