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

        public override string ToString(params object[] data)
        {
            double x = Math.Round(X, 2);
            double y = Math.Round(Y, 2);

            return base.ToString(x.ToString(Program.Culture), y.ToString(Program.Culture), string.Join('|', data));
        }

        public override XYMapObject Clone()
        {
            return new(ID, X, Y, Ms, Name);
        }
    }
}
