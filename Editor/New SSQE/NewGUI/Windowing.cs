﻿using New_SSQE.Audio;
using New_SSQE.ExternalUtils;
using New_SSQE.NewGUI.Base;
using New_SSQE.NewGUI.Font;
using New_SSQE.NewGUI.Windows;
using New_SSQE.NewMaps;
using New_SSQE.Preferences;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace New_SSQE.NewGUI
{
    internal static class Windowing
    {
        public static bool ButtonClicked = false;

        private static readonly Stack<GuiWindow> windowStack = [];
        public static GuiWindow? Current => windowStack.FirstOrDefault(n => n is not GuiWindowDialog);

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

            foreach (GuiWindow guiWindow in windowStack)
                guiWindow.Close();

            windowStack.Clear();
            windowStack.Push(window);
            window.Open();

            Settings.Save();
        }

        public static void OpenDialog(GuiWindowDialog window)
        {
            windowStack.Push(window);
            window.Open();
        }

        public static void Close(GuiWindow window)
        {
            Stack<GuiWindow> temp = [];
            while (windowStack.Peek() != window)
                temp.Push(windowStack.Pop());

            windowStack.Pop().Close();
            while (temp.Count > 0)
                windowStack.Push(temp.Pop());
        }

        public static void Render(float mousex, float mousey, float frametime)
        {
            foreach (GuiWindow window in windowStack.Reverse())
                window.Render(mousex, mousey, frametime);
        }

        public static void Resize(ResizeEventArgs e)
        {
            foreach (GuiWindow window in windowStack.ToArray())
                window.Resize(e);
        }

        public static void MouseDown(MouseButtonEventArgs e)
        {
            ButtonClicked = false;

            foreach (GuiWindow window in windowStack.ToArray())
            {
                if (window is GuiWindowDialog dialog)
                {
                    if (dialog.IsHovering())
                    {
                        window.MouseDown(e);
                        break;
                    }
                    else
                        continue;
                }

                window.MouseDown(e);
            }
        }

        public static void MouseUp(MouseButtonEventArgs e)
        {
            foreach (GuiWindow window in windowStack.ToArray())
                window.MouseUp(e);
        }

        public static void MouseScroll(float delta)
        {
            foreach (GuiWindow window in windowStack.ToArray())
            {
                if (window is GuiWindowDialog dialog)
                {
                    if (dialog.IsHovering())
                    {
                        window.MouseScroll(delta);
                        break;
                    }
                    else
                        continue;
                }

                window.MouseScroll(delta);
            }
        }

        public static void KeyDown(Keys key)
        {
            foreach (GuiWindow window in windowStack.ToArray())
            {
                if (window is GuiWindowDialog dialog)
                {
                    if (dialog.IsHovering())
                    {
                        window.KeyDown(key);
                        break;
                    }
                    else
                        continue;
                }

                window.KeyDown(key);
            }
        }

        public static void KeyUp(Keys key)
        {
            foreach (GuiWindow window in windowStack.ToArray())
                window.KeyUp(key);
        }

        public static void KeybindUsed(string keybind)
        {
            foreach (GuiWindow window in windowStack.ToArray())
                window.KeybindUsed(keybind);
        }

        public static void TextInput(string str)
        {
            foreach (GuiWindow window in windowStack.ToArray())
            {
                if (window.TextboxFocused())
                {
                    window.TextInput(str);
                    break;
                }
            }
        }

        public static void FileDrop(string file)
        {
            foreach (GuiWindow window in windowStack.ToArray())
            {
                if (window is not GuiWindowDialog)
                    window.FileDrop(file);
            }
        }

        public static bool TextboxFocused()
        {
            foreach (GuiWindow window in windowStack.ToArray())
            {
                if (window.TextboxFocused())
                    return true;
            }

            return false;
        }

        public static bool HoveringInteractive(InteractiveControl? exclude = null)
        {
            foreach (GuiWindow window in windowStack.ToArray())
            {
                if (window.HoveringInteractive(exclude))
                    return true;
            }
            
            return false;
        }
    }
}
