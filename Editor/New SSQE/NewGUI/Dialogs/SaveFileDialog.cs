using NativeFileDialogs.Net;
using New_SSQE.NewGUI.Windows;
using New_SSQE.Preferences;
using New_SSQE.Services;

namespace New_SSQE.NewGUI.Dialogs
{
    internal class SaveFileDialog
    {
        public string? Title;
        public string? Filter;
        public string? InitialDirectory;
        public string? InitialFileName;

        public string FileName = "";

        public DialogResult Show()
        {
            Windowing.Disable();

            string[] filters = (Filter ?? "").Split('|');
            string name = filters[0];
            string[] extensions = filters[1].Replace("*.", "").Split(';');
            string extensionStr = string.Join(',', extensions);

            string? result = null;

            try
            {
                NfdStatus status = Nfd.SaveDialog(out result, new Dictionary<string, string> {
                    { name, extensionStr }
                }, InitialFileName ?? "", InitialDirectory);

                Logging.Log($"Save NFD status: {status} | {result}");
            }
            catch (Exception ex)
            {
                try
                {
                    NfdStatus status = Nfd.SaveDialog(out result, new Dictionary<string, string> {
                        { name, extensionStr }
                    }, InitialFileName ?? "");

                    Logging.Log($"Save NFD fallback status: {status} | {result}");
                }
                catch
                {
                    Logging.Log($"Save NFD failed: {name} | {extensionStr}", LogSeverity.ERROR, ex);
                    GuiWindowEditor.ShowError("Failed to open dialog");
                }
            }

            Windowing.Enable();

            if (!string.IsNullOrWhiteSpace(result))
            {
                if (string.IsNullOrWhiteSpace(Path.GetExtension(result)))
                    result += extensions.Length > 0 ? $".{extensions[0]}" : "";
                FileName = result;

                return DialogResult.OK;
            }
            else
                return DialogResult.Cancel;
        }

        public DialogResult Show(Setting<string> directory)
        {
            if (directory.Value != "" && Directory.Exists(directory.Value))
            {
                string dir = Path.GetFullPath(directory.Value);
                InitialDirectory = dir;
            }

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
