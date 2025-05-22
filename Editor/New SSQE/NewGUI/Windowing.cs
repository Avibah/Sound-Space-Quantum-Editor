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
        private static Stack<GuiWindow> windowStack = [];

        public static void SwitchWindow(GuiWindow window)
        {
            if (window is GuiWindowEditor)
            {
                DiscordManager.SetActivity(DiscordStatus.Editor);
                Mapping.StartAutosaving();
            }
            else if (window is GuiWindowMenu)
                DiscordManager.SetActivity(DiscordStatus.Menu);

            if (Current is GuiWindowEditor)
                MusicPlayer.Reset();

            ExportSSPM.Instance?.Close();
            BPMTapper.Instance?.Close();
            TimingsWindow.Instance?.Close();
            BookmarksWindow.Instance?.Close();

            FontRenderer.Unicode = Settings.language.Value != "english";

            Current?.Close();
            Current = window;
            Current.Open();

            windowStack = [];
            windowStack.Push(window);
            Settings.Save();
        }

        public static void OpenOnTop(GuiWindow window)
        {
            // for a new export window
            windowStack.Push(window);
            Current = window;
            Current.Open();
        }

        public static void CloseTop(GuiWindow window)
        {
            windowStack.Pop().Close();
            Current = windowStack.Peek();
        }
    }
}
