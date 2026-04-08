using System.Reflection;
using New_SSQE.Services;

namespace New_SSQE.Misc
{
    internal class Assets
    {

        private static readonly string _this = Path.GetDirectoryName(Environment.ProcessPath) ?? "";
        private static readonly string _cached = Path.Combine(_this, "cached");
        private static readonly string _fonts = Path.Combine(_this, "assets", "fonts");
        private static readonly string _sounds = Path.Combine(_this, "assets", "sounds");
        private static readonly string _temp = Path.Combine(_this, "assets", "temp");
        private static readonly string _textures = Path.Combine(_this, "assets", "textures");
        private static readonly string _history = Path.Combine(_this, "history");
        private static readonly string _maps = Path.Combine(_this, "maps");

        public static readonly string FILE_NAME = Platforms.GetExecutableName($"{Assembly.GetExecutingAssembly().GetName().Name ?? "Sound Space Quantum Editor"}");
        public static readonly string EXE = Path.Combine(_this, FILE_NAME);

        public static string Validate(string directory)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            return directory;
        }

        public static string THIS => Validate(_this);
        public static string CACHED => Validate(_cached);
        public static string FONTS => Validate(_fonts);
        public static string SOUNDS => Validate(_sounds);
        public static string TEMP => Validate(_temp);
        public static string TEXTURES => Validate(_textures);
        public static string HISTORY => Validate(_history);
        public static string MAPS => Validate(_maps);

        public static string ThisAt(params string[] path) => Path.Combine([THIS, .. path]);
        public static string CachedAt(params string[] path) => Path.Combine([CACHED, .. path]);
        public static string FontsAt(params string[] path) => Path.Combine([FONTS, .. path]);
        public static string SoundsAt(params string[] path) => Path.Combine([SOUNDS, .. path]);
        public static string TempAt(params string[] path) => Path.Combine([TEMP, .. path]);
        public static string TexturesAt(params string[] path) => Path.Combine([TEXTURES, .. path]);
        public static string HistoryAt(params string[] path) => Path.Combine([HISTORY, .. path]);
        public static string MapsAt(params string[] path) => Path.Combine([MAPS, .. path]);
    }
}
