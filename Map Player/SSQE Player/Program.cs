using System.Globalization;

namespace SSQE_Player
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                string[] newArgs = ["true", "false", "false"];
                for (int i = 0; i < Math.Min(args.Length, newArgs.Length); i++)
                    newArgs[i] = args[i];

                if (!File.Exists("assets/temp/tempmap.txt")) { return; }
                
                if (bool.Parse(newArgs[1]) && File.Exists("assets/temp/tempreplay.qer"))
                    MainWindow.Replay = true;
                MainWindow.Autoplay = bool.Parse(newArgs[2]) && !MainWindow.Replay;

                CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                culture.NumberFormat.NumberDecimalSeparator = ".";

                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;

                Settings.Load();
                MainWindow window = new(bool.Parse(newArgs[0]), Settings.msaa.Value ? 32 : 0);

                using (window)
                    window.Run();
            }
            catch (Exception ex)
            {
                File.WriteAllText("player-crash-report.txt", ex.ToString());
            }
        }
    }
}