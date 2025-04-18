using DiscordRPC;
using DiscordRPC.Logging;
using New_SSQE.NewMaps;

namespace New_SSQE.ExternalUtils
{
    internal enum DiscordStatus
    {
        None,
        Menu,
        Editor,
        Special,
        VFX
    }

    internal class DiscordManager
    {
        private static DiscordRpcClient? client;
        private static bool enabled = true;

        private static string prevState = "";

        public static void Init()
        {
            try
            {
                client = new("1067849747710345346", -1)
                {
                    Logger = new ConsoleLogger() { Level = LogLevel.Warning }
                };

                client.OnReady += (sender, e) =>
                {
                    Logging.Log($"Discord integration ready");
                };

                client.OnPresenceUpdate += (sender, e) =>
                {
                    if (e.Presence.State != prevState)
                        Logging.Log($"Discord integration updated with activity '{e.Presence.State}'");
                    prevState = e.Presence.State;
                };

                client.Initialize();
            }
            catch { enabled = false; }
        }

        private static double time = 0;

        private static DiscordStatus prevStatus = DiscordStatus.None;
        private static DateTime prevTimestamp = DateTime.UtcNow;

        public static void Process(double frametime)
        {
            time += frametime;

            if (time >= 5)
            {
                SetActivity(prevStatus);
                time %= 5;
            }
        }

        public static void SetActivity(DiscordStatus status)
        {
            if (!enabled)
                return;

            string details = status switch
            {
                DiscordStatus.Menu => "Watching the sunset",
                DiscordStatus.Editor => $"Editing a map - {Mapping.Current.Notes.Count} notes",
                DiscordStatus.Special => $"Editing objects - {Mapping.Current.SpecialObjects.Count} objects",
                DiscordStatus.VFX => $"Editing VFX - {Mapping.Current.VfxObjects.Count} objects",
                _ => ""
            };

            string state = status switch
            {
                DiscordStatus.Editor or DiscordStatus.Special or DiscordStatus.VFX =>
                    Mapping.Current.FileID[..Math.Min(Mapping.Current.FileID.Length, 128)],
                _ => ""
            };

            if (prevStatus != status)
            {
                prevTimestamp = DateTime.UtcNow;
                prevStatus = status;
            }

            client?.SetPresence(new RichPresence
            {
                Details = details,
                State = state,
                Timestamps = new() { Start = prevTimestamp },
                Assets = new() { LargeImageKey = "logo", LargeImageText = $"Version {Program.Version}{(MainWindow.DebugVersion ? "-pre" : "")}" }
            });
        }

        public static void Dispose()
        {
            if (enabled)
                try { client?.Dispose(); } catch { }
        }
    }
}
