using New_SSQE.Audio;
using New_SSQE.ExternalUtils;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Font;
using New_SSQE.NewGUI.Windows;
using New_SSQE.NewMaps;
using New_SSQE.Preferences;

namespace New_SSQE.NewGUI
{
    internal static class Windowing
    {
        public static GuiWindow? Current;

        public static void SwitchWindow(GuiWindow window)
        {
            if (Current is GuiWindowEditor)
                CurrentMap.Map?.Close();

            if (window is GuiWindowEditor)
            {
                DiscordManager.SetActivity(DiscordStatus.Editor);
                CurrentMap.StartAutosaving();
            }
            else if (window is GuiWindowMenu)
            {
                DiscordManager.SetActivity(DiscordStatus.Menu);
                Waveform.Dispose();
            }

            ExportSSPM.Instance?.Close();
            BPMTapper.Instance?.Close();
            TimingsWindow.Instance?.Close();
            BookmarksWindow.Instance?.Close();

            FontRenderer.Unicode = Settings.language.Value != "english";

            Current?.Close();
            Current = window;

            Settings.Save();
        }
    }
}
