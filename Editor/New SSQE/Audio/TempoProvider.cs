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

        public float Tempo { get; set; } = 1;
        public bool CanSeek { get; private set; } = true;
        public bool IsDisposed { get; private set; } = false;
        public SoundFormatInfo? FormatInfo { get; private set; } = null;

        public event EventHandler<EventArgs>? EndOfStreamReached;
        public event EventHandler<PositionChangedEventArgs>? PositionChanged;

        private SoundTouchProcessor _processor;

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

        public int ReadBytes(Span<float> buffer)
        {
            int samplesLeft = _samples.Length - _position;
            if (samplesLeft <= 0)
            {
                EndOfStreamReached?.Invoke(this, EventArgs.Empty);
                return 0;
            }

            int toServe = Math.Min(samplesLeft, buffer.Length);
            float[] source = [.. _samples.Take(new Range(_position, _position + toServe))];

            if (Tempo == 1)
            {
                source.CopyTo(buffer);
                _position += toServe;

                PositionChanged?.Invoke(this, new(_position));
                return toServe;
            }

            int toRead = Math.Min(toServe, (int)(toServe / Tempo));
            toServe = (int)(toRead * Tempo);
            float[] processed = new float[toRead];

            _processor.PutSamples(source, toServe / _format.Channels);
            _processor.Flush();

            int result = _processor.ReceiveSamples(processed, toRead / _format.Channels) * _format.Channels;
            int received = result;
            // Clear processor buffer
            while (received > 0)
                received = _processor.ReceiveSamples(new float[4096], 4096 / _format.Channels);

            processed.AsSpan().CopyTo(buffer);
            _position += result;

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
        }
    }
}
