using Avalonia;
using New_SSQE.ExternalUtils;
using New_SSQE.Misc;
using New_SSQE.Misc.Dialogs;
using New_SSQE.NewMaps;
using New_SSQE.Preferences;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace New_SSQE
{
    internal class Program
    {
        public static readonly CultureInfo Culture;
        public static readonly JsonSerializerOptions JsonOptions;
        public static readonly string Version = $"{Assembly.GetExecutingAssembly().GetName().Version}";

        static Program()
        {
            Culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            Culture.NumberFormat.NumberDecimalSeparator = ".";

            JsonOptions = new() { WriteIndented = true };
        }

        private static void Start()
        {
            Platforms.CheckDependencies();
            BuildAvaloniaApp();

            Settings.Load();
            MainWindow.DebugVersion |= Settings.debugMode.Value;
            
            MainWindow window = new(Settings.msaa.Value ? 32 : 0);
            CmdArgs.Watch();

            using (window)
                window.Run();
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .SetupWithoutStarting();

        public static bool IsOtherWindowOpen()
        {
            Process[] processes = Process.GetProcessesByName("Sound Space Quantum Editor");
            return processes.Length > 1;
        }

        private static void RegisterCrash(object e)
        {
            try
            {
                if (MainWindow.Instance != null)
                {
                    Mapping.Autosave();
                    Mapping.SaveCache();
                }
            }
            catch (Exception ex) { Logging.Log("Map(s) failed to save on abort", LogSeverity.ERROR, ex); }

            Logging.Log("[Error encountered in application]", LogSeverity.FATAL);

            string text = @$"// whoops

{e}

|******************|
|  POSSIBLE FIXES  |
|******************|

If you do not already have it, install the Visual C++ 2015 Redistributable package. This is required for GLFW to work (which SSQE uses).

Try reinstalling from the latest release in case any required files are missing or broken.

Try updating your graphics driver to the latest version.

If none of these work or aren't applicable, report the error through {Links.FEEDBACK_FORM}

{Logging.GetLogs()}
                ";

            File.WriteAllText(Path.Combine(Assets.THIS, "crash-report.txt"), text);

            DialogResult result = MessageBox.Show(@"Fatal error encountered while running this application
A crash report has been created at '*\crash-report.txt'

Would you like to report this crash on GitHub?", MBoxIcon.Error, MBoxButtons.Yes_No);

            if (result == DialogResult.Yes)
            {
                Platforms.OpenLink(Links.NEW_GITHUB_ISSUE);
                Platforms.OpenDirectory();
            }

            Environment.Exit(0);
        }
        
        public static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0)
                {
                    switch (args[0])
                    {
                        case "registerProtocol":
                            Protocol.Finish();
                            return;
                        case not "registerProtocol" when args[0].StartsWith("ssqe://"):
                            string[] final = args[0].Replace("%20", " ").Replace("%5C", "\\").Split('/')[2..];
                            string file = Path.Combine(Assets.TEMP, "tempargs.txt");

                            if (File.Exists(file))
                                File.Delete(file);
                            File.WriteAllText(file, string.Join(' ', final));

                            if (final[0] == "open" && !IsOtherWindowOpen())
                            {
                                CmdArgs.Parse(final);
                                break;
                            }
                            return;
                    }
                }
            }
            catch
            {
                return;
            }

            AppDomain domain = AppDomain.CurrentDomain;
            domain.UnhandledException += (s, e) =>
            {
                RegisterCrash(e.ExceptionObject);
            };

            try
            {
                try
                {
                    CmdArgs.Start(args, IsOtherWindowOpen());
                }
                catch (Exception ae)
                {
                    Logging.Log("Failed to parse args", LogSeverity.WARN, ae);
                }

                try
                {
                    Protocol.Register();
                }
                catch (Exception pe)
                {
                    Logging.Log("Failed to register protocol", LogSeverity.WARN, pe);
                }

                TaskScheduler.UnobservedTaskException += (s, e) =>
                {
                    Logging.Log("Unobserved task exception occurred", LogSeverity.ERROR, e.Exception);
                };

                Logging.Log($"Operating System: {RuntimeInformation.OSDescription}");
                Start();

                Logging.Log("[Normal application exit]");
                File.WriteAllText(Path.Combine(Assets.THIS, "logs.txt"), Logging.GetLogs());
            }
            catch (Exception e)
            {
                RegisterCrash(e);
            }

            CmdArgs.Unwatch();
        }
    }
}