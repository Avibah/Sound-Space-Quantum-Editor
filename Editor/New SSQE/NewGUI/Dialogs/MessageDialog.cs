using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Controls;
using System.Drawing;

namespace New_SSQE.NewGUI.Dialogs
{
    public enum MBoxButtons
    {
        OK,
        Yes_No,
        Yes_No_Cancel,
        OK_Cancel
    }

    public enum MBoxIcon
    {
        Info,
        Warning,
        Error
    }

    internal class MessageDialog : GuiWindowDialog
    {
        private readonly Action<DialogResult>? callback;
        private bool closed = false;

        public readonly GuiSquareTextured IconSquare;

        public readonly GuiLabel InfoLabel;
        public readonly GuiButton[] Options;
        private readonly DialogResult[] results;

        private MessageDialog(string message, MBoxIcon icon, MBoxButtons buttons, Action<DialogResult>? callback) : base(735, 460, 450, 160)
        {
            IconSquare = new(10, 18, 44, 44, icon.ToString())
            {
                Color = Color.White,
                Stretch = StretchMode.None
            };

            InfoLabel = new(60, 0, 390, 110)
            {
                Text = message,
                TextSize = 18,
                TextColor = Color.Black,
                TextWrapped = true,
                CenterMode = CenterMode.Y,
                Font = "semibold"
            };

            BackgroundSquare.Color = Color.White;
            Modal = true;
            Persistent = true;

            string[] buttonSplit = buttons.ToString().Split('_');
            int offset = 3 - buttonSplit.Length;
            
            List<GuiButton> options = [];
            List<DialogResult> results = [];

            for (int i = 0; i < buttonSplit.Length; i++)
            {
                options.Add(new(198 + 85 * (i + offset), 128, 74, 24)
                {
                    Text = buttonSplit[i],
                    TextSize = 18,
                    TextColor = Color.Black,
                    TextWrapped = true,
                    Style = ControlStyle.Button_Light,
                    CornerRadius = 0,
                    Gradient = null,
                    LineThickness = 0.5f,
                    Font = "semibold"
                });

                results.Add(buttonSplit[i] switch
                {
                    "Yes" => DialogResult.Yes,
                    "No" => DialogResult.No,
                    "OK" => DialogResult.OK,
                    "Cancel" => DialogResult.Cancel,
                    _ => DialogResult.Cancel
                });
            }

            Options = [.. options];
            this.results = [.. results];
            this.callback = callback;

            SetControls([IconSquare, InfoLabel, .. Options]);
        }

        public override void ConnectEvents()
        {
            for (int i = 0; i < Options.Length; i++)
            {
                int index = i;
                Options[i].LeftClick += (s, e) => Close(results[index]);
            }

            base.ConnectEvents();
        }

        private void Close(DialogResult result)
        {
            if (!closed)
                callback?.Invoke(result);
            
            closed = true;
            CloseDialog();
        }

        public static MessageDialog Show(string message, MBoxIcon icon, MBoxButtons buttons, Action<DialogResult>? callback = null)
        {
            MessageDialog messageBox = new(message, icon, buttons, callback);
            Windowing.OpenDialog(messageBox);
            return messageBox;
        }
    }
}
