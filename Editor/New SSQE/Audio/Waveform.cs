using New_SSQE.NewGUI;
using New_SSQE.NewGUI.Base;
using New_SSQE.Preferences;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace New_SSQE.Audio
{
    internal class Waveform
    {
        private static float[] WaveModel = [];
        private static int waveLength = 0;
        private static int vao;
        private static int vbo;

        private static bool isUploaded = false;
        private static double resolution = 0.0d;
        private static bool classic = false;

        public static long Offset => (long)MusicPlayer.MusicOffset - 15;

        private struct Level
        {
            public float left;
            public float right;
        }

        private static Level ParseBuffer(float[] data, int start, int end)
        {
            int index = Math.Min(end, data.Length - 1);

            return new()
            {
                left = data[index - 1],
                right = data[index]
            };
        }

        public static void Load(TempoProvider provider)
        {
            provider.Seek(0);
            resolution = 1 / (double)Settings.waveformDetail.Value / 100;
            
            int sampleRate = provider.FormatInfo?.SampleRate ?? SoundEngine.SAMPLE_RATE;
            double samplesPerFrame = sampleRate * resolution;
            float[] buffer = new float[4096];
            List<Level> data = [];

            while (true)
            {
                int len = provider.ReadBytesRaw(buffer);
                if (len <= 0)
                    break;

                Level[] frame = new Level[(int)(len / samplesPerFrame / 2)];
                int cur = 0;

                for (double i = 0; i < len; i += samplesPerFrame)
                {
                    int index = (int)(i / 2) * 2;
                    if (index >= 0 && cur < frame.Length)
                        frame[cur] = ParseBuffer(buffer, index, index + (int)samplesPerFrame);
                    cur++;
                }

                data.AddRange(frame);
            }

            int offset = (int)Math.Abs(Offset / 1000d / resolution);
            Level[] blank = new Level[offset];

            if (Offset > 0)
                data = [.. blank.Concat(data[..^offset])];
            else
                data = [.. data[offset..].Concat(blank)];

            classic = Settings.classicWaveform.Value;
            WaveModel = new float[data.Count * (classic ? 2 : 4)];

            float maxPeak = 0;

            for (int i = 0; i < data.Count; i++)
            {
                float l = Math.Abs(data[i].left);
                float r = Math.Abs(data[i].right);

                maxPeak = Math.Max(maxPeak, Math.Max(l, r));
            }

            bool aboveZero = maxPeak > 0;

            for (int i = 0; i < data.Count; i++)
            {
                float pos = i / (float)data.Count;

                float l = data[i].left / (aboveZero ? maxPeak : 1);
                float r = data[i].right / (aboveZero ? maxPeak : 1);

                if (classic)
                {
                    float absL = Math.Abs(l);
                    float absR = Math.Abs(r);

                    WaveModel[i * 2 + 0] = pos;
                    WaveModel[i * 2 + 1] = 1 - Math.Max(absL, absR) * 2;
                }
                else
                {
                    WaveModel[i * 4 + 0] = pos;
                    WaveModel[i * 4 + 1] = l;
                    WaveModel[i * 4 + 2] = pos;
                    WaveModel[i * 4 + 3] = r;
                }
            }

            if (isUploaded)
                Reset();
            Upload();
        }

        private static void Upload()
        {
            Shader.Waveform.Uniform3("LineColor", Settings.color4.Value);
            (vao, vbo) = GLState.NewVAO_VBO(2);
            GLState.testBuffer = vbo;
            GLState.BufferData(vbo, WaveModel);

            waveLength = WaveModel.Length / 2;
            WaveModel = [];

            isUploaded = true;
        }

        public static void Render(Vector2 xPositions, Vector2 songPositions, float trackHeight)
        {
            if (!isUploaded)
                return;

            int start = Math.Max(0, (int)(songPositions.X * waveLength) - 1);
            int end = Math.Min(waveLength, (int)(songPositions.Y * waveLength) + 1);

            Shader.Waveform.Uniform3("WavePos", xPositions.X, xPositions.Y - xPositions.X, trackHeight);
            GLState.DrawArrays(vao, PrimitiveType.LineStrip, start, end - start);
        }

        public static void Reset()
        {
            if (!isUploaded)
                return;

            GLState.BufferData(vbo, Array.Empty<float>());
            GLState.CleanVBO(vbo);
            GLState.CleanVAO(vao);

            isUploaded = false;
        }
    }
}
