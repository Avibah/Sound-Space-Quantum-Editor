using New_SSQE.Misc.Dialogs;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Font;
using New_SSQE.Preferences;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiPathBox : ControlContainer
    {
        public readonly GuiButton PathButton;
        public readonly GuiLabel PathLabel;
        public readonly GuiSquare PathBackdrop;
        public readonly GuiSquare PathOutline;

        private readonly Setting<string>? setting;
        private readonly string filter;

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

        private readonly int numChars;

        public GuiPathBox(float x, float y, float w, float h, string filter, Setting<string>? setting = null, string text = "", int textSize = 0, string font = "main", CenterMode centerMode = CenterMode.Y) : base(x, y, w, h)
        {
            PathButton = new(0, 0, w / 4, h, "CHOOSE", textSize, font) { Stretch = StretchMode.XY };
            PathLabel = new(w / 4 + 8, 0, w * 3 / 4 - 8, h, null, text, textSize, font, centerMode) { Stretch = StretchMode.XY };
            PathBackdrop = new(w / 4, 0, w * 3 / 4, h, PathButton.Style.Primary) { Stretch = StretchMode.XY };
            PathOutline = new(w / 4, 0, w * 3 / 4, h, PathButton.Style.Secondary, true) { Stretch = StretchMode.XY };

            SetControls(PathButton, PathLabel, PathBackdrop, PathOutline);

            this.setting = setting;
            this.filter = filter;
            _file = text;

            UpdateFile();

            numChars = (int)(PathLabel.GetRect().Width / FontRenderer.GetWidth("0", textSize, font)) / 2;
        }

        public override void Reset()
        {
            base.Reset();

            PathButton.LeftClick += (s, e) => ChooseFile();
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
            DialogResult result = new OpenFileDialog()
            {
                Title = "Choose File",
                Filter = filter,
                InitialDirectory = Path.GetDirectoryName(_file)
            }.Show(out string fileName);

            if (result == DialogResult.OK)
                SelectedFile = fileName;
        }
    }
}
