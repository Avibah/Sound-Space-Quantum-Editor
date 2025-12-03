using New_SSQE.ExternalUtils;
using New_SSQE.Misc.Dialogs;
using New_SSQE.Misc.Static;
using New_SSQE.NewMaps;
using New_SSQE.Preferences;
using SoundFlow.Enums;
using Player = SoundFlow.Components.SoundPlayer;

namespace New_SSQE.Audio
{
    internal class MusicPlayer
    {
        private static string lastFile = "";
        private static Player? lastPlayer;
        private static TempoProvider? tempoProvider;

        private static void RunMP3Convert()
        {
            if (!File.Exists(Path.Combine(Assets.THIS, "lame.exe")))
            {
                MessageBox.Show("MP3 encoder not present from latest release", MBoxIcon.Warning, MBoxButtons.OK);
                return;
            }

            try
            {
                Pause();
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
            if (PlatformUtils.IsLinux)
                return;

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

        public static bool Load(string file)
        {
            if (!File.Exists(file))
                return true;
            if (lastFile != file)
                lastFile = file;

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

                IsMP3 = fileType == "mp3";
                IsOGG = fileType == "ogg";
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
            Settings.currentTime.Value.Value = (float)(CurrentTime.TotalMilliseconds + (0.03 + Settings.musicOffset.Value / 1000d) * (1 + (Mapping.Current.Tempo - 1) * 1.5));
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
            buffer = new short[4];
            return 0;

            /*
            byte[] data = new byte[buffer.Length * 2];
            int numBytes = reader?.Read(data, 0, data.Length) ?? 0;

            int index = 0;
            for (int i = 0; i < numBytes; i += 2)
                buffer[index++] = (short)(data[i] + data[i + 1]);

            return numBytes;
            */
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
                    lastPlayer.Volume = Math.Max(value, 0);
            }
            get => lastPlayer?.Volume ?? 1;
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
            set => lastPlayer?.Seek(value - TimeSpan.FromMilliseconds(Settings.musicOffset.Value));
            get => TimeSpan.FromSeconds(lastPlayer == null ? 0 : TotalTime.TotalSeconds * lastPlayer.DataProvider.Position / lastPlayer.DataProvider.Length + Settings.musicOffset.Value / 1000d);
        }

        public static bool IsMP3 { get; private set; } = false;
        public static bool IsOGG { get; private set; } = false;

        public static float DetectBPM(long start, long end)
        {
            return 0;
        }

        public static string[] SupportedExtensions => [.. SoundEngine.SupportedFormats.Concat(["egg", "asset"])];
        public static string SupportedExtensionsString => string.Join(';', SupportedExtensions.Select((e) => "*." + e));
    }
}
