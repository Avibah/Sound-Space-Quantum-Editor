using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;
using New_SSQE.NewGUI.Dialogs;
using New_SSQE.NewGUI.Font;
using New_SSQE.Preferences;

namespace New_SSQE.NewGUI.CompoundControls
{
    internal class GuiPathBox : ControlContainer
    {
        public readonly GuiButton PathButton;
        public readonly GuiLabel PathLabel;
        public readonly GuiSquare PathBackdrop;
        public readonly GuiSquare PathOutline;

        private Setting<string>? folder;
        private Setting<string>? setting;
        private string filter = "All Files (*.*)|*.*";

        public string Filter
        {
            get => filter;
            set => filter = value;
        }

        private string _file;
        public string SelectedFile
        {
            get => _file;
            set
            {
                _file = value;
                UpdateFile();
            }
        }

        public new string Text
        {
            get => PathButton.Text;
            set => PathButton.Text = value;
        }

        public new float TextSize
        {
            get => PathLabel.TextSize;
            set
            {
                PathLabel.TextSize = value;
                PathButton.TextSize = value;
            }
        }

        public new string Font
        {
            get => PathLabel.Font;
            set
            {
                PathLabel.Font = value;
                PathButton.Font = value;
            }
        }

        public new CenterMode CenterMode
        {
            get => PathLabel.CenterMode;
            set => PathLabel.CenterMode = value;
        }

        public Setting<string>? Folder
        {
            get => folder;
            set
            {
                if (value != folder)
                {
                    folder = value;
                    shouldUpdate = true;
                }
            }
        }

        public Setting<string>? Setting
        {
            get => setting;
            set
            {
                if (value != setting)
                {
                    setting = value;
                    shouldUpdate = true;
                }
            }
        }

        private int numChars;

        public GuiPathBox(float x, float y, float w, float h) : base(x, y, w, h)
        {
            PathButton = new(0, 0, w / 4, h) { Stretch = StretchMode.XY };
            PathLabel = new(w / 4 + 8, 0, w * 3 / 4 - 8, h) { Stretch = StretchMode.XY };
            PathBackdrop = new(w / 4, 0, w * 3 / 4, h) { Stretch = StretchMode.XY, Color = PathButton.Style.Primary };
            PathOutline = new(w / 4, 0, w * 3 / 4, h) { Stretch = StretchMode.XY, Color = PathButton.Style.Secondary, Outline = true };

            SetControls(PathButton, PathLabel, PathBackdrop, PathOutline);

            _file = setting?.Value ?? "";
            UpdateFile();
        }

        public override void Reset()
        {
            base.Reset();

            PathButton.LeftClick += (s, e) => ChooseFile();
            SelectedFile = setting?.Value ?? "";
            numChars = (int)(PathLabel.Rect.Width / FontRenderer.GetWidth("0", TextSize, Font)) / 2;
        }

        private void UpdateFile()
        {
            if (!string.IsNullOrWhiteSpace(_file) && !File.Exists(_file))
                _file = "";
            if (setting != null)
                setting.Value = _file;

            int startLength = Math.Min(_file.Length, numChars);
            int endLength = Math.Clamp(_file.Length - numChars, 0, numChars);

            string start = _file[..startLength];
            string end = _file[(_file.Length - endLength)..];
            string final = start;

            if (!string.IsNullOrWhiteSpace(end))
                final += $"{(_file.Length > numChars * 2 ? "..." : "")}{end}";

            PathLabel.Text = final;
        }

        public void ChooseFile()
        {
            OpenFileDialog dialog = new()
            {
                Title = "Choose File",
                Filter = filter,
                InitialDirectory = Path.GetDirectoryName(_file)
            };

            DialogResult result;
            string fileName;

            if (folder == null)
                result = dialog.Show(out fileName);
            else
                result = dialog.Show(folder, out fileName);

            if (result == DialogResult.OK)
                SelectedFile = fileName;
        }
    }
}
