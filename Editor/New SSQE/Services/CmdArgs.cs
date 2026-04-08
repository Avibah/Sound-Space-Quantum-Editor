using New_SSQE.Audio;
using New_SSQE.NewGUI;
using New_SSQE.Preferences;
using System.Globalization;
using New_SSQE.NewGUI.Windows;
using New_SSQE.Misc;

namespace New_SSQE.Services
{
    internal class CmdArgs
    {
        public static void Start(string[] args, bool writeFile)
        {
            if (writeFile)
            {
                string file = Assets.TempAt("tempargs.txt");

                if (File.Exists(file))
                    File.Delete(file);
                File.WriteAllText(file, string.Join(' ', args));
                return;
            }

            if (args.Length > 0)
                Logging.Log($"Program started with args: {string.Join(' ', args)}");
            Parse(args);
        }

        public static void Parse(string[] args)
        {
            if (args.Length < 3)
                return;

            switch (args[0])
            {
                case "edit" when Windowing.Current is GuiWindowEditor:
                    switch (args[1])
                    {
                        case "ms":
                            if (!float.TryParse(args[2], NumberStyles.Any, Program.Culture, out float ms))
                                break;

                            MusicPlayer.Pause();
                            Settings.currentTime.Value.Value = Math.Clamp(ms, 0, Settings.currentTime.Value.Max);
                            break;
                    }

                    break;
                case "open":
                    switch (args[1])
                    {
                        case "file":
                            MainWindow.FileToLoad = string.Join(' ', args[2..]);
                            break;
                    }

                    break;
            }
        }

        private static FileSystemWatcher? watcher;

        public static void Watch()
        {
            if (Platforms.IsLinux)
                return;

            watcher = new(Assets.TEMP)
            {
                Filter = "tempargs.txt",
                IncludeSubdirectories = false
            };

            watcher.Created += (s, e) =>
            {
                int attempts = 10;

                while (attempts-- > 0)
                {
                    try
                    {
                        string text = File.ReadAllText(e.FullPath);
                        if (string.IsNullOrWhiteSpace(text))
                            return;

                        string[] args = text.Trim().Split(' ');
                        if (args.Length == 0)
                            return;

                        string[] temp = new string[Math.Max(args.Length, 3)];
                        for (int i = 0; i < args.Length; i++)
                            temp[i] = args[i];

                        Parse(temp);
                    }
                    catch (IOException) { }
                    catch
                    {
                        return;
                    }

                    Thread.Sleep(250);
                }
            };

            watcher.EnableRaisingEvents = true;
        }

        public static void Unwatch()
        {
            watcher?.Dispose();
        }
    }
}
