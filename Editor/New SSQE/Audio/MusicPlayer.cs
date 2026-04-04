using New_SSQE.Misc;
using New_SSQE.NewGUI.Dialogs;
using New_SSQE.NewGUI.Windows;
using New_SSQE.NewMaps;
using New_SSQE.Preferences;
using New_SSQE.Services;
using SoundFlow.Enums;
using Player = SoundFlow.Components.SoundPlayer;

namespace New_SSQE.Audio
{
    internal class MusicPlayer
    {
        private static Player? lastPlayer;
        private static TempoProvider? tempoProvider;

        public static float MusicOffset => Settings.musicOffset.Value - 28;

        public static bool Load(string file)
        {
            if (!File.Exists(file))
                return false;

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

                Volume = Settings.masterVolume.Value.Value;
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

        public static void ConvertToMP3()
        {
            string message = IsMP3
                ? "This audio is already an MP3 file! Do you want to convert it anyway?\nThis will take a while!"
                : "Are you sure you want to convert this audio to MP3?\nThis will take a while!";

            MessageDialog.Show(message, MBoxIcon.Info, MBoxButtons.Yes_No, (result) =>
            {
                if (result != DialogResult.Yes)
                    return;

                if (IsPlaying)
                    Pause();

                string toEncode = Assets.CachedAt($"{Mapping.Current.SoundID}.asset");
                string encoding = Assets.TempAt("tempaudio.mp3");

                try
                {
                    if (SoundEngine.EncodeMusic(encoding))
                    {
                        File.Move(encoding, toEncode, true);
                        if (Load(toEncode))
                            GuiWindowEditor.ShowInfo("SUCCESSFULLY CONVERTED");
                    }
                    else
                        GuiWindowEditor.ShowWarn("FAILED TO CONVERT");
                }
                catch (Exception ex)
                {
                    Logging.Log("MP3 conversion failed", LogSeverity.ERROR, ex);
                    GuiWindowEditor.ShowWarn("FAILED TO CONVERT");
                }
            });
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
                if (Settings.muteMusic.Value)
                    value = 0;
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
