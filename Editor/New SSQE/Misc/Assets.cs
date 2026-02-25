using System.Reflection;
using New_SSQE.Services;

namespace New_SSQE.Misc
{
    internal class Assets
    {
        public static readonly string THIS = Path.GetDirectoryName(Environment.ProcessPath) ?? "";
        public static readonly string FILE_NAME = Platforms.GetExecutableName($"{Assembly.GetExecutingAssembly().GetName().Name ?? "Sound Space Quantum Editor"}");
        public static readonly string EXE = Path.Combine(THIS, FILE_NAME);

        public static readonly string CACHED = Path.Combine(THIS, "cached");
        public static readonly string FONTS = Path.Combine(THIS, "assets", "fonts");
        public static readonly string SOUNDS = Path.Combine(THIS, "assets", "sounds");
        public static readonly string TEMP = Path.Combine(THIS, "assets", "temp");
        public static readonly string TEXTURES = Path.Combine(THIS, "assets", "textures");
        public static readonly string HISTORY = Path.Combine(THIS, "history");

        public static string ThisAt(string path) => Path.Combine(THIS, path);
        public static string FileNameAt(string path) => Path.Combine(FILE_NAME, path);
        public static string ExeAt(string path) => Path.Combine(EXE, path);

        public static string CachedAt(string path) => Path.Combine(CACHED, path);
        public static string FontsAt(string path) => Path.Combine(FONTS, path);
        public static string SoundsAt(string path) => Path.Combine(SOUNDS, path);
        public static string TempAt(string path) => Path.Combine(TEMP, path);
        public static string TexturesAt(string path) => Path.Combine(TEXTURES, path);
        public static string HistoryAt(string path) => Path.Combine(HISTORY, path);

        static Assets()
        {
            if (!Directory.Exists(CACHED))
                Directory.CreateDirectory(CACHED);
            if (!Directory.Exists(TEMP))
                Directory.CreateDirectory(TEMP);
            if (!Directory.Exists(HISTORY))
                Directory.CreateDirectory(HISTORY);
        }
    }
}
