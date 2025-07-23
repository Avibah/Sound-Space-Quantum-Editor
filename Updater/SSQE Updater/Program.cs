using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace SSQE_Updater
{
    class Program
    {
        static void Main()
        {
            var linux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            var ssqeProcess = $"Sound Space Quantum Editor{(linux ? "" : ".exe")}";

            var currentPath = Directory.GetCurrentDirectory();
            string currentVersion = "";

            if (File.Exists(ssqeProcess))
                currentVersion = FileVersionInfo.GetVersionInfo(ssqeProcess).FileVersion ?? "";

            var newVersion = CheckVersion();
            var file = linux ? $"SSQE{newVersion}-linux.zip" : $"SSQE{newVersion}.zip";

            if (newVersion != "")
            {
                Console.WriteLine("Starting download...");
                
                try
                {
                    WebClient.DownloadFile($"https://github.com/Avibah/Sound-Space-Quantum-Editor/releases/download/{newVersion}/{file}", file);
                    ExtractFile();
                }
                catch
                {
                    Console.WriteLine("Failed to download new editor version");
                    Quit();
                }
            }

            string[] GetOverwrites()
            {
                try
                {
                    List<string> overwrites = [];

                    var overwriteList = WebClient.DownloadString("https://raw.githubusercontent.com/Avibah/Sound-Space-Quantum-Editor/master/updater_overwrite");
                    var split = overwriteList.Split('\n');
                    var overwriteVersion = "";

                    int i = 0;
                    while (i < split.Length && !string.IsNullOrWhiteSpace(split[i]))
                    {
                        overwrites.Add(split[i]);
                        i++;
                    }

                    for (int j = i; j < split.Length; j++)
                    {
                        var line = split[j];
                        if (j == 0 || string.IsNullOrWhiteSpace(split[j - 1]))
                            overwriteVersion = line;

                        if (!string.IsNullOrWhiteSpace(line) && line != overwriteVersion && Version.Parse(currentVersion) < Version.Parse(overwriteVersion))
                            overwrites.Add(line);
                    }

                    return [..overwrites];
                }
                catch
                {
                    Console.WriteLine("Failed to fetch overwrites");
                    Quit();
                }

                return [];
            }

            string CheckVersion()
            {
                try
                {
                    var redirect = WebClient.GetRedirect("https://github.com/Avibah/Sound-Space-Quantum-Editor/releases/latest");

                    if (!string.IsNullOrWhiteSpace(redirect))
                    {
                        var version = redirect[(redirect.LastIndexOf('/') + 1)..];
                        
                        if (version != currentVersion)
                            return version;
                        else
                        {
                            Console.Write("Latest editor version already installed. Would you like to update anyway? y/n: ");
                            if (Console.ReadKey() == new ConsoleKeyInfo('y', ConsoleKey.Y, false, false, false))
                            {
                                Console.WriteLine();
                                return version;
                            }
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("Failed to check version");
                    Quit();
                }

                return "";
            }

            void KillProcess()
            {
                foreach (var process in Process.GetProcessesByName("Sound Space Quantum Editor"))
                    process.Kill();

                Thread.Sleep(500);
            }

            bool IsInOverwrites(string[] list, string fileName)
            {
                foreach (var line in list)
                    if (fileName == line)
                        return true;

                return false;
            }

            void ExtractFile()
            {
                var overwriteList = GetOverwrites();

                Console.WriteLine("Completed, extracting...");

                KillProcess();

                using (ZipArchive archive = ZipFile.OpenRead(file))
                {
                    foreach (var entry in archive.Entries)
                    {
                        try
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(currentPath, entry.FullName)) ?? "");
                            entry.ExtractToFile(Path.Combine(currentPath, entry.FullName), IsInOverwrites(overwriteList, entry.FullName));
                        }
                        catch { }
                    }
                }

                Console.WriteLine("Completed, launching...");

                File.Delete(file);
                Process.Start(ssqeProcess);

                Quit();
            }

            void Quit()
            {
                Thread.Sleep(1500);
                Environment.Exit(0);
            }
        }
    }
}