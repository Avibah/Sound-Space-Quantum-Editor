﻿using New_SSQE.Misc.Static;

namespace New_SSQE.Objects
{
    internal class Blur : MapObject
    {
        public EasingStyle Style;
        public EasingDirection Direction;

        public float Intensity;

        public Blur(long ms, long duration, EasingStyle style, EasingDirection direction, float intensity) : base(5, ms, "Blur")
        {
            Duration = duration;

            Style = style;
            Direction = direction;

            Intensity = intensity;
        }

        public override string ToString(params object[] data)
        {
            return base.ToString(
                Duration,
                (int)Style,
                (int)Direction,
                Math.Round(Intensity, 2).ToString(Program.Culture)
            );
        }

        public override Blur Clone()
        {
            return new(Ms, Duration, Style, Direction, Intensity);
        }
    }
}