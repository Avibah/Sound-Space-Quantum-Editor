namespace New_SSQE.Objects
{
    internal enum GlideDirection
    {
        Up,
        Right,
        Down,
        Left
    }

    internal class Glide : MapObject
    {
        public GlideDirection Direction;

        public Glide(long ms, GlideDirection direction) : base(13, ms, "Glide", false)
        {
            Direction = direction;
        }

        public override string ToString()
        {
            int direction = (int)Direction;

            return base.ToString(direction.ToString());
        }

        public override Glide Clone()
        {
            return new(Ms, Direction);
        }
    }
}
