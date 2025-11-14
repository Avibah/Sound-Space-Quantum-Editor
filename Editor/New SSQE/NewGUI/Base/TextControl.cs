using New_SSQE.NewGUI.Font;
using OpenTK.Mathematics;
using System.Drawing;

namespace New_SSQE.NewGUI.Base
{
    internal enum CenterMode
    {
        None,
        X,
        Y,
        XY
    }

    internal abstract class TextControl : Control
    {
        private string text = "";
        private float textSize = 0;
        private string font = "main";
        private float alpha = 1;
        private Color textColor = Color.White;
        private bool textWrapped = false;
        private string prefix = "";
        private string suffix = "";

        public string RenderedText => prefix + (textWrapped ? WrapText() : text) + suffix;

        private CenterMode centerMode = CenterMode.XY;
        private Vector4[] verts = [];

        private float textX;
        private float textY;
        protected float xOffset = 0;
        protected float yOffset = 0;

        private string? startText = null;
        private float? startTextSize = null;
        private string? startFont = null;
        private float? startAlpha = null;
        private Color? startTextColor = null;

        public bool TextWrapped
        {
            get => textWrapped;
            set
            {
                if (value != textWrapped)
                {
                    textWrapped = value;
                    shouldUpdate = true;
                }
            }
        }

        public CenterMode CenterMode
        {
            get => centerMode;
            set
            {
                if (value != centerMode)
                {
                    centerMode = value;
                    shouldUpdate = true;
                }
            }
        }

        public string Text
        {
            get => text;
            set
            {
                if (value != text)
                {
                    text = value;
                    shouldUpdate = true;
                }

                startText ??= value;
            }
        }

        public string Prefix
        {
            get => prefix;
            set
            {
                if (value != prefix)
                {
                    prefix = value;
                    shouldUpdate = true;
                }
            }
        }

        public string Suffix
        {
            get => suffix;
            set
            {
                if (value != suffix)
                {
                    suffix = value;
                    shouldUpdate = true;
                }
            }
        }

        public float TextScale => Math.Min(rect.Width / StartRect.Width, rect.Height / StartRect.Height);

        public float TextSize
        {
            get => textSize * TextScale;
            set
            {
                if (value != textSize)
                {
                    textSize = value;
                    shouldUpdate = true;
                }

                startTextSize ??= value;
            }
        }
        
        public string Font
        {
            get => font;
            set
            {
                if (value != font)
                {
                    font = value;
                    shouldUpdate = true;
                }

                startFont ??= value;
            }
        }

        public float Alpha
        {
            get => alpha;
            set
            {
                if (value != alpha)
                {
                    alpha = value;
                    shouldUpdate = true;
                }

                startAlpha ??= value;
            }
        }

        public Color TextColor
        {
            get => textColor;
            set
            {
                if (value != textColor)
                {
                    textColor = value;
                    shouldUpdate = true;
                }

                startTextColor ??= value;
            }
        }

        public TextControl(float x, float y, float w, float h) : base(x, y, w, h) { }

        public override void Update()
        {
            base.Update();

            textX = rect.X + xOffset;
            textY = rect.Y + yOffset;

            string curText = RenderedText;

            if (CenterMode.HasFlag(CenterMode.X))
            {
                float width = FontRenderer.GetWidth(curText, TextSize, font);
                textX = rect.X + rect.Width / 2 - width / 2 + xOffset;
            }
            if (CenterMode.HasFlag(CenterMode.Y))
            {
                float height = FontRenderer.GetHeight(TextSize, font) * curText.Split('\n').Length;
                textY = rect.Y + rect.Height / 2 - height / 2 + yOffset;
            }

            verts = FontRenderer.Print(textX, textY, curText, TextSize, font);
        }

        public override void PostRender(float mousex, float mousey, float frametime)
        {
            if (verts.Length != 0)
            {
                float[] a = new float[verts.Length];
                Array.Fill(a, 1 - alpha);

                FontRenderer.SetActive(font);
                FontRenderer.SetColor(textColor);
                FontRenderer.RenderData(font, verts, a);
            }

            base.PostRender(mousex, mousey, frametime);
        }

        public override void Reset()
        {
            base.Reset();

            text = startText ?? text;
            textSize = startTextSize ?? textSize;
            font = startFont ?? font;
            alpha = startAlpha ?? alpha;
            textColor = startTextColor ?? textColor;

            shouldUpdate = true;
        }

        public override Vector4 GetExtents()
        {
            Vector4 extents = base.GetExtents();

            float tx = xOffset;
            float ty = yOffset;
            float tw = FontRenderer.GetWidth(text, TextSize, font);
            float th = FontRenderer.GetHeight(TextSize, font) * text.Split('\n').Length;

            if (CenterMode.HasFlag(CenterMode.X))
            {
                tx += rect.Width / 2;
                tw = tw / 2 + rect.Width / 2;
            }
            if (CenterMode.HasFlag(CenterMode.Y))
            {
                ty += rect.Height / 2;
                th = th / 2 + rect.Height / 2;
            }

            tw += xOffset;
            th += yOffset;

            return Vector4.ComponentMax(extents, (-tx, -ty, tw, th));
        }

        private string WrapText()
        {
            return text;
        }
    }
}
