using New_SSQE.Preferences;
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
        private const int INITIAL_CACHE_SIZE = 8;
        public const float VOLUME_MULT = 2;
        public const int SAMPLE_RATE = 44100;
        public const int PERIOD_MILLISECONDS = 4;

        private static readonly AudioFormat format = new()
        {
            SampleRate = SAMPLE_RATE,
            Channels = 2,
            Format = SampleFormat.F32
        };

        private static readonly FFmpegCodecFactory ffmpegFactory = new();

        public static IReadOnlyCollection<string> SupportedExtensions => [.. ffmpegFactory.SupportedFormatIds.Concat(["egg", "asset"])];
        public static string SupportedExtensionsString => string.Join(';', SupportedExtensions.Select((e) => "*." + e));

        private static readonly MiniAudioEngine engine;
        private static readonly AudioPlaybackDevice device;

        private static readonly Dictionary<string, Queue<Player>> cache = [];
        private static readonly Dictionary<string, ISoundDataProvider> providers = [];

        private static Player? prevMusic;
        private static bool _mono = false;
        
        static SoundEngine()
        {
            engine = new();
            engine.RegisterCodecFactory(ffmpegFactory);
            engine.UpdateAudioDevicesInfo();

            device = engine.InitializePlaybackDevice(null, format, new MiniAudioDeviceConfig()
            {
                Wasapi = new() { Usage = WasapiUsage.ProAudio },
                PeriodSizeInMilliseconds = PERIOD_MILLISECONDS
            });
            device.Start();
        }

        private static float[] GetResampledData(string filename, out string fileType, bool sourceIsMusic)
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

            provider.Dispose();
            return resampled;
        }

        private static void EnqueueNewSound(string filename)
        {
            if (!providers.TryGetValue(filename, out ISoundDataProvider? provider))
                provider = new RawDataProvider(GetResampledData(filename, out _, false));

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

        public static void InitializeSound(string filename)
        {
            if (!cache.TryAdd(filename, []))
                cache[filename] = [];

            for (int i = 0; i < INITIAL_CACHE_SIZE; i++)
                EnqueueNewSound(filename);
        }

        public static void PlaySound(string filename, float volume)
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
            if (prevMusic != null)
            {
                device.MasterMixer.RemoveComponent(prevMusic);
                prevMusic = null;
            }

            TempoProvider provider = new(GetResampledData(filename, out fileType, true), format);
            Player player = new(engine, format, provider);

            prevMusic = player;
            device.MasterMixer.AddComponent(player);
            return player;
        }

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

        public static void Dispose()
        {
            device.Stop();
            device.Dispose();
            engine.Dispose();
        }
    }
}
