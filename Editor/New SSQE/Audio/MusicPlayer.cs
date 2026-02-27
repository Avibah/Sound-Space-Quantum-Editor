using New_SSQE.NewGUI.Dialogs;
using New_SSQE.Preferences;
using New_SSQE.Services;
using SoundFlow.Enums;
using Player = SoundFlow.Components.SoundPlayer;

namespace New_SSQE.Audio
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

                if (tempoProvider != null)
                    Waveform.Load(tempoProvider);
                else
                    Waveform.Reset();
            }
            catch (Exception ex)
            {
                Logging.Log($"Audio failed to load - {file}", LogSeverity.ERROR, ex);
                MessageDialog.Show($"Audio failed to load [{Path.GetFileName(file)}]\n\n{ex.Message}", MBoxIcon.Warning, MBoxButtons.OK);
                return false;
            }

            Stop();

            return true;
        }

        private static void OnEnded()
        {
            Pause();
            CurrentTime = TotalTime;
            Settings.currentTime.Value.Value = (float)(CurrentTime.TotalMilliseconds - MusicOffset);
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

        public static float DetectBPM(long start, long end)
        {
            if (tempoProvider == null)
                return 0;

            int sampleRate = tempoProvider.Format.SampleRate;
            int startOffset = (int)(start / 1000d * sampleRate);
            int endOffset = (int)(end / 1000d * sampleRate);

            return tempoProvider.GetBPM(startOffset, endOffset);
        }
    }
}
