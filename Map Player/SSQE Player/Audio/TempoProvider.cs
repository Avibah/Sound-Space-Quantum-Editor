using SoundFlow.Enums;
using SoundFlow.Interfaces;
using SoundFlow.Metadata.Models;
using SoundFlow.Structs;
using SoundTouch;
using System.Diagnostics;

namespace SSQE_Player.Audio
{
    internal class TempoProvider : ISoundDataProvider
    {
        private readonly Stopwatch stopwatch = Stopwatch.StartNew();

        private readonly float[] _samples;
        private readonly int _sampleRate;
        private readonly SampleFormat _sampleFormat;
        private readonly AudioFormat _format;
        private int _position = 0;
        private double _playbackPosition = 0;
        private TimeSpan _lastRender = TimeSpan.Zero;

        public int SampleRate => _sampleRate;
        public SampleFormat SampleFormat => _sampleFormat;
        public AudioFormat Format => _format;
        public int Length => _samples.Length;

        public bool CanSeek { get; private set; } = true;
        public bool IsDisposed { get; private set; } = false;
        public SoundFormatInfo? FormatInfo { get; private set; } = null;

        public event EventHandler<EventArgs>? EndOfStreamReached;
        public event EventHandler<PositionChangedEventArgs>? PositionChanged;

        private readonly SoundTouchProcessor _processor;
        private readonly float[] _buffer = new float[1024];

        public float Tempo
        {
            get => (float)_processor.Rate;
            set
            {
                if (_processor.Rate != value)
                {
                    _processor.Clear();
                    _processor.Rate = value;
                    Seek(Position);
                }
            }
        }

        public int Position
        {
            get
            {
                double samplesPerFrame = _sampleRate / 1000d * SoundEngine.PERIOD_MILLISECONDS * Tempo * Format.Channels;
                double timeDiff = Math.Min((stopwatch.Elapsed - _lastRender).TotalMilliseconds / SoundEngine.PERIOD_MILLISECONDS, 1);

                return (int)(_playbackPosition + timeDiff * samplesPerFrame);
            }
        }

        public TempoProvider(float[] samples, AudioFormat format)
        {
            _samples = samples;
            _sampleRate = format.SampleRate;
            _sampleFormat = format.Format;
            _format = format;

            _processor = new()
            {
                SampleRate = format.SampleRate,
                Channels = format.Channels
            };
        }

        private void FillSTSamples(int samplesNeeded)
        {
            int channels = _format.Channels;

            while (_processor.AvailableSamples < samplesNeeded)
            {
                try
                {
                    int samplesLeft = _samples.Length - _position;

                    if (samplesLeft <= 0)
                    {
                        _processor.Flush();
                        break;
                    }

                    int toFill = Math.Min(samplesLeft, _buffer.Length);
                    Array.Copy(_samples, _position, _buffer, 0, toFill);

                    _processor.PutSamples(_buffer, toFill / channels);
                    _position += toFill;
                }
                catch { }
            }
        }

        public int ReadBytes(Span<float> buffer)
        {
            if (IsDisposed)
                return 0;

            int samplesLeft = _samples.Length - _position;

            if (samplesLeft <= 0)
            {
                EndOfStreamReached?.Invoke(this, EventArgs.Empty);
                return 0;
            }

            int toServe = Math.Min(samplesLeft, buffer.Length);
            int channels = _format.Channels;

            FillSTSamples(toServe);
            _processor.ReceiveSamples(buffer, toServe / channels);

            _playbackPosition += toServe * Tempo;
            _lastRender = stopwatch.Elapsed;
            PositionChanged?.Invoke(this, new(Position));
            return toServe;
        }

        public void Seek(int sampleOffset)
        {
            _processor.Clear();
            _position = Math.Clamp(sampleOffset, 0, _samples.Length);
            _playbackPosition = _position;
            _lastRender = stopwatch.Elapsed;
        }

        public void Dispose()
        {
            if (!IsDisposed)
                IsDisposed = true;
            EndOfStreamReached = null;
            PositionChanged = null;
        }
    }
}
