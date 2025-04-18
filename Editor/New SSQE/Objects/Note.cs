namespace New_SSQE.Objects
{
    internal class Note : XYMapObject
    {
        public bool Anchored;

        public Note(float x, float y, long ms) : base(0, x, y, ms, "Note")
        {

        }

        public Note(string data) : this(0, 0, 0)
        {
            string[] split = data.Split('|');

            X = float.Parse(split[1], Program.Culture);
            Y = float.Parse(split[2], Program.Culture);
            Ms = long.Parse(split[0]);
        }

        public override Note Clone()
        {
            return new(X, Y, Ms);
        }
    }
}
