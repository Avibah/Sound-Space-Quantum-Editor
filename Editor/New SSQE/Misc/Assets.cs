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
        public static readonly string MAPS = Path.Combine(THIS, "maps");

        public static string Validate(string directory)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            return directory;
        }

        public static string Get(string directory, params string[] path) => Path.Combine([Validate(directory), .. path]);

        public static string ThisAt(params string[] path) => Get(THIS, path);
        public static string CachedAt(params string[] path) => Get(CACHED, path);
        public static string FontsAt(params string[] path) => Get(FONTS, path);
        public static string SoundsAt(params string[] path) => Get(SOUNDS, path);
        public static string TempAt(params string[] path) => Get(TEMP, path);
        public static string TexturesAt(params string[] path) => Get(TEXTURES, path);
        public static string HistoryAt(params string[] path) => Get(HISTORY, path);
        public static string MapsAt(params string[] path) => Get(MAPS, path);
    }
}
