using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Codecs.FFMpeg;
using SoundFlow.Providers;
using SoundFlow.Structs;
using SoundFlow.Enums;
using Player = SoundFlow.Components.SoundPlayer;
using New_SSQE.Preferences;
using SoundFlow.Utils;
using SoundFlow.Interfaces;
using SoundFlow.Backends.MiniAudio.Devices;
using SoundFlow.Backends.MiniAudio.Enums;

namespace New_SSQE.Audio
{
    internal class SoundEngine
    {
        private const int INITIAL_CACHE_SIZE = 8;

        private static readonly AudioFormat format = new()
        {
            SampleRate = 44100,
            Channels = 2,
            Format = SampleFormat.F32
        };

        private static readonly FFmpegCodecFactory ffmpegFactory = new();
        public static IReadOnlyCollection<string> SupportedFormats => ffmpegFactory.SupportedFormatIds;

        private static readonly MiniAudioEngine engine = new();
        private static readonly AudioPlaybackDevice device;

        private static readonly Dictionary<string, Queue<Player>> cache = [];
        private static readonly Dictionary<string, ISoundDataProvider> providers = [];

        private static Player? prevMusic;
        
        static SoundEngine()
        {
            engine.RegisterCodecFactory(ffmpegFactory);
            engine.UpdateAudioDevicesInfo();
            device = engine.InitializePlaybackDevice(null, format, new MiniAudioDeviceConfig()
            {
                Wasapi = new() { Usage = WasapiUsage.ProAudio },
                PeriodSizeInMilliseconds = 1
            });
            device.Start();
        }

        private static float[] GetResampledData(string filename, out string fileType)
        {
            using FileStream stream = File.OpenRead(filename);
            stream.Seek(0, SeekOrigin.Begin);
            using AssetDataProvider provider = new(engine, stream);

            fileType = provider.FormatInfo?.FormatName.ToLower() ?? "";

            float[] data = new float[provider.Length];
            provider.ReadBytes(data);
            float[] resampled = MathHelper.ResampleLinear(data, provider.FormatInfo?.ChannelCount ?? 2, provider.SampleRate, format.SampleRate);

            return resampled;
        }

        private static void EnqueueNewSound(string filename)
        {
            if (!providers.TryGetValue(filename, out ISoundDataProvider? provider))
                provider = new RawDataProvider(GetResampledData(filename, out _));

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

            ISoundDataProvider provider = new TempoProvider(GetResampledData(filename, out fileType), format);
            Player player = new(engine, format, provider);

            prevMusic = player;
            device.MasterMixer.AddComponent(player);
            return player;
        }

        public static void Dispose()
        {
            device.Stop();
            device.Dispose();
            engine.Dispose();
        }
    }
}
