﻿using NativeFileDialogs.Net;
using New_SSQE.ExternalUtils;
using New_SSQE.NewGUI.Windows;
using New_SSQE.Preferences;

namespace New_SSQE.Misc.Dialogs
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
            string[] filters = (Filter ?? "").Split('|');
            string name = filters[0];
            string extensions = filters[1].Replace("*.", "").Replace(';', ',');

            string? result = null;

            try
            {
                NfdStatus status = Nfd.SaveDialog(out result, new Dictionary<string, string> {
                    { name, extensions}
                }, InitialFileName ?? "", InitialDirectory);

                Logging.Log($"Save NFD status: {status} | {result}");
            }
            catch (Exception ex)
            {
                Logging.Log($"Save NFD failed: {name} | {extensions}", LogSeverity.WARN, ex);
                GuiWindowEditor.ShowToast("Failed to open dialog");
            }

            if (!string.IsNullOrWhiteSpace(result))
            {
                FileName = result;
                return DialogResult.OK;
            }
            else
                return DialogResult.Cancel;
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
