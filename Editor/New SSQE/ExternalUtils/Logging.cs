using New_SSQE.Preferences;

namespace New_SSQE.ExternalUtils
{
    // for debugging purposes

    internal enum LogSeverity
    {
        INFO,
        WARN,
        ERROR,
        FATAL
    }

    internal class Logging
    {
        private static readonly List<(DateTime, string, string, int)> logs = [];

        public static void Log(string log, LogSeverity severity = LogSeverity.INFO, Exception? ex = null)
        {
            if (ex != null)
                log += $"\n{ex}";

            if (logs.Count > 0 && logs[^1].Item3 == log)
            {
                (DateTime, string, string, int) item = logs[^1];
                logs[^1] = (item.Item1, item.Item2, item.Item3, item.Item4 + 1);
            }
            else
                logs.Add((DateTime.Now, $"{severity}", log, 1));

            if (Settings.debugMode.Value || MainWindow.DebugVersion)
            {
                try
                {
                    File.WriteAllText("logs-debug.txt", GetLogs());
                }
                catch { }
            }
        }

        public static void Log(double log, LogSeverity severity = LogSeverity.INFO, Exception? ex = null) => Log(log.ToString(), severity, ex);

        public static string GetLogs()
        {
            string[] temp = new string[logs.Count];

            for (int i = 0; i < temp.Length; i++)
            {
                (DateTime, string, string, int) item = logs[i];

                temp[i] = $"[{item.Item1} - {item.Item2}] {item.Item3}";
                if (item.Item4 > 1)
                    temp[i] += $" [x{item.Item4}]";
            }

            return string.Join('\n', temp);
        }
    }
}
