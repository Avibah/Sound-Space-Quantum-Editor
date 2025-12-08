using New_SSQE.ExternalUtils;
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
        public const float VOLUME_MULT = 2f;
        public const int SAMPLE_RATE = 44100;

        private static readonly AudioFormat format = new()
        {
            SampleRate = SAMPLE_RATE,
            Channels = 2,
            Format = SampleFormat.F32
        };

        private static readonly FFmpegCodecFactory ffmpegFactory = new();
        public static IReadOnlyCollection<string> SupportedFormats => ffmpegFactory.SupportedFormatIds;

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
                PeriodSizeInMilliseconds = 4
            });
            device.Start();
        }

        /*
        public static void EncodeMusic(string outFile, string encoding)
        {
            if (prevMusic == null)
                return;
            if (prevMusic.DataProvider is not TempoProvider provider)
                return;

            MemoryStream output = new();
            using ISoundEncoder encoder = engine.CreateEncoder(output, encoding, provider.Format);
            
            encoder.Encode(provider.Samples);
        }
        */

        private static float[] GetResampledData(string filename, out string fileType, bool sourceIsMusic)
        {
            byte[] bytes = File.ReadAllBytes(filename);
            using MemoryStream stream = new(bytes);

            ISoundDataProvider provider = sourceIsMusic ? new StreamDataProvider(engine, stream) : new AssetDataProvider(engine, stream);
            fileType = provider.FormatInfo?.FormatName.ToLower() ?? "";

            float[] data = new float[provider.Length];
            int offset = 0;

            float[] buffer = new float[provider.SampleRate];
            while (provider.ReadBytes(buffer) > 0)
            {
                Array.Copy(buffer, 0, data, offset, Math.Min(data.Length - offset, buffer.Length));
                offset += buffer.Length;
            }
            
            float[] resampled = MathHelper.ResampleLinear(data, provider.FormatInfo?.ChannelCount ?? 2, provider.SampleRate, format.SampleRate);

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
