using New_SSQE.NewMaps.Parsing;

namespace New_SSQE.Objects
{
    internal class Lyric : MapObject
    {
        public string Text;

        public bool FadeIn;
        public bool FadeOut;

        public Lyric(long ms, string text, bool fadeIn, bool fadeOut) : base(15, ms, "Lyric")
        {
            HasDuration = false;
            Text = text;

            FadeIn = fadeIn;
            FadeOut = fadeOut;

            PlayHitsound = false;
        }

        public override string ToString(params object[] data)
        {
            string text = FormatUtils.ConvertString(Text);

            int fadeIn = FadeIn ? 1 : 0;
            int fadeOut = FadeOut ? 1 : 0;

            return base.ToString(text, fadeIn, fadeOut);
        }

        public override Lyric Clone()
        {
            return new(Ms, Text, FadeIn, FadeOut);
        }
    }
}
