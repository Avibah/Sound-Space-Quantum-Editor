﻿using System.Reflection;
using New_SSQE.ExternalUtils;

namespace New_SSQE.Misc.Static
{
    internal class Assets
    {
        public static readonly string THIS = Path.GetDirectoryName(Environment.ProcessPath) ?? "";
        public static readonly string FILE_NAME = PlatformUtils.GetExecutableName($"{Assembly.GetExecutingAssembly().GetName().Name ?? "Sound Space Quantum Editor"}");
        public static readonly string EXE = Path.Combine(THIS, FILE_NAME);

        public static readonly string CACHED = Path.Combine(THIS, "cached");
        public static readonly string FONTS = Path.Combine(THIS, "assets", "fonts");
        public static readonly string SOUNDS = Path.Combine(THIS, "assets", "sounds");
        public static readonly string TEMP = Path.Combine(THIS, "assets", "temp");
        public static readonly string TEXTURES = Path.Combine(THIS, "assets", "textures");
        public static readonly string HISTORY = Path.Combine(THIS, "history");

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
