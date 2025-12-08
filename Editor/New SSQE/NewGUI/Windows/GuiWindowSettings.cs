using New_SSQE.Audio;
using New_SSQE.ExternalUtils;
using New_SSQE.Misc.Dialogs;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;
using New_SSQE.Preferences;
using System.Drawing;

namespace New_SSQE.NewGUI.Windows
{
    internal partial class GuiWindowSettings : GuiWindow
    {
        public GuiWindowSettings() : base(BackgroundSquare, NavController, BackButton, ResetButton, OpenDirectoryButton, KeybindsButton)
        {
            RefreshNoteColors();
        }

        public override void Close()
        {
            base.Close();

            NavController.Disconnect();
        }

        private static void RefreshNoteColors()
        {
            List<Color> colors = Settings.noteColors.Value;
            GuiSquare[] squares = new GuiSquare[colors.Count];

            RectangleF rect = NoteColorSquares.GetOrigin();
            float colorWidth = rect.Width / colors.Count;

            for (int i = 0; i < colors.Count; i++)
                squares[i] = new(colorWidth * i, 0, colorWidth, rect.Height) { Color = colors[i] };

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
                    ResetButton.Text = "RESET TO DEFAULT";
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

            Color1Square.Color = Settings.color1.Value;
            Color2Square.Color = Settings.color2.Value;
            Color3Square.Color = Settings.color3.Value;
            Color4Square.Color = Settings.color4.Value;
            Color5Square.Color = Settings.color5.Value;

            int editorBGOpacity = (int)Math.Clamp(Settings.editorBGOpacity.Value, 0, 255);
            EditorBGOpacitySquare.Color = Color.FromArgb(editorBGOpacity, 255, 255, 255);

            int gridOpacity = (int)Math.Clamp(Settings.gridOpacity.Value, 0, 255);
            GridOpacitySquare.Color = Color.FromArgb(gridOpacity, 255, 255, 255);

            int trackOpacity = (int)Math.Clamp(Settings.trackOpacity.Value, 0, 255);
            TrackOpacitySquare.Color = Color.FromArgb(trackOpacity, 255, 255, 255);

            SliderSetting fpsLimit = Settings.fpsLimit.Value;

            FPSLimitLabel.Text = $"FPS Limit: {(Math.Round(fpsLimit.Value) == Math.Round(fpsLimit.Max) ? "Unlimited" : Math.Round(fpsLimit.Value + 60))}";
            RestartLabel.Visible = MainWindow.MSAA != Settings.msaa.Value;

            base.Render(mousex, mousey, frametime);
        }

        public override void ConnectEvents()
        {
            BackButton.LeftClick += (s, e) =>
            {
                Settings.Save();
                Windowing.Open<GuiWindowMenu>();
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
                    ResetButton.Text = "RESET TO DEFAULT";
                }
                else
                {
                    resetQuery = true;
                    ResetButton.Text = "ARE YOU SURE?";

                    RunResetQuery();
                }
            };

            OpenDirectoryButton.LeftClick += (s, e) => Platforms.OpenDirectory();
            KeybindsButton.LeftClick += (s, e) => Windowing.Open<GuiWindowKeybinds>();

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

            FontScaleTextbox.ValueChanged += (s, e) => Update();

            GridSquircleDetailTextbox.ValueChanged += (s, e) =>
            {
                GridSquirclePreview.CornerDetail = (int)e.Value;
                GridSquirclePreview.Update();
            };

            GridSquircleRadiusTextbox.ValueChanged += (s, e) =>
            {
                GridSquirclePreview.CornerRadius = e.Value;
                GridSquirclePreview.Update();
            };

            PlaytestGameController.Initialize();

            GradientCheckbox.ValueChanged += (s, e) => Update();
        }
    }
}
