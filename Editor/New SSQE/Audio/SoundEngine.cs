using New_SSQE.Preferences;
using New_SSQE.Services;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Backends.MiniAudio.Devices;
using SoundFlow.Backends.MiniAudio.Enums;
using SoundFlow.Codecs.FFMpeg;
using SoundFlow.Enums;
using SoundFlow.Interfaces;
using SoundFlow.Providers;
using SoundFlow.Structs;
using SoundFlow.Utils;
using Player = SoundFlow.Components.SoundPlayer;

namespace New_SSQE.Audio
{
    internal class SoundEngine
    {
        /// <summary>
        /// Initial queue size for sounds initialized with <see cref="InitializeSound(string)"/>
        /// <para></para>
        /// More copies of these sounds will be cached as needed
        /// </summary>
        private const int INITIAL_CACHE_SIZE = 8;
        /// <summary>
        /// Global volume multiplier for sounds
        /// </summary>
        public const float VOLUME_MULT = 2;
        /// <summary>
        /// Sample rate that all loaded sounds will be resampled to
        /// </summary>
        public const int SAMPLE_RATE = 44100;
        /// <summary>
        /// How often samples are read from the currently playing sound(s)
        /// <para></para>
        /// Time update frequency is 1 / PERIOD_MILLISECONDS Hz
        /// </summary>
        public const int PERIOD_MILLISECONDS = 4;

        private static readonly AudioFormat format = new()
        {
            SampleRate = SAMPLE_RATE,
            Channels = 2,
            Format = SampleFormat.F32
        };

        private static readonly FFmpegCodecFactory ffmpegFactory = new();

        /// <summary>
        /// All supported file extensions in this engine, including periods
        /// </summary>
        public static IReadOnlyCollection<string> SupportedExtensions => [.. ffmpegFactory.SupportedFormatIds.Concat(["egg", "asset"]).Select((e) => "." + e)];
        /// <summary>
        /// All supported file extensions in this engine, formatted as *.abc;*.def;...;*.xyz
        /// </summary>
        public static string SupportedExtensionsString => string.Join(';', SupportedExtensions.Select((e) => "*" + e));

        private static readonly MiniAudioEngine engine;
        private static readonly AudioPlaybackDevice device;

        private static readonly Dictionary<string, Queue<Player>> cache = [];
        private static readonly Dictionary<string, ISoundDataProvider> providers = [];

        private static bool _mono = false;
        
        static SoundEngine()
        {
            engine = new();
            engine.RegisterCodecFactory(ffmpegFactory);
            engine.SetCodecPriority(ffmpegFactory.FactoryId, -1);
            engine.UpdateAudioDevicesInfo();

            device = engine.InitializePlaybackDevice(null, format, new MiniAudioDeviceConfig()
            {
                Wasapi = new() { Usage = WasapiUsage.ProAudio },
                PeriodSizeInMilliseconds = PERIOD_MILLISECONDS
            });
            device.Start();
        }

        private static float[] GetResampledData(string filename, bool sourceIsMusic, out string fileType)
        {
            byte[] bytes = File.ReadAllBytes(filename);
            using MemoryStream stream = new(bytes);

            ISoundDataProvider provider = sourceIsMusic ? new StreamDataProvider(engine, stream) : new AssetDataProvider(engine, stream);
            fileType = provider.FormatInfo?.FormatName.ToLower() ?? "";

            List<float> parsed = new(provider.Length);
            float[] buffer = new float[provider.SampleRate];

            int samples = 0;

            while ((samples = provider.ReadBytes(buffer)) > 0)
            {
                if (samples < buffer.Length)
                {
                    float[] sizedBuffer = new float[samples];
                    Array.Copy(buffer, sizedBuffer, samples);
                    parsed.AddRange(sizedBuffer);
                }
                else
                    parsed.AddRange(buffer);
            }

            float[] data = [.. parsed];
            parsed = [];

            if ((provider.FormatInfo?.ChannelCount ?? 2) == 1)
            {
                float[] stereoData = new float[data.Length * 2];

                for (int i = 0; i < data.Length; i++)
                {
                    stereoData[i * 2 + 0] = data[i];
                    stereoData[i * 2 + 1] = data[i];
                }

                data = stereoData;
            }

            float[] resampled = MathHelper.ResampleLinear(data, provider.FormatInfo?.ChannelCount ?? 2, provider.SampleRate, format.SampleRate);
            data = [];

            if (_mono)
            {
                for (int i = 0; i < resampled.Length; i += 2)
                {
                    float left = resampled[i];
                    float right = resampled[i + 1];
                    float mid = (left + right) / 2;

                    resampled[i] = mid;
                    resampled[i + 1] = mid;
                }
            }
            
            for (int i = 0; i < resampled.Length; i++)
            {
                if (float.IsNaN(resampled[i]))
                    throw new InvalidDataException("Corrupted sample data");
            }

            provider.Dispose();
            return resampled;
        }

