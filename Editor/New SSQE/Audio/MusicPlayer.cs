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

        /// <summary>
        /// Global music offset for <see cref="CurrentTime"/>
        /// </summary>
        public static float MusicOffset => Settings.musicOffset.Value - 28;

        /// <summary>
        /// Loads music from <paramref name="file"/> if possible, disposing previously loaded music and resetting the current time to 0
        /// </summary>
        /// <param name="file">The file to load music from</param>
        /// <returns>Whether file loading succeeded</returns>
        public static bool Load(string file)
        {
            if (!File.Exists(file))
                return false;

            if (lastPlayer != null)
                SoundEngine.DisposePlayer(lastPlayer);
            SoundEngine.SetMono(Settings.monoAudio.Value);

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

        /// <summary>
        /// Attempts to convert the currently loaded music to an MP3 file at the same location, replacing the old file
        /// </summary>
        public static void ConvertToMP3()
        {
            if (lastPlayer == null)
                return;

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
                    if (SoundEngine.EncodeMusic(lastPlayer, encoding))
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

        /// <summary>
        /// Resume playback of the active player
        /// </summary>
        public static void Play()
        {
            CurrentTime = TimeSpan.FromMilliseconds(Settings.currentTime.Value.Value);
            lastPlayer?.Play();
        }

        /// <summary>
        /// Pause playback of the active player
        /// </summary>
        public static void Pause()
        {
            Settings.currentTime.Value.Value = (float)CurrentTime.TotalMilliseconds;
            lastPlayer?.Stop();
        }

        /// <summary>
        /// Gets or sets the active player's tempo / playback speed
        /// </summary>
        public static float Tempo
        {
            set
            {
                if (tempoProvider != null)
                    tempoProvider.Tempo = value;
            }
            get => tempoProvider?.Tempo ?? 1;
        }

        /// <summary>
        /// Gets or sets the active player's volume
        /// </summary>
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

        /// <summary>
        /// Stop playback and set the current time to 0
        /// </summary>
        public static void Stop()
        {
            lastPlayer?.Stop();
            CurrentTime = TimeSpan.Zero;
        }

        /// <summary>
        /// Whether the active player is currently playing
        /// </summary>
        public static bool IsPlaying => lastPlayer?.State == PlaybackState.Playing;

        /// <summary>
        /// Gets the active player's total time as a <see cref="TimeSpan"/>
        /// </summary>
        public static TimeSpan TotalTime => TimeSpan.FromSeconds(lastPlayer?.Duration ?? 0);
        
        /// <summary>
        /// Gets or sets the active player's current timestamp as a <see cref="TimeSpan"/>
        /// </summary>
        public static TimeSpan CurrentTime
        {
            set => lastPlayer?.Seek(value - TimeSpan.FromMilliseconds(MusicOffset));
            get => TimeSpan.FromSeconds(lastPlayer == null ? 0 : TotalTime.TotalSeconds * lastPlayer.DataProvider.Position / lastPlayer.DataProvider.Length + MusicOffset / 1000d);
        }

        /// <summary>
        /// Whether the currently loaded music is an MP3
        /// </summary>
        public static bool IsMP3 { get; private set; } = false;
        /// <summary>
        /// Whether the currently loaded music is an OGG
        /// </summary>
        public static bool IsOGG { get; private set; } = false;

        /// <summary>
        /// Attempts to detect a BPM between two millisecond timestamps, returning 0 if detection failed
        /// </summary>
        /// <param name="start">The start position of the range to detect BPM from, in milliseconds</param>
        /// <param name="end">The end position of the range to detect BPM from, in milliseconds</param>
        /// <returns>The detected BPM, or 0 if detection failed</returns>
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
