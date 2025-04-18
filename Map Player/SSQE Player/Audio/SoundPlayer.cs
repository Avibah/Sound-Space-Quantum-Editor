namespace SSQE_Player.Audio
{
    internal class SoundPlayer
    {
        private static readonly Dictionary<string, SoundQueue> queues = new();
        public static float Volume;

        static SoundPlayer()
        {
            string[] sounds = Directory.GetFiles("assets/sounds");

            foreach (string file in sounds)
                queues.Add(Path.GetFileNameWithoutExtension(file), new(file));
        }

        public static void Play(string fileName)
        {
            if (queues.TryGetValue(fileName, out SoundQueue? value))
                value.Play();
        }
    }
}
