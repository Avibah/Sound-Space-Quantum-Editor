using New_SSQE.NewGUI;
using New_SSQE.NewGUI.Base;
using New_SSQE.Preferences;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace New_SSQE.Audio
{
    internal class Waveform
    {
        private static float[] WaveModel = Array.Empty<float>();
        private static int waveLength = 0;
        private static int vao;
        private static int vbo;

        private static bool isUploaded = false;
        private static double resolution = 0.0d;
        private static bool classic = false;

        public static long Offset = -15;

        private struct Level
        {
            public short left;
            public short right;
        }

        private static Level ParseBuffer(short[] data, int start, int end)
        {
            int index = Math.Min(end, data.Length - 1);

            return new()
            {
                left = data[index - 1],
                right = data[index]
            };
        }

        public static void Init(int bps)
        {
            resolution = 1 / (double)Settings.waveformDetail.Value / 100;

            double bpf = bps * resolution;
            short[] buffer = new short[1024 * 1024];
            Level[] data = [];

            while (true)
            {
                int len = MusicPlayer.GetData(ref buffer);

                if (len > 0)
                {
                    Level[] set = new Level[(int)(len / bpf / 2)];
                    int cur = 0;

                    for (double i = 0; i < len; i += bpf)
                    {
                        int index = (int)(i / 2) * 2;
                        if (index >= 0 && cur < set.Length)
                            set[cur] = ParseBuffer(buffer, index, index + (int)bpf);
                        cur++;
                    }

                    data = data.Concat(set).ToArray();
                }
                else
                    break;
            }

            int offset = (int)Math.Abs(Offset / 1000d / resolution / 2);
            Level[] blank = new Level[offset];

            if (Offset > 0)
                data = blank.Concat(data[..^offset]).ToArray();
            else
                data = data[offset..].Concat(blank).ToArray();

            classic = Settings.classicWaveform.Value;
            WaveModel = new float[data.Length * (classic ? 2 : 4)];

            int maxPeak = 0;

            for (int i = 0; i < data.Length; i++)
            {
                int absL = Math.Abs((int)data[i].left);
                int absR = Math.Abs((int)data[i].right);

                maxPeak = Math.Max(maxPeak, Math.Max(absL, absR));
            }

            bool aboveZero = maxPeak > 0;

            for (int i = 0; i < data.Length; i++)
            {
                float pos = i / (float)data.Length;

                float left = (float)data[i].left / (aboveZero ? maxPeak : 1);
                float right = (float)data[i].right / (aboveZero ? maxPeak : 1);

                if (classic)
                {
                    float absL = Math.Abs(left);
                    float absR = Math.Abs(right);

                    WaveModel[i * 2 + 0] = pos;
                    WaveModel[i * 2 + 1] = 1 - Math.Max(absL, absR) * 2f;
                }
                else
                {
                    WaveModel[i * 4 + 0] = pos;
                    WaveModel[i * 4 + 1] = left;
                    WaveModel[i * 4 + 2] = pos;
                    WaveModel[i * 4 + 3] = right;
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
