using SoundFlow.Enums;
using Player = SoundFlow.Components.SoundPlayer;

namespace SSQE_Player.Audio
{
    internal class MusicPlayer
    {
        private static string lastFile = "";
        private static Player? lastPlayer;
        private static TempoProvider? tempoProvider;

        public static float MusicOffset => Settings.musicOffset.Value - 28;

        public static bool Load(string file)
        {
            if (!File.Exists(file))
                return false;
            if (lastFile != file)
                lastFile = file;

            SoundEngine.SetMono(Settings.monoAudio.Value);
            tempoProvider?.Dispose();
            lastPlayer?.Dispose();

            try
            {
                Player player = SoundEngine.InitializeMusic(file, out string fileType);
                player.PlaybackEnded += (s, e) => OnEnded();
                lastPlayer = player;
                tempoProvider = player.DataProvider as TempoProvider;

                IsMP3 = fileType.StartsWith("mp3");
                IsOGG = fileType.StartsWith("ogg");
            }
            catch
            {
                return false;
            }

            Stop();

            return true;
        }

        private static void OnEnded()
        {
            Pause();
            MainWindow.Instance.Close();
        }

        public static void Play()
        {
            CurrentTime = TimeSpan.FromMilliseconds(Settings.currentTime.Value.Value);
            lastPlayer?.Play();
        }

        public static void Pause()
        {
            Settings.currentTime.Value.Value = (float)CurrentTime.TotalMilliseconds;
            lastPlayer?.Stop();
        }

        public static float Tempo
        {
            set
            {
                if (tempoProvider != null)
                    tempoProvider.Tempo = value;
            }
            get => tempoProvider?.Tempo ?? 1;
        }

        public static float Volume
        {
            set
            {
                if (lastPlayer != null)
                    lastPlayer.Volume = Math.Max(value * SoundEngine.VOLUME_MULT, 0);
            }
            get => (lastPlayer?.Volume / SoundEngine.VOLUME_MULT) ?? 1;
        }

        public static void Stop()
        {
            lastPlayer?.Stop();
            CurrentTime = TimeSpan.Zero;
        }

        public static bool IsPlaying => lastPlayer?.State == PlaybackState.Playing;

        public static TimeSpan TotalTime => TimeSpan.FromSeconds(lastPlayer?.Duration ?? 0);
        
        public static TimeSpan CurrentTime
        {
            set => lastPlayer?.Seek(value - TimeSpan.FromMilliseconds(MusicOffset));
            get => TimeSpan.FromSeconds(lastPlayer == null ? 0 : TotalTime.TotalSeconds * lastPlayer.DataProvider.Position / lastPlayer.DataProvider.Length + MusicOffset / 1000d);
        }

        public static bool IsMP3 { get; private set; } = false;
        public static bool IsOGG { get; private set; } = false;
    }
}
