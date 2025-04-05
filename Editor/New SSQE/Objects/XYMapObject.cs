namespace New_SSQE.Objects
{
    internal class XYMapObject : MapObject
    {
        public float X;
        public float Y;

        public XYMapObject(int id, float x, float y, long ms, string? name = null) : base(id, ms, name)
        {
            X = x;
            Y = y;
        }

        public override XYMapObject Clone()
        {
            return new(ID, X, Y, Ms, Name);
        }
    }
}
