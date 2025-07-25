using New_SSQE.Misc.Static;

namespace New_SSQE.Objects
{
    internal class Note : XYMapObject
    {
        public bool Anchored;

        public bool EnableEasing;
        public EasingStyle Style;
        public EasingDirection Direction;

        public Note(float x, float y, long ms, bool enableEasing = false, EasingStyle style = EasingStyle.Linear, EasingDirection direction = EasingDirection.In) : base(0, x, y, ms, "Note")
        {
            EnableEasing = enableEasing;
            Style = style;
            Direction = direction;
        }

        public Note(string data) : this(0, 0, 0)
        {
            string[] split = data.Split('|');

            X = float.Parse(split[1], Program.Culture);
            Y = float.Parse(split[2], Program.Culture);
            Ms = long.Parse(split[0]);
        }

        public override string ToString(params object[] data)
        {
            return base.ToString(
                (EnableEasing ? 1 : 0).ToString(),
                (int)Style,
                (int)Direction
            );
        }

        public override Note Clone()
        {
            return new(X, Y, Ms, EnableEasing, Style, Direction);
        }
    }
}
