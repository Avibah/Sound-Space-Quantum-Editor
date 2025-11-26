using New_SSQE.Misc.Static;
using New_SSQE.Preferences;

namespace New_SSQE.Audio
{
    internal class SoundPlayer
    {
        public static float Volume;

        private static readonly Dictionary<string, string> tags = [];

        static SoundPlayer()
        {
            string[] sounds = Directory.GetFiles(Assets.SOUNDS);

            foreach (string file in sounds)
            {
                string tag = Path.GetFileNameWithoutExtension(file);
                string ext = Path.GetExtension(file);

                if (tags.ContainsKey(ext) && ext == ".wav")
                    continue;

                if (!tags.TryAdd(tag, file))
                    tags[tag] = file;
            }
        }

        public static void Play(string sound)
        {
            tags.TryGetValue(sound, out string? file);
            file ??= sound;

            SoundEngine.PlaySound(file, sound == Settings.clickSound.Value ? 0.035f : Volume);
        }
    }
}
