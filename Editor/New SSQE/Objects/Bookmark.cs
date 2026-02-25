namespace New_SSQE.Objects
{
    internal class Bookmark : MapObject
    {
        public string Text { get; set; }
        public long EndMs { get; set; }

        public Bookmark(string text, long ms, long endMs) : base(17, ms, "Bookmark")
        {
            Text = text;
            EndMs = endMs;

            HasDuration = false;
        }

        public Bookmark(string data) : this("", 0, 0)
        {
            string[] split = data.Split('|');

            Text = split[2].Replace("\0\0", "|").Replace("\0", ",");
            Ms = long.Parse(split[0]);
            EndMs = long.Parse(split[1]);
        }

        public override string ToString(params object[] data)
        {
            string text = Text.Replace(",", "\0").Replace("|", "\0\0");

            return base.ToString(EndMs.ToString(), text);
        }

        public override Bookmark Clone()
        {
            return new(Text, Ms, EndMs);
        }
    }
}
