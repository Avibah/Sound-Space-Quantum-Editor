namespace New_SSQE.Objects
{
    internal class Fever : MapObject
    {
        public Fever(long ms, long duration) : base(16, ms, "Fever")
        {
            Duration = duration;

            PlayHitsound = false;
        }

        public override string ToString(params object[] data)
        {
            return base.ToString(
                Duration
            );
        }

        public override Fever Clone()
        {
            return new(Ms, Duration);
        }
    }
}
