﻿namespace New_SSQE.Objects
{
    internal enum GlideDirection
    {
        Up,
        Right,
        Down,
        Left,
        None
    }

    internal class Glide : MapObject
    {
        public GlideDirection Direction;

        public Glide(long ms, GlideDirection direction) : base(13, ms, "Glide")
        {
            HasDuration = false;
            Direction = direction;
        }

        public override string ToString(params object[] data)
        {
            int direction = (int)Direction;

            return base.ToString(direction);
        }

        public override Glide Clone()
        {
            return new(Ms, Direction);
        }
    }
}
