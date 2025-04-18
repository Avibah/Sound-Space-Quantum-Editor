using OpenTK.Mathematics;

namespace New_SSQE.Objects
{
    internal class TimingPoint : MapObject
    {
        public float BPM { get; set; }
        public Vector2 TimeSignature { get; set; }

        public TimingPoint(float bpm, long ms, Vector2? timeSignature = null) : base(1, ms, "BPM")
        {
            BPM = bpm;
            Ms = ms;

            TimeSignature = timeSignature ?? (4, 4);
            HasDuration = false;
        }

        public TimingPoint(string data) : this(0, 0)
        {
            string[] split = data.Split('|');

            BPM = float.Parse(split[1], Program.Culture);
            Ms = long.Parse(split[0]);
        }

        public override string ToString(params object[] data)
        {
            double bpm = Math.Round(BPM, 2);

            return base.ToString(bpm.ToString(Program.Culture));
        }

        public override TimingPoint Clone()
        {
            return new(BPM, Ms);
        }
    }
}
