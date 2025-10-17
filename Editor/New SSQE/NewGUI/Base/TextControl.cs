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
        private int textSize = 0;
        private string font = "main";
        private float alpha = 1;
        private Color textColor = Color.White;

        private CenterMode centerMode = CenterMode.XY;
        private Vector4[] verts = [];

        private float textX;
        private float textY;
        protected float xOffset = 0;
        protected float yOffset = 0;

        protected string? startText = null;
        protected int? startTextSize = null;
        protected string? startFont = null;
        protected float? startAlpha = null;
        protected Color? startTextColor = null;

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

        public float TextScale => Math.Min(rect.Width / startRect.Width, rect.Height / startRect.Height);

        public int TextSize
        {
            get => (int)(textSize * TextScale);
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
            }
        }

        public TextControl(float x, float y, float w, float h) : base(x, y, w, h) { }

        public override void Update()
        {
            base.Update();

            textX = rect.X + xOffset;
            textY = rect.Y + yOffset;

            if (CenterMode.HasFlag(CenterMode.X))
            {
                float width = FontRenderer.GetWidth(text, TextSize, font);
                textX = rect.X + rect.Width / 2 - width / 2 + xOffset;
            }
            if (CenterMode.HasFlag(CenterMode.Y))
            {
                float height = FontRenderer.GetHeight(TextSize, font) * text.Split('\n').Length;
                textY = rect.Y + rect.Height / 2 - height / 2 + yOffset;
            }

            verts = FontRenderer.Print(textX, textY, text, TextSize, font);
        }

        public override void PostRender(float mousex, float mousey, float frametime)
        {
            base.PostRender(mousex, mousey, frametime);
            if (verts.Length == 0)
                return;

            float[] a = new float[verts.Length];
            Array.Fill(a, 1 - alpha);

            FontRenderer.SetActive(font);
            FontRenderer.SetColor(textColor);
            FontRenderer.RenderData(font, verts, a);
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
    }
}
