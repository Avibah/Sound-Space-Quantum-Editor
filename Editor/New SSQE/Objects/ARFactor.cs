using New_SSQE.Misc.Static;

namespace New_SSQE.Objects
{
    internal class ARFactor : MapObject
    {
        public EasingStyle Style;
        public EasingDirection Direction;

        public float Factor;

        public ARFactor(long ms, long duration, EasingStyle style, EasingDirection direction, float factor) : base(10, ms, "AR Factor")
        {
            Duration = duration;

            Style = style;
            Direction = direction;

            Factor = factor;
        }

        public override string ToString(params object[] data)
        {
            return base.ToString(
                Duration,
                (int)Style,
                (int)Direction,
                Math.Round(Factor, 2).ToString(Program.Culture)
            );
        }

        public override ARFactor Clone()
        {
            return new(Ms, Duration, Style, Direction, Factor);
        }
    }
}
