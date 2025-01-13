namespace New_SSQE.Objects
{
    internal class Beat : MapObject
    {
        public Beat(long ms) : base(12, ms, "Beat", false)
        {

        }

        public override Beat Clone()
        {
            return new(Ms);
        }
    }
}
