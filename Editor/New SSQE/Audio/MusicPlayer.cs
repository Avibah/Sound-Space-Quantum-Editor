using New_SSQE.ExternalUtils;
using New_SSQE.Misc.Dialogs;
using New_SSQE.Misc.Static;
using New_SSQE.NewMaps;
using New_SSQE.Preferences;
using SoundFlow.Enums;
using SoundTouch;
using Player = SoundFlow.Components.SoundPlayer;

namespace New_SSQE.Audio
{
    internal class MusicPlayer
    {
        private static string lastFile = "";
        private static Player? lastPlayer;
        private static TempoProvider? tempoProvider;

        public static float MusicOffset => Settings.musicOffset.Value - 28;

        /*
        private static void RunMP3Convert()
        {
            try
            {
                Pause();
                SoundEngine.EncodeMusic(Path.Combine(Assets.TEMP, "tempaudio.mp3"), "mp3");
            }
            catch (Exception ex)
            {
                Logging.Log($"Failed to encode file to mp3: {lastFile}", LogSeverity.WARN, ex);
                MessageBox.Show($"Failed to convert file {lastFile}\n\nExternal playtesting may not be possible with this asset.", MBoxIcon.Warning, MBoxButtons.OK);
            }

            Load(lastFile);
            Volume = Settings.masterVolume.Value.Value;
        }

        public static void ConvertToMP3()
        {
            if (IsMP3)
            {
                DialogResult result = MessageBox.Show("This asset is already an MP3. Do you want to convert it anyway?\n\nThis may take a while.", MBoxIcon.Info, MBoxButtons.Yes_No);
                if (result != DialogResult.Yes)
                    return;
            }
            else
            {
                DialogResult result = MessageBox.Show("Are you sure you want to convert this asset to MP3?\n\nThis may take a while.", MBoxIcon.Info, MBoxButtons.Yes_No);
                if (result != DialogResult.Yes)
                    return;
            }

            RunMP3Convert();
        }
        */

        public static bool Load(string file)
        {
            if (!File.Exists(file))
                return true;
            if (lastFile != file)
                lastFile = file;

            SoundEngine.SetMono(Settings.monoAudio.Value);
            tempoProvider?.Dispose();
            lastPlayer?.Dispose();

            bool Error(Exception ex)
            {
                string id = Path.GetFileNameWithoutExtension(file);

                Logging.Log($"Audio failed to load - {file}", LogSeverity.ERROR, ex);
                DialogResult message = MessageBox.Show($"Audio file with id '{id}' is corrupt.\n\nWould you like to try importing a new file?", MBoxIcon.Warning, MBoxButtons.OK_Cancel);

                return message == DialogResult.OK && Mapping.ImportAudio(id);
            }

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
                return Error(ex);
            }

            Reset();

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
            Stop();
        }

        public static void Stop()
        {
            lastPlayer?.Stop();
        }

        public static int GetData(ref short[] buffer)
        {
            float[] data = new float[buffer.Length];
            int numBytes = tempoProvider?.ReadBytes(data) ?? 0;

            for (int i = 0; i < data.Length; i++)
                buffer[i] = (short)(data[i] * short.MaxValue);

            return numBytes;
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

        public static void Reset()
        {
            Stop();
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
            float tempo = Tempo;

            int channels = tempoProvider.Format.Channels;
            int sampleRate = tempoProvider.Format.SampleRate;

            int startOffset = (int)(start / 1000d * sampleRate);
            startOffset = startOffset / 2 * 2;
            int endOffset = (int)(end / 1000d * sampleRate);
            endOffset = endOffset / 2 * 2;

            float bpm = 0;

            for (int i = 0; i < 5; i++)
            {
                Tempo = 1 / (float)Math.Pow(2, i);

                tempoProvider.Seek(startOffset);
                BpmDetect detector = new(channels, sampleRate);

                float[] buffer = new float[4096];

                while (tempoProvider.Position < endOffset)
                {
                    int samplesRead = tempoProvider.ReadBytes(buffer);
                    detector.InputSamples(buffer, samplesRead / channels);

                    if (samplesRead == 0)
                        break;
                }

                bpm = detector.GetBpm();
                if (bpm > 0)
                    break;

                Logging.Log($"BPM detect pass {i} failed");
            }

            Tempo = tempo;
            return bpm;
        }

        public static string[] SupportedExtensions => [.. SoundEngine.SupportedFormats.Concat(["egg", "asset"])];
        public static string SupportedExtensionsString => string.Join(';', SupportedExtensions.Select((e) => "*." + e));
    }
}
