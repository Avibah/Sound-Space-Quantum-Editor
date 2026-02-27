namespace SSQE_Player.Audio
{
    internal class SoundPlayer
    {
        private static readonly string THIS = Path.GetDirectoryName(Environment.ProcessPath) ?? "";
        private static readonly string SOUNDS = Path.Combine(THIS, "assets", "sounds");

        public static float Volume;

        private static readonly Dictionary<string, string> tags = [];

        static SoundPlayer()
        {
            string[] sounds = Directory.GetFiles(SOUNDS);

            foreach (string file in sounds)
            {
                string tag = Path.GetFileNameWithoutExtension(file);
                string ext = Path.GetExtension(file);

                if (tags.ContainsKey(ext) && ext == ".wav")
                    continue;

                if (!tags.TryAdd(tag, file))
                    tags[tag] = file;
                SoundEngine.InitializeSound(file);
            }
        }

        public static void Play(string sound)
        {
            tags.TryGetValue(sound, out string? file);
            if (!File.Exists(file))
                return;
            
            SoundEngine.PlaySound(file, sound == Settings.clickSound.Value ? 0.035f : Volume);
        }
    }
}
