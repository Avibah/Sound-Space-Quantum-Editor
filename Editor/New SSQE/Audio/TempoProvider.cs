using SoundFlow.Enums;
using SoundFlow.Interfaces;
using SoundFlow.Metadata.Models;
using SoundFlow.Structs;
using SoundTouch;

namespace New_SSQE.Audio
{
    internal class TempoProvider : ISoundDataProvider
    {
        private readonly float[] _samples;
        private readonly int _sampleRate;
        private readonly SampleFormat _sampleFormat;
        private readonly AudioFormat _format;
        private int _position = 0;

        public int SampleRate => _sampleRate;
        public SampleFormat SampleFormat => _sampleFormat;
        public AudioFormat Format => _format;
        public int Position => _position;
        public int Length => _samples.Length;

        public bool CanSeek { get; private set; } = true;
        public bool IsDisposed { get; private set; } = false;
        public SoundFormatInfo? FormatInfo { get; private set; } = null;

        public event EventHandler<EventArgs>? EndOfStreamReached;
        public event EventHandler<PositionChangedEventArgs>? PositionChanged;

        private readonly SoundTouchProcessor _processor;
        private bool clearSamples = false;
        public float Tempo
        {
            get => (float)_processor.Rate;
            set
            {
                clearSamples = !_processor.IsEmpty;
                _processor.Rate = value;
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

        private bool ClearSTBuffer()
        {
            try
            {
                if (_processor.IsEmpty)
                    return true;

                int channels = _format.Channels;

                _processor.Flush();
                while (_processor.AvailableSamples > 0)
                    _processor.ReceiveSamples(new float[4096 * channels], 4096);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void FillSTSamples(int samplesNeeded)
        {
            if (clearSamples)
            {
                while (!ClearSTBuffer());
                clearSamples = false;
            }

            int samplesPerFill = _sampleRate / 4;

            int channels = _format.Channels;
            int samplesLeft = _samples.Length - _position;
            int offset = 0;

            while (_processor.AvailableSamples < samplesNeeded / channels)
            {
                try
                {
                    int toFill = Math.Min(samplesLeft, samplesPerFill);
                    float[] toPut = [.. _samples.AsSpan().Slice(_position + offset, toFill)];

                    _processor.PutSamples(toPut, toFill / channels);
                    if (toFill < samplesPerFill)
                        _processor.Flush();

                    samplesLeft -= samplesPerFill;
                    offset += samplesPerFill;
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

            if (Math.Abs(Tempo - 1) < 0.001)
            {
                _samples.AsSpan().Slice(_position, toServe).CopyTo(buffer);
                _position += toServe;

                PositionChanged?.Invoke(this, new(_position));
                return toServe;
            }

            FillSTSamples(toServe);

            float[] processed = new float[toServe];
            _processor.ReceiveSamples(processed, toServe / channels);

            processed.CopyTo(buffer);
            _position += (int)(toServe * Tempo);

            PositionChanged?.Invoke(this, new(_position));
            return toServe;
        }

        public void Seek(int sampleOffset)
        {
            _position = Math.Clamp(sampleOffset, 0, _samples.Length);
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
