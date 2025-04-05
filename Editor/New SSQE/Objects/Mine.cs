namespace New_SSQE.Objects
{
    internal class Mine : XYMapObject
    {
        public Mine(float x, float y, long ms) : base(14, x, y, ms, "Mine")
        {
            HasDuration = false;
            PlayHitsound = false;
        }

        public override Mine Clone()
        {
            return new(X, Y, Ms);
        }
    }
}
