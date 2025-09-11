using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;
using New_SSQE.NewGUI.Input;
using New_SSQE.Preferences;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace New_SSQE.NewGUI.Windows
{
    internal partial class GuiWindowKeybinds : GuiWindow
    {
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
            StaticKeysLabel.Text = string.Join('\n',
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
            );
        }

        public override void ConnectEvents()
        {
            BackButton.LeftClick += (s, e) => Windowing.SwitchWindow(new GuiWindowSettings());

            void ResetKeybind(Setting<Keybind> setting, Keybind keybind, GuiTextboxKeybind control)
            {
                setting.Value = keybind;
                control.Text = keybind.Key.ToString().ToUpper();
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
                control.Text = key.ToString().ToUpper();
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

            HFlipCAS.Text = CAS(Settings.hFlip.Value);
            VFlipCAS.Text = CAS(Settings.vFlip.Value);
            SwitchClickCAS.Text = CAS(Settings.switchClickTool.Value);
            ToggleQuantumCAS.Text = CAS(Settings.quantum.Value);
            OpenTimingsCAS.Text = CAS(Settings.openTimings.Value);
            OpenBookmarksCAS.Text = CAS(Settings.openBookmarks.Value);
            StoreNodesCAS.Text = CAS(Settings.storeNodes.Value);
            DrawBezierCAS.Text = CAS(Settings.drawBezier.Value);
            AnchorNodeCAS.Text = CAS(Settings.anchorNode.Value);
            OpenDirectoryCAS.Text = CAS(Settings.openDirectory.Value);
            ExportSSPMCAS.Text = CAS(Settings.exportSSPM.Value);
            CreateBPMCAS.Text = CAS(Settings.createBPM.Value);

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
