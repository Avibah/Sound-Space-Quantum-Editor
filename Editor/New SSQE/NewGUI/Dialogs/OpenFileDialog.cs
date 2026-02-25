using NativeFileDialogs.Net;
using New_SSQE.NewGUI;
using New_SSQE.NewGUI.Windows;
using New_SSQE.Preferences;
using New_SSQE.Services;


namespace New_SSQE.NewGUI.Dialogs
{
    internal class OpenFileDialog
    {
        public string? Title;
        public string? Filter;
        public string? InitialDirectory;
        
        public string FileName = "";

        public DialogResult Show()
        {
            Windowing.Disable();

            string[] filters = (Filter ?? "").Split('|');
            string name = filters[0];
            string extensions = filters[1].Replace("*.", "").Replace(';', ',');

            string? result = null;

            try
            {
                NfdStatus status = Nfd.OpenDialog(out result, new Dictionary<string, string> {
                    { name, extensions}
                }, InitialDirectory);

                Logging.Log($"Open NFD status: {status} | {result}");
            }
            catch (Exception ex)
            {
                Logging.Log($"Open NFD failed: {name} | {extensions}", LogSeverity.ERROR, ex);
                GuiWindowEditor.ShowError("Failed to open dialog");
            }

            Windowing.Enable();

            if (!string.IsNullOrWhiteSpace(result))
            {
                FileName = result;
                return DialogResult.OK;
            }
            else
                return DialogResult.Cancel;
        }

        public DialogResult Show(out string fileName)
        {
            DialogResult result = Show();

            fileName = FileName;
            return result;
        }

        public DialogResult Show(Setting<string> directory)
        {
            if (directory.Value != "")
                InitialDirectory = directory.Value;

            DialogResult result = Show();

            if (result == DialogResult.OK)
                directory.Value = Path.GetDirectoryName(FileName) ?? "";

            return result;
        }

        public DialogResult Show(Setting<string> directory, out string fileName)
        {
            DialogResult result = Show(directory);

            fileName = FileName;
            return result;
        }
    }
}
