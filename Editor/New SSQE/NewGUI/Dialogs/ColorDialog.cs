using System.Drawing;
using Egorozh.ColorPicker.Dialog;
using Avalonia.Controls;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using New_SSQE.NewGUI;
using New_SSQE.NewGUI.Dialogs;

namespace New_SSQE.Misc.Dialogs
{
    internal class ColorDialog
    {
        public Color Color;

        private static DialogResult Result;
        private static TaskCompletionSource<bool>? tcs = new();

        public DialogResult ShowDialog()
        {
            Windowing.Disable();

            ColorPickerDialog dialog = new()
            {
                Color = Avalonia.Media.Color.FromArgb(Color.A, Color.R, Color.G, Color.B),
                Icon = new(new Bitmap(Assets.TexturesAt("Empty.png"))),
                Topmost = true
            };

            Result = DialogResult.Cancel;
            tcs = new();

            Button okControl = dialog.GetControl<Button>("btOk");
            okControl.Click += (s, e) =>
            {
                Result = DialogResult.OK;
                tcs.TrySetResult(true);
            };

            Button cancelControl = dialog.GetControl<Button>("btCancel");
            cancelControl.Click += (s, e) =>
            {
                Result = DialogResult.Cancel;
                tcs.TrySetResult(true);
            };

            dialog.Show();
            BackgroundWindow.YieldWindow(dialog);

            Color = Color.FromArgb(dialog.Color.A, dialog.Color.R, dialog.Color.G, dialog.Color.B);

            Windowing.Enable();
            return Result;
        }
    }
}