        private static void EnqueueNewSound(string filename)
        {
            if (!providers.TryGetValue(filename, out ISoundDataProvider? provider))
                provider = new RawDataProvider(GetResampledData(filename, false, out _));

            Queue<Player> queue = cache[filename];
            Player player = new(engine, format, provider);
            device.MasterMixer.AddComponent(player);

            player.PlaybackEnded += (s, e) =>
            {
                player.Stop();
                queue.Enqueue(player);
            };

            queue.Enqueue(player);
        }

        /// <summary>
        /// Prepare an initial queue for a sound at <paramref name="filename"/>
        /// </summary>
        /// <param name="filename">The file to initialize as a repeatable sound</param>
        public static void InitializeSound(string filename)
        {
            if (!cache.TryAdd(filename, []))
                cache[filename] = [];

            for (int i = 0; i < INITIAL_CACHE_SIZE; i++)
                EnqueueNewSound(filename);
        }

        /// <summary>
        /// Play the sound at <paramref name="filename"/>, initializing it if necessary
        /// </summary>
        /// <param name="filename">The file the sound is at, equivalent to </param>
        /// <param name="volume">The volume to play this sound at</param>
        public static void PlaySound(string filename, float volume = 1)
        {
            volume *= VOLUME_MULT;

            if (!cache.TryGetValue(filename, out Queue<Player>? queue))
            {
                InitializeSound(filename);
                queue = cache[filename];
            }

            try
            {
                if (queue.Count == 0)
                    EnqueueNewSound(filename);

                Player player = queue.Dequeue();
                player.Mute = Settings.muteSfx.Value;
                if (player.Volume != volume)
                    player.Volume = volume;
                player.Play();
            }
            catch { }
        }

        public static Player InitializeMusic(string filename, out string fileType)
        {
            TempoProvider provider = new(GetResampledData(filename, true, out fileType), format);
            Player player = new(engine, format, provider);

            device.MasterMixer.AddComponent(player);
            return player;
        }

        /// <summary>
        /// Dispose a <see cref="Player"/> initialized via <see cref="InitializeMusic(string, out string)"/>
        /// </summary>
        /// <param name="player">The <see cref="Player"/> to dispose of</param>
        public static void DisposeMusic(Player player)
        {
            device.MasterMixer.RemoveComponent(player);

            player.DataProvider.Dispose();
            player.Dispose();
        }

        /// <summary>
        /// Whether sounds should play in Mono (single-channel)
        /// </summary>
        public static bool IsMono => _mono;

        /// <summary>
        /// Sets whether sounds should play in Mono (single-channel)
        /// <para></para>
        /// Note that this will re-initialize all loaded sounds, and it WILL NOT re-initialize music!
        /// </summary>
        /// <param name="mono">Whether sounds should play in Mono</param>
        public static void SetMono(bool mono)
        {
            if (mono == _mono)
                return;
            _mono = mono;

            string[] sounds = [.. cache.Keys];
            cache.Clear();

            foreach (string sound in sounds)
                InitializeSound(sound);
        }

        /// <summary>
        /// Encode <paramref name="player"/> as an MP3 file to <paramref name="filename"/>
        /// </summary>
        /// <param name="player">The <see cref="Player"/> to encode to an MP3</param>
        /// <param name="filename">The destination file for the encoded MP3</param>
        /// <returns>Whether encoding succeeded</returns>
        public static bool EncodeMusic(Player player, string filename)
        {
            try
            {
                if (player.DataProvider is not TempoProvider provider)
                    return false;

                using Stream stream = File.OpenWrite(filename);
                ISoundEncoder? encoder = ffmpegFactory.CreateEncoder(stream, "mp3", format);
                if (encoder == null)
                    return false;

                provider.Seek(0);

                float[] buffer = new float[SAMPLE_RATE];
                int samples = 0;

                while ((samples = provider.ReadBytesRaw(buffer)) > 0)
                {
                    float[] data = buffer;

                    if (samples < buffer.Length)
                    {
                        data = new float[samples];
                        Array.Copy(buffer, data, samples);
                    }

                    encoder.Encode(data);
                }

                encoder.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                Logging.Log("MP3 encoding failed", LogSeverity.ERROR, ex);
            }

            return false;
        }

        /// <summary>
        /// Safely dispose resources for this engine and its playback device
        /// </summary>
        public static void Dispose()
        {
            device.Stop();
            device.Dispose();
            engine.Dispose();
        }
    }
}
